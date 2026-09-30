using Contracts;
using Contracts.IServices;
using Dto;
using ExceptionHandler;
using Microsoft.AspNetCore.Http;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Contracts.IRepository;
using ClosedXML.Excel;

namespace Services
{
    public class ExcelUploadService : IExcelUploadService
    {
        private readonly IAribaServicePRService _aribaServicePRService;
        private readonly ILoggerManager _logger;
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly IPublicRelationService _publicRelationService;
        private readonly IPurchaseOrderBotProcessService _purchaseOrderBotProcessService;
        private readonly ILegalDepartmentMaterialService _legalDepartmentMaterialService;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IConciergePRCreationService _conciergeService;
        private readonly IContractExcelService _contractExcelService;

        public ExcelUploadService(ILoggerManager logger, IPurchaseOrderService purchaseOrderService, IPublicRelationService publicRelationService, IPurchaseOrderBotProcessService purchaseOrderBotProcessService, IRepositoryWrapper repositoryWrapper, IConciergePRCreationService conciergeService, IAribaServicePRService aribaServicePRService, ILegalDepartmentMaterialService legalDepartmentMaterialService, IContractExcelService contractExcelService)
        {
            _logger = logger;
            _purchaseOrderService = purchaseOrderService;
            _publicRelationService = publicRelationService;
            _purchaseOrderBotProcessService = purchaseOrderBotProcessService;
            _repositoryWrapper = repositoryWrapper;
            _conciergeService = conciergeService;
            _aribaServicePRService = aribaServicePRService;
            _legalDepartmentMaterialService = legalDepartmentMaterialService;
            _contractExcelService = contractExcelService;
        }

        /// <summary>
        /// Upload the PO Excel File with batch processing optimization
        /// </summary>
        /// <param name="excelFile"></param>
        public async Task<ExcelProcessingResultDto> UploadExcelFileAsync(IFormFile excelFile, Guid domainId)
        {
            _logger.LogInfo("Validating and reading Excel file.");

            using (Stream stream = excelFile.OpenReadStream())
            {
                XSSFWorkbook workbook = new XSSFWorkbook(stream);
                ISheet worksheet = workbook.GetSheetAt(0);

                List<string> expectedHeadersRoster = new List<string>
                {
                    "POOrderId", "EPID", "LineItem",
                    "UnitOfMeasure", "MaterialCode", "Quantity","Previous Qty",
                    "DomainId", "ReceiptID", "Status", "ProcessedState", "HardReprocess"
                };

                ValidateHeaders(worksheet, expectedHeadersRoster);
                _logger.LogInfo("Excel file validated successfully.");

                var allData = ExtractAllDataFromWorksheet(worksheet, domainId);
                _logger.LogInfo($"Extracted {allData.Count} rows from Excel file.");

                const int batchSize = 1000;
                var result = await ProcessDataInBatchesAsync(allData, batchSize);

                _logger.LogDebug("Excel File processing completed.");
                return result;
            }
        }

        private List<POExcelExtractDto> ExtractAllDataFromWorksheet(ISheet worksheet, Guid domainId)
        {
            var allData = new List<POExcelExtractDto>();

            for (int row = 1; row <= worksheet.LastRowNum; row++)
            {
                var currentRow = worksheet.GetRow(row);
                if (currentRow == null) continue;

                var dto = ExtractPurchaseOrderDataFromRow(currentRow);
                if (dto.DomainId == domainId)
                {
                    dto.RowNumber = row;
                    allData.Add(dto);
                }
            }

            return allData;
        }


        private async Task<ExcelProcessingResultDto> ProcessDataInBatchesAsync(
            List<POExcelExtractDto> allData,
            int batchSize)
        {
            int totalRows = allData.Count;
            int successfulUploads = 0;
            int failedUploads = 0;
            List<ErrorResponseDto> errorResponses = new List<ErrorResponseDto>();

            _logger.LogInfo($"Processing {totalRows} rows in batches of {batchSize}.");

            // Process batches SEQUENTIALLY to avoid DbContext threading issues
            for (int i = 0; i < allData.Count; i += batchSize)
            {
                List<POExcelExtractDto> batch = allData.Skip(i).Take(batchSize).ToList();
                int batchNumber = (i / batchSize) + 1;
                int totalBatches = (int)Math.Ceiling((double)allData.Count / batchSize);

                _logger.LogInfo($"Processing batch {batchNumber} of {totalBatches} (rows {i + 1} to {Math.Min(i + batchSize, allData.Count)})");

                // Validate batch
                List<POExcelExtractDto> validRows = new List<POExcelExtractDto>();
                foreach (POExcelExtractDto dto in batch)
                {
                    if (AnyRequiredColumnEmpty(dto))
                    {
                        AddErrorResponse(errorResponses, 400, $"One or more required columns are empty in row {dto.RowNumber}");
                        _logger.LogError($"One or more required columns are empty in row {dto.RowNumber}");
                        failedUploads++;
                    }
                    else
                    {
                        validRows.Add(dto);
                    }
                }

                if (validRows.Any())
                {
                    try
                    {
                        BatchProcessingResultDto batchResult = await _purchaseOrderService.CreatePurchaseOrdersBatchAsync(validRows);
                        successfulUploads += batchResult.SuccessfulCount;
                        failedUploads += batchResult.FailedCount;
                        errorResponses.AddRange(batchResult.Errors);

                        _logger.LogInfo($"Batch {batchNumber} completed. Successfully processed: {batchResult.SuccessfulCount}, Failed: {batchResult.FailedCount}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing batch {batchNumber}: {ex.Message}");
                        foreach (POExcelExtractDto dto in validRows)
                        {
                            AddErrorResponse(errorResponses, 500, $"Batch processing error for row {dto.RowNumber}: {ex.Message}");
                            failedUploads++;
                        }
                    }
                }
                else
                {
                    _logger.LogInfo($"Batch {batchNumber} contained no valid rows to process.");
                }
            }

            _logger.LogInfo($"All batches completed. Total successful: {successfulUploads}, Total failed: {failedUploads}");

            return new ExcelProcessingResultDto
            {
                TotalRows = totalRows,
                SuccessfulUploads = successfulUploads,
                FailedUploads = failedUploads,
                ErrorResponses = errorResponses
            };
        }

        private void ValidateHeaders(ISheet worksheet, List<string> expectedHeaders)
        {
            var headerRow = worksheet.GetRow(0);
            if (headerRow == null)
            {
                _logger.LogError("Excel file is empty.");
                throw new NoContentCustomException("Excel file is empty.", "Excel file is empty.");
            }

            for (int i = 0; i < headerRow.LastCellNum; i++)
            {
                var headerCell = headerRow.GetCell(i);
                if (headerCell == null || string.IsNullOrEmpty(headerCell.ToString()))
                {
                    _logger.LogError("Excel file contains empty column header.");
                    throw new NoContentCustomException("Excel file contains empty column header.", "Excel file contains empty column header.");
                }

                var headerText = headerCell.ToString()!.Trim();
                if (!expectedHeaders.Contains(headerText))
                {
                    _logger.LogError($"Excel file does not contain expected column header: {headerText}.");
                    throw new NotFoundCustomException($"Excel file does not contain expected column header: {headerText}.", $"Excel file does not contain expected column header: {headerText}.");
                }
            }
        }

        private static void AddErrorResponse(List<ErrorResponseDto> errorResponses, int statusCode, string description)
        {
            var errorResponse = new ErrorResponseDto
            {
                StatusCode = statusCode,
                Message = "Error processing row",
                Description = description
            };
            errorResponses.Add(errorResponse);
        }

        private POExcelExtractDto ExtractPurchaseOrderDataFromRow(IRow currentRow)
        {
            POExcelExtractDto dto = new POExcelExtractDto
            {
                POOrderId = currentRow.GetCell(0)?.ToString()!,
                EPID = currentRow.GetCell(1)?.ToString()!,
                LineItem = currentRow.GetCell(2)?.ToString()!,
                UnitOfMeasure = currentRow.GetCell(3)?.ToString()!,
                MaterialCode = currentRow.GetCell(4)?.ToString()!,
                Quantity = currentRow.GetCell(5) != null && double.TryParse(currentRow.GetCell(5).ToString(), out double quantity) ? Math.Round(quantity, 2) : 0.0,
                AcceptedQty = currentRow.GetCell(6) != null && double.TryParse(currentRow.GetCell(6).ToString(), out double acceptedQty) ? Math.Round(acceptedQty, 2) : 0.0,
                DomainId = currentRow.GetCell(7) != null && Guid.TryParse(currentRow.GetCell(7).ToString(), out Guid domainId) ? domainId : Guid.Empty,
                ReceiptID = currentRow.GetCell(8)?.ToString()!,
                Status = currentRow.GetCell(9)?.ToString(),
                ProcessedState = currentRow.GetCell(10) != null && int.TryParse(currentRow.GetCell(10).ToString(), out int processedStata) ? processedStata : (int?)null,
                HardReprocess = currentRow.GetCell(11) != null && bool.TryParse(currentRow.GetCell(11).ToString(), out bool hardReprocess) ? hardReprocess : false
            };
            return dto;
        }

        private bool AnyRequiredColumnEmpty(POExcelExtractDto dto)
        {
            return string.IsNullOrEmpty(dto.POOrderId) ||
                   string.IsNullOrEmpty(dto.EPID) ||
                   string.IsNullOrEmpty(dto.LineItem) ||
                   string.IsNullOrEmpty(dto.ReceiptID) ||
                   dto.DomainId == Guid.Empty;
        }

        /// <summary>
        /// Upload the PO Excel File with batch processing optimization
        /// </summary>
        /// <param name="excelFile"></param>
        public async Task<ExcelProcessingResultDto> UploadMasterExcelFileAsync(IFormFile excelFile, Guid domainId)
        {
            _logger.LogInfo("Validating and reading Master Excel file.");

            using (Stream stream = excelFile.OpenReadStream())
            {
                XSSFWorkbook workbook = new XSSFWorkbook(stream);
                ISheet worksheet = workbook.GetSheetAt(0);

                List<string> expectedHeadersRoster = new List<string>
                {
                    "POOrderId", "EPID", "LineItem", "UnitOfMeasure",
                    "MaterialCode", "Quantity", "DomainId", "RequisitionRequester",
                    "ERPCommodityCode", "SupplierId", "SupplierName", "Price","PriceCurrency",
                    "UnitPrice","RequisitionId","CompanyCode","UnitPriceCurrency",
                    "PurchaseGroup","PurchaseOrg","Description","UnitPriceInBasePrice","CommodityCode",
                    "ExternalLineNumber","Version","NeedByDate","AccountCategory","ItemCategory","ParentLineNumber",
                    "TaxCode","TaxPercentage","TaxAmountInBaseCurrency","LineTypeCategory","GLAccount",
                    "CostCenter","AssetCode","ContractId","ServiceMaxAmount","ServiceStartDate","ServiceEndDate","OrderedDate",
                    "POTitle","POStatus"
                };

                ValidateHeaders(worksheet, expectedHeadersRoster);
                _logger.LogInfo("Excel file validated successfully.");

                var allData = ExtractAllDataFromMasterWorksheet(worksheet, domainId);
                _logger.LogInfo($"Extracted {allData.Count} rows from Excel file.");

                const int batchSize = 1000;
                var result = await ProcessMasterDataInBatchesAsync(allData, batchSize);

                _logger.LogDebug("Master Excel File processing completed.");
                return result;
            }
        }

        private List<POMasterExcelExtractDto> ExtractAllDataFromMasterWorksheet(ISheet worksheet, Guid domainId)
        {
            var allData = new List<POMasterExcelExtractDto>();

            for (int row = 1; row <= worksheet.LastRowNum; row++)
            {
                var currentRow = worksheet.GetRow(row);
                if (currentRow == null) continue;

                var dto = ExtractPurchaseOrderMasterDataFromRow(currentRow);
                if (dto.DomainId == domainId)
                {
                    dto.RowNumber = row;
                    allData.Add(dto);
                }
            }

            return allData;
        }

        private POMasterExcelExtractDto ExtractPurchaseOrderMasterDataFromRow(IRow currentRow)
        {
            POMasterExcelExtractDto dto = new POMasterExcelExtractDto
            {
                POOrderId = currentRow.GetCell(0)?.ToString()!,
                EPID = currentRow.GetCell(1)?.ToString()!,
                LineItem = currentRow.GetCell(2)?.ToString()!,
                UnitOfMeasure = currentRow.GetCell(3)?.ToString()!,
                MaterialCode = currentRow.GetCell(4)?.ToString()!,
                Quantity = currentRow.GetCell(5) != null && double.TryParse(currentRow.GetCell(5).ToString(), out double quantity) ? Math.Round(quantity, 2) : 0.0,
                DomainId = currentRow.GetCell(6) != null && Guid.TryParse(currentRow.GetCell(6).ToString(), out Guid domainId) ? domainId : Guid.Empty,
                RequisitionRequester = currentRow.GetCell(7)?.ToString()!,
                ERPCommodityCode = currentRow.GetCell(8) != null && int.TryParse(currentRow.GetCell(8).ToString(), out int erpCommodityCode) ? erpCommodityCode : (int?)null,
                SupplierId = currentRow.GetCell(9)?.ToString()!,
                SupplierName = currentRow.GetCell(10)?.ToString()!,
                Price = currentRow.GetCell(11) != null && double.TryParse(currentRow.GetCell(11).ToString(), out double price) ? Math.Round(price, 2) : 0.0,
                PriceCurrency = currentRow.GetCell(12)?.ToString()!,
                UnitPrice = currentRow.GetCell(13) != null && double.TryParse(currentRow.GetCell(13).ToString(), out double unitPrice) ? Math.Round(unitPrice, 2) : (double?)null,
                RequisitionId = currentRow.GetCell(14)?.ToString()!,
                CompanyCode = currentRow.GetCell(15)?.ToString()!,
                UnitPriceCurrency = currentRow.GetCell(16)?.ToString()!,
                PurchaseGroup = currentRow.GetCell(17) != null && int.TryParse(currentRow.GetCell(17).ToString(), out int purchaseGroup) ? purchaseGroup : (int?)null,
                PurchaseOrg = currentRow.GetCell(18)?.ToString()!,
                Description = currentRow.GetCell(19)?.ToString()!,
                UnitPriceInBasePrice = currentRow.GetCell(20) != null && double.TryParse(currentRow.GetCell(20).ToString(), out double unitPriceInBasePrice) ? Math.Round(unitPriceInBasePrice, 2) : (double?)null,
                CommodityCode = currentRow.GetCell(21) != null && int.TryParse(currentRow.GetCell(21).ToString(), out int commodityCode) ? commodityCode : (int?)null,
                ExternalLineNumber = currentRow.GetCell(22)?.ToString()!,
                Version = currentRow.GetCell(23) != null && int.TryParse(currentRow.GetCell(23).ToString(), out int version) ? version : (int?)null,
                NeedByDate = currentRow.GetCell(24) != null && currentRow.GetCell(24).CellType == NPOI.SS.UserModel.CellType.Numeric && NPOI.SS.UserModel.DateUtil.IsCellDateFormatted(currentRow.GetCell(24)) ? currentRow.GetCell(24).DateCellValue : (DateTime?)null,
                AccountCategory = currentRow.GetCell(25)?.ToString()!,
                ItemCategory = currentRow.GetCell(26)?.ToString()!,
                ParentLineNumber = currentRow.GetCell(27) != null && int.TryParse(currentRow.GetCell(27).ToString(), out int parentLineNumber) ? parentLineNumber : (int?)null,
                TaxCode = currentRow.GetCell(28)?.ToString()!,
                TaxPercentage = currentRow.GetCell(29) != null && int.TryParse(currentRow.GetCell(29).ToString(), out int taxPercentage) ? taxPercentage : (int?)null,
                TaxAmountInBaseCurrency = currentRow.GetCell(30) != null && double.TryParse(currentRow.GetCell(30).ToString(), out double taxAmountInBaseCurrency) ? Math.Round(taxAmountInBaseCurrency, 2) : (double?)null,
                LineTypeCategory = currentRow.GetCell(31) != null ? currentRow.GetCell(31)?.ToString() : null,
                GLAccount = currentRow.GetCell(32)?.ToString()!,
                CostCenter = currentRow.GetCell(33)?.ToString()!,
                AssetCode = currentRow.GetCell(34)?.ToString()!,
                ContractId = currentRow.GetCell(35)?.ToString()!,
                ServiceMaxAmount = currentRow.GetCell(36) != null && double.TryParse(currentRow.GetCell(36).ToString(), out double serviceMaxAmount) ? Math.Round(serviceMaxAmount, 2) : (double?)null,
                ServiceStartDate = currentRow.GetCell(37) != null && currentRow.GetCell(37).CellType == NPOI.SS.UserModel.CellType.Numeric && NPOI.SS.UserModel.DateUtil.IsCellDateFormatted(currentRow.GetCell(37)) ? currentRow.GetCell(37).DateCellValue : (DateTime?)null,
                ServiceEndDate = currentRow.GetCell(38) != null && currentRow.GetCell(38).CellType == NPOI.SS.UserModel.CellType.Numeric && NPOI.SS.UserModel.DateUtil.IsCellDateFormatted(currentRow.GetCell(38)) ? currentRow.GetCell(38).DateCellValue : (DateTime?)null,
                OrderedDate = currentRow.GetCell(39) == null ? (DateTime?)null : (currentRow.GetCell(39).CellType == NPOI.SS.UserModel.CellType.Numeric && NPOI.SS.UserModel.DateUtil.IsCellDateFormatted(currentRow.GetCell(39)) ? currentRow.GetCell(39).DateCellValue : (currentRow.GetCell(39).CellType == NPOI.SS.UserModel.CellType.String && DateTime.TryParse(currentRow.GetCell(39).StringCellValue, out var parsedDate) ? parsedDate : (DateTime?)null)),
                HeaderTitle = currentRow.GetCell(40)?.ToString(),
                POStatus = currentRow.GetCell(41)?.ToString()
            };
            return dto;
        }

        private async Task<ExcelProcessingResultDto> ProcessMasterDataInBatchesAsync(
            List<POMasterExcelExtractDto> allData,
            int batchSize)
        {
            int totalRows = allData.Count;
            int successfulUploads = 0;
            int failedUploads = 0;
            List<ErrorResponseDto> errorResponses = new List<ErrorResponseDto>();

            _logger.LogInfo($"Processing {totalRows} rows in batches of {batchSize}.");

            // Process batches SEQUENTIALLY to avoid DbContext threading issues
            for (int i = 0; i < allData.Count; i += batchSize)
            {
                List<POMasterExcelExtractDto> batch = allData.Skip(i).Take(batchSize).ToList();
                int batchNumber = (i / batchSize) + 1;
                int totalBatches = (int)Math.Ceiling((double)allData.Count / batchSize);

                _logger.LogInfo($"Processing batch {batchNumber} of {totalBatches} (rows {i + 1} to {Math.Min(i + batchSize, allData.Count)})");

                // Validate batch
                List<POMasterExcelExtractDto> validRows = new List<POMasterExcelExtractDto>();
                foreach (POMasterExcelExtractDto dto in batch)
                {
                    if (AnyRequiredMasterColumnEmpty(dto))
                    {
                        AddErrorResponse(errorResponses, 400, $"One or more required columns are empty in row {dto.RowNumber}");
                        _logger.LogError($"One or more required columns are empty in row {dto.RowNumber}");
                        failedUploads++;
                    }
                    else
                    {
                        validRows.Add(dto);
                    }
                }

                if (validRows.Any())
                {
                    try
                    {
                        BatchProcessingResultDto batchResult = await _purchaseOrderService.CreatePurchaseOrdersMasterBatchAsync(validRows);
                        successfulUploads += batchResult.SuccessfulCount;
                        failedUploads += batchResult.FailedCount;
                        errorResponses.AddRange(batchResult.Errors);

                        _logger.LogInfo($"Batch {batchNumber} completed. Successfully processed: {batchResult.SuccessfulCount}, Failed: {batchResult.FailedCount}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing batch {batchNumber}: {ex.Message}");
                        foreach (POMasterExcelExtractDto dto in validRows)
                        {
                            AddErrorResponse(errorResponses, 500, $"Batch processing error for row {dto.RowNumber}: {ex.Message}");
                            failedUploads++;
                        }
                    }
                }
                else
                {
                    _logger.LogInfo($"Batch {batchNumber} contained no valid rows to process.");
                }
            }

            _logger.LogInfo($"All batches completed. Total successful: {successfulUploads}, Total failed: {failedUploads}");

            return new ExcelProcessingResultDto
            {
                TotalRows = totalRows,
                SuccessfulUploads = successfulUploads,
                FailedUploads = failedUploads,
                ErrorResponses = errorResponses
            };
        }

        private bool AnyRequiredMasterColumnEmpty(POMasterExcelExtractDto dto)
        {
            return string.IsNullOrEmpty(dto.POOrderId) ||
                   string.IsNullOrEmpty(dto.EPID) ||
                   string.IsNullOrEmpty(dto.LineItem) ||
                   string.IsNullOrEmpty(dto.RequisitionRequester) ||
                   dto.DomainId == Guid.Empty;
        }

        /// <summary>
        /// Uplaod the Contact Excel File
        /// </summary>
        /// <param name="excelFile"></param>
        public async Task<ExcelProcessingResultDto> UploadContractExcelFileAsync(IFormFile excelFile, Guid domainId)
        {
            _logger.LogInfo("Validating and reading Excel file.");

            using (Stream stream = excelFile.OpenReadStream())
            {
                XSSFWorkbook workbook = new XSSFWorkbook(stream);
                ISheet worksheet = workbook.GetSheetAt(0);

                List<string> expectedHeadersRoster = new List<string>
                {
                    "DomainId", "ContractName", "ContractId",
                    "BeginDate", "EndDate", "TotalAmount","WorkspaceID",
                    "AttachmentID", "MaterialCode", "CommodityCode", "Description", "SupplierID",
                    "SupplierName", "AmountSpend", "BalanceAmount","Currency"
                };

                ValidateHeaders(worksheet, expectedHeadersRoster);
                _logger.LogInfo("Excel file validated successfully.");

                var allData = ExtractAllContractDataFromWorksheet(worksheet, domainId);
                _logger.LogInfo($"Extracted {allData.Count} rows from Excel file.");

                const int batchSize = 1000;
                var result = await ProcessContractDataInBatchesAsync(allData, batchSize);

                _logger.LogDebug("Excel File processing completed.");
                return result;
            }
        }

        private List<ContractExcelExtractDto> ExtractAllContractDataFromWorksheet(ISheet worksheet, Guid domainId)
        {
            var allData = new List<ContractExcelExtractDto>();

            for (int row = 1; row <= worksheet.LastRowNum; row++)
            {
                var currentRow = worksheet.GetRow(row);
                if (currentRow == null) continue;

                var dto = ExtractContractDataFromRow(currentRow);
                if (dto.DomainId == domainId)
                {
                    dto.RowNumber = row;
                    allData.Add(dto);
                }
            }

            return allData;
        }

        private ContractExcelExtractDto ExtractContractDataFromRow(IRow currentRow)
        {
            ContractExcelExtractDto dto = new ContractExcelExtractDto
            {
                DomainId = currentRow.GetCell(0) != null && Guid.TryParse(currentRow.GetCell(0).ToString(), out Guid domainId) ? domainId : Guid.Empty,
                ContractName = currentRow.GetCell(1)?.ToString(),
                ContractId = currentRow.GetCell(2)?.ToString()!,
                BeginDate = currentRow.GetCell(3)?.CellType == NPOI.SS.UserModel.CellType.Numeric && NPOI.SS.UserModel.DateUtil.IsCellDateFormatted(currentRow.GetCell(3)) ? currentRow.GetCell(3).DateCellValue : DateTime.TryParse(currentRow.GetCell(3)?.ToString(), out var bd) ? bd : (DateTime?)null,
                EndDate = currentRow.GetCell(4)?.CellType == NPOI.SS.UserModel.CellType.Numeric && NPOI.SS.UserModel.DateUtil.IsCellDateFormatted(currentRow.GetCell(4)) ? currentRow.GetCell(4).DateCellValue : DateTime.TryParse(currentRow.GetCell(4)?.ToString(), out var ed) ? ed : (DateTime?)null,
                TotalAmount = currentRow.GetCell(5)?.ToString(),
                WorkSpaceId = currentRow.GetCell(6)?.ToString(),
                AttachmentId = currentRow.GetCell(7)?.ToString(),
                MaterialCode = currentRow.GetCell(8)?.ToString(),
                CommodityCode = currentRow.GetCell(9)?.ToString(),
                Description = currentRow.GetCell(10)?.ToString(),
                SupplierId = currentRow.GetCell(11)?.ToString(),
                SupplierName = currentRow.GetCell(12)?.ToString(),
                AmountSpend = currentRow.GetCell(13)?.ToString(),
                BalanceAmount = currentRow.GetCell(14)?.ToString(),
                Currency = currentRow.GetCell(15)?.ToString()
            };
            return dto;
        }

        private async Task<ExcelProcessingResultDto> ProcessContractDataInBatchesAsync(
            List<ContractExcelExtractDto> allData,
            int batchSize)
        {
            int totalRows = allData.Count;
            int successfulUploads = 0;
            int failedUploads = 0;
            List<ErrorResponseDto> errorResponses = new List<ErrorResponseDto>();

            _logger.LogInfo($"Processing {totalRows} rows in batches of {batchSize}.");

            // Process batches SEQUENTIALLY to avoid DbContext threading issues
            for (int i = 0; i < allData.Count; i += batchSize)
            {
                List<ContractExcelExtractDto> batch = allData.Skip(i).Take(batchSize).ToList();
                int batchNumber = (i / batchSize) + 1;
                int totalBatches = (int)Math.Ceiling((double)allData.Count / batchSize);

                _logger.LogInfo($"Processing batch {batchNumber} of {totalBatches} (rows {i + 1} to {Math.Min(i + batchSize, allData.Count)})");

                // Validate batch
                List<ContractExcelExtractDto> validRows = new List<ContractExcelExtractDto>();
                foreach (ContractExcelExtractDto dto in batch)
                {
                    if (AnyRequiredContractColumnEmpty(dto))
                    {
                        AddErrorResponse(errorResponses, 400, $"One or more required columns are empty in row {dto.RowNumber}");
                        _logger.LogError($"One or more required columns are empty in row {dto.RowNumber}");
                        failedUploads++;
                    }
                    else
                    {
                        validRows.Add(dto);
                    }
                }

                if (validRows.Any())
                {
                    try
                    {
                        BatchProcessingResultDto batchResult = await _purchaseOrderService.CreateContractBatchAsync(validRows);
                        successfulUploads += batchResult.SuccessfulCount;
                        failedUploads += batchResult.FailedCount;
                        errorResponses.AddRange(batchResult.Errors);

                        _logger.LogInfo($"Batch {batchNumber} completed. Successfully processed: {batchResult.SuccessfulCount}, Failed: {batchResult.FailedCount}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing batch {batchNumber}: {ex.Message}");
                        foreach (ContractExcelExtractDto dto in validRows)
                        {
                            AddErrorResponse(errorResponses, 500, $"Batch processing error for row {dto.RowNumber}: {ex.Message}");
                            failedUploads++;
                        }
                    }
                }
                else
                {
                    _logger.LogInfo($"Batch {batchNumber} contained no valid rows to process.");
                }
            }

            _logger.LogInfo($"All batches completed. Total successful: {successfulUploads}, Total failed: {failedUploads}");

            return new ExcelProcessingResultDto
            {
                TotalRows = totalRows,
                SuccessfulUploads = successfulUploads,
                FailedUploads = failedUploads,
                ErrorResponses = errorResponses
            };
        }

        private bool AnyRequiredContractColumnEmpty(ContractExcelExtractDto dto)
        {
            return string.IsNullOrEmpty(dto.ContractId) ||
                   dto.DomainId == Guid.Empty;
        }

        /// <summary>
        /// Uplaod the Contact Excel File
        /// </summary>
        /// <param name="excelFile"></param>
        public async Task<ExcelProcessingResultDto> UploadCompanyCodeExcelFileAsync(IFormFile excelFile, Guid domainId)
        {
            _logger.LogInfo("Validating and reading Excel file.");

            using (Stream stream = excelFile.OpenReadStream())
            {
                XSSFWorkbook workbook = new XSSFWorkbook(stream);
                ISheet worksheet = workbook.GetSheetAt(0);

                List<string> expectedHeadersRoster = new List<string>
                {
                    "DomainId", "CompanyCode", "CompanyName"
                };

                ValidateHeaders(worksheet, expectedHeadersRoster);
                _logger.LogInfo("Excel file validated successfully.");

                var allData = ExtractAllCompanyCodeFromWorksheet(worksheet, domainId);
                _logger.LogInfo($"Extracted {allData.Count} rows from Excel file.");

                const int batchSize = 1000;
                var result = await ProcessCompanyDataInBatchesAsync(allData, batchSize);

                _logger.LogDebug("Excel File processing completed.");
                return result;
            }
        }

        private List<CompanyCodeExcelDto> ExtractAllCompanyCodeFromWorksheet(ISheet worksheet, Guid domainId)
        {
            var allData = new List<CompanyCodeExcelDto>();

            for (int row = 1; row <= worksheet.LastRowNum; row++)
            {
                var currentRow = worksheet.GetRow(row);
                if (currentRow == null) continue;

                var dto = ExtractCompanyDataFromRow(currentRow);
                if (dto.DomainId == domainId)
                {
                    dto.RowNumber = row;
                    allData.Add(dto);
                }
            }

            return allData;
        }

        private CompanyCodeExcelDto ExtractCompanyDataFromRow(IRow currentRow)
        {
            CompanyCodeExcelDto dto = new CompanyCodeExcelDto
            {
                DomainId = currentRow.GetCell(0) != null && Guid.TryParse(currentRow.GetCell(0).ToString(), out Guid domainId) ? domainId : Guid.Empty,
                CompanyCode = currentRow.GetCell(1)?.ToString(),
                CompanyName = currentRow.GetCell(2)?.ToString()
            };
            return dto;
        }

        private async Task<ExcelProcessingResultDto> ProcessCompanyDataInBatchesAsync(
            List<CompanyCodeExcelDto> allData,
            int batchSize)
        {
            int totalRows = allData.Count;
            int successfulUploads = 0;
            int failedUploads = 0;
            List<ErrorResponseDto> errorResponses = new List<ErrorResponseDto>();

            _logger.LogInfo($"Processing {totalRows} rows in batches of {batchSize}.");

            // Process batches SEQUENTIALLY to avoid DbContext threading issues
            for (int i = 0; i < allData.Count; i += batchSize)
            {
                List<CompanyCodeExcelDto> batch = allData.Skip(i).Take(batchSize).ToList();
                int batchNumber = (i / batchSize) + 1;
                int totalBatches = (int)Math.Ceiling((double)allData.Count / batchSize);

                _logger.LogInfo($"Processing batch {batchNumber} of {totalBatches} (rows {i + 1} to {Math.Min(i + batchSize, allData.Count)})");

                // Validate batch
                List<CompanyCodeExcelDto> validRows = new List<CompanyCodeExcelDto>();
                foreach (CompanyCodeExcelDto dto in batch)
                {
                    if (AnyRequiredCompanyColumnEmpty(dto))
                    {
                        AddErrorResponse(errorResponses, 400, $"One or more required columns are empty in row {dto.RowNumber}");
                        _logger.LogError($"One or more required columns are empty in row {dto.RowNumber}");
                        failedUploads++;
                    }
                    else
                    {
                        validRows.Add(dto);
                    }
                }

                if (validRows.Any())
                {
                    try
                    {
                        BatchProcessingResultDto batchResult = await _purchaseOrderService.CreateCompanyCodeBatchAsync(validRows);
                        successfulUploads += batchResult.SuccessfulCount;
                        failedUploads += batchResult.FailedCount;
                        errorResponses.AddRange(batchResult.Errors);

                        _logger.LogInfo($"Batch {batchNumber} completed. Successfully processed: {batchResult.SuccessfulCount}, Failed: {batchResult.FailedCount}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing batch {batchNumber}: {ex.Message}");
                        foreach (CompanyCodeExcelDto dto in validRows)
                        {
                            AddErrorResponse(errorResponses, 500, $"Batch processing error for row {dto.RowNumber}: {ex.Message}");
                            failedUploads++;
                        }
                    }
                }
                else
                {
                    _logger.LogInfo($"Batch {batchNumber} contained no valid rows to process.");
                }
            }

            _logger.LogInfo($"All batches completed. Total successful: {successfulUploads}, Total failed: {failedUploads}");

            return new ExcelProcessingResultDto
            {
                TotalRows = totalRows,
                SuccessfulUploads = successfulUploads,
                FailedUploads = failedUploads,
                ErrorResponses = errorResponses
            };
        }

        private bool AnyRequiredCompanyColumnEmpty(CompanyCodeExcelDto dto)
        {
            return string.IsNullOrEmpty(dto.CompanyCode) ||
                   string.IsNullOrEmpty(dto.CompanyName) ||
                   dto.DomainId == Guid.Empty;
        }
        /// <summary>
        /// Uplaod the Material PR Excel File
        /// </summary>
        /// <param name="excelFile"></param>
        public async Task<ExcelProcessingResultDto> UploadMaterialPRExcelFileAsync(IFormFile excelFile, Guid domainId, Guid userId)
        {
            _logger.LogInfo("Validating and reading Material PR Excel file.");
            var profileDto = _repositoryWrapper.Person.GetMyProfileByUserId(userId);
            if (profileDto == null)
            {
                _logger.LogError($"User profile missing: No profile found for userId {userId}");
                throw new NotFoundCustomException("User profile missing", $"No profile found for userId {userId}");
            }
            using (Stream stream = excelFile.OpenReadStream())
            {
                XSSFWorkbook workbook = new XSSFWorkbook(stream);
                ISheet worksheet = workbook.GetSheetAt(0);

                List<string> expectedHeadersRoster = new List<string>
                {
                    "PurchaseOrg", "Description",
                    "Commodity", "Amount",
                    "DeliveryDate", "Quantity", "UOM", "Vendor",
                    "Contract", "Title", "Currency",
                    "GLAccount", "CostCenter", "ShipToPlant", "CompanyCode",
                    "PurchaseGroup", "BillTo", "TaxCode",
                  "DeliverTo","NeedBy","LineItemNumber","PRUniqueName","MaterialCode"
                };

                ValidateHeaders(worksheet, expectedHeadersRoster);
                _logger.LogInfo("Excel file validated successfully.");

                var allData = ExtractAllMaterialPRDataFromWorksheet(worksheet, domainId, profileDto);
                _logger.LogInfo($"Extracted {allData.Count} rows from Excel file.");

                const int batchSize = 1000;
                var result = await ProcessMaterialPRDataInBatchesAsync(allData, batchSize);

                _logger.LogDebug("Material PR Excel File processing completed.");
                return result;
            }
        }
        private List<MaterialPRExcelDto> ExtractAllMaterialPRDataFromWorksheet(ISheet worksheet, Guid domainId, MyProfileDto profileDto)
        {
            var allData = new List<MaterialPRExcelDto>();

            for (int row = 1; row <= worksheet.LastRowNum; row++)
            {
                var currentRow = worksheet.GetRow(row);
                if (currentRow == null) continue;

                var dto = ExtractMaterialPRDataFromRow(currentRow, profileDto);
                dto.DomainId = domainId;
                dto.RowNumber = row;
                allData.Add(dto);
            }

            return allData;
        }
        private MaterialPRExcelDto ExtractMaterialPRDataFromRow(IRow currentRow, MyProfileDto profileDto)
        {
            MaterialPRExcelDto dto = new MaterialPRExcelDto
            {
                PurchaseOrg = currentRow.GetCell(0)?.ToString()!,
                Description = currentRow.GetCell(1)?.ToString()!,
                Commodity = currentRow.GetCell(2)?.ToString()!,

                Amount = currentRow.GetCell(3) != null && decimal.TryParse(currentRow.GetCell(3).ToString(), out decimal amount) ? amount : 0.0m,
                DeliveryDate = currentRow.GetCell(4)?.CellType == NPOI.SS.UserModel.CellType.Numeric && NPOI.SS.UserModel.DateUtil.IsCellDateFormatted(currentRow.GetCell(4)) ? currentRow.GetCell(4).DateCellValue : DateTime.TryParse(currentRow.GetCell(4)?.ToString(), out var dd) ? dd : DateTime.MinValue,
                Quantity = currentRow.GetCell(5) != null && decimal.TryParse(currentRow.GetCell(5).ToString(), out decimal quantity) ? quantity : 0.0m,
                UOM = currentRow.GetCell(6)?.ToString()!,
                Vendor = currentRow.GetCell(7)?.ToString()!,
                Contract = currentRow.GetCell(8)?.ToString()!,
                Title = currentRow.GetCell(9)?.ToString()!,
                Currency = currentRow.GetCell(10)?.ToString()!,
                RequesterName = profileDto.AribaUserName,
                RequesterPasswordAdapter = profileDto.AribaPassword,

                GLAccount = currentRow.GetCell(11)?.ToString()!,
                CostCenter = currentRow.GetCell(12)?.ToString()!,
                ShipToPlant = currentRow.GetCell(13)?.ToString()!,
                CompanyCode = currentRow.GetCell(14)?.ToString()!,
                PurchaseGroup = currentRow.GetCell(15)?.ToString()!,
                BillTo = currentRow.GetCell(16)?.ToString()!,
                TaxCode = currentRow.GetCell(17)?.ToString()!,
                DeliverTo = currentRow.GetCell(18)?.ToString()!,

                NeedBy = currentRow.GetCell(19)?.CellType == NPOI.SS.UserModel.CellType.Numeric &&
                 NPOI.SS.UserModel.DateUtil.IsCellDateFormatted(currentRow.GetCell(19))
                 ? currentRow.GetCell(19).DateCellValue
                 : DateTime.TryParse(currentRow.GetCell(19)?.ToString(), out var dds) ? dds : DateTime.MinValue,

                LineItemNumber = currentRow.GetCell(20) != null &&
                         int.TryParse(currentRow.GetCell(20).ToString(), out int lineItemNumber)
                         ? lineItemNumber : 0,

                PRUniqueName = currentRow.GetCell(21)?.ToString()!,
                MaterialCode = currentRow.GetCell(22)?.ToString()!

            };
            return dto;
        }
        private bool AnyRequiredMaterialPRColumnEmpty(MaterialPRExcelDto dto)
        {
            return string.IsNullOrEmpty(dto.PurchaseOrg) ||
                   string.IsNullOrEmpty(dto.Description) ||
                   string.IsNullOrEmpty(dto.Commodity)
||
                   dto.Amount <= 0 ||
                   dto.DeliveryDate == DateTime.MinValue || dto.NeedBy == DateTime.MinValue ||
                   dto.Quantity <= 0;
        }
        private async Task<ExcelProcessingResultDto> ProcessMaterialPRDataInBatchesAsync(
            List<MaterialPRExcelDto> allData,
            int batchSize)
        {
            int totalRows = allData.Count;
            int successfulUploads = 0;
            int failedUploads = 0;
            List<ErrorResponseDto> errorResponses = new List<ErrorResponseDto>();

            _logger.LogInfo($"Processing {totalRows} rows in batches of {batchSize}.");

            // Process batches SEQUENTIALLY to avoid DbContext threading issues
            for (int i = 0; i < allData.Count; i += batchSize)
            {
                List<MaterialPRExcelDto> batch = allData.Skip(i).Take(batchSize).ToList();
                int batchNumber = (i / batchSize) + 1;
                int totalBatches = (int)Math.Ceiling((double)allData.Count / batchSize);

                _logger.LogInfo($"Processing batch {batchNumber} of {totalBatches} (rows {i + 1} to {Math.Min(i + batchSize, allData.Count)})");

                // Validate batch
                List<MaterialPRExcelDto> validRows = new List<MaterialPRExcelDto>();
                foreach (MaterialPRExcelDto dto in batch)
                {
                    if (AnyRequiredMaterialPRColumnEmpty(dto))
                    {
                        AddErrorResponse(errorResponses, 400, $"One or more required columns are empty in row {dto.RowNumber}");
                        _logger.LogError($"One or more required columns are empty in row {dto.RowNumber}");
                        failedUploads++;
                    }
                    else
                    {
                        validRows.Add(dto);
                    }
                }

                if (validRows.Any())
                {
                    try
                    {
                        BatchProcessingResultDto batchResult = await _purchaseOrderService.CreateMaterialPRBatchAsync(validRows);
                        successfulUploads += batchResult.SuccessfulCount;
                        failedUploads += batchResult.FailedCount;
                        errorResponses.AddRange(batchResult.Errors);

                        _logger.LogInfo($"Batch {batchNumber} completed. Successfully processed: {batchResult.SuccessfulCount}, Failed: {batchResult.FailedCount}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing batch {batchNumber}: {ex.Message}");
                        foreach (MaterialPRExcelDto dto in validRows)
                        {
                            AddErrorResponse(errorResponses, 500, $"Batch processing error for row {dto.RowNumber}: {ex.Message}");
                            failedUploads++;
                        }
                    }
                }
                else
                {
                    _logger.LogInfo($"Batch {batchNumber} contained no valid rows to process.");
                }
            }
            _logger.LogInfo($"All batches completed. Total successful: {successfulUploads}, Total failed: {failedUploads}");
            return new ExcelProcessingResultDto
            {
                TotalRows = totalRows,
                SuccessfulUploads = successfulUploads,
                FailedUploads = failedUploads,
                ErrorResponses = errorResponses
            };
        }
        /// <summary>
        /// Upload the Block and Unblock Excel File
        /// <summary>
        public async Task<ExcelProcessingResultDto> UploadBlockUnblockExcelFileAsync(IFormFile excelFile, Guid domainId)
        {
            _logger.LogInfo("Validating and reading Block And Unblock Excel file.");

            using (Stream stream = excelFile.OpenReadStream())
            {
                XSSFWorkbook workbook = new XSSFWorkbook(stream);
                ISheet worksheet = workbook.GetSheetAt(0);

                List<string> expectedHeadersRoster = new List<string>
                {
                    "POOrderId", "DomainId", "BlockUnblockStatus"
                };

                ValidateHeaders(worksheet, expectedHeadersRoster);
                _logger.LogInfo("Excel file validated successfully.");

                var allData = ExtractAllBlockAndUnblockFromWorksheet(worksheet, domainId);
                _logger.LogInfo($"Extracted {allData.Count} rows from Excel file.");

                const int batchSize = 1000;
                var result = await ProcessBlockAndUnblockDataInBatchesAsync(allData, batchSize);

                _logger.LogDebug("Block and Unblock Excel File processing completed.");
                return result;
            }
        }

        private List<BlockOrUnblockDataDto> ExtractAllBlockAndUnblockFromWorksheet(ISheet worksheet, Guid domainId)
        {
            var allData = new List<BlockOrUnblockDataDto>();

            for (int row = 1; row <= worksheet.LastRowNum; row++)
            {
                var currentRow = worksheet.GetRow(row);
                if (currentRow == null) continue;

                var dto = ExtractBlockOrUnblockDataFromRow(currentRow);
                if (dto.DomainId == domainId)
                {
                    dto.RowNumber = row;
                    allData.Add(dto);
                }
            }

            return allData;
        }

        private BlockOrUnblockDataDto ExtractBlockOrUnblockDataFromRow(IRow currentRow)
        {
            BlockOrUnblockDataDto dto = new BlockOrUnblockDataDto
            {
                POOrderId = currentRow.GetCell(0)?.ToString()!,
                DomainId = currentRow.GetCell(1) != null && Guid.TryParse(currentRow.GetCell(1).ToString(), out Guid domainId) ? domainId : Guid.Empty,
                IsBlocked = currentRow.GetCell(2) != null && bool.TryParse(currentRow.GetCell(2).ToString(), out bool hardReprocess) ? hardReprocess : false
            };
            return dto;
        }

        private async Task<ExcelProcessingResultDto> ProcessBlockAndUnblockDataInBatchesAsync(
            List<BlockOrUnblockDataDto> allData,
            int batchSize)
        {
            int totalRows = allData.Count;
            int successfulUploads = 0;
            int failedUploads = 0;
            List<ErrorResponseDto> errorResponses = new List<ErrorResponseDto>();

            _logger.LogInfo($"Processing {totalRows} rows in batches of {batchSize}.");

            // Process batches SEQUENTIALLY to avoid DbContext threading issues
            for (int i = 0; i < allData.Count; i += batchSize)
            {
                List<BlockOrUnblockDataDto> batch = allData.Skip(i).Take(batchSize).ToList();
                int batchNumber = (i / batchSize) + 1;
                int totalBatches = (int)Math.Ceiling((double)allData.Count / batchSize);

                _logger.LogInfo($"Processing batch {batchNumber} of {totalBatches} (rows {i + 1} to {Math.Min(i + batchSize, allData.Count)})");

                // Validate batch
                List<BlockOrUnblockDataDto> validRows = new List<BlockOrUnblockDataDto>();
                foreach (BlockOrUnblockDataDto dto in batch)
                {
                    if (AnyRequiredColumnBlockOrUnblockEmpty(dto))
                    {
                        AddErrorResponse(errorResponses, 400, $"One or more required columns are empty in row {dto.RowNumber}");
                        _logger.LogError($"One or more required columns are empty in row {dto.RowNumber}");
                        failedUploads++;
                    }
                    else
                    {
                        validRows.Add(dto);
                    }
                }

                if (validRows.Any())
                {
                    try
                    {
                        BatchProcessingResultDto batchResult = await _purchaseOrderService.UpdateBlockOrUnblockPurchaseOrderBatchAsync(validRows);
                        successfulUploads += batchResult.SuccessfulCount;
                        failedUploads += batchResult.FailedCount;
                        errorResponses.AddRange(batchResult.Errors);

                        _logger.LogInfo($"Batch {batchNumber} completed. Successfully processed: {batchResult.SuccessfulCount}, Failed: {batchResult.FailedCount}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing batch {batchNumber}: {ex.Message}");
                        foreach (BlockOrUnblockDataDto dto in validRows)
                        {
                            AddErrorResponse(errorResponses, 500, $"Batch processing error for row {dto.RowNumber}: {ex.Message}");
                            failedUploads++;
                        }
                    }
                }
                else
                {
                    _logger.LogInfo($"Batch {batchNumber} contained no valid rows to process.");
                }
            }

            _logger.LogInfo($"All batches completed. Total successful: {successfulUploads}, Total failed: {failedUploads}");

            return new ExcelProcessingResultDto
            {
                TotalRows = totalRows,
                SuccessfulUploads = successfulUploads,
                FailedUploads = failedUploads,
                ErrorResponses = errorResponses
            };
        }

        private bool AnyRequiredColumnBlockOrUnblockEmpty(BlockOrUnblockDataDto dto)
        {
            return string.IsNullOrEmpty(dto.POOrderId) ||
                   dto.DomainId == Guid.Empty;
        }

        /// <summary>
        /// Uplaod the Service PR Excel File
        /// </summary>
        /// <param name="excelFile"></param>
        public async Task<ExcelProcessingResultDto> UploadServicePRExcelFileAsync(
        IFormFile excelFile,
        Guid domainId,
        Guid userId)
        {
            _logger.LogInfo("Validating and reading Service PR Excel file.");

            var profileDto = _repositoryWrapper.Person.GetMyProfileByUserId(userId);
            if (profileDto == null)
            {
                _logger.LogError($"User profile missing: No profile found for userId {userId}");
                throw new NotFoundCustomException("User profile missing", $"No profile found for userId {userId}");
            }

            using (Stream stream = excelFile.OpenReadStream())
            {
                XSSFWorkbook workbook = new XSSFWorkbook(stream);
                ISheet worksheet = workbook.GetSheetAt(0);

                List<string> expectedHeadersRoster = new List<string>
        {
            "PurchaseOrg", "Description", "Commodity", "Amount",
            "ServiceStartDate", "ServiceEndDate",
            "Vendor", "Contract", "Title", "Currency",
            "GLAccount", "CostCenter", "CompanyCode",
            "PurchaseGroup", "BillTo", "TaxCode",
            "PRServiceUniqueName", "LineItemNumber", "ExpectedAmount","MaxAmount","DeliverTo","MaterialCode","UOM","ShipToPlant"
        };

                ValidateHeaders(worksheet, expectedHeadersRoster);
                _logger.LogInfo("Service PR Excel file validated successfully.");

                var allData = ExtractAllServicePRDataFromWorksheet(
                    worksheet, domainId, profileDto);

                _logger.LogInfo($"Extracted {allData.Count} rows from Service PR Excel file.");

                const int batchSize = 1000;
                var result = await ProcessServicePRDataInBatchesAsync(allData, batchSize);

                _logger.LogDebug("Service PR Excel file processing completed.");
                return result;
            }
        }
        private List<ServicePRExcelDto> ExtractAllServicePRDataFromWorksheet(
            ISheet worksheet,
            Guid domainId,
            MyProfileDto profileDto)
        {
            var allData = new List<ServicePRExcelDto>();

            for (int row = 1; row <= worksheet.LastRowNum; row++)
            {
                var currentRow = worksheet.GetRow(row);
                if (currentRow == null) continue;

                var dto = ExtractServicePRDataFromRow(currentRow, profileDto);
                dto.DomainId = domainId;
                dto.RowNumber = row;

                allData.Add(dto);
            }

            return allData;
        }
        private ServicePRExcelDto ExtractServicePRDataFromRow(
            IRow currentRow,
            MyProfileDto profileDto)
        {
            return new ServicePRExcelDto
            {
                PurchaseOrg = currentRow.GetCell(0)?.ToString()!,
                Description = currentRow.GetCell(1)?.ToString()!,
                Commodity = currentRow.GetCell(2)?.ToString()!,

                Amount = currentRow.GetCell(3) != null &&
                         decimal.TryParse(currentRow.GetCell(3).ToString(), out var amount)
                         ? amount : 0.0m,

                ServiceStartDate = currentRow.GetCell(4)?.CellType == CellType.Numeric &&
                            DateUtil.IsCellDateFormatted(currentRow.GetCell(4))
                            ? currentRow.GetCell(4).DateCellValue
                            : DateTime.TryParse(currentRow.GetCell(4)?.ToString(), out var sd)
                                ? sd : DateTime.MinValue,

                ServiceEndDate = currentRow.GetCell(5)?.CellType == CellType.Numeric &&
                          DateUtil.IsCellDateFormatted(currentRow.GetCell(5))
                          ? currentRow.GetCell(5).DateCellValue
                          : DateTime.TryParse(currentRow.GetCell(5)?.ToString(), out var ed)
                              ? ed : DateTime.MinValue,

                Vendor = currentRow.GetCell(6)?.ToString()!,
                Contract = currentRow.GetCell(7)?.ToString()!,
                Title = currentRow.GetCell(8)?.ToString()!,
                Currency = currentRow.GetCell(9)?.ToString()!,

                PreparerName = profileDto.AribaUserName,
                PreparerPasswordAdapter = profileDto.AribaPassword,

                GLAccount = currentRow.GetCell(10)?.ToString()!,
                CostCenter = currentRow.GetCell(11)?.ToString()!,
                CompanyCode = currentRow.GetCell(12)?.ToString()!,
                PurchaseGroup = currentRow.GetCell(13)?.ToString()!,
                BillTo = currentRow.GetCell(14)?.ToString()!,
                TaxCode = currentRow.GetCell(15)?.ToString()!,

                PRServiceUniqueName = currentRow.GetCell(16)?.ToString()!,

                LineItemNumber = currentRow.GetCell(17) != null &&
                                 int.TryParse(currentRow.GetCell(17).ToString(), out var line)
                                 ? line : 0,
                ExpectedAmount = currentRow.GetCell(18) != null &&
                         decimal.TryParse(currentRow.GetCell(18).ToString(), out var Expectedamount)
                         ? Expectedamount : 0.0m,
                MaxAmount = currentRow.GetCell(19) != null &&
                         decimal.TryParse(currentRow.GetCell(19).ToString(), out var Maxamount)
                         ? Maxamount : 0.0m,
                DeliverTo = currentRow.GetCell(20)?.ToString()!,
                MaterialCode = currentRow.GetCell(21)?.ToString()!,
                UOM = currentRow.GetCell(22)?.ToString()!,
                ShipToPlant = currentRow.GetCell(23)?.ToString()!


            };
        }
        private bool AnyRequiredServicePRColumnEmpty(ServicePRExcelDto dto)
        {
            return string.IsNullOrEmpty(dto.PurchaseOrg) ||
                   string.IsNullOrEmpty(dto.Description) ||
                   string.IsNullOrEmpty(dto.Commodity) ||
                   string.IsNullOrEmpty(dto.PRServiceUniqueName) ||
                   dto.Amount <= 0 ||
                   dto.ServiceStartDate == DateTime.MinValue ||
                   dto.ServiceEndDate == DateTime.MinValue;
        }
        private async Task<ExcelProcessingResultDto> ProcessServicePRDataInBatchesAsync(
            List<ServicePRExcelDto> allData,
            int batchSize)
        {
            int totalRows = allData.Count;
            int successfulUploads = 0;
            int failedUploads = 0;

            List<ErrorResponseDto> errorResponses = new();

            _logger.LogInfo($"Processing {totalRows} Service PR rows in batches of {batchSize}.");

            for (int i = 0; i < allData.Count; i += batchSize)
            {
                var batch = allData.Skip(i).Take(batchSize).ToList();
                int batchNumber = (i / batchSize) + 1;

                _logger.LogInfo($"Processing Service PR batch {batchNumber}");

                List<ServicePRExcelDto> validRows = new();

                foreach (var dto in batch)
                {
                    if (AnyRequiredServicePRColumnEmpty(dto))
                    {
                        AddErrorResponse(
                            errorResponses,
                            400,
                            $"One or more required columns are empty in row {dto.RowNumber}");

                        failedUploads++;
                    }
                    else
                    {
                        validRows.Add(dto);
                    }
                }

                if (validRows.Any())
                {
                    try
                    {
                        BatchProcessingResultDto batchResult =
                            await _purchaseOrderService.CreateServicePRBatchAsync(validRows);

                        successfulUploads += batchResult.SuccessfulCount;
                        failedUploads += batchResult.FailedCount;
                        errorResponses.AddRange(batchResult.Errors);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Service PR batch failed: {ex.Message}");

                        foreach (var dto in validRows)
                        {
                            AddErrorResponse(
                                errorResponses,
                                500,
                                $"Batch processing error for row {dto.RowNumber}: {ex.Message}");

                            failedUploads++;
                        }
                    }
                }
            }

            return new ExcelProcessingResultDto
            {
                TotalRows = totalRows,
                SuccessfulUploads = successfulUploads,
                FailedUploads = failedUploads,
                ErrorResponses = errorResponses
            };
        }


        /// <summary>
        /// Uplaod the Contact Excel File
        /// </summary>
        /// <param name="excelFile"></param>
        public async Task<ExcelProcessingResultDto> UploadSupplierNameExcelFileAsync(IFormFile excelFile, Guid domainId)
        {
            _logger.LogInfo("Validating and reading Excel file.");

            using (Stream stream = excelFile.OpenReadStream())
            {
                XSSFWorkbook workbook = new XSSFWorkbook(stream);
                ISheet worksheet = workbook.GetSheetAt(0);

                List<string> expectedHeadersRoster = new List<string>
                {
                    "DomainId", "SupplierName", "SupplierId"
                };

                ValidateHeaders(worksheet, expectedHeadersRoster);
                _logger.LogInfo("Excel file validated successfully.");

                var allData = ExtractAllSupplierNameFromWorksheet(worksheet, domainId);
                _logger.LogInfo($"Extracted {allData.Count} rows from Excel file.");

                const int batchSize = 1000;
                var result = await ProcessSupplierNameInBatchesAsync(allData, batchSize);

                _logger.LogDebug("Excel File processing completed.");
                return result;
            }
        }

        private List<SupplierNameExcelDto> ExtractAllSupplierNameFromWorksheet(ISheet worksheet, Guid domainId)
        {
            var allData = new List<SupplierNameExcelDto>();

            for (int row = 1; row <= worksheet.LastRowNum; row++)
            {
                var currentRow = worksheet.GetRow(row);
                if (currentRow == null) continue;

                var dto = ExtractSupplierNameFromRow(currentRow);
                if (dto.DomainId == domainId)
                {
                    dto.RowNumber = row;
                    allData.Add(dto);
                }
            }

            return allData;
        }

        private SupplierNameExcelDto ExtractSupplierNameFromRow(IRow currentRow)
        {
            SupplierNameExcelDto dto = new SupplierNameExcelDto
            {
                DomainId = currentRow.GetCell(0) != null && Guid.TryParse(currentRow.GetCell(0).ToString(), out Guid domainId) ? domainId : Guid.Empty,
                SupplierName = currentRow.GetCell(1)?.ToString(),
                SupplierId = currentRow.GetCell(2)?.ToString()
            };
            return dto;
        }

        private async Task<ExcelProcessingResultDto> ProcessSupplierNameInBatchesAsync(
            List<SupplierNameExcelDto> allData,
            int batchSize)
        {
            int totalRows = allData.Count;
            int successfulUploads = 0;
            int failedUploads = 0;
            List<ErrorResponseDto> errorResponses = new List<ErrorResponseDto>();

            _logger.LogInfo($"Processing {totalRows} rows in batches of {batchSize}.");

            // Process batches SEQUENTIALLY to avoid DbContext threading issues
            for (int i = 0; i < allData.Count; i += batchSize)
            {
                List<SupplierNameExcelDto> batch = allData.Skip(i).Take(batchSize).ToList();
                int batchNumber = (i / batchSize) + 1;
                int totalBatches = (int)Math.Ceiling((double)allData.Count / batchSize);

                _logger.LogInfo($"Processing batch {batchNumber} of {totalBatches} (rows {i + 1} to {Math.Min(i + batchSize, allData.Count)})");

                // Validate batch
                List<SupplierNameExcelDto> validRows = new List<SupplierNameExcelDto>();
                foreach (SupplierNameExcelDto dto in batch)
                {
                    if (AnyRequiredSupplierColumnEmpty(dto))
                    {
                        AddErrorResponse(errorResponses, 400, $"One or more required columns are empty in row {dto.RowNumber}");
                        _logger.LogError($"One or more required columns are empty in row {dto.RowNumber}");
                        failedUploads++;
                    }
                    else
                    {
                        validRows.Add(dto);
                    }
                }

                if (validRows.Any())
                {
                    try
                    {
                        BatchProcessingResultDto batchResult = await _purchaseOrderService.CreateSupplierNameBatchAsync(validRows);
                        successfulUploads += batchResult.SuccessfulCount;
                        failedUploads += batchResult.FailedCount;
                        errorResponses.AddRange(batchResult.Errors);

                        _logger.LogInfo($"Batch {batchNumber} completed. Successfully processed: {batchResult.SuccessfulCount}, Failed: {batchResult.FailedCount}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing batch {batchNumber}: {ex.Message}");
                        foreach (SupplierNameExcelDto dto in validRows)
                        {
                            AddErrorResponse(errorResponses, 500, $"Batch processing error for row {dto.RowNumber}: {ex.Message}");
                            failedUploads++;
                        }
                    }
                }
                else
                {
                    _logger.LogInfo($"Batch {batchNumber} contained no valid rows to process.");
                }
            }

            _logger.LogInfo($"All batches completed. Total successful: {successfulUploads}, Total failed: {failedUploads}");

            return new ExcelProcessingResultDto
            {
                TotalRows = totalRows,
                SuccessfulUploads = successfulUploads,
                FailedUploads = failedUploads,
                ErrorResponses = errorResponses
            };
        }

        private bool AnyRequiredSupplierColumnEmpty(SupplierNameExcelDto dto)
        {
            return string.IsNullOrEmpty(dto.SupplierName) ||
                   string.IsNullOrEmpty(dto.SupplierId) ||
                   dto.DomainId == Guid.Empty;
        }
        /// <summary>
        /// Uplaod the Contact Excel File
        /// </summary>
        /// <param name="excelFile"></param>
        public async Task<ExcelProcessingResultDto> UploadVenueExcelFileAsync(IFormFile excelFile, Guid domainId)
        {
            _logger.LogInfo("Validating and reading Excel file.");

            using (Stream stream = excelFile.OpenReadStream())
            {
                XSSFWorkbook workbook = new XSSFWorkbook(stream);
                ISheet worksheet = workbook.GetSheetAt(0);

                List<string> expectedHeadersRoster = new List<string>
                {
                    "DomainId","VenueName","CompanyCode","CompanyName","CostCenter","GLAccount","ShipToPlant","PurchaseOrg","PurchaseGroup","BillTo"
                };

                ValidateHeaders(worksheet, expectedHeadersRoster);
                _logger.LogInfo("Excel file validated successfully.");

                var allData = ExtractAllVenueFromWorksheet(worksheet, domainId);
                _logger.LogInfo($"Extracted {allData.Count} rows from Excel file.");

                const int batchSize = 1000;
                var result = await ProcessVenueInBatchesAsync(allData, batchSize);
                _logger.LogDebug("Excel File processing completed.");
                return result;
            }
        }

        private List<VenueExcelDto> ExtractAllVenueFromWorksheet(ISheet worksheet, Guid domainId)
        {
            var allData = new List<VenueExcelDto>();

            for (int row = 1; row <= worksheet.LastRowNum; row++)
            {
                var currentRow = worksheet.GetRow(row);
                if (currentRow == null) continue;

                var dto = ExtractVenueFromRow(currentRow);
                if (dto.DomainId == domainId)
                {
                    dto.RowNumber = row;
                    allData.Add(dto);
                }
            }

            return allData;
        }

        private VenueExcelDto ExtractVenueFromRow(IRow currentRow)
        {
            VenueExcelDto dto = new VenueExcelDto
            {
                DomainId = currentRow.GetCell(0) != null && Guid.TryParse(currentRow.GetCell(0).ToString(), out Guid domainId) ? domainId : Guid.Empty,
                VenueName = currentRow.GetCell(1)?.ToString(),
                CompanyCode = currentRow.GetCell(2)?.ToString(),
                CompanyName = currentRow.GetCell(3)?.ToString(),
                CostCenter = currentRow.GetCell(4)?.ToString(),
                GLAccount = currentRow.GetCell(5)?.ToString(),
                ShipToPlant = currentRow.GetCell(6)?.ToString(),
                PurchaseOrg = currentRow.GetCell(7)?.ToString(),
                PurchaseGroup = currentRow.GetCell(8)?.ToString(),
                BillTo = currentRow.GetCell(9)?.ToString()
            };
            return dto;
        }

        private async Task<ExcelProcessingResultDto> ProcessVenueInBatchesAsync(
            List<VenueExcelDto> allData,
            int batchSize)
        {
            int totalRows = allData.Count;
            int successfulUploads = 0;
            int failedUploads = 0;
            List<ErrorResponseDto> errorResponses = new List<ErrorResponseDto>();

            _logger.LogInfo($"Processing {totalRows} rows in batches of {batchSize}.");

            // Process batches SEQUENTIALLY to avoid DbContext threading issues
            for (int i = 0; i < allData.Count; i += batchSize)
            {
                List<VenueExcelDto> batch = allData.Skip(i).Take(batchSize).ToList();
                int batchNumber = (i / batchSize) + 1;
                int totalBatches = (int)Math.Ceiling((double)allData.Count / batchSize);

                _logger.LogInfo($"Processing batch {batchNumber} of {totalBatches} (rows {i + 1} to {Math.Min(i + batchSize, allData.Count)})");

                // Validate batch
                List<VenueExcelDto> validRows = new List<VenueExcelDto>();
                foreach (VenueExcelDto dto in batch)
                {
                    if (AnyRequiredVenueColumnEmpty(dto))
                    {
                        AddErrorResponse(errorResponses, 400, $"One or more required columns are empty in row {dto.RowNumber}");
                        _logger.LogError($"One or more required columns are empty in row {dto.RowNumber}");
                        failedUploads++;
                    }
                    else
                    {
                        validRows.Add(dto);
                    }
                }

                if (validRows.Any())
                {
                    try
                    {
                        BatchProcessingResultDto batchResult = await _purchaseOrderService.CreateVenueBatchAsync(validRows);
                        successfulUploads += batchResult.SuccessfulCount;
                        failedUploads += batchResult.FailedCount;
                        errorResponses.AddRange(batchResult.Errors);

                        _logger.LogInfo($"Batch {batchNumber} completed. Successfully processed: {batchResult.SuccessfulCount}, Failed: {batchResult.FailedCount}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing batch {batchNumber}: {ex.Message}");
                        foreach (VenueExcelDto dto in validRows)
                        {
                            AddErrorResponse(errorResponses, 500, $"Batch processing error for row {dto.RowNumber}: {ex.Message}");
                            failedUploads++;
                        }
                    }
                }
                else
                {
                    _logger.LogInfo($"Batch {batchNumber} contained no valid rows to process.");
                }
            }

            _logger.LogInfo($"All batches completed. Total successful: {successfulUploads}, Total failed: {failedUploads}");

            return new ExcelProcessingResultDto
            {
                TotalRows = totalRows,
                SuccessfulUploads = successfulUploads,
                FailedUploads = failedUploads,
                ErrorResponses = errorResponses
            };
        }

        private bool AnyRequiredVenueColumnEmpty(VenueExcelDto dto)
        {
            return string.IsNullOrEmpty(dto.VenueName) ||
                dto.DomainId == Guid.Empty;
        }

        /// <summary>
        /// Upload the PO Bot Process Excel File with batch processing optimization
        /// </summary>
        /// <param name="excelFile"></param>
        public async Task<ExcelProcessingResultDto> UploadBotProcessExcelFileAsync(IFormFile excelFile, Guid domainId)
        {
            _logger.LogInfo("Validating and reading Bot Process Excel file.");

            using (Stream stream = excelFile.OpenReadStream())
            {
                XSSFWorkbook workbook = new XSSFWorkbook(stream);
                ISheet worksheet = workbook.GetSheetAt(0);

                List<string> expectedHeadersRoster = new List<string>
                {
                    "POOrderId", "EPID", "LineItem",
                    "UnitOfMeasure", "MaterialCode", "Quantity","Previous Qty",
                    "DomainId", "ReceiptID", "Status", "ProcessedState", "HardReprocess"
                };

                ValidateHeaders(worksheet, expectedHeadersRoster);
                _logger.LogInfo("Excel file validated successfully.");

                var allData = ExtractAllDataFromBotProcessWorksheet(worksheet, domainId);
                _logger.LogInfo($"Extracted {allData.Count} rows from Excel file.");

                const int batchSize = 1000;
                var result = await ProcessBotProcessDataInBatchesAsync(allData, batchSize);

                _logger.LogDebug("Bot Process Excel File processing completed.");
                return result;
            }
        }

        private List<PurchaseOrderBotProcessExcelDto> ExtractAllDataFromBotProcessWorksheet(ISheet worksheet, Guid domainId)
        {
            var allData = new List<PurchaseOrderBotProcessExcelDto>();

            for (int row = 1; row <= worksheet.LastRowNum; row++)
            {
                var currentRow = worksheet.GetRow(row);
                if (currentRow == null) continue;

                var dto = ExtractBotProcessDataFromRow(currentRow);
                if (dto.DomainId == domainId)
                {
                    dto.RowNumber = row;
                    allData.Add(dto);
                }
            }

            return allData;
        }

        private PurchaseOrderBotProcessExcelDto ExtractBotProcessDataFromRow(IRow currentRow)
        {
            PurchaseOrderBotProcessExcelDto dto = new PurchaseOrderBotProcessExcelDto
            {
                POOrderId = currentRow.GetCell(0)?.ToString()!,
                EPID = currentRow.GetCell(1)?.ToString()!,
                LineItem = currentRow.GetCell(2)?.ToString()!,
                UnitOfMeasure = currentRow.GetCell(3)?.ToString()!,
                MaterialCode = currentRow.GetCell(4)?.ToString()!,
                Quantity = currentRow.GetCell(5) != null && double.TryParse(currentRow.GetCell(5).ToString(), out double quantity) ? Math.Round(quantity, 2) : 0.0,
                PreviousAcceptedQuantity = currentRow.GetCell(6) != null && double.TryParse(currentRow.GetCell(6).ToString(), out double acceptedQty) ? Math.Round(acceptedQty, 2) : 0.0,
                DomainId = currentRow.GetCell(7) != null && Guid.TryParse(currentRow.GetCell(7).ToString(), out Guid domainId) ? domainId : Guid.Empty,
                ReceiptID = currentRow.GetCell(8)?.ToString()!,
                Status = currentRow.GetCell(9)?.ToString(),
                ProcessedState = currentRow.GetCell(10) != null && int.TryParse(currentRow.GetCell(10).ToString(), out int processedState) ? processedState : (int?)null,
                HardReprocess = currentRow.GetCell(11) != null && bool.TryParse(currentRow.GetCell(11).ToString(), out bool hardReprocess) ? hardReprocess : false
            };
            return dto;
        }

        private bool AnyRequiredBotProcessColumnEmpty(PurchaseOrderBotProcessExcelDto dto)
        {
            return string.IsNullOrEmpty(dto.POOrderId) ||
                   string.IsNullOrEmpty(dto.EPID) ||
                   string.IsNullOrEmpty(dto.LineItem) ||
                   string.IsNullOrEmpty(dto.ReceiptID) ||
                   dto.DomainId == Guid.Empty;
        }

        private async Task<ExcelProcessingResultDto> ProcessBotProcessDataInBatchesAsync(
            List<PurchaseOrderBotProcessExcelDto> allData,
            int batchSize)
        {
            int totalRows = allData.Count;
            int successfulUploads = 0;
            int failedUploads = 0;
            List<ErrorResponseDto> errorResponses = new List<ErrorResponseDto>();

            _logger.LogInfo($"Processing {totalRows} rows in batches of {batchSize}.");

            // Process batches SEQUENTIALLY to avoid DbContext threading issues
            for (int i = 0; i < allData.Count; i += batchSize)
            {
                List<PurchaseOrderBotProcessExcelDto> batch = allData.Skip(i).Take(batchSize).ToList();
                int batchNumber = (i / batchSize) + 1;
                int totalBatches = (int)Math.Ceiling((double)allData.Count / batchSize);

                _logger.LogInfo($"Processing batch {batchNumber} of {totalBatches} (rows {i + 1} to {Math.Min(i + batchSize, allData.Count)})");

                // Validate batch
                List<PurchaseOrderBotProcessExcelDto> validRows = new List<PurchaseOrderBotProcessExcelDto>();
                foreach (PurchaseOrderBotProcessExcelDto dto in batch)
                {
                    if (AnyRequiredBotProcessColumnEmpty(dto))
                    {
                        AddErrorResponse(errorResponses, 400, $"One or more required columns are empty in row {dto.RowNumber}");
                        _logger.LogError($"One or more required columns are empty in row {dto.RowNumber}");
                        failedUploads++;
                    }
                    else
                    {
                        validRows.Add(dto);
                    }
                }

                if (validRows.Any())
                {
                    try
                    {
                        BatchProcessingResultDto batchResult = await _purchaseOrderBotProcessService.CreatePurchaseOrderBotProcessBatchAsync(validRows);
                        successfulUploads += batchResult.SuccessfulCount;
                        failedUploads += batchResult.FailedCount;
                        errorResponses.AddRange(batchResult.Errors);

                        _logger.LogInfo($"Batch {batchNumber} completed. Successfully processed: {batchResult.SuccessfulCount}, Failed: {batchResult.FailedCount}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing batch {batchNumber}: {ex.Message}");
                        foreach (PurchaseOrderBotProcessExcelDto dto in validRows)
                        {
                            AddErrorResponse(errorResponses, 500, $"Batch processing error for row {dto.RowNumber}: {ex.Message}");
                            failedUploads++;
                        }
                    }
                }
                else
                {
                    _logger.LogInfo($"Batch {batchNumber} contained no valid rows to process.");
                }
            }

            _logger.LogInfo($"All batches completed. Total successful: {successfulUploads}, Total failed: {failedUploads}");

            return new ExcelProcessingResultDto
            {
                TotalRows = totalRows,
                SuccessfulUploads = successfulUploads,
                FailedUploads = failedUploads,
                ErrorResponses = errorResponses
            };
        }

        public async Task<ExcelProcessingResultDto> UploadDraftMaterialPRFileAsync(IFormFile excelFile, Guid domainId, Guid userId)
        {
            _logger.LogInfo("Validating and reading Excel file.");

            using (Stream stream = excelFile.OpenReadStream())
            {
                XSSFWorkbook workbook = new XSSFWorkbook(stream);
                ISheet worksheet = workbook.GetSheetAt(0);

                List<string> expectedHeadersRoster = new List<string>
                {
                    "UniqueName",
                    "VENUE",
                    "SUPPLIER",
                    "NeedByDate",
                    "QUANTITY",
                    "AMOUNT",
                    "TAX",
                    "CURRENCY",
                    "AUTOINVOICE",
                    "InvoiceID",
                    "Invoice Date",
                    "Description",
                    "Title",
                    "AutoGRN",
                    "TeamReceived"

                };

                ValidateHeaders(worksheet, expectedHeadersRoster);
                _logger.LogInfo("Excel file validated successfully.");

                var allData = ExtractAllDraftMaterialPRFromWorksheet(worksheet, domainId, userId);
                _logger.LogInfo($"Extracted {allData.Count} rows from Excel file.");

                const int batchSize = 1000;
                var result = await ProcessDraftMaterialPRInBatchesAsync(allData, batchSize, userId, domainId);
                _logger.LogDebug("Excel File processing completed.");
                return result;
            }
        }

        private List<DraftMaterialPRExcelUploadDto> ExtractAllDraftMaterialPRFromWorksheet(ISheet worksheet, Guid domainId, Guid userId)
        {
            var allData = new List<DraftMaterialPRExcelUploadDto>();

            for (int row = 1; row <= worksheet.LastRowNum; row++)
            {
                var currentRow = worksheet.GetRow(row);
                if (currentRow == null) continue;

                var dto = ExtractDraftMaterialPRFromRow(currentRow, domainId);
                if (dto.DomainId == domainId)
                {
                    dto.RowNumber = row;
                    allData.Add(dto);
                }
            }

            return allData;
        }

        private DraftMaterialPRExcelUploadDto ExtractDraftMaterialPRFromRow(IRow currentRow, Guid domainId)
        {
            DraftMaterialPRExcelUploadDto dto = new DraftMaterialPRExcelUploadDto
            {
                DomainId = domainId,
                PRUniqueName = currentRow.GetCell(0)?.ToString()!,
                Venue = currentRow.GetCell(1)?.ToString(),
                Vendor = currentRow.GetCell(2)?.ToString(),
                NeedBy = currentRow.GetCell(3)?.CellType == NPOI.SS.UserModel.CellType.Numeric && NPOI.SS.UserModel.DateUtil.IsCellDateFormatted(currentRow.GetCell(3)) ? currentRow.GetCell(3).DateCellValue : DateTime.TryParse(currentRow.GetCell(3)?.ToString(), out var dd) ? dd : DateTime.MinValue,
                Quantity = decimal.TryParse(currentRow.GetCell(4)?.ToString(), out var q) ? q : 0,
                Amount = decimal.TryParse(currentRow.GetCell(5)?.ToString(), out var a) ? a : 0,
                TaxInclude = currentRow.GetCell(6) != null && bool.TryParse(currentRow.GetCell(6).ToString(), out bool taxInclude) ? taxInclude : false,
                Currency = currentRow.GetCell(7)?.ToString(),
                AutoInvoices = currentRow.GetCell(8) != null && bool.TryParse(currentRow.GetCell(8).ToString(), out bool autoInvoices) ? autoInvoices : false,
                InvoiceID = currentRow.GetCell(9)?.ToString(),
                InvoiceDate =
    currentRow.GetCell(10)?.CellType == CellType.Numeric &&
    DateUtil.IsCellDateFormatted(currentRow.GetCell(10))
        ? currentRow.GetCell(10).DateCellValue
        : DateTime.TryParse(currentRow.GetCell(10)?.ToString(), out var ddd)
            ? ddd
            : (DateTime?)null,
                Description = currentRow.GetCell(11)?.ToString(),
                Title = currentRow.GetCell(12)?.ToString(),
                AutoGRN = currentRow.GetCell(13) != null && bool.TryParse(currentRow.GetCell(13).ToString(), out bool autoGrn) ? autoGrn : false,
                TeamReceived = currentRow.GetCell(14) != null && bool.TryParse(currentRow.GetCell(14).ToString(), out bool teamReceived) ? teamReceived : false



            };
            return dto;
        }

        private async Task<ExcelProcessingResultDto> ProcessDraftMaterialPRInBatchesAsync(
            List<DraftMaterialPRExcelUploadDto> allData,
            int batchSize, Guid userId, Guid domainId)
        {
            int totalRows = allData.Count;
            int successfulUploads = 0;
            int failedUploads = 0;
            List<ErrorResponseDto> errorResponses = new List<ErrorResponseDto>();

            _logger.LogInfo($"Processing {totalRows} rows in batches of {batchSize}.");

            // Process batches SEQUENTIALLY to avoid DbContext threading issues
            for (int i = 0; i < allData.Count; i += batchSize)
            {
                List<DraftMaterialPRExcelUploadDto> batch = allData.Skip(i).Take(batchSize).ToList();
                int batchNumber = (i / batchSize) + 1;
                int totalBatches = (int)Math.Ceiling((double)allData.Count / batchSize);

                _logger.LogInfo($"Processing batch {batchNumber} of {totalBatches} (rows {i + 1} to {Math.Min(i + batchSize, allData.Count)})");

                // Validate batch
                List<DraftMaterialPRExcelUploadDto> validRows = new List<DraftMaterialPRExcelUploadDto>();
                foreach (DraftMaterialPRExcelUploadDto dto in batch)
                {
                    if (!ValidateDraftMaterialPR(dto, dto.RowNumber, errorResponses))
                    {
                        failedUploads++;
                        continue; // ❗ DO NOT SAVE THIS ROW
                    }

                    validRows.Add(dto);
                }

                if (validRows.Any())
                {
                    try
                    {
                        BatchProcessingResultDto batchResult = await _purchaseOrderService.CreateDraftMaterialPRBatchAsync(validRows, userId, domainId);
                        successfulUploads += batchResult.SuccessfulCount;
                        failedUploads += batchResult.FailedCount;
                        errorResponses.AddRange(batchResult.Errors);

                        _logger.LogInfo($"Batch {batchNumber} completed. Successfully processed: {batchResult.SuccessfulCount}, Failed: {batchResult.FailedCount}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing batch {batchNumber}: {ex.Message}");
                        foreach (DraftMaterialPRExcelUploadDto dto in validRows)
                        {
                            AddErrorResponse(errorResponses, 500, $"Batch processing error for row {dto.RowNumber}: {ex.Message}");
                            failedUploads++;
                        }
                    }
                }
                else
                {
                    _logger.LogInfo($"Batch {batchNumber} contained no valid rows to process.");
                }
            }

            _logger.LogInfo($"All batches completed. Total successful: {successfulUploads}, Total failed: {failedUploads}");

            return new ExcelProcessingResultDto
            {
                TotalRows = totalRows,
                SuccessfulUploads = successfulUploads,
                FailedUploads = failedUploads,
                ErrorResponses = errorResponses
            };
        }
        private bool AnyRequiredDraftMaterialPRColumnEmpty(DraftMaterialPRExcelUploadDto dto)
        {
            return string.IsNullOrEmpty(dto.Venue) ||
                dto.DomainId == Guid.Empty;
        }
        private bool ValidateDraftMaterialPR(
            DraftMaterialPRExcelUploadDto dto,
            int rowNumber,
            List<ErrorResponseDto> errors)
        {
            if (string.IsNullOrWhiteSpace(dto.Venue))
            {
                errors.Add(new ErrorResponseDto
                {
                    StatusCode = 400,
                    Message = $"Row {rowNumber}: Venue is required"
                });
                return false;
            }

            if (string.IsNullOrWhiteSpace(dto.Vendor))
            {
                errors.Add(new ErrorResponseDto
                {
                    StatusCode = 400,
                    Message = $"Row {rowNumber}: Vendor is required"
                });
                return false;
            }

            if (dto.Quantity <= 0 || dto.Amount <= 0)
            {
                errors.Add(new ErrorResponseDto
                {
                    StatusCode = 400,
                    Message = $"Row {rowNumber}: Quantity and Amount must be greater than zero"
                });
                return false;
            }

            // ⭐ AUTO INVOICE RULE (THIS is what you asked)
            if (dto.AutoInvoices)
            {
                if (string.IsNullOrWhiteSpace(dto.InvoiceID))
                {
                    errors.Add(new ErrorResponseDto
                    {
                        StatusCode = 400,
                        Message = $"Row {rowNumber}: InvoiceID is required when AutoInvoice is true"
                    });
                    return false;
                }

                if (!dto.InvoiceDate.HasValue)
                {
                    errors.Add(new ErrorResponseDto
                    {
                        StatusCode = 400,
                        Message = $"Row {rowNumber}: InvoiceDate is required when AutoInvoice is true"
                    });
                    return false;
                }
            }

            return true;
        }
        /// <summary>
        /// Helper method to add error responses        /// </summary>
        /// <param name="errors"></param>
        /// <param name="statusCode"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <summary>
        /// Upload the Block and Unblock Excel File
        /// <summary>
        public async Task<ExcelProcessingResultDto> UploadPRBlockUnblockExcelFileAsync(IFormFile excelFile, Guid domainId)
        {
            _logger.LogInfo("Validating and reading Block And Unblock Excel file.");

            using (Stream stream = excelFile.OpenReadStream())
            {
                XSSFWorkbook workbook = new XSSFWorkbook(stream);
                ISheet worksheet = workbook.GetSheetAt(0);

                List<string> expectedHeadersRoster = new List<string>
                {
                    "POOrderId", "DomainId", "BlockUnblockStatus"
                };

                ValidateHeaders(worksheet, expectedHeadersRoster);
                _logger.LogInfo("Excel file validated successfully.");

                var allData = ExtractAllPRBlockAndUnblockFromWorksheet(worksheet, domainId);
                _logger.LogInfo($"Extracted {allData.Count} rows from Excel file.");

                const int batchSize = 1000;
                var result = await ProcessPRBlockAndUnblockDataInBatchesAsync(allData, batchSize);

                _logger.LogDebug("PR Block and Unblock Excel File processing completed.");
                return result;
            }
        }

        private List<PRBlockOrUnblockDataDto> ExtractAllPRBlockAndUnblockFromWorksheet(ISheet worksheet, Guid domainId)
        {
            var allData = new List<PRBlockOrUnblockDataDto>();

            for (int row = 1; row <= worksheet.LastRowNum; row++)
            {
                var currentRow = worksheet.GetRow(row);
                if (currentRow == null) continue;

                var dto = ExtractPRBlockOrUnblockDataFromRow(currentRow);
                if (dto.DomainId == domainId)
                {
                    dto.RowNumber = row;
                    allData.Add(dto);
                }
            }

            return allData;
        }

        private PRBlockOrUnblockDataDto ExtractPRBlockOrUnblockDataFromRow(IRow currentRow)
        {
            PRBlockOrUnblockDataDto dto = new PRBlockOrUnblockDataDto
            {
                POOrderId = currentRow.GetCell(0)?.ToString()!,
                DomainId = currentRow.GetCell(1) != null && Guid.TryParse(currentRow.GetCell(1).ToString(), out Guid domainId) ? domainId : Guid.Empty,
                IsBlocked = currentRow.GetCell(2) != null && bool.TryParse(currentRow.GetCell(2).ToString(), out bool hardReprocess) ? hardReprocess : false

            };
            return dto;
        }

        private async Task<ExcelProcessingResultDto> ProcessPRBlockAndUnblockDataInBatchesAsync(
            List<PRBlockOrUnblockDataDto> allData,
            int batchSize)
        {
            int totalRows = allData.Count;
            int successfulUploads = 0;
            int failedUploads = 0;
            List<ErrorResponseDto> errorResponses = new List<ErrorResponseDto>();

            _logger.LogInfo($"Processing {totalRows} rows in batches of {batchSize}.");

            // Process batches SEQUENTIALLY to avoid DbContext threading issues
            for (int i = 0; i < allData.Count; i += batchSize)
            {
                List<PRBlockOrUnblockDataDto> batch = allData.Skip(i).Take(batchSize).ToList();
                int batchNumber = (i / batchSize) + 1;
                int totalBatches = (int)Math.Ceiling((double)allData.Count / batchSize);

                _logger.LogInfo($"Processing batch {batchNumber} of {totalBatches} (rows {i + 1} to {Math.Min(i + batchSize, allData.Count)})");

                // Validate batch
                List<PRBlockOrUnblockDataDto> validRows = new List<PRBlockOrUnblockDataDto>();
                foreach (PRBlockOrUnblockDataDto dto in batch)
                {
                    if (AnyRequiredColumnPRBlockOrUnblockEmpty(dto))
                    {
                        AddErrorResponse(errorResponses, 400, $"One or more required columns are empty in row {dto.RowNumber}");
                        _logger.LogError($"One or more required columns are empty in row {dto.RowNumber}");
                        failedUploads++;
                    }
                    else
                    {
                        validRows.Add(dto);
                    }
                }

                if (validRows.Any())
                {
                    try
                    {
                        BatchProcessingResultDto batchResult = await _purchaseOrderService.UpdatePRBlockOrUnblockPurchaseOrderBatchAsync(validRows);
                        successfulUploads += batchResult.SuccessfulCount;
                        failedUploads += batchResult.FailedCount;
                        errorResponses.AddRange(batchResult.Errors);

                        _logger.LogInfo($"Batch {batchNumber} completed. Successfully processed: {batchResult.SuccessfulCount}, Failed: {batchResult.FailedCount}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing batch {batchNumber}: {ex.Message}");
                        foreach (PRBlockOrUnblockDataDto dto in validRows)
                        {
                            AddErrorResponse(errorResponses, 500, $"Batch processing error for row {dto.RowNumber}: {ex.Message}");
                            failedUploads++;
                        }
                    }
                }
                else
                {
                    _logger.LogInfo($"Batch {batchNumber} contained no valid rows to process.");
                }
            }

            _logger.LogInfo($"All batches completed. Total successful: {successfulUploads}, Total failed: {failedUploads}");

            return new ExcelProcessingResultDto
            {
                TotalRows = totalRows,
                SuccessfulUploads = successfulUploads,
                FailedUploads = failedUploads,
                ErrorResponses = errorResponses
            };
        }

        private bool AnyRequiredColumnPRBlockOrUnblockEmpty(PRBlockOrUnblockDataDto dto)
        {
            return string.IsNullOrEmpty(dto.POOrderId) ||
                   dto.DomainId == Guid.Empty;
        }
        /// <summary> Upload the Cross Charge Excel File
        /// </summary> <param name="excelFile"></param>

        public async Task<ExcelProcessingResultDto> UploadCrossChargeExcelFileAsync(IFormFile excelFile, Guid domainId)
        {
            _logger.LogInfo("Validating and reading Excel file.");

            using (Stream stream = excelFile.OpenReadStream())
            {
                XSSFWorkbook workbook = new XSSFWorkbook(stream);
                ISheet worksheet = workbook.GetSheetAt(0);

                List<string> expectedHeadersRoster = new List<string>
                {
                    "DomainId","PropertyCrossChargeName","CompanyCode","CompanyName","CostCenter","GLAccount","ShipToPlant","PurchaseOrg","PurchaseGroup","BillTo"
                };

                ValidateHeaders(worksheet, expectedHeadersRoster);
                _logger.LogInfo("Excel file validated successfully.");

                var allData = ExtractAllPropertyCrossChargeFromWorksheet(worksheet, domainId);
                _logger.LogInfo($"Extracted {allData.Count} rows from Excel file.");

                const int batchSize = 1000;
                var result = await ProcessPropertyCrossChargeInBatchesAsync(allData, batchSize);
                _logger.LogDebug("Excel File processing completed.");
                return result;
            }
        }

        private List<PropertyCrossChargeExcelDto> ExtractAllPropertyCrossChargeFromWorksheet(ISheet worksheet, Guid domainId)
        {
            var allData = new List<PropertyCrossChargeExcelDto>();

            for (int row = 1; row <= worksheet.LastRowNum; row++)
            {
                var currentRow = worksheet.GetRow(row);
                if (currentRow == null) continue;

                var dto = ExtractPropertyCrossChargeFromRow(currentRow);
                if (dto.DomainId == domainId)
                {
                    dto.RowNumber = row;
                    allData.Add(dto);
                }
            }

            return allData;
        }

        private PropertyCrossChargeExcelDto ExtractPropertyCrossChargeFromRow(IRow currentRow)
        {
            PropertyCrossChargeExcelDto dto = new PropertyCrossChargeExcelDto
            {
                DomainId = currentRow.GetCell(0) != null && Guid.TryParse(currentRow.GetCell(0).ToString(), out Guid domainId) ? domainId : Guid.Empty,
                PropertyCrossChargeName = currentRow.GetCell(1)?.ToString(),
                CompanyCode = currentRow.GetCell(2)?.ToString(),
                CompanyName = currentRow.GetCell(3)?.ToString(),
                CostCenter = currentRow.GetCell(4)?.ToString(),
                GLAccount = currentRow.GetCell(5)?.ToString(),
                ShipToPlant = currentRow.GetCell(6)?.ToString(),
                PurchaseOrg = currentRow.GetCell(7)?.ToString(),
                PurchaseGroup = currentRow.GetCell(8)?.ToString(),
                BillTo = currentRow.GetCell(9)?.ToString()
            };
            return dto;
        }

        private async Task<ExcelProcessingResultDto> ProcessPropertyCrossChargeInBatchesAsync(
            List<PropertyCrossChargeExcelDto> allData,
            int batchSize)
        {
            int totalRows = allData.Count;
            int successfulUploads = 0;
            int failedUploads = 0;
            List<ErrorResponseDto> errorResponses = new List<ErrorResponseDto>();

            _logger.LogInfo($"Processing {totalRows} rows in batches of {batchSize}.");

            // Process batches SEQUENTIALLY to avoid DbContext threading issues
            for (int i = 0; i < allData.Count; i += batchSize)
            {
                List<PropertyCrossChargeExcelDto> batch = allData.Skip(i).Take(batchSize).ToList();
                int batchNumber = (i / batchSize) + 1;
                int totalBatches = (int)Math.Ceiling((double)allData.Count / batchSize);

                _logger.LogInfo($"Processing batch {batchNumber} of {totalBatches} (rows {i + 1} to {Math.Min(i + batchSize, allData.Count)})");

                // Validate batch
                List<PropertyCrossChargeExcelDto> validRows = new List<PropertyCrossChargeExcelDto>();
                foreach (PropertyCrossChargeExcelDto dto in batch)
                {
                    if (AnyRequiredPropertyCrossChargeColumnEmpty(dto))
                    {
                        AddErrorResponse(errorResponses, 400, $"One or more required columns are empty in row {dto.RowNumber}");
                        _logger.LogError($"One or more required columns are empty in row {dto.RowNumber}");
                        failedUploads++;
                    }
                    else
                    {
                        validRows.Add(dto);
                    }
                }

                if (validRows.Any())
                {
                    try
                    {
                        BatchProcessingResultDto batchResult = await _purchaseOrderService.CreatePropertyCrossChargeBatchAsync(validRows);
                        successfulUploads += batchResult.SuccessfulCount;
                        failedUploads += batchResult.FailedCount;
                        errorResponses.AddRange(batchResult.Errors);

                        _logger.LogInfo($"Batch {batchNumber} completed. Successfully processed: {batchResult.SuccessfulCount}, Failed: {batchResult.FailedCount}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing batch {batchNumber}: {ex.Message}");
                        foreach (PropertyCrossChargeExcelDto dto in validRows)
                        {
                            AddErrorResponse(errorResponses, 500, $"Batch processing error for row {dto.RowNumber}: {ex.Message}");
                            failedUploads++;
                        }
                    }
                }
                else
                {
                    _logger.LogInfo($"Batch {batchNumber} contained no valid rows to process.");
                }
            }

            _logger.LogInfo($"All batches completed. Total successful: {successfulUploads}, Total failed: {failedUploads}");

            return new ExcelProcessingResultDto
            {
                TotalRows = totalRows,
                SuccessfulUploads = successfulUploads,
                FailedUploads = failedUploads,
                ErrorResponses = errorResponses
            };
        }

        private bool AnyRequiredPropertyCrossChargeColumnEmpty(PropertyCrossChargeExcelDto dto)
        {
            return string.IsNullOrEmpty(dto.PropertyCrossChargeName) ||
                dto.DomainId == Guid.Empty;
        }

        public async Task<byte[]> ProcessAndGenerateExcel(IFormFile file)
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);

            var grnRows = ReadGrnSheet(workbook);
            var s4Rows = ReadS4Sheet(workbook);
            var invoiceRows = ReadInvoiceSheet(workbook);

            var results = ComputeResults(grnRows, s4Rows, invoiceRows);

            return GenerateResultExcel(results);
        }

        // ─── Readers ────────────────────────────────────────────────

        private List<GrnRow> ReadGrnSheet(XLWorkbook wb)
        {
            var sheet = wb.Worksheet("GRN") ?? wb.Worksheet(1);
            var rows = new List<GrnRow>();

            foreach (var row in sheet.RowsUsed().Skip(1)) // skip header
            {
                rows.Add(new GrnRow
                {
                    PO_Number = row.Cell(1).GetString().Trim(),
                    GRN_Description = row.Cell(2).GetString().Trim(),
                    Line_Item = row.Cell(3).GetString().Trim(),
                    PO_Quantity = GetDecimal(row.Cell(4)),
                    PO_Line_Item = row.Cell(5).GetString().Trim(),
                    GRN_Quantity = GetDecimal(row.Cell(6)),
                    GRN_Amount = GetDecimal(row.Cell(7))
                });
            }
            return rows;
        }

        private List<S4Row> ReadS4Sheet(XLWorkbook wb)
        {
            var sheet = wb.Worksheet("S4") ?? wb.Worksheet(2);
            var rows = new List<S4Row>();

            foreach (var row in sheet.RowsUsed().Skip(1))
            {
                rows.Add(new S4Row
                {
                    PO_Number = row.Cell(1).GetString().Trim(),
                    Line_Item = row.Cell(2).GetString().Trim(),
                    S4_Quantity = GetDecimal(row.Cell(4)),
                    S4_Amount = GetDecimal(row.Cell(5))
                });
            }
            return rows;
        }

        private List<InvoiceRow> ReadInvoiceSheet(XLWorkbook wb)
        {
            var sheet = wb.Worksheet("Invoice") ?? wb.Worksheet(3);
            var rows = new List<InvoiceRow>();

            foreach (var row in sheet.RowsUsed().Skip(1))
            {
                rows.Add(new InvoiceRow
                {
                    PO_Number = row.Cell(1).GetString().Trim(),
                    Line_Item = row.Cell(2).GetString().Trim(),
                    Invoice_Quantity = GetDecimal(row.Cell(4)),
                    Invoice_Amount = GetDecimal(row.Cell(5))
                });
            }
            return rows;
        }

        // ─── Computation ─────────────────────────────────────────────

        private List<ResultRow> ComputeResults(
            List<GrnRow> grnRows,
            List<S4Row> s4Rows,
            List<InvoiceRow> invoiceRows)
        {
            // Get all unique PO + Line combinations from S4
            var s4Groups = s4Rows
                .GroupBy(x => new { x.PO_Number, x.Line_Item })
                .ToList();

            var results = new List<ResultRow>();

            foreach (var s4Group in s4Groups)
            {
                var key = s4Group.Key;

                // S4 totals
                decimal s4QtyTotal = s4Group.Sum(x => x.S4_Quantity);
                decimal s4AmountTotal = s4Group.Sum(x => x.S4_Amount);

                // GRN totals for same PO + Line
                var matchingGrn = grnRows
                    .Where(x => x.PO_Number == key.PO_Number && x.Line_Item == key.Line_Item)
                    .ToList();

                decimal grnQtyTotal = matchingGrn.Sum(x => x.GRN_Quantity);
                decimal grnAmountTotal = matchingGrn.Sum(x => x.GRN_Amount);
                decimal poQuantity = matchingGrn.FirstOrDefault()?.PO_Quantity ?? 0;

                // Duplicate S4 = S4 Total - GRN Total
                decimal s4Duplicate = s4QtyTotal - grnQtyTotal;

                // Invoice totals for same PO + Line
                var matchingInvoice = invoiceRows
                    .Where(x => x.PO_Number == key.PO_Number && x.Line_Item == key.Line_Item)
                    .ToList();

                decimal invoiceQtyTotal = matchingInvoice.Sum(x => x.Invoice_Quantity);
                decimal invoiceAmountTotal = matchingInvoice.Sum(x => x.Invoice_Amount);

                // Invoice duplicate = Invoice Total - GRN Total
                decimal invoiceDuplicate = invoiceQtyTotal - grnQtyTotal;

                results.Add(new ResultRow
                {
                    PO_Number = key.PO_Number,
                    Line_Item = key.Line_Item,
                    S4_Quantity_Total = s4QtyTotal,
                    PO_Quantity_GRN_Total = grnQtyTotal,
                    S4_Duplicate_Quantity = s4Duplicate,
                    Invoice_PO_Number = key.PO_Number,
                    Invoice_Line_Item = key.Line_Item,
                    Invoice_Total_Quantity = invoiceQtyTotal,
                    Invoice_Duplicate_Quantity = invoiceDuplicate,
                    GRN_Amount_Total = grnAmountTotal,
                    PO_Quantity = poQuantity,
                    S4_Amount_Total = s4AmountTotal,
                    Invoice_Amount_Total = invoiceAmountTotal,
                    GRN_Description = matchingGrn.FirstOrDefault()?.GRN_Description ?? ""
                });
            }

            return results;
        }

        // ─── Excel Generation ─────────────────────────────────────────

        private byte[] GenerateResultExcel(List<ResultRow> results)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("S4 Duplicates");

            // ── Headers
            var headers = new[]
            {
         "PO_NUMBER", "LINE_ITEM", "S4_Quantity_TOTAL", "PO_Quantity_GRN_TOTAL",
         "S4_Duplicate_Quantity", "Invoice_PO_Number", "Line_Item",
         "Invoice_Total_Quantity", "Invoice_Duplicate_Quantity",
         "Sum(GRN_Amount)", "PO_Quantity", "Sum(S4_Amount)", "Sum(Invoice_Amount)", "GRN_Description"
     };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontName = "Arial";
                cell.Style.Fill.BackgroundColor = XLColor.FromArgb(68, 114, 196); // blue header
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            // ── Data rows
            for (int i = 0; i < results.Count; i++)
            {
                var r = results[i];
                int row = i + 2;

                ws.Cell(row, 1).Value = r.PO_Number;
                ws.Cell(row, 2).Value = r.Line_Item;
                ws.Cell(row, 3).Value = r.S4_Quantity_Total;
                ws.Cell(row, 4).Value = r.PO_Quantity_GRN_Total;
                ws.Cell(row, 5).Value = r.S4_Duplicate_Quantity;
                ws.Cell(row, 6).Value = r.Invoice_PO_Number;
                ws.Cell(row, 7).Value = r.Invoice_Line_Item;
                ws.Cell(row, 8).Value = r.Invoice_Total_Quantity;
                ws.Cell(row, 9).Value = r.Invoice_Duplicate_Quantity;
                ws.Cell(row, 10).Value = r.GRN_Amount_Total;
                ws.Cell(row, 11).Value = r.PO_Quantity;
                ws.Cell(row, 12).Value = r.S4_Amount_Total;
                ws.Cell(row, 13).Value = r.Invoice_Amount_Total;
                ws.Cell(row, 14).Value = r.GRN_Description;

                // Highlight rows where duplicates exist
                bool hasDuplicate = r.S4_Duplicate_Quantity > 0 || r.Invoice_Duplicate_Quantity > 0;
                if (hasDuplicate)
                {
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 235, 156); // yellow highlight
                }

                // Style all data cells
                for (int col = 1; col <= 13; col++)
                {
                    var cell = ws.Cell(row, col);
                    cell.Style.Font.FontName = "Arial";
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
            }

            // ── Auto-fit columns
            ws.ColumnsUsed().AdjustToContents();

            // ── Freeze header row
            ws.SheetView.FreezeRows(1);

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        // ─── Helper ───────────────────────────────────────────────────

        private decimal GetDecimal(IXLCell cell)
        {
            if (cell == null || cell.IsEmpty())
                return 0;

            // If Excel internally stored it as number, this will work directly
            if (cell.DataType == XLDataType.Number)
                return cell.GetValue<decimal>();

            var value = cell.GetString();

            if (string.IsNullOrWhiteSpace(value))
                return 0;

            // Remove everything except digits, dot, comma and minus
            value = System.Text.RegularExpressions.Regex
                        .Replace(value, @"[^\d\.,\-]", "");

            // Remove thousand separators if any
            value = value.Replace(",", "");

            decimal.TryParse(value, out decimal result);
            return result;
        }

        public async Task<ExcelProcessingResultDto> UploadPublicVenueExcelFileAsync(IFormFile excelFile, Guid domainId)
        {
            _logger.LogInfo("Validating and reading Excel file.");

            using (Stream stream = excelFile.OpenReadStream())
            {
                XSSFWorkbook workbook = new XSSFWorkbook(stream);
                ISheet worksheet = workbook.GetSheetAt(0);

                List<string> expectedHeadersRoster = new List<string>
                {
                    "DomainId","VenueName","CompanyCode","CompanyName","CostCenter","GLAccount","ShipToPlant","PurchaseOrg","PurchaseGroup","BillTo"
                };

                ValidateHeaders(worksheet, expectedHeadersRoster);
                _logger.LogInfo("Excel file validated successfully.");

                var allData = ExtractAllPublicVenueFromWorksheet(worksheet, domainId);
                _logger.LogInfo($"Extracted {allData.Count} rows from Excel file.");

                const int batchSize = 1000;
                var result = await ProcessPublicVenueInBatchesAsync(allData, batchSize);
                _logger.LogDebug("Excel File processing completed.");
                return result;
            }
        }

        private List<VenueExcelDto> ExtractAllPublicVenueFromWorksheet(ISheet worksheet, Guid domainId)
        {
            var allData = new List<VenueExcelDto>();

            for (int row = 1; row <= worksheet.LastRowNum; row++)
            {
                var currentRow = worksheet.GetRow(row);
                if (currentRow == null) continue;

                var dto = ExtractPublicVenueFromRow(currentRow);
                if (dto.DomainId == domainId)
                {
                    dto.RowNumber = row;
                    allData.Add(dto);
                }
            }

            return allData;
        }

        private VenueExcelDto ExtractPublicVenueFromRow(IRow currentRow)
        {
            VenueExcelDto dto = new VenueExcelDto
            {
                DomainId = currentRow.GetCell(0) != null && Guid.TryParse(currentRow.GetCell(0).ToString(), out Guid domainId) ? domainId : Guid.Empty,
                VenueName = currentRow.GetCell(1)?.ToString(),
                CompanyCode = currentRow.GetCell(2)?.ToString(),
                CompanyName = currentRow.GetCell(3)?.ToString(),
                CostCenter = currentRow.GetCell(4)?.ToString(),
                GLAccount = currentRow.GetCell(5)?.ToString(),
                ShipToPlant = currentRow.GetCell(6)?.ToString(),
                PurchaseOrg = currentRow.GetCell(7)?.ToString(),
                PurchaseGroup = currentRow.GetCell(8)?.ToString(),
                BillTo = currentRow.GetCell(9)?.ToString()
            };
            return dto;
        }

        private async Task<ExcelProcessingResultDto> ProcessPublicVenueInBatchesAsync(
            List<VenueExcelDto> allData,
            int batchSize)
        {
            int totalRows = allData.Count;
            int successfulUploads = 0;
            int failedUploads = 0;
            List<ErrorResponseDto> errorResponses = new List<ErrorResponseDto>();

            _logger.LogInfo($"Processing {totalRows} rows in batches of {batchSize}.");

            // Process batches SEQUENTIALLY to avoid DbContext threading issues
            for (int i = 0; i < allData.Count; i += batchSize)
            {
                List<VenueExcelDto> batch = allData.Skip(i).Take(batchSize).ToList();
                int batchNumber = (i / batchSize) + 1;
                int totalBatches = (int)Math.Ceiling((double)allData.Count / batchSize);

                _logger.LogInfo($"Processing batch {batchNumber} of {totalBatches} (rows {i + 1} to {Math.Min(i + batchSize, allData.Count)})");

                // Validate batch
                List<VenueExcelDto> validRows = new List<VenueExcelDto>();
                foreach (VenueExcelDto dto in batch)
                {
                    if (AnyRequiredPublicVenueColumnEmpty(dto))
                    {
                        AddErrorResponse(errorResponses, 400, $"One or more required columns are empty in row {dto.RowNumber}");
                        _logger.LogError($"One or more required columns are empty in row {dto.RowNumber}");
                        failedUploads++;
                    }
                    else
                    {
                        validRows.Add(dto);
                    }
                }

                if (validRows.Any())
                {
                    try
                    {
                        BatchProcessingResultDto batchResult = await _publicRelationService.CreatePublicVenueBatchAsync(validRows);
                        successfulUploads += batchResult.SuccessfulCount;
                        failedUploads += batchResult.FailedCount;
                        errorResponses.AddRange(batchResult.Errors);

                        _logger.LogInfo($"Batch {batchNumber} completed. Successfully processed: {batchResult.SuccessfulCount}, Failed: {batchResult.FailedCount}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing batch {batchNumber}: {ex.Message}");
                        foreach (VenueExcelDto dto in validRows)
                        {
                            AddErrorResponse(errorResponses, 500, $"Batch processing error for row {dto.RowNumber}: {ex.Message}");
                            failedUploads++;
                        }
                    }
                }
                else
                {
                    _logger.LogInfo($"Batch {batchNumber} contained no valid rows to process.");
                }
            }

            _logger.LogInfo($"All batches completed. Total successful: {successfulUploads}, Total failed: {failedUploads}");

            return new ExcelProcessingResultDto
            {
                TotalRows = totalRows,
                SuccessfulUploads = successfulUploads,
                FailedUploads = failedUploads,
                ErrorResponses = errorResponses
            };
        }

        private bool AnyRequiredPublicVenueColumnEmpty(VenueExcelDto dto)
        {
            return string.IsNullOrEmpty(dto.VenueName) ||
                dto.DomainId == Guid.Empty;
        }

        private bool AnyRequiredConciergeVenueColumnEmpty(VenueExcelDto dto)
        {
            return string.IsNullOrEmpty(dto.VenueName) ||
                dto.DomainId == Guid.Empty;
        }


        public async Task<ExcelProcessingResultDto> UploadPublicCategoryExcelFileAsync(IFormFile excelFile, Guid domainId)
        {
            _logger.LogInfo("Validating and reading Excel file.");

            using (Stream stream = excelFile.OpenReadStream())
            {
                XSSFWorkbook workbook = new XSSFWorkbook(stream);
                ISheet worksheet = workbook.GetSheetAt(0);

                List<string> expectedHeadersRoster = new List<string>
                {
                    "DomainId","CategoryName"
                };

                ValidateHeaders(worksheet, expectedHeadersRoster);
                _logger.LogInfo("Excel file validated successfully.");

                var allData = ExtractAllPublicCategoryFromWorksheet(worksheet, domainId);
                _logger.LogInfo($"Extracted {allData.Count} rows from Excel file.");

                const int batchSize = 1000;
                var result = await ProcessPublicCategoryInBatchesAsync(allData, batchSize);
                _logger.LogDebug("Excel File processing completed.");
                return result;
            }
        }


        private List<CategoryExcelDto> ExtractAllPublicCategoryFromWorksheet(ISheet worksheet, Guid domainId)
        {
            var allData = new List<CategoryExcelDto>();

            for (int row = 1; row <= worksheet.LastRowNum; row++)
            {
                var currentRow = worksheet.GetRow(row);
                if (currentRow == null) continue;

                var dto = ExtractPublicCategoryFromRow(currentRow);
                if (dto.DomainId == domainId)
                {
                    dto.RowNumber = row;
                    allData.Add(dto);
                }
            }

            return allData;
        }

        private CategoryExcelDto ExtractPublicCategoryFromRow(IRow currentRow)
        {
            CategoryExcelDto dto = new CategoryExcelDto
            {
                DomainId = currentRow.GetCell(0) != null && Guid.TryParse(currentRow.GetCell(0).ToString(), out Guid domainId) ? domainId : Guid.Empty,
                CategoryName = currentRow.GetCell(1)?.ToString()

            };
            return dto;
        }

        private async Task<ExcelProcessingResultDto> ProcessPublicCategoryInBatchesAsync(
            List<CategoryExcelDto> allData,
            int batchSize)
        {
            int totalRows = allData.Count;
            int successfulUploads = 0;
            int failedUploads = 0;
            List<ErrorResponseDto> errorResponses = new List<ErrorResponseDto>();

            _logger.LogInfo($"Processing {totalRows} rows in batches of {batchSize}.");

            // Process batches SEQUENTIALLY to avoid DbContext threading issues
            for (int i = 0; i < allData.Count; i += batchSize)
            {
                List<CategoryExcelDto> batch = allData.Skip(i).Take(batchSize).ToList();
                int batchNumber = (i / batchSize) + 1;
                int totalBatches = (int)Math.Ceiling((double)allData.Count / batchSize);

                _logger.LogInfo($"Processing batch {batchNumber} of {totalBatches} (rows {i + 1} to {Math.Min(i + batchSize, allData.Count)})");

                // Validate batch
                List<CategoryExcelDto> validRows = new List<CategoryExcelDto>();
                foreach (CategoryExcelDto dto in batch)
                {
                    if (AnyRequiredPublicCategoryColumnEmpty(dto))
                    {
                        AddErrorResponse(errorResponses, 400, $"One or more required columns are empty in row {dto.RowNumber}");
                        _logger.LogError($"One or more required columns are empty in row {dto.RowNumber}");
                        failedUploads++;
                    }
                    else
                    {
                        validRows.Add(dto);
                    }
                }

                if (validRows.Any())
                {
                    try
                    {
                        BatchProcessingResultDto batchResult = await _publicRelationService.CreatePublicCategoryBatchAsync(validRows);
                        successfulUploads += batchResult.SuccessfulCount;
                        failedUploads += batchResult.FailedCount;
                        errorResponses.AddRange(batchResult.Errors);

                        _logger.LogInfo($"Batch {batchNumber} completed. Successfully processed: {batchResult.SuccessfulCount}, Failed: {batchResult.FailedCount}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing batch {batchNumber}: {ex.Message}");
                        foreach (CategoryExcelDto dto in validRows)
                        {
                            AddErrorResponse(errorResponses, 500, $"Batch processing error for row {dto.RowNumber}: {ex.Message}");
                            failedUploads++;
                        }
                    }
                }
                else
                {
                    _logger.LogInfo($"Batch {batchNumber} contained no valid rows to process.");
                }
            }

            _logger.LogInfo($"All batches completed. Total successful: {successfulUploads}, Total failed: {failedUploads}");

            return new ExcelProcessingResultDto
            {
                TotalRows = totalRows,
                SuccessfulUploads = successfulUploads,
                FailedUploads = failedUploads,
                ErrorResponses = errorResponses
            };
        }
        public async Task<ExcelProcessingResultDto> UploadDraftServicePRFileAsync(IFormFile excelFile, Guid domainId, Guid userId)
        {
            _logger.LogInfo("Validating and reading Excel file.");

            using (Stream stream = excelFile.OpenReadStream())
            {
                XSSFWorkbook workbook = new XSSFWorkbook(stream);
                ISheet worksheet = workbook.GetSheetAt(0);

                List<string> expectedHeaders = new List<string>
    {
        "UniqueName",
        "VENUE",
        "SUPPLIER",

        "QUANTITY",
        "AMOUNT",
        "TAX",
        "CURRENCY",
        "AUTOINVOICE",
        "InvoiceID",
        "Invoice Date",
        "ServiceStartDate",
        "ServiceEndDate",
        "Description",
        "Title",
        "AutoSES",
        "TeamReceived"
    };

                ValidateHeaders(worksheet, expectedHeaders);
                _logger.LogInfo("Excel file validated successfully.");

                var allData = ExtractAllDraftServicePRFromWorksheet(worksheet, domainId, userId);
                _logger.LogInfo($"Extracted {allData.Count} rows from Excel file.");

                const int batchSize = 1000;
                var result = await ProcessDraftServicePRInBatchesAsync(allData, batchSize, userId, domainId);
                _logger.LogDebug("Excel File processing completed.");
                return result;
            }
        }

        private List<DraftServicePRExcelUploadDto> ExtractAllDraftServicePRFromWorksheet(ISheet worksheet, Guid domainId, Guid userId)
        {
            var allData = new List<DraftServicePRExcelUploadDto>();

            for (int row = 1; row <= worksheet.LastRowNum; row++)
            {
                var currentRow = worksheet.GetRow(row);
                if (currentRow == null) continue;

                var dto = ExtractDraftServicePRFromRow(currentRow, domainId);
                if (dto.DomainId == domainId)
                {
                    dto.RowNumber = row;
                    allData.Add(dto);
                }
            }

            return allData;
        }

        private DraftServicePRExcelUploadDto ExtractDraftServicePRFromRow(IRow currentRow, Guid domainId)
        {
            DraftServicePRExcelUploadDto dto = new DraftServicePRExcelUploadDto
            {
                DomainId = domainId,
                PRServiceUniqueName = currentRow.GetCell(0)?.ToString()!,
                Venue = currentRow.GetCell(1)?.ToString(),
                Vendor = currentRow.GetCell(2)?.ToString(),

                Quantity = decimal.TryParse(currentRow.GetCell(3)?.ToString(), out var q) ? q : 0,
                Amount = decimal.TryParse(currentRow.GetCell(4)?.ToString(), out var a) ? a : 0,
                TaxInclude = currentRow.GetCell(5) != null && bool.TryParse(currentRow.GetCell(5).ToString(), out bool taxInclude) ? taxInclude : false,
                Currency = currentRow.GetCell(6)?.ToString(),
                AutoInvoices = currentRow.GetCell(7) != null && bool.TryParse(currentRow.GetCell(7).ToString(), out bool autoInvoices) ? autoInvoices : false,
                InvoiceID = currentRow.GetCell(8)?.ToString(),
                InvoiceDate =
    currentRow.GetCell(9)?.CellType == CellType.Numeric &&
    DateUtil.IsCellDateFormatted(currentRow.GetCell(9))
        ? currentRow.GetCell(9).DateCellValue
        : DateTime.TryParse(currentRow.GetCell(9)?.ToString(), out var ddd)
            ? ddd
            : (DateTime?)null,
                ServiceStartDate = currentRow.GetCell(10)?.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(currentRow.GetCell(10)) ? currentRow.GetCell(10).DateCellValue : DateTime.TryParse(currentRow.GetCell(10)?.ToString(), out var ssd) ? ssd : (DateTime?)null,
                ServiceEndDate = currentRow.GetCell(11)?.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(currentRow.GetCell(11)) ? currentRow.GetCell(11).DateCellValue : DateTime.TryParse(currentRow.GetCell(11)?.ToString(), out var sed) ? sed : (DateTime?)null,
                Description = currentRow.GetCell(12)?.ToString(),
                Title = currentRow.GetCell(13)?.ToString(), 
                AutoSES = currentRow.GetCell(14) != null && bool.TryParse(currentRow.GetCell(14).ToString(), out bool autoSES) ? autoSES : false,
                TeamReceived = currentRow.GetCell(15)!= null && bool.TryParse(currentRow.GetCell(15).ToString(), out bool teamReceived) ? teamReceived : false



            };
            return dto;
        }

        private async Task<ExcelProcessingResultDto> ProcessDraftServicePRInBatchesAsync(
            List<DraftServicePRExcelUploadDto> allData,
            int batchSize, Guid userId, Guid domainId)
        {
            int totalRows = allData.Count;
            int successfulUploads = 0;
            int failedUploads = 0;
            List<ErrorResponseDto> errorResponses = new List<ErrorResponseDto>();

            _logger.LogInfo($"Processing {totalRows} rows in batches of {batchSize}.");

            // Process batches SEQUENTIALLY to avoid DbContext threading issues
            for (int i = 0; i < allData.Count; i += batchSize)
            {
                List<DraftServicePRExcelUploadDto> batch = allData.Skip(i).Take(batchSize).ToList();
                int batchNumber = (i / batchSize) + 1;
                int totalBatches = (int)Math.Ceiling((double)allData.Count / batchSize);

                _logger.LogInfo($"Processing batch {batchNumber} of {totalBatches} (rows {i + 1} to {Math.Min(i + batchSize, allData.Count)})");

                // Validate batch
                List<DraftServicePRExcelUploadDto> validRows = new List<DraftServicePRExcelUploadDto>();
                foreach (DraftServicePRExcelUploadDto dto in batch)
                {
                    if (!ValidateDraftServicePR(dto, dto.RowNumber, errorResponses))
                    {
                        failedUploads++;
                        continue; // ❗ DO NOT SAVE THIS ROW
                    }

                    validRows.Add(dto);
                }

                if (validRows.Any())
                {
                    try
                    {
                        BatchProcessingResultDto batchResult = await _aribaServicePRService.CreateDraftServicePRBatchAsync(validRows, userId, domainId);
                        successfulUploads += batchResult.SuccessfulCount;
                        failedUploads += batchResult.FailedCount;
                        errorResponses.AddRange(batchResult.Errors);

                        _logger.LogInfo($"Batch {batchNumber} completed. Successfully processed: {batchResult.SuccessfulCount}, Failed: {batchResult.FailedCount}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing batch {batchNumber}: {ex.Message}");
                        foreach (DraftServicePRExcelUploadDto dto in validRows)
                        {
                            AddErrorResponse(errorResponses, 500, $"Batch processing error for row {dto.RowNumber}: {ex.Message}");
                            failedUploads++;
                        }
                    }
                }
                else
                {
                    _logger.LogInfo($"Batch {batchNumber} contained no valid rows to process.");
                }
            }

            _logger.LogInfo($"All batches completed. Total successful: {successfulUploads}, Total failed: {failedUploads}");

            return new ExcelProcessingResultDto
            {
                TotalRows = totalRows,
                SuccessfulUploads = successfulUploads,
                FailedUploads = failedUploads,
                ErrorResponses = errorResponses
            };
        }
        private bool AnyRequiredServicePRColumnEmpty(DraftServicePRExcelUploadDto dto)
        {
            return string.IsNullOrEmpty(dto.PRServiceUniqueName) ||
                dto.DomainId == Guid.Empty;
        }
        private bool ValidateDraftServicePR(
            DraftServicePRExcelUploadDto dto,
            int rowNumber,
            List<ErrorResponseDto> errors)
        {
            if (string.IsNullOrWhiteSpace(dto.PRServiceUniqueName))
            {
                errors.Add(new ErrorResponseDto
                {
                    StatusCode = 400,
                    Message = $"Row {rowNumber}: PRServiceUniqueName is required"
                });
                return false;
            }

            if (string.IsNullOrWhiteSpace(dto.Vendor))
            {
                errors.Add(new ErrorResponseDto
                {
                    StatusCode = 400,
                    Message = $"Row {rowNumber}: Vendor is required"
                });
                return false;
            }

            if (dto.Quantity <= 0 || dto.Amount <= 0)
            {
                errors.Add(new ErrorResponseDto
                {
                    StatusCode = 400,
                    Message = $"Row {rowNumber}: Quantity and Amount must be greater than zero"
                });
                return false;
            }

            // ⭐ AUTO INVOICE RULE (THIS is what you asked)
            if (dto.AutoInvoices)
            {
                if (string.IsNullOrWhiteSpace(dto.InvoiceID))
                {
                    errors.Add(new ErrorResponseDto
                    {
                        StatusCode = 400,
                        Message = $"Row {rowNumber}: InvoiceID is required when AutoInvoice is true"
                    });
                    return false;
                }

                if (!dto.InvoiceDate.HasValue)
                {
                    errors.Add(new ErrorResponseDto
                    {
                        StatusCode = 400,
                        Message = $"Row {rowNumber}: InvoiceDate is required when AutoInvoice is true"
                    });
                    return false;
                }
            }

            return true;
        }

        private bool AnyRequiredPublicCategoryColumnEmpty(CategoryExcelDto dto)
        {
            return string.IsNullOrEmpty(dto.CategoryName) ||
                dto.DomainId == Guid.Empty;
        }

        public async Task<ExcelProcessingResultDto> UploadConciergeVenueExcelFileAsync(IFormFile excelFile, Guid domainId)
        {
            _logger.LogInfo("Validating and reading Excel file.");

            using (Stream stream = excelFile.OpenReadStream())
            {
                XSSFWorkbook workbook = new XSSFWorkbook(stream);
                ISheet worksheet = workbook.GetSheetAt(0);

                List<string> expectedHeadersRoster = new List<string>
                {
                    "DomainId","VenueName","CompanyCode","CompanyName","CostCenter","GLAccount","ShipToPlant","PurchaseOrg","PurchaseGroup","BillTo","Type"
                };

                ValidateHeaders(worksheet, expectedHeadersRoster);
                _logger.LogInfo("Excel file validated successfully.");

                var allData = ExtractAllConciergeVenueFromWorksheet(worksheet, domainId);
                _logger.LogInfo($"Extracted {allData.Count} rows from Excel file.");

                const int batchSize = 1000;
                var result = await ProcessConciergeVenueInBatchesAsync(allData, batchSize);
                _logger.LogDebug("Excel File processing completed.");
                return result;
            }
        }

        private List<VenueExcelDto> ExtractAllConciergeVenueFromWorksheet(ISheet worksheet, Guid domainId)
        {
            var allData = new List<VenueExcelDto>();

            for (int row = 1; row <= worksheet.LastRowNum; row++)
            {
                var currentRow = worksheet.GetRow(row);
                if (currentRow == null) continue;

                var dto = ExtractConciergeVenueFromRow(currentRow);
                if (dto.DomainId == domainId)
                {
                    dto.RowNumber = row;
                    allData.Add(dto);
                }
            }

            return allData;
        }
        private VenueExcelDto ExtractConciergeVenueFromRow(IRow currentRow)
        {
            VenueExcelDto dto = new VenueExcelDto
            {
                DomainId = currentRow.GetCell(0) != null && Guid.TryParse(currentRow.GetCell(0).ToString(), out Guid domainId) ? domainId : Guid.Empty,
                VenueName = currentRow.GetCell(1)?.ToString(),
                CompanyCode = currentRow.GetCell(2)?.ToString(),
                CompanyName = currentRow.GetCell(3)?.ToString(),
                CostCenter = currentRow.GetCell(4)?.ToString(),
                GLAccount = currentRow.GetCell(5)?.ToString(),
                ShipToPlant = currentRow.GetCell(6)?.ToString(),
                PurchaseOrg = currentRow.GetCell(7)?.ToString(),
                PurchaseGroup = currentRow.GetCell(8)?.ToString(),
                BillTo = currentRow.GetCell(9)?.ToString(),
                Type = currentRow.GetCell(10)?.ToString()
            };
            return dto;
        }


        private async Task<ExcelProcessingResultDto> ProcessConciergeVenueInBatchesAsync(
            List<VenueExcelDto> allData,
            int batchSize)
        {
            int totalRows = allData.Count;
            int successfulUploads = 0;
            int failedUploads = 0;
            List<ErrorResponseDto> errorResponses = new List<ErrorResponseDto>();

            _logger.LogInfo($"Processing {totalRows} rows in batches of {batchSize}.");

            // Process batches SEQUENTIALLY to avoid DbContext threading issues
            for (int i = 0; i < allData.Count; i += batchSize)
            {
                List<VenueExcelDto> batch = allData.Skip(i).Take(batchSize).ToList();
                int batchNumber = (i / batchSize) + 1;
                int totalBatches = (int)Math.Ceiling((double)allData.Count / batchSize);

                _logger.LogInfo($"Processing batch {batchNumber} of {totalBatches} (rows {i + 1} to {Math.Min(i + batchSize, allData.Count)})");

                // Validate batch
                List<VenueExcelDto> validRows = new List<VenueExcelDto>();
                foreach (VenueExcelDto dto in batch)
                {
                    if (AnyRequiredConciergeVenueColumnEmpty(dto))
                    {
                        AddErrorResponse(errorResponses, 400, $"One or more required columns are empty in row {dto.RowNumber}");
                        _logger.LogError($"One or more required columns are empty in row {dto.RowNumber}");
                        failedUploads++;
                    }
                    else
                    {
                        validRows.Add(dto);
                    }
                }

                if (validRows.Any())
                {
                    try
                    {
                        BatchProcessingResultDto batchResult = await _conciergeService.CreateConciergeVenueBatchAsync(validRows);
                        successfulUploads += batchResult.SuccessfulCount;
                        failedUploads += batchResult.FailedCount;
                        errorResponses.AddRange(batchResult.Errors);

                        _logger.LogInfo($"Batch {batchNumber} completed. Successfully processed: {batchResult.SuccessfulCount}, Failed: {batchResult.FailedCount}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing batch {batchNumber}: {ex.Message}");
                        foreach (VenueExcelDto dto in validRows)
                        {
                            AddErrorResponse(errorResponses, 500, $"Batch processing error for row {dto.RowNumber}: {ex.Message}");
                            failedUploads++;
                        }
                    }
                }
                else
                {
                    _logger.LogInfo($"Batch {batchNumber} contained no valid rows to process.");
                }
            }

            _logger.LogInfo($"All batches completed. Total successful: {successfulUploads}, Total failed: {failedUploads}");

            return new ExcelProcessingResultDto
            {
                TotalRows = totalRows,
                SuccessfulUploads = successfulUploads,
                FailedUploads = failedUploads,
                ErrorResponses = errorResponses
            };
        }
        public async Task<ExcelProcessingResultDto> UploadLegalVenueExcelFileAsync(IFormFile excelFile, Guid domainId)
        {
            _logger.LogInfo("Validating and reading Excel file.");

            using (Stream stream = excelFile.OpenReadStream())
            {
                XSSFWorkbook workbook = new XSSFWorkbook(stream);
                ISheet worksheet = workbook.GetSheetAt(0);

                List<string> expectedHeadersRoster = new List<string>
                {
                    "DomainId","VenueName","CompanyCode","CompanyName","CostCenter","ShipToPlant","PurchaseOrg","PurchaseGroup","BillTo"
                };

                ValidateHeaders(worksheet, expectedHeadersRoster);
                _logger.LogInfo("Excel file validated successfully.");

                var allData = ExtractAllLegalVenueFromWorksheet(worksheet, domainId);
                _logger.LogInfo($"Extracted {allData.Count} rows from Excel file.");

                const int batchSize = 1000;
                var result = await ProcessLegalVenueInBatchesAsync(allData, batchSize);
                _logger.LogDebug("Excel File processing completed.");
                return result;
            }
        }

        private List<VenueExcelDto> ExtractAllLegalVenueFromWorksheet(ISheet worksheet, Guid domainId)
        {
            var allData = new List<VenueExcelDto>();

            for (int row = 1; row <= worksheet.LastRowNum; row++)
            {
                var currentRow = worksheet.GetRow(row);
                if (currentRow == null) continue;

                var dto = ExtractLegalVenueFromRow(currentRow);
                if (dto.DomainId == domainId)
                {
                    dto.RowNumber = row;
                    allData.Add(dto);
                }
            }

            return allData;
        }

        private VenueExcelDto ExtractLegalVenueFromRow(IRow currentRow)
        {
            VenueExcelDto dto = new VenueExcelDto
            {
                DomainId = currentRow.GetCell(0) != null && Guid.TryParse(currentRow.GetCell(0).ToString(), out Guid domainId) ? domainId : Guid.Empty,
                VenueName = currentRow.GetCell(1)?.ToString(),
                CompanyCode = currentRow.GetCell(2)?.ToString(),
                CompanyName = currentRow.GetCell(3)?.ToString(),
                CostCenter = currentRow.GetCell(4)?.ToString(),

                ShipToPlant = currentRow.GetCell(5)?.ToString(),
                PurchaseOrg = currentRow.GetCell(6)?.ToString(),
                PurchaseGroup = currentRow.GetCell(7)?.ToString(),
                BillTo = currentRow.GetCell(8)?.ToString()
            };
            return dto;
        }

        private async Task<ExcelProcessingResultDto> ProcessLegalVenueInBatchesAsync(
            List<VenueExcelDto> allData,
            int batchSize)
        {
            int totalRows = allData.Count;
            int successfulUploads = 0;
            int failedUploads = 0;
            List<ErrorResponseDto> errorResponses = new List<ErrorResponseDto>();

            _logger.LogInfo($"Processing {totalRows} rows in batches of {batchSize}.");

            // Process batches SEQUENTIALLY to avoid DbContext threading issues
            for (int i = 0; i < allData.Count; i += batchSize)
            {
                List<VenueExcelDto> batch = allData.Skip(i).Take(batchSize).ToList();
                int batchNumber = (i / batchSize) + 1;
                int totalBatches = (int)Math.Ceiling((double)allData.Count / batchSize);

                _logger.LogInfo($"Processing batch {batchNumber} of {totalBatches} (rows {i + 1} to {Math.Min(i + batchSize, allData.Count)})");

                // Validate batch
                List<VenueExcelDto> validRows = new List<VenueExcelDto>();
                foreach (VenueExcelDto dto in batch)
                {
                    if (AnyRequiredLegalVenueColumnEmpty(dto))
                    {
                        AddErrorResponse(errorResponses, 400, $"One or more required columns are empty in row {dto.RowNumber}");
                        _logger.LogError($"One or more required columns are empty in row {dto.RowNumber}");
                        failedUploads++;
                    }
                    else
                    {
                        validRows.Add(dto);
                    }
                }

                if (validRows.Any())
                {
                    try
                    {
                        BatchProcessingResultDto batchResult = await _legalDepartmentMaterialService.CreateVenueBatchAsync(validRows);
                        successfulUploads += batchResult.SuccessfulCount;
                        failedUploads += batchResult.FailedCount;
                        errorResponses.AddRange(batchResult.Errors);

                        _logger.LogInfo($"Batch {batchNumber} completed. Successfully processed: {batchResult.SuccessfulCount}, Failed: {batchResult.FailedCount}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing batch {batchNumber}: {ex.Message}");
                        foreach (VenueExcelDto dto in validRows)
                        {
                            AddErrorResponse(errorResponses, 500, $"Batch processing error for row {dto.RowNumber}: {ex.Message}");
                            failedUploads++;
                        }
                    }
                }
                else
                {
                    _logger.LogInfo($"Batch {batchNumber} contained no valid rows to process.");
                }
            }

            _logger.LogInfo($"All batches completed. Total successful: {successfulUploads}, Total failed: {failedUploads}");

            return new ExcelProcessingResultDto
            {
                TotalRows = totalRows,
                SuccessfulUploads = successfulUploads,
                FailedUploads = failedUploads,
                ErrorResponses = errorResponses
            };
        }

        private bool AnyRequiredLegalVenueColumnEmpty(VenueExcelDto dto)
        {
            return string.IsNullOrEmpty(dto.VenueName) ||
                dto.DomainId == Guid.Empty;
        }
        //Contract Supplier

        public async Task<ExcelProcessingResultDto> UploadContractSupplierNameExcelFileAsync(IFormFile excelFile, Guid domainId)
        {
            _logger.LogInfo("Validating and reading Excel file.");

            using (Stream stream = excelFile.OpenReadStream())
            {
                XSSFWorkbook workbook = new XSSFWorkbook(stream);
                ISheet worksheet = workbook.GetSheetAt(0);

                List<string> expectedHeadersRoster = new List<string>
                {
                    "DomainId", "Name", "SmVendorId"
                };

                ValidateHeaders(worksheet, expectedHeadersRoster);
                _logger.LogInfo("Excel file validated successfully.");

                var allData = ExtractAllContractSupplierNameFromWorksheet(worksheet, domainId);
                _logger.LogInfo($"Extracted {allData.Count} rows from Excel file.");

                const int batchSize = 1000;
                var result = await ProcessContractSupplierNameInBatchesAsync(allData, batchSize);

                _logger.LogDebug("Excel File processing completed.");
                return result;
            }
        }

        private List<ContractSupplierNameExcelDto> ExtractAllContractSupplierNameFromWorksheet(ISheet worksheet, Guid domainId)
        {
            var allData = new List<ContractSupplierNameExcelDto>();

            for (int row = 1; row <= worksheet.LastRowNum; row++)
            {
                var currentRow = worksheet.GetRow(row);
                if (currentRow == null) continue;

                var dto = ExtractContractSupplierNameFromRow(currentRow);
                if (dto.DomainId == domainId)
                {
                    dto.RowNumber = row;
                    allData.Add(dto);
                }
            }

            return allData;
        }

        private ContractSupplierNameExcelDto ExtractContractSupplierNameFromRow(IRow currentRow)
        {
            ContractSupplierNameExcelDto dto = new ContractSupplierNameExcelDto
            {
                DomainId = currentRow.GetCell(0) != null && Guid.TryParse(currentRow.GetCell(0).ToString(), out Guid domainId) ? domainId : Guid.Empty,
                Name = currentRow.GetCell(1)?.ToString(),
                SmVendorId = currentRow.GetCell(2)?.ToString()
            };
            return dto;
        }

        private async Task<ExcelProcessingResultDto> ProcessContractSupplierNameInBatchesAsync(
            List<ContractSupplierNameExcelDto> allData,
            int batchSize)
        {
            int totalRows = allData.Count;
            int successfulUploads = 0;
            int failedUploads = 0;
            List<ErrorResponseDto> errorResponses = new List<ErrorResponseDto>();

            _logger.LogInfo($"Processing {totalRows} rows in batches of {batchSize}.");

            // Process batches SEQUENTIALLY to avoid DbContext threading issues
            for (int i = 0; i < allData.Count; i += batchSize)
            {
                List<ContractSupplierNameExcelDto> batch = allData.Skip(i).Take(batchSize).ToList();
                int batchNumber = (i / batchSize) + 1;
                int totalBatches = (int)Math.Ceiling((double)allData.Count / batchSize);

                _logger.LogInfo($"Processing batch {batchNumber} of {totalBatches} (rows {i + 1} to {Math.Min(i + batchSize, allData.Count)})");

                // Validate batch
                List<ContractSupplierNameExcelDto> validRows = new List<ContractSupplierNameExcelDto>();
                foreach (ContractSupplierNameExcelDto dto in batch)
                {
                    if (AnyRequiredContractSupplierColumnEmpty(dto))
                    {
                        AddErrorResponse(errorResponses, 400, $"One or more required columns are empty in row {dto.RowNumber}");
                        _logger.LogError($"One or more required columns are empty in row {dto.RowNumber}");
                        failedUploads++;
                    }
                    else
                    {
                        validRows.Add(dto);
                    }
                }

                if (validRows.Any())
                {
                    try
                    {
                        BatchProcessingResultDto batchResult = await _contractExcelService.CreateSupplierNameBatchAsync(validRows);
                        successfulUploads += batchResult.SuccessfulCount;
                        failedUploads += batchResult.FailedCount;
                        errorResponses.AddRange(batchResult.Errors);

                        _logger.LogInfo($"Batch {batchNumber} completed. Successfully processed: {batchResult.SuccessfulCount}, Failed: {batchResult.FailedCount}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing batch {batchNumber}: {ex.Message}");
                        foreach (ContractSupplierNameExcelDto dto in validRows)
                        {
                            AddErrorResponse(errorResponses, 500, $"Batch processing error for row {dto.RowNumber}: {ex.Message}");
                            failedUploads++;
                        }
                    }
                }
                else
                {
                    _logger.LogInfo($"Batch {batchNumber} contained no valid rows to process.");
                }
            }

            _logger.LogInfo($"All batches completed. Total successful: {successfulUploads}, Total failed: {failedUploads}");

            return new ExcelProcessingResultDto
            {
                TotalRows = totalRows,
                SuccessfulUploads = successfulUploads,
                FailedUploads = failedUploads,
                ErrorResponses = errorResponses
            };
        }

        private bool AnyRequiredContractSupplierColumnEmpty(ContractSupplierNameExcelDto dto)
        {
            return string.IsNullOrEmpty(dto.Name) ||
                   string.IsNullOrEmpty(dto.SmVendorId) ||
                   dto.DomainId == Guid.Empty;
        }
        public async Task<ExcelProcessingResultDto> UploadContractDepartmentNameExcelFileAsync(IFormFile excelFile, Guid domainId)
        {
            _logger.LogInfo("Validating and reading Excel file.");

            using (Stream stream = excelFile.OpenReadStream())
            {
                XSSFWorkbook workbook = new XSSFWorkbook(stream);
                ISheet worksheet = workbook.GetSheetAt(0);

                List<string> expectedHeadersRoster = new List<string>
                {
                    "DomainId", "UniqueName", "Name"
                };

                ValidateHeaders(worksheet, expectedHeadersRoster);
                _logger.LogInfo("Excel file validated successfully.");

                var allData = ExtractAllContractDepartmentNameFromWorksheet(worksheet, domainId);
                _logger.LogInfo($"Extracted {allData.Count} rows from Excel file.");

                const int batchSize = 1000;
                var result = await ProcessContractDepartmentNameInBatchesAsync(allData, batchSize);

                _logger.LogDebug("Excel File processing completed.");
                return result;
            }
        }

        private List<ContractDepartmentExcelDto> ExtractAllContractDepartmentNameFromWorksheet(ISheet worksheet, Guid domainId)
        {
            var allData = new List<ContractDepartmentExcelDto>();

            for (int row = 1; row <= worksheet.LastRowNum; row++)
            {
                var currentRow = worksheet.GetRow(row);
                if (currentRow == null) continue;

                var dto = ExtractContractDepartmentNameFromRow(currentRow);
                if (dto.DomainId == domainId)
                {
                    dto.RowNumber = row;
                    allData.Add(dto);
                }
            }

            return allData;
        }

        private ContractDepartmentExcelDto ExtractContractDepartmentNameFromRow(IRow currentRow)
        {
            ContractDepartmentExcelDto dto = new ContractDepartmentExcelDto
            {
                DomainId = currentRow.GetCell(0) != null && Guid.TryParse(currentRow.GetCell(0).ToString(), out Guid domainId) ? domainId : Guid.Empty,
                UniqueName = currentRow.GetCell(1)?.ToString(),
                Name = currentRow.GetCell(2)?.ToString()

            };
            return dto;
        }

        private async Task<ExcelProcessingResultDto> ProcessContractDepartmentNameInBatchesAsync(
            List<ContractDepartmentExcelDto> allData,
            int batchSize)
        {
            int totalRows = allData.Count;
            int successfulUploads = 0;
            int failedUploads = 0;
            List<ErrorResponseDto> errorResponses = new List<ErrorResponseDto>();

            _logger.LogInfo($"Processing {totalRows} rows in batches of {batchSize}.");

            // Process batches SEQUENTIALLY to avoid DbContext threading issues
            for (int i = 0; i < allData.Count; i += batchSize)
            {
                List<ContractDepartmentExcelDto> batch = allData.Skip(i).Take(batchSize).ToList();
                int batchNumber = (i / batchSize) + 1;
                int totalBatches = (int)Math.Ceiling((double)allData.Count / batchSize);

                _logger.LogInfo($"Processing batch {batchNumber} of {totalBatches} (rows {i + 1} to {Math.Min(i + batchSize, allData.Count)})");

                // Validate batch
                List<ContractDepartmentExcelDto> validRows = new List<ContractDepartmentExcelDto>();
                foreach (ContractDepartmentExcelDto dto in batch)
                {
                    if (AnyRequiredContractDepartmentColumnEmpty(dto))
                    {
                        AddErrorResponse(errorResponses, 400, $"One or more required columns are empty in row {dto.RowNumber}");
                        _logger.LogError($"One or more required columns are empty in row {dto.RowNumber}");
                        failedUploads++;
                    }
                    else
                    {
                        validRows.Add(dto);
                    }
                }

                if (validRows.Any())
                {
                    try
                    {
                        BatchProcessingResultDto batchResult = await _contractExcelService.CreateContractDepartmentNameBatchAsync(validRows);
                        successfulUploads += batchResult.SuccessfulCount;
                        failedUploads += batchResult.FailedCount;
                        errorResponses.AddRange(batchResult.Errors);

                        _logger.LogInfo($"Batch {batchNumber} completed. Successfully processed: {batchResult.SuccessfulCount}, Failed: {batchResult.FailedCount}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing batch {batchNumber}: {ex.Message}");
                        foreach (ContractDepartmentExcelDto dto in validRows)
                        {
                            AddErrorResponse(errorResponses, 500, $"Batch processing error for row {dto.RowNumber}: {ex.Message}");
                            failedUploads++;
                        }
                    }
                }
                else
                {
                    _logger.LogInfo($"Batch {batchNumber} contained no valid rows to process.");
                }
            }

            _logger.LogInfo($"All batches completed. Total successful: {successfulUploads}, Total failed: {failedUploads}");

            return new ExcelProcessingResultDto
            {
                TotalRows = totalRows,
                SuccessfulUploads = successfulUploads,
                FailedUploads = failedUploads,
                ErrorResponses = errorResponses
            };
        }

        private bool AnyRequiredContractDepartmentColumnEmpty(ContractDepartmentExcelDto dto)
        {
            return string.IsNullOrEmpty(dto.UniqueName) ||
                   string.IsNullOrEmpty(dto.Name) ||
                   dto.DomainId == Guid.Empty;
        }

        public async Task<ExcelProcessingResultDto> UploadContractRegionNameExcelFileAsync(IFormFile excelFile, Guid domainId)
        {
            _logger.LogInfo("Validating and reading Excel file.");

            using (Stream stream = excelFile.OpenReadStream())
            {
                XSSFWorkbook workbook = new XSSFWorkbook(stream);
                ISheet worksheet = workbook.GetSheetAt(0);

                List<string> expectedHeadersRoster = new List<string>
                {
                    "DomainId", "UniqueName", "Name"
                };

                ValidateHeaders(worksheet, expectedHeadersRoster);
                _logger.LogInfo("Excel file validated successfully.");

                var allData = ExtractAllContractRegionNameFromWorksheet(worksheet, domainId);
                _logger.LogInfo($"Extracted {allData.Count} rows from Excel file.");

                const int batchSize = 1000;
                var result = await ProcessContractRegionNameInBatchesAsync(allData, batchSize);

                _logger.LogDebug("Excel File processing completed.");
                return result;
            }
        }

        private List<ContractRegionExcelDto> ExtractAllContractRegionNameFromWorksheet(ISheet worksheet, Guid domainId)
        {
            var allData = new List<ContractRegionExcelDto>();

            for (int row = 1; row <= worksheet.LastRowNum; row++)
            {
                var currentRow = worksheet.GetRow(row);
                if (currentRow == null) continue;

                var dto = ExtractContractRegionNameFromRow(currentRow);
                if (dto.DomainId == domainId)
                {
                    dto.RowNumber = row;
                    allData.Add(dto);
                }
            }

            return allData;
        }

        private ContractRegionExcelDto ExtractContractRegionNameFromRow(IRow currentRow)
        {
            ContractRegionExcelDto dto = new ContractRegionExcelDto
            {
                DomainId = currentRow.GetCell(0) != null && Guid.TryParse(currentRow.GetCell(0).ToString(), out Guid domainId) ? domainId : Guid.Empty,
                UniqueName = currentRow.GetCell(1)?.ToString(),
                Name = currentRow.GetCell(2)?.ToString()

            };
            return dto;
        }

        private async Task<ExcelProcessingResultDto> ProcessContractRegionNameInBatchesAsync(
            List<ContractRegionExcelDto> allData,
            int batchSize)
        {
            int totalRows = allData.Count;
            int successfulUploads = 0;
            int failedUploads = 0;
            List<ErrorResponseDto> errorResponses = new List<ErrorResponseDto>();

            _logger.LogInfo($"Processing {totalRows} rows in batches of {batchSize}.");

            // Process batches SEQUENTIALLY to avoid DbContext threading issues
            for (int i = 0; i < allData.Count; i += batchSize)
            {
                List<ContractRegionExcelDto> batch = allData.Skip(i).Take(batchSize).ToList();
                int batchNumber = (i / batchSize) + 1;
                int totalBatches = (int)Math.Ceiling((double)allData.Count / batchSize);

                _logger.LogInfo($"Processing batch {batchNumber} of {totalBatches} (rows {i + 1} to {Math.Min(i + batchSize, allData.Count)})");

                // Validate batch
                List<ContractRegionExcelDto> validRows = new List<ContractRegionExcelDto>();
                foreach (ContractRegionExcelDto dto in batch)
                {
                    if (AnyRequiredContractRegionColumnEmpty(dto))
                    {
                        AddErrorResponse(errorResponses, 400, $"One or more required columns are empty in row {dto.RowNumber}");
                        _logger.LogError($"One or more required columns are empty in row {dto.RowNumber}");
                        failedUploads++;
                    }
                    else
                    {
                        validRows.Add(dto);
                    }
                }

                if (validRows.Any())
                {
                    try
                    {
                        BatchProcessingResultDto batchResult = await _contractExcelService.CreateContractRegionNameBatchAsync(validRows);
                        successfulUploads += batchResult.SuccessfulCount;
                        failedUploads += batchResult.FailedCount;
                        errorResponses.AddRange(batchResult.Errors);

                        _logger.LogInfo($"Batch {batchNumber} completed. Successfully processed: {batchResult.SuccessfulCount}, Failed: {batchResult.FailedCount}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing batch {batchNumber}: {ex.Message}");
                        foreach (ContractRegionExcelDto dto in validRows)
                        {
                            AddErrorResponse(errorResponses, 500, $"Batch processing error for row {dto.RowNumber}: {ex.Message}");
                            failedUploads++;
                        }
                    }
                }
                else
                {
                    _logger.LogInfo($"Batch {batchNumber} contained no valid rows to process.");
                }
            }

            _logger.LogInfo($"All batches completed. Total successful: {successfulUploads}, Total failed: {failedUploads}");

            return new ExcelProcessingResultDto
            {
                TotalRows = totalRows,
                SuccessfulUploads = successfulUploads,
                FailedUploads = failedUploads,
                ErrorResponses = errorResponses
            };
        }

        private bool AnyRequiredContractRegionColumnEmpty(ContractRegionExcelDto dto)
        {
            return string.IsNullOrEmpty(dto.UniqueName) ||
                   string.IsNullOrEmpty(dto.Name) ||
                   dto.DomainId == Guid.Empty;
        }


        public async Task<ExcelProcessingResultDto> UploadContractComoditiesNameExcelFileAsync(IFormFile excelFile, Guid domainId)
        {
            _logger.LogInfo("Validating and reading Excel file.");

            using (Stream stream = excelFile.OpenReadStream())
            {
                XSSFWorkbook workbook = new XSSFWorkbook(stream);
                ISheet worksheet = workbook.GetSheetAt(0);

                List<string> expectedHeadersRoster = new List<string>
                {
                    "DomainId", "UniqueName", "Name","ContractDomain"
                };

                ValidateHeaders(worksheet, expectedHeadersRoster);
                _logger.LogInfo("Excel file validated successfully.");

                var allData = ExtractAllContractComoditiesNameFromWorksheet(worksheet, domainId);
                _logger.LogInfo($"Extracted {allData.Count} rows from Excel file.");

                const int batchSize = 1000;
                var result = await ProcessContractComoditiesNameInBatchesAsync(allData, batchSize);

                _logger.LogDebug("Excel File processing completed.");
                return result;
            }
        }

        private List<ContractComoditiesExcelDto> ExtractAllContractComoditiesNameFromWorksheet(ISheet worksheet, Guid domainId)
        {
            var allData = new List<ContractComoditiesExcelDto>();

            for (int row = 1; row <= worksheet.LastRowNum; row++)
            {
                var currentRow = worksheet.GetRow(row);
                if (currentRow == null) continue;

                var dto = ExtractContractComoditiesNameFromRow(currentRow);
                if (dto.DomainId == domainId)
                {
                    dto.RowNumber = row;
                    allData.Add(dto);
                }
            }

            return allData;
        }

        private ContractComoditiesExcelDto ExtractContractComoditiesNameFromRow(IRow currentRow)
        {
            ContractComoditiesExcelDto dto = new ContractComoditiesExcelDto
            {
                DomainId = currentRow.GetCell(0) != null && Guid.TryParse(currentRow.GetCell(0).ToString(), out Guid domainId) ? domainId : Guid.Empty,
                UniqueName = currentRow.GetCell(1)?.ToString(),
                Name = currentRow.GetCell(2)?.ToString(),
                ContractDomain = currentRow.GetCell(3)?.ToString()
            };
            return dto;
        }

        private async Task<ExcelProcessingResultDto> ProcessContractComoditiesNameInBatchesAsync(
            List<ContractComoditiesExcelDto> allData,
            int batchSize)
        {
            int totalRows = allData.Count;
            int successfulUploads = 0;
            int failedUploads = 0;
            List<ErrorResponseDto> errorResponses = new List<ErrorResponseDto>();

            _logger.LogInfo($"Processing {totalRows} rows in batches of {batchSize}.");

            // Process batches SEQUENTIALLY to avoid DbContext threading issues
            for (int i = 0; i < allData.Count; i += batchSize)
            {
                List<ContractComoditiesExcelDto> batch = allData.Skip(i).Take(batchSize).ToList();
                int batchNumber = (i / batchSize) + 1;
                int totalBatches = (int)Math.Ceiling((double)allData.Count / batchSize);

                _logger.LogInfo($"Processing batch {batchNumber} of {totalBatches} (rows {i + 1} to {Math.Min(i + batchSize, allData.Count)})");

                // Validate batch
                List<ContractComoditiesExcelDto> validRows = new List<ContractComoditiesExcelDto>();
                foreach (ContractComoditiesExcelDto dto in batch)
                {
                    if (AnyRequiredContractComoditiesColumnEmpty(dto))
                    {
                        AddErrorResponse(errorResponses, 400, $"One or more required columns are empty in row {dto.RowNumber}");
                        _logger.LogError($"One or more required columns are empty in row {dto.RowNumber}");
                        failedUploads++;
                    }
                    else
                    {
                        validRows.Add(dto);
                    }
                }

                if (validRows.Any())
                {
                    try
                    {
                        BatchProcessingResultDto batchResult = await _contractExcelService.CreateContractComoditiesNameBatchAsync(validRows);
                        successfulUploads += batchResult.SuccessfulCount;
                        failedUploads += batchResult.FailedCount;
                        errorResponses.AddRange(batchResult.Errors);

                        _logger.LogInfo($"Batch {batchNumber} completed. Successfully processed: {batchResult.SuccessfulCount}, Failed: {batchResult.FailedCount}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing batch {batchNumber}: {ex.Message}");
                        foreach (ContractComoditiesExcelDto dto in validRows)
                        {
                            AddErrorResponse(errorResponses, 500, $"Batch processing error for row {dto.RowNumber}: {ex.Message}");
                            failedUploads++;
                        }
                    }
                }
                else
                {
                    _logger.LogInfo($"Batch {batchNumber} contained no valid rows to process.");
                }
            }

            _logger.LogInfo($"All batches completed. Total successful: {successfulUploads}, Total failed: {failedUploads}");

            return new ExcelProcessingResultDto
            {
                TotalRows = totalRows,
                SuccessfulUploads = successfulUploads,
                FailedUploads = failedUploads,
                ErrorResponses = errorResponses
            };
        }

        private bool AnyRequiredContractComoditiesColumnEmpty(ContractComoditiesExcelDto dto)
        {
            return string.IsNullOrEmpty(dto.UniqueName) ||
                   string.IsNullOrEmpty(dto.Name) ||
                   string.IsNullOrEmpty(dto.ContractDomain);
        }
        public async Task<ExcelProcessingResultDto> UploadLegalMaterialCodeExcelFileAsync(IFormFile excelFile, Guid domainId)
        {
            _logger.LogInfo("Validating and reading Excel file.");

            using (Stream stream = excelFile.OpenReadStream())
            {
                XSSFWorkbook workbook = new XSSFWorkbook(stream);
                ISheet worksheet = workbook.GetSheetAt(0);

                List<string> expectedHeadersRoster = new List<string>
                {
                    "DomainId", "MaterialCode", "Description","GLAccount"
                };

                ValidateHeaders(worksheet, expectedHeadersRoster);
                _logger.LogInfo("Excel file validated successfully.");

                var allData = ExtractAllLegalMaterialCodesFromWorksheet(worksheet, domainId);
                _logger.LogInfo($"Extracted {allData.Count} rows from Excel file.");

                const int batchSize = 1000;
                var result = await ProcessLegalMaterialCodesInBatchesAsync(allData, batchSize);

                _logger.LogDebug("Excel File processing completed.");
                return result;
            }
        }

        private List<LegalMaterialCodeExcelDto> ExtractAllLegalMaterialCodesFromWorksheet(ISheet worksheet, Guid domainId)
        {
            var allData = new List<LegalMaterialCodeExcelDto>();

            for (int row = 1; row <= worksheet.LastRowNum; row++)
            {
                var currentRow = worksheet.GetRow(row);
                if (currentRow == null) continue;

                var dto = ExtractLegalMaterialCodeFromRow(currentRow);
                if (dto.DomainId == domainId)
                {
                    dto.RowNumber = row;
                    allData.Add(dto);
                }
            }

            return allData;
        }

        private LegalMaterialCodeExcelDto ExtractLegalMaterialCodeFromRow(IRow currentRow)
        {
            LegalMaterialCodeExcelDto dto = new LegalMaterialCodeExcelDto
            {
                DomainId = currentRow.GetCell(0) != null && Guid.TryParse(currentRow.GetCell(0).ToString(), out Guid domainId) ? domainId : Guid.Empty,
                MaterialCode = currentRow.GetCell(1)?.ToString(),
                Description = currentRow.GetCell(2)?.ToString(),
                GLAccount = currentRow.GetCell(3)?.ToString()

            };
            return dto;
        }

        private async Task<ExcelProcessingResultDto> ProcessLegalMaterialCodesInBatchesAsync(
            List<LegalMaterialCodeExcelDto> allData,
            int batchSize)
        {
            int totalRows = allData.Count;
            int successfulUploads = 0;
            int failedUploads = 0;
            List<ErrorResponseDto> errorResponses = new List<ErrorResponseDto>();

            _logger.LogInfo($"Processing {totalRows} rows in batches of {batchSize}.");

            // Process batches SEQUENTIALLY to avoid DbContext threading issues
            for (int i = 0; i < allData.Count; i += batchSize)
            {
                List<LegalMaterialCodeExcelDto> batch = allData.Skip(i).Take(batchSize).ToList();
                int batchNumber = (i / batchSize) + 1;
                int totalBatches = (int)Math.Ceiling((double)allData.Count / batchSize);

                _logger.LogInfo($"Processing batch {batchNumber} of {totalBatches} (rows {i + 1} to {Math.Min(i + batchSize, allData.Count)})");

                // Validate batch
                List<LegalMaterialCodeExcelDto> validRows = new List<LegalMaterialCodeExcelDto>();
                foreach (LegalMaterialCodeExcelDto dto in batch)
                {
                    if (AnyRequiredLegalMaterialCodeColumnEmpty(dto))
                    {
                        AddErrorResponse(errorResponses, 400, $"One or more required columns are empty in row {dto.RowNumber}");
                        _logger.LogError($"One or more required columns are empty in row {dto.RowNumber}");
                        failedUploads++;
                    }
                    else
                    {
                        validRows.Add(dto);
                    }
                }

                if (validRows.Any())
                {
                    try
                    {
                        BatchProcessingResultDto batchResult = await _legalDepartmentMaterialService.CreateLegalMaterialCodeBatchAsync(validRows);
                        successfulUploads += batchResult.SuccessfulCount;
                        failedUploads += batchResult.FailedCount;
                        errorResponses.AddRange(batchResult.Errors);

                        _logger.LogInfo($"Batch {batchNumber} completed. Successfully processed: {batchResult.SuccessfulCount}, Failed: {batchResult.FailedCount}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing batch {batchNumber}: {ex.Message}");
                        foreach (LegalMaterialCodeExcelDto dto in validRows)
                        {
                            AddErrorResponse(errorResponses, 500, $"Batch processing error for row {dto.RowNumber}: {ex.Message}");
                            failedUploads++;
                        }
                    }
                }
                else
                {
                    _logger.LogInfo($"Batch {batchNumber} contained no valid rows to process.");
                }
            }

            _logger.LogInfo($"All batches completed. Total successful: {successfulUploads}, Total failed: {failedUploads}");

            return new ExcelProcessingResultDto
            {
                TotalRows = totalRows,
                SuccessfulUploads = successfulUploads,
                FailedUploads = failedUploads,
                ErrorResponses = errorResponses
            };
        }

        private bool AnyRequiredLegalMaterialCodeColumnEmpty(LegalMaterialCodeExcelDto dto)
        {
            return string.IsNullOrEmpty(dto.MaterialCode) ||
                   string.IsNullOrEmpty(dto.Description) ||
                   dto.DomainId == Guid.Empty;
        }



    }
}

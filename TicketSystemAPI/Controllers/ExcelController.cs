using Contracts;
using Contracts.IServices;
using Dto;
using ExceptionHandler;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using TicketSystemAPI.Attributes;
using Contracts.IRepository;

namespace TicketSystemAPI.Controllers
{
    [ApiController]
    public class ExcelController : BaseController
    {
        private readonly ILoggerManager _logger;
        private readonly IExcelUploadService _excelUploadService;

        public ExcelController(ILoggerManager logger, IExcelUploadService excelUploadService)
        {
            _logger = logger;
            _excelUploadService = excelUploadService;

        }

        /// <summary>
        /// Upload the PO Excel File
        /// </summary>
        [HttpPost]
        [Route("api/v1/upload-excel")]
        [ValidateModelState]
        [ApiAuthorization(Name = "UPLOAD_PO_EXCEL")]
        [SwaggerOperation("UploadExcel")]
        [SwaggerResponse(statusCode: 201, type: typeof(ExcelProcessingResultDto), description: "Excel Uploaded successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> UploadExcel([Required] IFormFile excelFile)
        {
            _logger.LogDebug($"Uplaoding PO Excel File...");
            if (excelFile == null || excelFile.Length == 0 || !Path.GetExtension(excelFile.FileName).Equals(".xlsx"))
            {
                _logger.LogError("Invalid Excel file.");
                throw new BadRequestCustomException("Invalid Excel file.", "Invalid Excel file.");
            }
            Guid domainId = getUserDomainId();
            ExcelProcessingResultDto result = await _excelUploadService.UploadExcelFileAsync(excelFile, domainId);
            _logger.LogDebug($"Uplaoded PO Excel File...");
            return Ok(result);
        }

        /// <summary>
        /// Upload the PO Master Excel File
        /// </summary>
        [HttpPost]
        [Route("api/v1/upload-excel-master")]
        [ValidateModelState]
        [ApiAuthorization(Name = "UPLOAD_PO_MASTER_EXCEL")]
        [SwaggerOperation("UploadExcelMaster")]
        [SwaggerResponse(statusCode: 201, type: typeof(ExcelProcessingResultDto), description: "Excel Uploaded successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> UploadExcelMaster([Required] IFormFile excelFile)
        {
            _logger.LogDebug($"Uplaoding PO Master Excel File...");
            if (excelFile == null || excelFile.Length == 0 || !Path.GetExtension(excelFile.FileName).Equals(".xlsx"))
            {
                _logger.LogError("Invalid Excel file.");
                throw new BadRequestCustomException("Invalid Excel file.", "Invalid Excel file.");
            }
            Guid domainId = getUserDomainId();
            ExcelProcessingResultDto result = await _excelUploadService.UploadMasterExcelFileAsync(excelFile, domainId);
            _logger.LogDebug($"Uplaoded PO Master Excel File...");
            return Ok(result);
        }

        /// <summary>
        /// Upload the PO Master Excel File
        /// </summary>
        [HttpPost]
        [Route("api/v1/upload-excel-contract")]
        [ValidateModelState]
        [ApiAuthorization(Name = "UPLOAD_CONTRACT_EXCEL")]
        [SwaggerOperation("UploadExcelContract")]
        [SwaggerResponse(statusCode: 201, type: typeof(ExcelProcessingResultDto), description: "Excel Uploaded successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> UploadExcelContract([Required] IFormFile excelFile)
        {
            _logger.LogDebug($"Uplaoding Contract Excel File...");
            if (excelFile == null || excelFile.Length == 0 || !Path.GetExtension(excelFile.FileName).Equals(".xlsx"))
            {
                _logger.LogError("Invalid Excel file.");
                throw new BadRequestCustomException("Invalid Excel file.", "Invalid Excel file.");
            }
            Guid domainId = getUserDomainId();
            ExcelProcessingResultDto result = await _excelUploadService.UploadContractExcelFileAsync(excelFile, domainId);
            _logger.LogDebug($"Uplaoded Contract Excel File...");
            return Ok(result);
        }

        /// <summary>
        /// Upload the PO Master Excel File
        /// </summary>
        [HttpPost]
        [Route("api/v1/upload-excel-company-code")]
        [ValidateModelState]
        [ApiAuthorization(Name = "UPLOAD_COMPANY_CODE_EXCEL")]
        [SwaggerOperation("UploadExcelCompanyCode")]
        [SwaggerResponse(statusCode: 201, type: typeof(ExcelProcessingResultDto), description: "Excel Uploaded successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> UploadExcelCompanyCode([Required] IFormFile excelFile)
        {
            _logger.LogDebug($"Uplaoding Company Code Excel File...");
            if (excelFile == null || excelFile.Length == 0 || !Path.GetExtension(excelFile.FileName).Equals(".xlsx"))
            {
                _logger.LogError("Invalid Excel file.");
                throw new BadRequestCustomException("Invalid Excel file.", "Invalid Excel file.");
            }
            Guid domainId = getUserDomainId();
            ExcelProcessingResultDto result = await _excelUploadService.UploadCompanyCodeExcelFileAsync(excelFile, domainId);
            _logger.LogDebug($"Uplaoded Company Code File...");
            return Ok(result);
        }
        /// <summary>
        /// Upload the Material PR Excel File
        /// </summary>
        [HttpPost]
        [Route("api/v1/upload-excel-material-pr")]
        [ValidateModelState]
        [ApiAuthorization(Name = "UPLOAD_MATERIAL_PR_EXCEL")]
        [SwaggerOperation("UploadExcelMaterialPR")]
        [SwaggerResponse(statusCode: 201, type: typeof(ExcelProcessingResultDto), description: "Excel Uploaded successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> UploadExcelMaterialPR([Required] IFormFile excelFile)
        {
            _logger.LogDebug($"Uplaoding Material PR Excel File...");
            if (excelFile == null || excelFile.Length == 0 || !Path.GetExtension(excelFile.FileName).Equals(".xlsx"))
            {
                _logger.LogError("Invalid Excel file.");
                throw new BadRequestCustomException("Invalid Excel file.", "Invalid Excel file.");
            }
            Guid domainId = getUserDomainId();
            Guid userId = getLoggedUser();


            ExcelProcessingResultDto result = await _excelUploadService.UploadMaterialPRExcelFileAsync(excelFile, domainId, userId);
            _logger.LogDebug($"Uplaoded Material PR Excel File...");
            return Ok(result);
        }

        /// <summary>
        /// Upload the Block Or Unblock PO Excel File
        /// </summary>
        [HttpPost]
        [Route("api/v1/upload-excel-block-unblock")]
        [ValidateModelState]
        [ApiAuthorization(Name = "UPLOAD_BLOCK_UNBLOCK_PO_EXCEL")]
        [SwaggerOperation("UploadBlockOrUnblockPurchaseOrder")]
        [SwaggerResponse(statusCode: 201, type: typeof(ExcelProcessingResultDto), description: "Excel Uploaded successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> UploadBlockOrUnblockPurchaseOrder([Required] IFormFile excelFile)
        {
            _logger.LogDebug($"Uplaoding Block Or Unblock PO Excel File...");
            if (excelFile == null || excelFile.Length == 0 || !Path.GetExtension(excelFile.FileName).Equals(".xlsx"))
            {
                _logger.LogError("Invalid Excel file.");
                throw new BadRequestCustomException("Invalid Excel file.", "Invalid Excel file.");
            }
            Guid domainId = getUserDomainId();
            ExcelProcessingResultDto result = await _excelUploadService.UploadBlockUnblockExcelFileAsync(excelFile, domainId);
            _logger.LogDebug($"Uplaoded Block Or Unblock PO Excel File...");
            return Ok(result);
        }

        /// <summary>
        /// Upload the Material PR Excel File
        /// </summary>
        [HttpPost]
        [Route("api/v1/upload-excel-service-pr")]
        [ValidateModelState]
        [ApiAuthorization(Name = "UPLOAD_SERVICE_PR_EXCEL")]
        [SwaggerOperation("UploadExcelMaterialPR")]
        [SwaggerResponse(statusCode: 201, type: typeof(ExcelProcessingResultDto), description: "Excel Uploaded successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> UploadExcelServicePR([Required] IFormFile excelFile)
        {
            _logger.LogDebug($"Uplaoding Material PR Excel File...");
            if (excelFile == null || excelFile.Length == 0 || !Path.GetExtension(excelFile.FileName).Equals(".xlsx"))
            {
                _logger.LogError("Invalid Excel file.");
                throw new BadRequestCustomException("Invalid Excel file.", "Invalid Excel file.");
            }
            Guid domainId = getUserDomainId();
            Guid userId = getLoggedUser();


            ExcelProcessingResultDto result = await _excelUploadService.UploadServicePRExcelFileAsync(excelFile, domainId, userId);
            _logger.LogDebug($"Uplaoded Service PR Excel File...");
            return Ok(result);
        }

        [HttpPost]
        [Route("api/v1/upload-excel-supplier-name")]
        [ValidateModelState]
        [ApiAuthorization(Name = "UPLOAD_SUPPLIER_NAME_EXCEL")]
        [SwaggerOperation("UploadExcelSupplierName")]
        [SwaggerResponse(statusCode: 201, type: typeof(ExcelProcessingResultDto), description: "Excel Uploaded successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> UploadExcelSupplierName([Required] IFormFile excelFile)
        {
            _logger.LogDebug($"Uplaoding Supplier Name Excel File...");
            if (excelFile == null || excelFile.Length == 0 || !Path.GetExtension(excelFile.FileName).Equals(".xlsx"))
            {
                _logger.LogError("Invalid Excel file.");
                throw new BadRequestCustomException("Invalid Excel file.", "Invalid Excel file.");
            }
            Guid domainId = getUserDomainId();
            ExcelProcessingResultDto result = await _excelUploadService.UploadSupplierNameExcelFileAsync(excelFile, domainId);
            _logger.LogDebug($"Uplaoded Supplier Name File...");
            return Ok(result);
        }

        [HttpPost]
        [Route("api/v1/upload-excel-venue-name")]
        [ValidateModelState]
        [ApiAuthorization(Name = "UPLOAD_VENUE_NAME_EXCEL")]
        [SwaggerOperation("UploadExcelVenueName")]
        [SwaggerResponse(statusCode: 201, type: typeof(ExcelProcessingResultDto), description: "Excel Uploaded successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> UploadExcelVenueName([Required] IFormFile excelFile)
        {
            _logger.LogDebug($"Uplaoding Venue Name Excel File...");
            if (excelFile == null || excelFile.Length == 0 || !Path.GetExtension(excelFile.FileName).Equals(".xlsx"))
            {
                _logger.LogError("Invalid Excel file.");
                throw new BadRequestCustomException("Invalid Excel file.", "Invalid Excel file.");
            }
            Guid domainId = getUserDomainId();
            ExcelProcessingResultDto result = await _excelUploadService.UploadVenueExcelFileAsync(excelFile, domainId);
            _logger.LogDebug($"Uplaoded Venue Name File...");
            return Ok(result);
        }

        /// <summary>
        /// Upload the PO Bot Process Excel File
        /// </summary>
        [HttpPost]
        [Route("api/v1/upload-excel-bot-process")]
        [ValidateModelState]
        [ApiAuthorization(Name = "UPLOAD_BOT_PROCESS_EXCEL")]
        [SwaggerOperation("UploadExcelBotProcess")]
        [SwaggerResponse(statusCode: 201, type: typeof(ExcelProcessingResultDto), description: "Excel Uploaded successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> UploadExcelBotProcess([Required] IFormFile excelFile)
        {
            _logger.LogDebug($"Uploading Bot Process Excel File...");
            if (excelFile == null || excelFile.Length == 0 || !Path.GetExtension(excelFile.FileName).Equals(".xlsx"))
            {
                _logger.LogError("Invalid Excel file.");
                throw new BadRequestCustomException("Invalid Excel file.", "Invalid Excel file.");
            }
            Guid domainId = getUserDomainId();
            ExcelProcessingResultDto result = await _excelUploadService.UploadBotProcessExcelFileAsync(excelFile, domainId);
            _logger.LogDebug($"Uploaded Bot Process Excel File...");
            return Ok(result);
        }

        [HttpPost]
        [Route("api/v1/upload-excel-draft-material-PR")]
        [ValidateModelState]
        [ApiAuthorization(Name = "UPLOAD_DRAFT_MATERIAL_PR")]
        [SwaggerOperation("UploadDrqaftMaterialPR")]
        [SwaggerResponse(statusCode: 201, type: typeof(ExcelProcessingResultDto), description: "Excel Uploaded successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> UploadExcelDraftMaterialPR([Required] IFormFile excelFile)
        {
            _logger.LogDebug($"Uplaoding Venue Name Excel File...");
            if (excelFile == null || excelFile.Length == 0 || !Path.GetExtension(excelFile.FileName).Equals(".xlsx"))
            {
                _logger.LogError("Invalid Excel file.");
                throw new BadRequestCustomException("Invalid Excel file.", "Invalid Excel file.");
            }
            Guid domainId = getUserDomainId();
            Guid userId = getLoggedUser();
            ExcelProcessingResultDto result = await _excelUploadService.UploadDraftMaterialPRFileAsync(excelFile, domainId, userId);
            _logger.LogDebug($"Uplaoded draft materialpr File...");
            return Ok(result);
        }
         [HttpPost]
        [Route("api/v1/upload-excel-draft-service-PR")]
        [ValidateModelState]
        [ApiAuthorization(Name = "UPLOAD_DRAFT_SERVICE_PR")]
        [SwaggerOperation("UploadDraftServicePR")]
        [SwaggerResponse(statusCode: 201, type: typeof(ExcelProcessingResultDto), description: "Excel Uploaded successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> UploadExcelDraftServicePR([Required] IFormFile excelFile)
        {
            _logger.LogDebug($"Uplaoding Draft Service PR Excel File...");
            if (excelFile == null || excelFile.Length == 0 || !Path.GetExtension(excelFile.FileName).Equals(".xlsx"))
            {
                _logger.LogError("Invalid Excel file.");
                throw new BadRequestCustomException("Invalid Excel file.", "Invalid Excel file.");
            }
            Guid domainId = getUserDomainId();
            Guid userId = getLoggedUser();
            ExcelProcessingResultDto result = await _excelUploadService.UploadDraftServicePRFileAsync(excelFile, domainId,userId);
            _logger.LogDebug($"Uplaoded draft servicepr File...");
            return Ok(result);
        }


        /// <summary>
        /// Upload the Block Or Unblock PO Excel File
        /// </summary>
        [HttpPost]
        [Route("api/v1/upload-excel-PR-block-unblock")]
        [ValidateModelState]
        [ApiAuthorization(Name = "UPLOAD_BLOCK_UNBLOCK_PR_PO_EXCEL")]
        [SwaggerOperation("UploadPRBlockOrUnblockPurchaseOrder")]
        [SwaggerResponse(statusCode: 201, type: typeof(ExcelProcessingResultDto), description: "Excel Uploaded successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> UploadPRBlockOrUnblockPurchaseOrder([Required] IFormFile excelFile)
        {
            _logger.LogDebug($"Uplaoding Block Or Unblock PO Excel File...");
            if (excelFile == null || excelFile.Length == 0 || !Path.GetExtension(excelFile.FileName).Equals(".xlsx"))
            {
                _logger.LogError("Invalid Excel file.");
                throw new BadRequestCustomException("Invalid Excel file.", "Invalid Excel file.");
            }
            Guid domainId = getUserDomainId();
            ExcelProcessingResultDto result = await _excelUploadService.UploadPRBlockUnblockExcelFileAsync(excelFile, domainId);
            _logger.LogDebug($"Uplaoded Block Or Unblock PO Excel File...");
            return Ok(result);
        }
        /// <summary>
        /// Upload the Property Cross Charge Excel File
        ///     /// </summary>
        [HttpPost]
        [Route("api/v1/upload-excel-property-cross-charge")]
        [ValidateModelState]
        [ApiAuthorization(Name = "UPLOAD_PROPERTY_CROSS_CHARGE_EXCEL")]
        [SwaggerOperation("UploadExcelPropertyCrossCharge")]
        [SwaggerResponse(statusCode: 201, type: typeof(ExcelProcessingResultDto), description: "Excel Uploaded successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> UploadExcelPropertyCrossCharge([Required] IFormFile excelFile)
        {
            _logger.LogDebug($"Uplaoding Property Cross Charge Excel File...");
            if (excelFile == null || excelFile.Length == 0 || !Path.GetExtension(excelFile.FileName).Equals(".xlsx"))
            {
                _logger.LogError("Invalid Excel file.");
                throw new BadRequestCustomException("Invalid Excel file.", "Invalid Excel file.");
            }
            Guid domainId = getUserDomainId();
            ExcelProcessingResultDto result = await _excelUploadService.UploadCrossChargeExcelFileAsync(excelFile, domainId);
            _logger.LogDebug($"Uplaoded Property Cross Charge File...");
            return Ok(result);
        }

        /// <summary>
        /// Upload the Property Cross Charge Excel File
        ///     /// </summary>
        [HttpPost]
        [Route("api/v1/upload-excel-duplicate-check")]
        [ValidateModelState]
        [SwaggerOperation("UploadExcelDuplicateCheck")]
        [SwaggerResponse(statusCode: 201, type: typeof(ExcelProcessingResultDto), description: "Excel Uploaded successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> UploadExcelDuplicateCheck([Required] IFormFile excelFile)
        {
            _logger.LogDebug($"Uplaoding Property Cross Charge Excel File...");
            if (excelFile == null || excelFile.Length == 0 || !Path.GetExtension(excelFile.FileName).Equals(".xlsx"))
            {
                _logger.LogError("Invalid Excel file.");
                throw new BadRequestCustomException("Invalid Excel file.", "Invalid Excel file.");
            }
            var resultBytes = await _excelUploadService.ProcessAndGenerateExcel(excelFile);
            return File(resultBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"DuplicateCheck_Result_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }

        [HttpPost]
        [Route("api/v1/upload-excel-public-venue-name")]
        [ValidateModelState]
        [ApiAuthorization(Name = "UPLOAD_PUBLIC_VENUE_NAME_EXCEL")]
        [SwaggerOperation("UploadExcelPublicVenueName")]
        [SwaggerResponse(statusCode: 201, type: typeof(ExcelProcessingResultDto), description: "Excel Uploaded successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> UploadExcelPublicVenueName([Required] IFormFile excelFile)
        {
            _logger.LogDebug($"Uplaoding Public Venue Name Excel File...");
            if (excelFile == null || excelFile.Length == 0 || !Path.GetExtension(excelFile.FileName).Equals(".xlsx"))
            {
                _logger.LogError("Invalid Excel file.");
                throw new BadRequestCustomException("Invalid Excel file.", "Invalid Excel file.");
            }
            Guid domainId = getUserDomainId();
            ExcelProcessingResultDto result = await _excelUploadService.UploadPublicVenueExcelFileAsync(excelFile, domainId);
            _logger.LogDebug($"Uplaoded Public Venue Name File...");
            return Ok(result);
        }
        [HttpPost]
        [Route("api/v1/upload-excel-public-category")]
        [ValidateModelState]
        [ApiAuthorization(Name = "UPLOAD_PUBLIC_CATEGORY_EXCEL")]
        [SwaggerOperation("UploadExcelPublicCategory")]
        [SwaggerResponse(statusCode: 201, type: typeof(ExcelProcessingResultDto), description: "Excel Uploaded successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> UploadExcelPublicCategory([Required] IFormFile excelFile)
        {
            _logger.LogDebug($"Uplaoding Public Category Excel File...");
            if (excelFile == null || excelFile.Length == 0 || !Path.GetExtension(excelFile.FileName).Equals(".xlsx"))
            {
                _logger.LogError("Invalid Excel file.");
                throw new BadRequestCustomException("Invalid Excel file.", "Invalid Excel file.");
            }
            Guid domainId = getUserDomainId();
            ExcelProcessingResultDto result = await _excelUploadService.UploadPublicCategoryExcelFileAsync(excelFile, domainId);
            _logger.LogDebug($"Uplaoded Public Category File...");
            return Ok(result);
        }

        [HttpPost]
        [Route("api/v1/upload-excel-concierge-venue-name")]
        [ValidateModelState]
        [ApiAuthorization(Name = "UPLOAD_CONCIERGE_VENUE_NAME_EXCEL")]
        [SwaggerOperation("UploadExcelConciergeVenueName")]
        [SwaggerResponse(statusCode: 201, type: typeof(ExcelProcessingResultDto), description: "Excel Uploaded successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> UploadExcelConciergeVenueName([Required] IFormFile excelFile)
        {
            _logger.LogDebug($"Uplaoding Concierge Venue Name Excel File...");
            if (excelFile == null || excelFile.Length == 0 || !Path.GetExtension(excelFile.FileName).Equals(".xlsx"))
            {
                _logger.LogError("Invalid Excel file.");
                throw new BadRequestCustomException("Invalid Excel file.", "Invalid Excel file.");
            }
            Guid domainId = getUserDomainId();
            ExcelProcessingResultDto result = await _excelUploadService.UploadConciergeVenueExcelFileAsync(excelFile, domainId);
            _logger.LogDebug($"Uplaoded Concierge Venue Name File...");
            return Ok(result);
        }
        [HttpPost]
        [Route("api/v1/upload-excel-legal-venue-name")]
        [ValidateModelState]
        [ApiAuthorization(Name = "UPLOAD_LEGAL_VENUE_NAME_EXCEL")]
        [SwaggerOperation("UploadExcelLegalVenueName")]
        [SwaggerResponse(statusCode: 201, type: typeof(ExcelProcessingResultDto), description: "Excel Uploaded successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> UploadLegalExcelVenueName([Required] IFormFile excelFile)
        {
            _logger.LogDebug($"Uplaoding Venue Name Excel File...");
            if (excelFile == null || excelFile.Length == 0 || !Path.GetExtension(excelFile.FileName).Equals(".xlsx"))
            {
                _logger.LogError("Invalid Excel file.");
                throw new BadRequestCustomException("Invalid Excel file.", "Invalid Excel file.");
            }
            Guid domainId = getUserDomainId();
            ExcelProcessingResultDto result = await _excelUploadService.UploadLegalVenueExcelFileAsync(excelFile, domainId);
            _logger.LogDebug($"Uplaoded Venue Name File...");
            return Ok(result);
        }
         [HttpPost]
        [Route("api/v1/upload-excel-contract-supplier-name")]
        [ValidateModelState]
        [ApiAuthorization(Name = "UPLOAD_CONTRACT_SUPPLIER_NAME_EXCEL")]
        [SwaggerOperation("UploadExcelContractSupplierName")]
        [SwaggerResponse(statusCode: 201, type: typeof(ExcelProcessingResultDto), description: "Excel Uploaded successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> UploadContractSupplierName([Required] IFormFile excelFile)
        {
            _logger.LogDebug($"Uplaoding Venue Name Excel File...");
            if (excelFile == null || excelFile.Length == 0 || !Path.GetExtension(excelFile.FileName).Equals(".xlsx"))
            {
                _logger.LogError("Invalid Excel file.");
                throw new BadRequestCustomException("Invalid Excel file.", "Invalid Excel file.");
            }
            Guid domainId = getUserDomainId();
            ExcelProcessingResultDto result = await _excelUploadService.UploadContractSupplierNameExcelFileAsync(excelFile, domainId);
            _logger.LogDebug($"Uplaoded Venue Name File...");
            return Ok(result);
        }

         [HttpPost]
        [Route("api/v1/upload-excel-contract-department-name")]
        [ValidateModelState]
        [ApiAuthorization(Name = "UPLOAD_CONTRACT_DEPARTMENT_NAME_EXCEL")]
        [SwaggerOperation("UploadExcelContractDepartmentName")]
        [SwaggerResponse(statusCode: 201, type: typeof(ExcelProcessingResultDto), description: "Excel Uploaded successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> UploadContractDepartmentName([Required] IFormFile excelFile)
        {
            _logger.LogDebug($"Uplaoding Venue Name Excel File...");
            if (excelFile == null || excelFile.Length == 0 || !Path.GetExtension(excelFile.FileName).Equals(".xlsx"))
            {
                _logger.LogError("Invalid Excel file.");
                throw new BadRequestCustomException("Invalid Excel file.", "Invalid Excel file.");
            }
            Guid domainId = getUserDomainId();
            ExcelProcessingResultDto result = await _excelUploadService.UploadContractDepartmentNameExcelFileAsync(excelFile, domainId);
            _logger.LogDebug($"Uplaoded Venue Name File...");
            return Ok(result);
        }

          [HttpPost]
        [Route("api/v1/upload-excel-contract-region-name")]
        [ValidateModelState]
        [ApiAuthorization(Name = "UPLOAD_CONTRACT_REGION_NAME_EXCEL")]
        [SwaggerOperation("UploadExcelContractRegionName")]
        [SwaggerResponse(statusCode: 201, type: typeof(ExcelProcessingResultDto), description: "Excel Uploaded successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> UploadContractRegionName([Required] IFormFile excelFile)
        {
            _logger.LogDebug($"Uplaoding Venue Name Excel File...");
            if (excelFile == null || excelFile.Length == 0 || !Path.GetExtension(excelFile.FileName).Equals(".xlsx"))
            {
                _logger.LogError("Invalid Excel file.");
                throw new BadRequestCustomException("Invalid Excel file.", "Invalid Excel file.");
            }
            Guid domainId = getUserDomainId();
            ExcelProcessingResultDto result = await _excelUploadService.UploadContractRegionNameExcelFileAsync(excelFile, domainId);
            _logger.LogDebug($"Uplaoded Venue Name File...");
            return Ok(result);
        }

          [HttpPost]
        [Route("api/v1/upload-excel-contract-comodities-name")]
        [ValidateModelState]
        [ApiAuthorization(Name = "UPLOAD_CONTRACT_COMODITIES_NAME_EXCEL")]
        [SwaggerOperation("UploadExcelContractComoditiesName")]
        [SwaggerResponse(statusCode: 201, type: typeof(ExcelProcessingResultDto), description: "Excel Uploaded successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> UploadContractComoditiesName([Required] IFormFile excelFile)
        {
            _logger.LogDebug($"Uplaoding Venue Name Excel File...");
            if (excelFile == null || excelFile.Length == 0 || !Path.GetExtension(excelFile.FileName).Equals(".xlsx"))
            {
                _logger.LogError("Invalid Excel file.");
                throw new BadRequestCustomException("Invalid Excel file.", "Invalid Excel file.");
            }
            Guid domainId = getUserDomainId();
            ExcelProcessingResultDto result = await _excelUploadService.UploadContractComoditiesNameExcelFileAsync(excelFile, domainId);
            _logger.LogDebug($"Uplaoded Venue Name File...");
            return Ok(result);
        }
        [HttpPost]
        [Route("api/v1/upload-excel-legal-material-group")]
        [ValidateModelState]
        [ApiAuthorization(Name = "UPLOAD_LEGAL_MATERIAL_GROUP_EXCEL")]
        [SwaggerOperation("UploadExcelLegalMaterialGroup")]
        [SwaggerResponse(statusCode: 201, type: typeof(ExcelProcessingResultDto), description: "Excel Uploaded successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> UploadLegalMaterialGroup([Required] IFormFile excelFile)
        {
            _logger.LogDebug($"Uplaoding Venue Name Excel File...");
            if (excelFile == null || excelFile.Length == 0 || !Path.GetExtension(excelFile.FileName).Equals(".xlsx"))
            {
                _logger.LogError("Invalid Excel file.");
                throw new BadRequestCustomException("Invalid Excel file.", "Invalid Excel file.");
            }
            Guid domainId = getUserDomainId();
            ExcelProcessingResultDto result = await _excelUploadService.UploadLegalMaterialCodeExcelFileAsync(excelFile, domainId);
            _logger.LogDebug($"Uplaoded Venue Name File...");
            return Ok(result);
        }


    }
}

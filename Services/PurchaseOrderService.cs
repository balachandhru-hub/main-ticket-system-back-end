using Contracts;
using Contracts.IRepository;
using Contracts.IServices;
using Dto;
using Entities;
using Entities.Common;
using Entities.Models;
using ExceptionHandler;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ClosedXML.Excel;
using NPOI.SS.Formula.Functions;

namespace Services
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerManager _logger;
        private readonly IAribaService _aribaService;
        private readonly IBulkInsertHelper _bulkInsertHelper;

        public PurchaseOrderService(
            IRepositoryWrapper repositoryWrapper,
            ILoggerManager logger,
            IAribaService aribaService,
            IBulkInsertHelper bulkInsertHelper)
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
            _aribaService = aribaService;
            _bulkInsertHelper = bulkInsertHelper;
        }

        public List<string> GetPurchaseOrderIds(Guid domainId, int index, int limit, string? searchTerm, PurchaseOrderRqMode? mode = null)
        {
            _logger.LogInfo($"[START] Fetching PurchaseOrders - Page: {index}, Limit: {limit}, Search: {searchTerm}");

            List<string> data = _repositoryWrapper.PurchaseOrder.GetPurchaseOrderIds(domainId, index, limit, searchTerm, mode);

            if (data.Count < 1)
            {
                _logger.LogError($"PurchaseOrders not found.");
                throw new NotFoundCustomException("PurchaseOrders Not Found", $"PurchaseOrders for this user is not found.");
            }

            _logger.LogInfo($"[END] Returned {data.Count} PurchaseOrderIds");
            return data;
        }

        /// <summary>
        /// Creates the Purchase Order Record from Excel
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public Guid CreatePurchaseOrder(POExcelExtractDto dto)
        {
            _logger.LogInfo($"Create the Purchase Order Record for EPID : {dto.EPID}");
            PurchaseOrder existingOrder = _repositoryWrapper.PurchaseOrder.FindFirstByCondition(x => x.IsActive && x.PurchaseOrderId == dto.POOrderId && x.EPID == dto.EPID && x.MaterialCode == dto.MaterialCode && x.LineItem == dto.LineItem && x.UnitOfMeasure == dto.UnitOfMeasure);

            if (existingOrder != null)
            {
                PurchaseOrder purchaseOrder = new PurchaseOrder
                {
                    Id = existingOrder.Id,
                    PurchaseOrderId = dto.POOrderId,
                    EPID = dto.EPID,
                    LineItem = dto.LineItem,
                    UnitOfMeasure = dto.UnitOfMeasure,
                    MaterialCode = dto.MaterialCode,
                    DomainId = dto.DomainId,
                };
                _repositoryWrapper.PurchaseOrder.Create(purchaseOrder);
                _repositoryWrapper.Save();
                return purchaseOrder.Id;
            }
            return existingOrder!.Id;
        }

        /// <summary>
        /// Optimized batch processing method for bulk uploads
        /// </summary>
        public async Task<BatchProcessingResultDto> CreatePurchaseOrdersBatchAsync(List<POExcelExtractDto> dtos)
        {
            _logger.LogInfo($"Batch creating {dtos.Count} Purchase Order records");

            BatchProcessingResultDto result = new BatchProcessingResultDto();
            List<PurchaseOrder> newOrders = new List<PurchaseOrder>();
            List<PurchaseOrder> ordersToUpdate = new List<PurchaseOrder>();

            try
            {
                var poKeys = dtos
                    .Select(t => $"{t.POOrderId}|{t.EPID}|{t.LineItem}|{t.DomainId}")
                    .Distinct()
                    .ToList();

                List<PurchaseOrder> existingOrders = new List<PurchaseOrder>();
                int batchSize = 1000; // tune this based on your DB performance

                for (int i = 0; i < poKeys.Count; i += batchSize)
                {
                    var batch = poKeys.Skip(i).Take(batchSize).ToList();

                    var batchQuery = _repositoryWrapper.PurchaseOrder.FindByCondition(x =>
                        x.IsActive &&
                        batch.Contains(x.PurchaseOrderId + "|" + x.EPID + "|" + x.LineItem + "|" + x.DomainId)
                    );

                    var batchResult = await batchQuery.ToListAsync();
                    existingOrders.AddRange(batchResult);
                }

                Dictionary<string, PurchaseOrder> existingOrdersLookup = existingOrders
                    .GroupBy(x => $"{x.PurchaseOrderId}|{x.EPID}|{x.LineItem}|{x.DomainId}")
                    .ToDictionary(
                        g => g.Key,
                        g => g.OrderByDescending(x => x.DateCreated).FirstOrDefault() ?? g.First()
                    );

                foreach (var dto in dtos)
                {
                    try
                    {
                        string lookupKey = $"{dto.POOrderId}|{dto.EPID}|{dto.LineItem}|{dto.DomainId}";

                        if (existingOrdersLookup.TryGetValue(lookupKey, out PurchaseOrder existingOrder) && existingOrder.DomainId == dto.DomainId)
                        {

                            if (existingOrder.PreviousAcceptedQuantity != dto.AcceptedQty)
                            {
                                existingOrder.PreviousAcceptedQuantity = dto.AcceptedQty;
                            }

                            if (existingOrder.UnitOfMeasure != dto.UnitOfMeasure)
                            {
                                existingOrder.UnitOfMeasure = dto.UnitOfMeasure;
                            }

                            if (existingOrder.MaterialCode != dto.MaterialCode)
                            {
                                existingOrder.MaterialCode = dto.MaterialCode;
                            }
                            if (existingOrder.Quantity != dto.Quantity)
                            {
                                existingOrder.Quantity = dto.Quantity;
                            }
                            if (existingOrder.HardReprocess != dto.HardReprocess)
                            {
                                existingOrder.HardReprocess = dto.HardReprocess;
                            }

                            existingOrder.Status = dto.Status;
                            existingOrder.ProcessedState = dto.ProcessedState;
                            if (existingOrder.SuccessCloudERPId == null || string.IsNullOrEmpty(existingOrder.SuccessCloudERPId))
                            {
                                existingOrder.ERPReceiptId = null;
                                existingOrder.ProcessedReceiptId = null;
                                existingOrder.ErrorComment = null;
                            }

                            ordersToUpdate.Add(existingOrder);
                            _logger.LogDebug($"Marked order for update: {lookupKey}");

                        }
                        else
                        {
                            PurchaseOrder purchaseOrder = new PurchaseOrder
                            {
                                Id = Guid.NewGuid(),
                                PurchaseOrderId = dto.POOrderId,
                                EPID = dto.EPID,
                                LineItem = dto.LineItem,
                                UnitOfMeasure = dto.UnitOfMeasure,
                                MaterialCode = dto.MaterialCode,
                                DomainId = dto.DomainId,
                                PreviousAcceptedQuantity = dto.AcceptedQty,
                                ReceiptId = dto.ReceiptID,
                                Quantity = dto.Quantity,
                                Status = dto.Status,
                                ProcessedState = dto.ProcessedState,
                                HardReprocess = dto.HardReprocess
                            };
                            newOrders.Add(purchaseOrder);
                        }

                        result.SuccessfulCount++;
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount++;
                        result.Errors.Add(new ErrorResponseDto
                        {
                            StatusCode = 500,
                            Message = "Error processing row",
                            Description = $"Error processing row {dto.RowNumber}: {ex.Message}"
                        });
                        _logger.LogError($"Error processing row {dto.RowNumber}: {ex.Message}");
                    }
                }

                // Step 4: Bulk insert all new orders in one transaction
                if (newOrders.Any())
                {
                    newOrders = newOrders.GroupBy(t => new { t.PurchaseOrderId, t.EPID, t.LineItem, t.DomainId })
                                                    .Select(g => g.Last())
                                                    .ToList();
                    _logger.LogInfo($"Bulk inserting {newOrders.Count} new purchase orders");
                    await _bulkInsertHelper.BulkInsertOrUpdateSafeAsync(newOrders);
                }

                if (ordersToUpdate.Any())
                {
                    _logger.LogInfo($"Bulk Updating {ordersToUpdate.Count}");
                    _repositoryWrapper.PurchaseOrder.UpdateRange(ordersToUpdate);
                }

                await _repositoryWrapper.SaveAsync();

                _logger.LogInfo($"Batch processing completed. Success: {result.SuccessfulCount}, Failed: {result.FailedCount}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Critical error in batch processing: {ex.Message}");
                throw;
            }
        }

        public List<PurchaseOrderDto>? GetPurchaseOrderByIds(PurchaseOrderRqDto purchaseOrderRq, Guid domainId)
        {
            _logger.LogInfo($"[START] Fetching PurchaseOrder - {string.Join(",", purchaseOrderRq.PurchaseOrderIds)}");

            List<PurchaseOrderDto>? data = _repositoryWrapper.PurchaseOrder.GetPurchaseOrderByIds(purchaseOrderRq, domainId);

            if (data.Count < 1)
            {
                _logger.LogError($"PurchaseOrders not found.");
                throw new NotFoundCustomException("PurchaseOrders Not Found", $"PurchaseOrders for this user is not found.");
            }

            _logger.LogInfo($"Successfully fetched PurchaseOrder: {string.Join(",", purchaseOrderRq.PurchaseOrderIds)}");
            return data;
        }

        /// <summary>
        /// Get the Reciept by Purchase Id
        /// </summary>
        /// <param name="purchaseId"></param>
        /// <returns></returns>
        public List<string> GetRecieptByPurchaseId(List<string> purchaseId)
        {
            _logger.LogInfo("Fetching the Reciept for the Given Purchase Ids");
            List<string>? result = _repositoryWrapper.PurchaseOrder.GetRecieptByPurchaseIds(purchaseId);
            if (!result.Any())
            {
                _logger.LogError("Unable to find any reciepts to the given purchase ids");
                throw new NotFoundCustomException("Unable to Find Any Reciept", "Unable to Find Any Reciept");
            }
            _logger.LogInfo("Fetched the Reciept for the Given Purchase Ids");
            return result;
        }

        public async Task PostPoDetailsToAriba(List<PuchaseOrderPostRqDto> poRequest, Guid domainId, bool isReversible, DateOnly? date)
        {
            _logger.LogInfo("Fetching the Reciept for the Given Purchase Ids");

            var repoData = _repositoryWrapper.PurchaseOrder.FindByCondition(t => poRequest.Select(t => t.PurchaseOrderId).Contains(t.PurchaseOrderId)).ToList();

            var aribaConfig = _repositoryWrapper.DomainSetting.FindFirstByCondition(t => t.DomainId == domainId && t.SettingsKey == Common.DOMAIN_SETTINGS_ARIBA_PURCHASE_ORDER_CONFIG);

            if (aribaConfig != null)
            {
                var config = JsonConvert.DeserializeObject<AribaPurchaseOrderSettings>(aribaConfig.SettingsValue);

                List<PurchaseOrderDto> poDetails = repoData
                    .GroupBy(t => new { t.PurchaseOrderId, t.EPID })
                    .Select(g => new PurchaseOrderDto
                    {
                        PurchaseOrderId = g.Key.PurchaseOrderId,
                        RecievableId = g.Key.EPID,
                        LineItems = g.Select(li => new PurchaseOrderLineItem
                        {
                            LineItem = li.LineItem,
                            UnitOfMeasure = li.UnitOfMeasure,
                            MaterialCode = li.MaterialCode,
                            Quantity = li.Quantity,
                            PreviousAcceptedQuantity = li.PreviousAcceptedQuantity,
                            ReceiptId = li.ReceiptId,
                            ProcessedReceiptId = li.ProcessedReceiptId,
                            ERPReceiptId = li.ERPReceiptId,
                            ErrorComment = li.ErrorComment
                        }).ToList()
                    }).ToList();

                foreach (var reqItem in poDetails)
                {
                    string? errorComment = null;

                    var matchingRequest = poRequest
                        .Where(t => t.PurchaseOrderId == reqItem.PurchaseOrderId)
                        .SelectMany(t => t.LineItems)
                        .ToList();

                    List<PurchaseOrderRqToAribaDto> aribaRequests = new();

                    if (isReversible)
                    {
                        var acceptedReq = new PurchaseOrderRqToAribaDto
                        {
                            PurchaseOrderId = reqItem.PurchaseOrderId,
                            Date = date,
                            ReceivableId = reqItem.RecievableId,
                            AribaEndPoint = config.aribaUrl,
                            Partition = config.Partition,
                            Variant = config.Variant,
                            Password = config.Password,
                            Username = config.UserName,
                            Server = config.Server,
                            LineItems = matchingRequest
                                .Where(li => li.AcceptedQuantity > 0)
                                .Select(li => new PurchaseOrderLineItemRq
                                {
                                    AcceptedQuantity = li.AcceptedQuantity,
                                    LineItem = li.LineItem,
                                }).ToList()
                        };

                        var rejectedReq = new PurchaseOrderRqToAribaDto
                        {
                            PurchaseOrderId = reqItem.PurchaseOrderId,
                            ReceivableId = reqItem.RecievableId,
                            Date = date,
                            AribaEndPoint = config.aribaUrl,
                            Partition = config.Partition,
                            Variant = config.Variant,
                            Password = config.Password,
                            Username = config.UserName,
                            Server = config.Server,
                            LineItems = matchingRequest
                                .Where(li => li.AcceptedQuantity > 0)
                                .Select(li => new PurchaseOrderLineItemRq
                                {
                                    AcceptedQuantity = li.AcceptedQuantity.HasValue ? -li.AcceptedQuantity.Value : null,
                                    LineItem = li.LineItem,
                                }).ToList()
                        };

                        if (rejectedReq.LineItems.Any())
                            aribaRequests.Add(rejectedReq);
                        if (acceptedReq.LineItems.Any())
                            aribaRequests.Add(acceptedReq);
                    }
                    else
                    {
                        var aribaReq = new PurchaseOrderRqToAribaDto()
                        {
                            PurchaseOrderId = reqItem.PurchaseOrderId,
                            ReceivableId = reqItem.RecievableId,
                            Date = date,
                            AribaEndPoint = config.aribaUrl,
                            Partition = config.Partition,
                            Variant = config.Variant,
                            Password = config.Password,
                            Username = config.UserName,
                            Server = config.Server,
                            LineItems = matchingRequest
                                .Select(li => new PurchaseOrderLineItemRq
                                {
                                    AcceptedQuantity = li.AcceptedQuantity,
                                    LineItem = li.LineItem,
                                })
                                .ToList()
                        };

                        aribaRequests.Add(aribaReq);
                    }

                    ExternalReceiptImportReply ret = new ExternalReceiptImportReply();
                    try
                    {
                        foreach (var aribaReq in aribaRequests)
                        {
                            ret = await _aribaService.PostPoDetailsToAriba(aribaReq);
                            if (isReversible)
                            {
                                await Task.Delay(2000);
                            }

                            if (ret?.Items != null)
                            {
                                foreach (var item in ret.Items)
                                {
                                    var recordsToUpdate = repoData.Where(t =>
                                        t.PurchaseOrderId == reqItem.PurchaseOrderId).ToList();

                                    foreach (var record in recordsToUpdate)
                                    {
                                        record.ERPReceiptId = item?.ERPReceiptNumber;
                                        record.ProcessedReceiptId = item?.UniqueName;
                                        record.ErrorComment = errorComment;
                                        record.PreviousAcceptedQuantity = (errorComment == null && !isReversible) ? aribaReq.LineItems?.FirstOrDefault(x => x.LineItem == record.LineItem)?.AcceptedQuantity : record.PreviousAcceptedQuantity;
                                    }
                                }
                            }
                        }

                        var updatedRecords = repoData.Where(t => t.PurchaseOrderId == reqItem.PurchaseOrderId).ToList();
                        _repositoryWrapper.PurchaseOrder.UpdateRange(updatedRecords);
                    }
                    catch (Exception ex)
                    {
                        errorComment = ex.Message;
                        _logger.LogError("Error occured for line items: " + ex.Message);

                        var recordsToUpdate = repoData.Where(t => t.PurchaseOrderId == reqItem.PurchaseOrderId).ToList();
                        foreach (var record in recordsToUpdate)
                        {
                            record.ErrorComment = errorComment;
                        }

                        _repositoryWrapper.PurchaseOrder.UpdateRange(recordsToUpdate);
                        continue;
                    }
                }

                // Save all changes to database
                await _repositoryWrapper.SaveAsync();
                _logger.LogInfo("Fetched the Receipt for the Given Purchase Ids");
            }
            else
            {
                _logger.LogError("Settings is missed");
                throw new NotFoundCustomException("Domain settings not found", "Domain Settings not found for ariba");
            }
        }

        /// <summary>
        /// Blocks or Unblocks the Purchase Order
        /// </summary>
        /// <param name="PurchaseOrder"></param>
        /// <returns></returns>
        public void BlockOrUnblockPO(List<string> purchaseOrder, Guid domainId, bool isBlocked)
        {
            _logger.LogInfo($"Blocking or Unblocking the Purchase Order for Domain with Id : {domainId}");
            List<PurchaseOrder>? purchaseOrders = _repositoryWrapper.PurchaseOrder.FindByCondition(x => x.IsActive && x.DomainId == domainId && purchaseOrder.Contains(x.PurchaseOrderId)).ToList();

            if (!purchaseOrders.Any())
            {
                _logger.LogInfo($"Unable to Find Any Purchase Order for domain : {domainId}");
                throw new NotFoundCustomException("Unable to Find Any Purchase Order", "Unable to Find Any Purchase Order");
            }

            purchaseOrders.ForEach(po =>
            {
                po.IsBlocked = isBlocked;
                po.Counter = isBlocked ? po.Counter : 0;
                po.ReceiptId = !isBlocked ? po.ProcessedReceiptId : po.ReceiptId;
                po.ProcessedReceiptId = !isBlocked ? null : po.ProcessedReceiptId;
                po.ERPReceiptId = !isBlocked ? null : po.ERPReceiptId;
            });
            _repositoryWrapper.PurchaseOrder.UpdateRange(purchaseOrders);
            _repositoryWrapper.Save();
            _logger.LogInfo($"Blocked or Unblocked the Purchase Order for Domain with Id : {domainId}");

        }

        /// <summary>
        /// Fetches the blocked or unblocked Purchase Orders
        /// </summary>
        /// <param name="index"></param>
        /// <param name="limit"></param>
        /// <param name="domainId"></param>
        /// <param name="isBlocked"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundCustomException"></exception>
        public List<string> GetBlockOrUnblockedPurchaseOrder(int index, int limit, Guid domainId, bool isBlocked, string? searchTerm)
        {
            _logger.LogInfo($"Fetching the Blocked or Unblocked Purchase Order for Domain : {domainId}");
            List<string>? purchaseOrders = _repositoryWrapper.PurchaseOrder.FindByCondition(x => x.IsActive && x.DomainId == domainId && x.IsBlocked == isBlocked && (string.IsNullOrEmpty(searchTerm) || x.PurchaseOrderId.Contains(searchTerm))).OrderByDescending(x => x.DateUpdated).Select(x => x.PurchaseOrderId).Distinct().Skip(index).Take(limit).ToList();
            if (!purchaseOrders.Any())
            {
                _logger.LogInfo($"Unable to Find Any Purchase Order for domain : {domainId}");
                throw new NotFoundCustomException("Unable to Find Any Purchase Order", "Unable to Find Any Purchase Order");
            }
            _logger.LogInfo($"Fetched the Blocked or Unblocked Purchase Order for Domain : {domainId}");
            return purchaseOrders;
        }

        /// <summary>
        /// Fetches the List of Batch Id
        /// </summary>
        /// <param name="index"></param>
        /// <param name="limit"></param>
        /// <param name="domainId"></param>
        /// <returns></returns>
        public List<int> GetBatchId(int index, int limit, Guid domainId)
        {
            _logger.LogInfo($"Fetching the Batch Ids for Domain : {domainId}");
            List<int>? batchIds = _repositoryWrapper.PurchaseOrderLog.FindByCondition(x => x.IsActive && x.DomainId == domainId).Select(x => x.BatchId).Distinct().OrderByDescending(x => x).Skip(index).Take(limit).ToList();
            if (!batchIds.Any())
            {
                _logger.LogInfo($"Unable to Find Any Batch Id for domain : {domainId}");
                throw new NotFoundCustomException("Unable to Find Any Batch Id", "Unable to Find Any Batch Id");
            }
            _logger.LogInfo($"Fetched the Batch Ids for Domain : {domainId}");
            return batchIds;
        }

        /// <summary>
        /// Fetches the List of Purchase Order Log Result by Batch Id
        /// </summary>
        /// <param name="index"></param>
        /// <param name="limit"></param>
        /// <param name="domainId"></param>
        /// <param name="batchId"></param>
        /// <returns></returns>
        public List<PurchaseOrderLogResultDto> GetBatchIdResult(
    int index,
    int limit,
    Guid domainId,
    int batchId,
    string? purchaseOrderId)
        {
            _logger.LogInfo($"Fetching the Purchase Order Log Result for Domain : {domainId} and Batch Id : {batchId}");

            IQueryable<PurchaseOrderLog> query = _repositoryWrapper.PurchaseOrderLog.FindByCondition(
                x => x.IsActive && x.DomainId == domainId && x.BatchId == batchId
            );

            if (!string.IsNullOrWhiteSpace(purchaseOrderId))
            {
                string searchTerm = purchaseOrderId.Trim().ToLower();
                query = query.Where(x => x.PurchaseOrderId.ToLower().Contains(searchTerm));
            }

            List<PurchaseOrderLogResultDto> batchIdResult = query
                .OrderByDescending(x => x.DateUpdated)
                .Skip(index)
                .Take(limit)
                .Select(x => new PurchaseOrderLogResultDto
                {
                    Id = x.Id,
                    PurchaseOrderId = x.PurchaseOrderId,
                    LineItemId = x.LineItemId,
                    Result = x.Result,
                })
                .ToList();

            if (!batchIdResult.Any())
            {
                _logger.LogInfo($"Unable to Find Any Purchase Order Log Result for domain : {domainId} and Batch Id : {batchId}");
                throw new NotFoundCustomException("Unable to Find Any Purchase Order Log Result", "Unable to Find Any Purchase Order Log Result");
            }

            _logger.LogInfo($"Fetched the Purchase Order Log Result for Domain : {domainId} and Batch Id : {batchId}");
            return batchIdResult;
        }

        /// <summary>
        /// Optimized batch processing method for bulk uploads
        /// </summary>
        public async Task<BatchProcessingResultDto> CreatePurchaseOrdersMasterBatchAsync(List<POMasterExcelExtractDto> dtos)
        {
            _logger.LogInfo($"Batch creating {dtos.Count} Purchase Order Master records");

            BatchProcessingResultDto result = new BatchProcessingResultDto();
            List<PurchaseOrderMaster> newPOMasterDataList = new List<PurchaseOrderMaster>();
            List<PurchaseOrderMaster> poMasterDataToUpdate = new List<PurchaseOrderMaster>();

            try
            {
                List<string> poOrderIds = dtos.Select(k => k.POOrderId).Distinct().ToList();
                List<string> epids = dtos.Select(k => k.EPID).Distinct().ToList();
                List<string> lineItems = dtos.Select(k => k.LineItem).Distinct().ToList();

                IQueryable<PurchaseOrderMaster> poMasterQuery = _repositoryWrapper.PurchaseOrderMaster.FindByCondition(x => x.IsActive &&
                    poOrderIds.Contains(x.PurchaseOrderId) &&
                    epids.Contains(x.EPID) &&
                    lineItems.Contains(x.LineItem));

                List<PurchaseOrderMaster> existingPOMasterData = await poMasterQuery.ToListAsync();

                Dictionary<string, PurchaseOrderMaster> existingPOMasterLookup = existingPOMasterData.ToDictionary(x =>
                    $"{x.PurchaseOrderId}|{x.EPID}|{x.LineItem}|{x.DomainId}");

                HashSet<string> processedPOMasterKeys = new HashSet<string>();

                foreach (var dto in dtos)
                {
                    try
                    {
                        string poMasterLookupKey = $"{dto.POOrderId}|{dto.EPID}|{dto.LineItem}|{dto.DomainId}";

                        if (!processedPOMasterKeys.Contains(poMasterLookupKey))
                        {
                            processedPOMasterKeys.Add(poMasterLookupKey);

                            if (existingPOMasterLookup.TryGetValue(poMasterLookupKey, out PurchaseOrderMaster existingPOMaster) && existingPOMaster.DomainId == dto.DomainId)
                            {
                                existingPOMaster.Quantity = dto.Quantity;
                                existingPOMaster.UnitOfMeasure = dto.UnitOfMeasure;
                                existingPOMaster.MaterialCode = dto.MaterialCode;
                                existingPOMaster.RequisitionRequester = dto.RequisitionRequester;
                                existingPOMaster.AccountCategory = dto.AccountCategory;
                                existingPOMaster.CostCenter = dto.CostCenter;
                                existingPOMaster.CompanyCode = dto.CompanyCode;
                                existingPOMaster.ContractId = dto.ContractId;
                                existingPOMaster.Description = dto.Description;
                                existingPOMaster.ERPCommodityCode = dto.ERPCommodityCode;
                                existingPOMaster.ExternalLineNumber = dto.ExternalLineNumber;
                                existingPOMaster.GLAccount = dto.GLAccount;
                                existingPOMaster.ItemCategory = dto.ItemCategory;
                                existingPOMaster.LineTypeCategory = dto.LineTypeCategory;
                                existingPOMaster.NeedByDate = dto.NeedByDate;
                                existingPOMaster.ParentLineNumber = dto.ParentLineNumber;
                                existingPOMaster.Price = dto.Price;
                                existingPOMaster.PriceCurrency = dto.PriceCurrency;
                                existingPOMaster.PurchaseGroup = dto.PurchaseGroup;
                                existingPOMaster.Version = dto.Version;
                                existingPOMaster.PurchaseOrg = dto.PurchaseOrg;
                                existingPOMaster.RequisitionId = dto.RequisitionId;
                                existingPOMaster.ServiceEndDate = dto.ServiceEndDate;
                                existingPOMaster.ServiceStartDate = dto.ServiceStartDate;
                                existingPOMaster.SupplierId = dto.SupplierId;
                                existingPOMaster.SupplierName = dto.SupplierName;
                                existingPOMaster.TaxAmountInBaseCurrency = dto.TaxAmountInBaseCurrency;
                                existingPOMaster.TaxCode = dto.TaxCode;
                                existingPOMaster.TaxPercentage = dto.TaxPercentage;
                                existingPOMaster.UnitPrice = dto.UnitPrice;
                                existingPOMaster.UnitPriceCurrency = dto.UnitPriceCurrency;
                                existingPOMaster.UnitPriceInBasePrice = dto.UnitPriceInBasePrice;
                                existingPOMaster.CommodityCode = dto.CommodityCode;
                                existingPOMaster.AssetCode = dto.AssetCode;
                                existingPOMaster.HeaderTittle = dto.HeaderTitle;
                                existingPOMaster.OrderDate = dto.OrderedDate;
                                existingPOMaster.Status = dto.POStatus;

                                poMasterDataToUpdate.Add(existingPOMaster);
                                _logger.LogDebug($"Marked PO Master Data for update: {poMasterLookupKey}");

                            }
                            else
                            {
                                PurchaseOrderMaster poMaster = new PurchaseOrderMaster
                                {
                                    Id = Guid.NewGuid(),
                                    PurchaseOrderId = dto.POOrderId,
                                    EPID = dto.EPID,
                                    LineItem = dto.LineItem,
                                    Quantity = dto.Quantity,
                                    UnitOfMeasure = dto.UnitOfMeasure,
                                    MaterialCode = dto.MaterialCode,
                                    DomainId = dto.DomainId,
                                    RequisitionRequester = dto.RequisitionRequester,
                                    AccountCategory = dto.AccountCategory,
                                    CostCenter = dto.CostCenter,
                                    CompanyCode = dto.CompanyCode,
                                    ContractId = dto.ContractId,
                                    Description = dto.Description,
                                    ERPCommodityCode = dto.ERPCommodityCode,
                                    ExternalLineNumber = dto.ExternalLineNumber,
                                    GLAccount = dto.GLAccount,
                                    ItemCategory = dto.ItemCategory,
                                    LineTypeCategory = dto.LineTypeCategory,
                                    NeedByDate = dto.NeedByDate,
                                    ParentLineNumber = dto.ParentLineNumber,
                                    Price = dto.Price,
                                    PriceCurrency = dto.PriceCurrency,
                                    PurchaseGroup = dto.PurchaseGroup,
                                    Version = dto.Version,
                                    PurchaseOrg = dto.PurchaseOrg,
                                    RequisitionId = dto.RequisitionId,
                                    ServiceEndDate = dto.ServiceEndDate,
                                    ServiceStartDate = dto.ServiceStartDate,
                                    SupplierId = dto.SupplierId,
                                    SupplierName = dto.SupplierName,
                                    TaxAmountInBaseCurrency = dto.TaxAmountInBaseCurrency,
                                    TaxCode = dto.TaxCode,
                                    TaxPercentage = dto.TaxPercentage,
                                    UnitPrice = dto.UnitPrice,
                                    UnitPriceCurrency = dto.UnitPriceCurrency,
                                    UnitPriceInBasePrice = dto.UnitPriceInBasePrice,
                                    CommodityCode = dto.CommodityCode,
                                    AssetCode = dto.AssetCode,
                                    HeaderTittle = dto.HeaderTitle,
                                    OrderDate = dto.OrderedDate,
                                    Status = dto.POStatus
                                };
                                newPOMasterDataList.Add(poMaster);
                            }
                        }

                        result.SuccessfulCount++;
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount++;
                        result.Errors.Add(new ErrorResponseDto
                        {
                            StatusCode = 500,
                            Message = "Error processing row",
                            Description = $"Error processing row {dto.RowNumber}: {ex.Message}"
                        });
                        _logger.LogError($"Error processing row {dto.RowNumber}: {ex.Message}");
                    }
                }

                // Step 4: Bulk insert all new orders in one transaction
                if (newPOMasterDataList.Any())
                {
                    newPOMasterDataList = newPOMasterDataList.GroupBy(t => new { t.PurchaseOrderId, t.EPID, t.LineItem, t.DomainId })
                                                    .Select(g => g.Last())
                                                    .ToList();
                    _logger.LogInfo($"Bulk inserting {newPOMasterDataList.Count} new purchase orders master");
                    await _bulkInsertHelper.BulkInsertOrUpdateSafeAsync(newPOMasterDataList);
                }

                if (poMasterDataToUpdate.Any())
                {
                    _logger.LogInfo($"Bulk Updating {poMasterDataToUpdate.Count}");
                    _repositoryWrapper.PurchaseOrderMaster.UpdateRange(poMasterDataToUpdate);
                }

                if (newPOMasterDataList.Any())
                {
                    _logger.LogInfo($"Bulk inserting {newPOMasterDataList.Count} new PO master data records");
                    await _repositoryWrapper.PurchaseOrderMaster.CreateRangeAsync(newPOMasterDataList);
                }

                if (poMasterDataToUpdate.Any())
                {
                    _logger.LogInfo($"Bulk updating {poMasterDataToUpdate.Count} PO master data records");
                    _repositoryWrapper.PurchaseOrderMaster.UpdateRange(poMasterDataToUpdate);
                }
                await _repositoryWrapper.SaveAsync();

                _logger.LogInfo($"Batch processing completed. Success: {result.SuccessfulCount}, Failed: {result.FailedCount}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Critical error in batch processing: {ex.Message}");
                throw;
            }
        }
        /// <summary>
        /// Create purchase order
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public Guid CreatePurchaseOrderManual(List<PurchaseOrderCreateDto> dtos, Guid domainId)
        {
            _logger.LogInfo($"Creating the Purchase Orders for DomainId: {domainId}");

            foreach (var dto in dtos)
            {
                PurchaseOrder existingOrder = _repositoryWrapper.PurchaseOrder
                    .FindFirstByCondition(x =>
                        x.IsActive &&
                        x.DomainId == domainId &&
                        x.PurchaseOrderId == dto.PurchaseOrderId &&
                        x.LineItem == dto.LineItem &&
                        x.EPID == dto.EPID &&
                        x.ReceiptId == dto.RecieptId);

                if (existingOrder != null)
                {
                    existingOrder.Quantity = dto.Quantity;
                    existingOrder.UnitOfMeasure = dto.UnitOfMeasure;
                    existingOrder.MaterialCode = dto.MaterialCode;
                    existingOrder.PreviousAcceptedQuantity = dto.PrevQunatity;
                    _repositoryWrapper.PurchaseOrder.Update(existingOrder);
                }
                else
                {
                    var newOrder = new PurchaseOrder
                    {
                        Id = Guid.NewGuid(),
                        DomainId = domainId,
                        PurchaseOrderId = dto.PurchaseOrderId,
                        LineItem = dto.LineItem,
                        EPID = dto.EPID,
                        ReceiptId = dto.RecieptId,
                        UnitOfMeasure = dto.UnitOfMeasure,
                        MaterialCode = dto.MaterialCode,
                        PreviousAcceptedQuantity = dto.PrevQunatity,
                        Quantity = dto.Quantity
                    };
                    _repositoryWrapper.PurchaseOrder.Create(newOrder);
                }
            }
            _repositoryWrapper.Save();

            _logger.LogInfo($"Completed creating/updating Purchase Orders for DomainId: {domainId}");

            return domainId;

        }
        public InvoicePODto? GetPODetailsByPONumber(Guid domainId, string poNumber)
        {
            _logger.LogInfo($"Fetching ContractId for Domain: {domainId}, PO Number: {poNumber}");

            PurchaseOrderMaster? poMaster = _repositoryWrapper.PurchaseOrderMaster
               .FindFirstByCondition(x => x.IsActive && x.DomainId == domainId && x.EPID == poNumber);

            if (poMaster == null)
            {
                _logger.LogError($"No PurchaseOrderMaster found for PO Number: {poNumber} in Domain: {domainId}");
                return null;
            }

            _logger.LogInfo($"Found ContractId {poMaster.ContractId} for PO Number: {poNumber}");

            InvoicePODto podetail = new InvoicePODto
            {
                ContractID = poMaster.ContractId,
                Description = poMaster.HeaderTittle
            };
            string requester = poMaster.RequisitionRequester;
            if (!String.IsNullOrEmpty(requester))
            {
                podetail.Department = _repositoryWrapper.Person.GetDepartmentDetail(requester);
            }
            return podetail;
        }



        public InvoicePODto? GetPODetailsByRequisitionsId(Guid domainId, string RequisitionsId)
        {
            _logger.LogInfo($"Fetching ContractId for Domain: {domainId},RequisitionsId: {RequisitionsId}");

            PurchaseOrderMaster? poMaster = _repositoryWrapper.PurchaseOrderMaster
               .FindFirstByCondition(x => x.IsActive && x.DomainId == domainId && x.RequisitionId == RequisitionsId);

            if (poMaster == null)
            {
                _logger.LogError($"No PurchaseOrderMaster found for RequisitionId: {RequisitionsId} in Domain: {domainId}");
                return null;
            }

            _logger.LogInfo($"Found ContractId {poMaster.ContractId} forRequisitionId: {RequisitionsId}");

            InvoicePODto podetail = new InvoicePODto
            {
                ContractID = poMaster.ContractId,
                Description = poMaster.HeaderTittle
            };
            string requester = poMaster.RequisitionRequester;
            if (!String.IsNullOrEmpty(requester))
            {
                podetail.Department = _repositoryWrapper.Person.GetDepartmentDetail(requester);
            }
            return podetail;
        }

        /// <summary>
        /// Optimized batch processing method for bulk uploads
        /// </summary>
        public async Task<BatchProcessingResultDto> CreateContractBatchAsync(List<ContractExcelExtractDto> dtos)
        {
            _logger.LogInfo($"Batch creating {dtos.Count} Contract Data records");

            BatchProcessingResultDto result = new BatchProcessingResultDto();
            List<AribaContract> newContractDataList = new List<AribaContract>();
            List<AribaContract> contractDataToUpdate = new List<AribaContract>();


            List<AribaContractDetails> newContractDetails = new List<AribaContractDetails>();
            List<AribaContractDetails> contractDetailsToUpdate = new List<AribaContractDetails>();

            // Track processed contracts to avoid duplicates
            HashSet<string> processedContractKeys = new HashSet<string>();

            try
            {
                List<string> contractIds = dtos.Select(k => k.ContractId).Distinct().ToList();

                Dictionary<string, AribaContract> existingContracts = await _repositoryWrapper.AribaContract.FindByCondition(x => x.IsActive && contractIds.Contains(x.ContractId!))
                        .ToDictionaryAsync(x => x.ContractId!);

                Dictionary<string, AribaContractDetails> existingContractDetails = await _repositoryWrapper.AribaContractDetails.FindByCondition(x => x.IsActive && contractIds.Contains(x.ContractId!))
                        .ToDictionaryAsync(x => x.ContractId!);

                foreach (var dto in dtos)
                {
                    try
                    {
                        string contractKey = dto.ContractId;

                        if (!processedContractKeys.Contains(contractKey))
                        {
                            processedContractKeys.Add(contractKey);

                            if (existingContracts.TryGetValue(contractKey, out AribaContract existingContract) && existingContract.DomainId == dto.DomainId)
                            {
                                existingContract.ContractName = dto.ContractName;
                                existingContract.BeginDate = dto.BeginDate;
                                existingContract.EndDate = dto.EndDate;
                                existingContract.TotalAmount = dto.TotalAmount;
                                existingContract.Currency = dto.Currency;

                                contractDataToUpdate.Add(existingContract);
                            }
                            else
                            {
                                AribaContract newContract = new AribaContract
                                {
                                    Id = Guid.NewGuid(),
                                    DomainId = dto.DomainId,
                                    ContractId = dto.ContractId,
                                    ContractName = dto.ContractName,
                                    BeginDate = dto.BeginDate,
                                    EndDate = dto.EndDate,
                                    TotalAmount = dto.TotalAmount,
                                    Currency = dto.Currency
                                };

                                newContractDataList.Add(newContract);
                            }

                            if (existingContractDetails.TryGetValue(contractKey, out AribaContractDetails existingDetail) && existingDetail.DomainId == dto.DomainId)
                            {
                                existingDetail.WorkspaceID = dto.WorkSpaceId;
                                existingDetail.AttachmentID = dto.AttachmentId;
                                existingDetail.MaterialCode = dto.MaterialCode;
                                existingDetail.CommodityCode = dto.CommodityCode;
                                existingDetail.Description = dto.Description;
                                existingDetail.SupplierID = dto.SupplierId;
                                existingDetail.SupplierName = dto.SupplierName;
                                existingDetail.AmountSpend = dto.AmountSpend;
                                existingDetail.BalanceAmount = dto.BalanceAmount;

                                contractDetailsToUpdate.Add(existingDetail);
                            }
                            else
                            {
                                AribaContractDetails newDetail = new AribaContractDetails
                                {
                                    Id = Guid.NewGuid(),
                                    DomainId = dto.DomainId,
                                    ContractId = dto.ContractId,
                                    WorkspaceID = dto.WorkSpaceId,
                                    AttachmentID = dto.AttachmentId,
                                    MaterialCode = dto.MaterialCode,
                                    CommodityCode = dto.CommodityCode,
                                    Description = dto.Description,
                                    SupplierID = dto.SupplierId,
                                    SupplierName = dto.SupplierName,
                                    AmountSpend = dto.AmountSpend,
                                    BalanceAmount = dto.BalanceAmount
                                };

                                newContractDetails.Add(newDetail);
                            }
                        }
                        result.SuccessfulCount++;
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount++;
                        result.Errors.Add(new ErrorResponseDto
                        {
                            StatusCode = 500,
                            Message = "Error processing row",
                            Description = $"Error processing row {dto.RowNumber}: {ex.Message}"
                        });
                        _logger.LogError($"Error processing row {dto.RowNumber}: {ex.Message}");
                    }
                }

                // Step 4: Bulk insert all new orders in one transaction
                if (newContractDataList.Any())
                {
                    _logger.LogInfo($"Bulk inserting {newContractDataList.Count} new contracts");
                    newContractDataList = newContractDataList.GroupBy(t => new { t.ContractId, t.DomainId })
                                                    .Select(g => g.Last())
                                                    .ToList();
                    _logger.LogInfo($"Bulk inserting {newContractDataList.Count} new contracts");
                    await _bulkInsertHelper.BulkInsertOrUpdateSafeAsync(newContractDataList);
                }

                if (contractDataToUpdate.Any())
                {
                    _logger.LogInfo($"Bulk updating {contractDataToUpdate.Count} contracts");
                    _repositoryWrapper.AribaContract.UpdateRange(contractDataToUpdate);
                }

                if (newContractDetails.Any())
                {
                    _logger.LogInfo($"Bulk inserting {newContractDetails.Count} new contract details");
                    await _repositoryWrapper.AribaContractDetails.CreateRangeAsync(newContractDetails);
                }

                if (contractDetailsToUpdate.Any())
                {
                    _logger.LogInfo($"Bulk updating {contractDetailsToUpdate.Count} contract details");
                    _repositoryWrapper.AribaContractDetails.UpdateRange(contractDetailsToUpdate);
                }

                await _repositoryWrapper.SaveAsync();

                _logger.LogInfo($"Batch processing completed. Success: {result.SuccessfulCount}, Failed: {result.FailedCount}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Critical error in batch processing: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Optimized batch processing method for bulk uploads
        /// </summary>
        public async Task<BatchProcessingResultDto> CreateCompanyCodeBatchAsync(List<CompanyCodeExcelDto> dtos)
        {
            _logger.LogInfo($"Batch creating {dtos.Count} Contract Data records");

            BatchProcessingResultDto result = new BatchProcessingResultDto();
            List<AribaCompanyMapping> newCompanyDataList = new List<AribaCompanyMapping>();
            List<AribaCompanyMapping> updateCompanyDataList = new List<AribaCompanyMapping>();

            // Track processed contracts to avoid duplicates
            HashSet<string> processedCompanyKeys = new HashSet<string>();

            try
            {
                List<string> companyCodes = dtos.Select(k => k.CompanyCode).Distinct().ToList();

                Dictionary<string, AribaCompanyMapping> existingCompanys = await _repositoryWrapper.AribaCompanyMapping.FindByCondition(x => x.IsActive && companyCodes.Contains(x.CompanyCode!))
                        .ToDictionaryAsync(x => x.CompanyCode!);

                foreach (var dto in dtos)
                {
                    try
                    {
                        string companyKey = dto.CompanyCode;

                        if (!processedCompanyKeys.Contains(companyKey))
                        {
                            processedCompanyKeys.Add(companyKey);

                            if (existingCompanys.TryGetValue(companyKey, out AribaCompanyMapping existingCompany) && existingCompany.DomainId == dto.DomainId)
                            {
                                existingCompany.CompanyName = dto.CompanyName;
                                updateCompanyDataList.Add(existingCompany);
                            }
                            else
                            {
                                AribaCompanyMapping newCompany = new AribaCompanyMapping
                                {
                                    Id = Guid.NewGuid(),
                                    DomainId = dto.DomainId,
                                    CompanyCode = dto.CompanyCode,
                                    CompanyName = dto.CompanyName
                                };

                                newCompanyDataList.Add(newCompany);
                            }
                        }
                        result.SuccessfulCount++;
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount++;
                        result.Errors.Add(new ErrorResponseDto
                        {
                            StatusCode = 500,
                            Message = "Error processing row",
                            Description = $"Error processing row {dto.RowNumber}: {ex.Message}"
                        });
                        _logger.LogError($"Error processing row {dto.RowNumber}: {ex.Message}");
                    }
                }

                // Step 4: Bulk insert all new orders in one transaction
                if (newCompanyDataList.Any())
                {
                    _logger.LogInfo($"Bulk inserting {newCompanyDataList.Count} new contracts");
                    await _repositoryWrapper.AribaCompanyMapping.CreateRangeAsync(newCompanyDataList);
                }

                if (updateCompanyDataList.Any())
                {
                    _logger.LogInfo($"Bulk updating {updateCompanyDataList.Count} contracts");
                    _repositoryWrapper.AribaCompanyMapping.UpdateRange(updateCompanyDataList);
                }

                await _repositoryWrapper.SaveAsync();

                _logger.LogInfo($"Batch processing completed. Success: {result.SuccessfulCount}, Failed: {result.FailedCount}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Critical error in batch processing: {ex.Message}");
                throw;
            }
        }

        public async Task<byte[]> DownloadPurchaseOrderLogWithSuccessErpIdAsync(
     Guid domainId, DateTime fromDate, DateTime toDate)
        {
            _logger.LogInfo($"[START] DownloadPurchaseOrderLogWithSuccessErpIdAsync for Domain: {domainId}, From: {fromDate}, To: {toDate}");
            if (fromDate > toDate)
            {
                _logger.LogError("FromDate cannot be after ToDate.");
                throw new PreConditionFailedCustomException("Invalid date range", "FromDate cannot be after ToDate.");
            }

            List<PurchaseOrderLog> logs = _repositoryWrapper.PurchaseOrderLog
                .FindByCondition(x => x.IsActive
                    && x.DomainId == domainId
                    && x.DateCreated >= fromDate
                    && x.DateCreated <= toDate)
                .ToList();

            _logger.LogInfo($"Fetched {logs.Count} logs for export.");

            if (logs == null || !logs.Any())
            {
                _logger.LogError("No purchase order logs found for the given date range.");
                throw new NotFoundCustomException("No Data", "No purchase order logs found for the given date range.");
            }

            List<string> poIds = logs.Select(l => l.PurchaseOrderId).Distinct().ToList();
            List<string> lineItems = logs.Select(l => l.LineItemId).Distinct().ToList();

            List<PurchaseOrder> purchaseOrders = _repositoryWrapper.PurchaseOrder
                .FindByCondition(x => x.IsActive
                    && poIds.Contains(x.PurchaseOrderId)
                    && lineItems.Contains(x.LineItem))
                .ToList();

            _logger.LogInfo($"Fetched {purchaseOrders.Count} purchase orders for join.");

            List<dynamic> exportRows = new List<dynamic>();
            foreach (PurchaseOrderLog log in logs)
            {
                List<PurchaseOrder> matchingOrders = purchaseOrders
                    .Where(po => po.PurchaseOrderId == log.PurchaseOrderId && po.LineItem == log.LineItemId)
                    .ToList();

                _logger.LogDebug($"Log PO: {log.PurchaseOrderId}, Line: {log.LineItemId}, Matches: {matchingOrders.Count}");

                string successErpId = matchingOrders.FirstOrDefault(po => !string.IsNullOrEmpty(po.SuccessCloudERPId))?.SuccessCloudERPId
                                    ?? matchingOrders.FirstOrDefault()?.SuccessCloudERPId;

                exportRows.Add(new
                {
                    PurchaseOrderId = log.PurchaseOrderId,
                    LineItemId = log.LineItemId,
                    Quantity = log.Quantity,
                    Result = log.Result,
                    SuccessCloudERPId = successErpId,
                    DateCreated = log.DateCreated,
                    BatchId = log.BatchId
                });

                _logger.LogDebug($"ExportRow - PO: {log.PurchaseOrderId}, Line: {log.LineItemId}, Qty: {log.Quantity}, Result: {log.Result}, SuccessCloudERPId: {successErpId}");
            }

            using (XLWorkbook workbook = new XLWorkbook())
            {
                IXLWorksheet ws = workbook.Worksheets.Add("PO_Log");

                ws.Cell(1, 1).Value = "PurchaseOrderId";
                ws.Cell(1, 2).Value = "LineItemId";
                ws.Cell(1, 3).Value = "Quantity";
                ws.Cell(1, 4).Value = "Result";
                ws.Cell(1, 5).Value = "SuccessCloudERPId";
                ws.Cell(1, 6).Value = "DateCreated";
                ws.Cell(1, 7).Value = "BatchId";


                for (int i = 0; i < exportRows.Count; i++)
                {
                    dynamic row = exportRows[i];
                    ws.Cell(i + 2, 1).Value = row.PurchaseOrderId ?? "";
                    ws.Cell(i + 2, 2).Value = row.LineItemId ?? "";
                    ws.Cell(i + 2, 3).Value = row.Quantity ?? "";
                    ws.Cell(i + 2, 4).Value = row.Result ?? "";
                    ws.Cell(i + 2, 5).Value = row.SuccessCloudERPId ?? "";
                    ws.Cell(i + 2, 6).Value = row.DateCreated;
                    ws.Cell(i + 2, 7).Value = row.BatchId;
                }

                using (MemoryStream stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    _logger.LogInfo($"[END] DownloadPurchaseOrderLogWithSuccessErpIdAsync - Exported {exportRows.Count} rows.");
                    return stream.ToArray();
                }
            }
        }
        public async Task<byte[]> DownloadBlockedPurchaseOrdersAsync(Guid domainId)
        {
            _logger.LogInfo($"[START] DownloadBlockedPurchaseOrdersAsync for Domain: {domainId}");

            List<PurchaseOrder> purchaseOrders = _repositoryWrapper.PurchaseOrder
                .FindByCondition(x => x.IsActive && x.DomainId == domainId && x.IsBlocked)
                .ToList();

            _logger.LogInfo($"Fetched {purchaseOrders.Count} blocked purchase orders for export.");

            if (purchaseOrders == null || purchaseOrders.Count == 0)
            {
                _logger.LogError("No blocked purchase orders found for the given domain.");
                throw new NotFoundCustomException("No Data", "No blocked purchase orders found for the given domain.");
            }

            using (XLWorkbook workbook = new XLWorkbook())
            {
                IXLWorksheet ws = workbook.Worksheets.Add("BlockedPurchaseOrders");


                ws.Cell(1, 1).Value = "PurchaseOrderId";
                ws.Cell(1, 2).Value = "IsBlocked";
                ws.Cell(1, 3).Value = "Result";

                int row = 2;
                foreach (PurchaseOrder po in purchaseOrders)
                {
                    ws.Cell(row, 1).Value = po.PurchaseOrderId ?? "";
                    ws.Cell(row, 2).Value = po.IsBlocked ? "True" : "False";
                    ws.Cell(row, 3).Value = po.ErrorComment ?? "";
                    row++;
                }

                using (MemoryStream stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    _logger.LogInfo($"[END] DownloadBlockedPurchaseOrdersAsync - Exported {purchaseOrders.Count} rows.");
                    return stream.ToArray();
                }
            }
        }
        /// <summary>
        /// Optimized batch processing method for bulk uploads
        /// </summary>
        /// 
        public async Task<BatchProcessingResultDto> CreateMaterialPRBatchAsync(List<MaterialPRExcelDto> dtos)
        {

            _logger.LogInfo($"Starting batch MaterialPR import | Total rows: {dtos.Count}");

            BatchProcessingResultDto result = new BatchProcessingResultDto();
            List<MaterialPR> newMaterialPRList = new List<MaterialPR>();
            List<MaterialPR> updateMaterialPRList = new List<MaterialPR>();

            HashSet<string> processedKeys = new HashSet<string>();

            try
            {
                List<string> PRUniqueName = dtos.Select(x => x.PRUniqueName).Where(x => x != null).Distinct().ToList();

                // Fetch existing MaterialPRs
                var existingRecords = await _repositoryWrapper.MaterialPR
                    .FindByCondition(x => x.IsActive && PRUniqueName.Contains(x.PRUniqueName))
                    .ToListAsync();

                foreach (var dto in dtos)
                {
                    try
                    {
                        string key = $"{dto.PRUniqueName}-{dto.DomainId}-{dto.LineItemNumber}";

                        if (processedKeys.Contains(key))
                            continue;

                        processedKeys.Add(key);


                        var existing = existingRecords
            .FirstOrDefault(x =>
                x.PRUniqueName == dto.PRUniqueName &&
                x.LineItemNumber == dto.LineItemNumber &&
                x.DomainId == dto.DomainId);


                        if (existing != null)
                        {
                            // Update only changed or updated fields
                            existing.PurchaseOrg = dto.PurchaseOrg;
                            existing.Description = dto.Description;
                            existing.CommodityCode = dto.Commodity;
                            existing.AccountType = Common.ACCOUNT_TYPE; // Always override
                            existing.Type = Common.MATERIAL_TYPE;
                            existing.UOM = dto.UOM;
                            existing.Amount = dto.Amount;
                            existing.DeliveryDate = dto.DeliveryDate;
                            existing.Quantity = dto.Quantity;

                            existing.Vendor = dto.Vendor;
                            existing.Contract = dto.Contract;
                            existing.Title = dto.Title;
                            existing.Currency = dto.Currency;
                            existing.RequesterName = dto.RequesterName;
                            existing.GLAccount = dto.GLAccount;
                            existing.CostCenter = dto.CostCenter;
                            existing.ShipToPlant = dto.ShipToPlant;
                            existing.CompanyCode = dto.CompanyCode;
                            existing.PurchaseGroup = dto.PurchaseGroup;
                            existing.BillTo = dto.BillTo;
                            existing.TaxCode = dto.TaxCode;
                            existing.NeedBy = dto.NeedBy;
                            existing.DeliverTo = dto.DeliverTo;
                            existing.MaterialCode = dto.MaterialCode;
                            existing.LineItemNumber = dto.LineItemNumber;
                            if (existing.Status == Common.MATERIAL_PR_STATUS_COMPOSING)
                            {
                                existing.Status = Common.MATERIAL_PR_STATUS_PENDING;

                            }

                            existing.RequesterPasswordAdapter = dto.RequesterPasswordAdapter;
                            updateMaterialPRList.Add(existing);
                        }
                        else
                        {
                            MaterialPR newItem = new MaterialPR
                            {
                                Id = Guid.NewGuid(),
                                DomainId = dto.DomainId,
                                PRNumber = dto.PRNumber,
                                PurchaseOrg = dto.PurchaseOrg,
                                Description = dto.Description,
                                CommodityCode = dto.Commodity,

                                Amount = dto.Amount,
                                DeliveryDate = dto.DeliveryDate,
                                Quantity = dto.Quantity,

                                Vendor = dto.Vendor,
                                Contract = dto.Contract,
                                Title = dto.Title,
                                Currency = dto.Currency,
                                RequesterName = dto.RequesterName,
                                GLAccount = dto.GLAccount,
                                CostCenter = dto.CostCenter,
                                ShipToPlant = dto.ShipToPlant,
                                CompanyCode = dto.CompanyCode,
                                PurchaseGroup = dto.PurchaseGroup,
                                BillTo = dto.BillTo,
                                NeedBy = dto.NeedBy,
                                TaxCode = dto.TaxCode,
                                Status = Common.STATUS_PENDING,
                                DeliverTo = dto.DeliverTo,
                                AccountType = Common.ACCOUNT_TYPE,
                                Type = Common.MATERIAL_TYPE,
                                UOM = dto.UOM,
                                PRUniqueName = dto.PRUniqueName,
                                MaterialCode = dto.MaterialCode,
                                RequesterPasswordAdapter = dto.RequesterPasswordAdapter,
                                LineItemNumber = dto.LineItemNumber




                            };

                            newMaterialPRList.Add(newItem);
                        }

                        result.SuccessfulCount++;
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount++;
                        result.Errors.Add(new ErrorResponseDto
                        {
                            StatusCode = 500,
                            Message = "Error processing row",
                            Description = $"Row {dto.RowNumber}: {ex.Message}"
                        });

                        _logger.LogError($"Error processing row {dto.RowNumber} : {ex.Message}");
                    }
                }

                // Bulk insert
                if (newMaterialPRList.Any())
                {
                    _logger.LogInfo($"Bulk insert {newMaterialPRList.Count} Material PR data");
                    await _bulkInsertHelper.BulkInsertOrUpdateSafeAsync(newMaterialPRList);
                }

                // Bulk update
                if (updateMaterialPRList.Any())
                {
                    _logger.LogInfo($"Bulk update {updateMaterialPRList.Count} Material PR data");
                    _repositoryWrapper.MaterialPR.UpdateRange(updateMaterialPRList);
                }

                await _repositoryWrapper.SaveAsync();
                _logger.LogInfo($"MaterialPR Batch Completed | Success: {result.SuccessfulCount}, Failed: {result.FailedCount}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Critical Batch Error: {ex.Message}");
                throw;
            }
        }

        public async Task<BatchProcessingResultDto> UpdateBlockOrUnblockPurchaseOrderBatchAsync(List<BlockOrUnblockDataDto> dtos)
        {
            _logger.LogInfo($"Batch Updating Block or Unblock Status for {dtos.Count} Purchase Order records");
            try
            {
                BatchProcessingResultDto result = new BatchProcessingResultDto();
                List<string> poIds = dtos.Select(k => k.POOrderId).Distinct().ToList();

                List<PurchaseOrder> existingPurchaseOrders = _repositoryWrapper.PurchaseOrder
                    .FindByCondition(x => x.IsActive && poIds.Contains(x.PurchaseOrderId))
                    .ToList();

                List<PurchaseOrder> purchaseOrdersToUpdate = new List<PurchaseOrder>();


                foreach (var dto in dtos)
                {
                    try
                    {
                        List<PurchaseOrder>? existingPurchaseOrder = existingPurchaseOrders
                            .Where(x => x.PurchaseOrderId == dto.POOrderId).ToList();
                        if (existingPurchaseOrder.Any())
                        {
                            existingPurchaseOrder.ForEach(x =>
                            {
                                x.IsBlocked = dto.IsBlocked;

                                if (!dto.IsBlocked)
                                {
                                    x.Counter = 0;
                                }
                            });
                            purchaseOrdersToUpdate.AddRange(existingPurchaseOrder);
                            result.SuccessfulCount++;
                        }
                        else
                        {
                            result.FailedCount++;
                            result.Errors.Add(new ErrorResponseDto
                            {
                                StatusCode = 404,
                                Message = "Purchase Order not found",
                                Description = $"No Purchase Order found for POOrderId {dto.POOrderId} in DomainId {dto.DomainId} at row {dto.RowNumber}"
                            });
                            _logger.LogError($"No Purchase Order found for POOrderId {dto.POOrderId} in DomainId {dto.DomainId} at row {dto.RowNumber}");
                        }
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount++;
                        result.Errors.Add(new ErrorResponseDto
                        {
                            StatusCode = 500,
                            Message = "Error processing row",
                            Description = $"Error processing row {dto.RowNumber}: {ex.Message}"
                        });
                        _logger.LogError($"Error processing row {dto.RowNumber}: {ex.Message}");
                    }
                }

                if (purchaseOrdersToUpdate.Any())
                {
                    _logger.LogInfo($"Bulk updating {purchaseOrdersToUpdate.Count} Purchase Orders");
                    _repositoryWrapper.PurchaseOrder.UpdateRange(purchaseOrdersToUpdate);
                }
                _repositoryWrapper.Save();

                _logger.LogInfo($"Batch processing completed. Success: {result.SuccessfulCount}, Failed: {result.FailedCount}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Critical error in batch processing: {ex.Message}");
                throw;
            }
        }

        public async Task<BatchProcessingResultDto> CreateServicePRBatchAsync(List<ServicePRExcelDto> dtos)
        {

            _logger.LogInfo($"Starting batch MaterialPR import | Total rows: {dtos.Count}");

            BatchProcessingResultDto result = new BatchProcessingResultDto();
            List<ServicePR> newServicePRList = new List<ServicePR>();
            List<ServicePR> updateServicePRList = new List<ServicePR>();

            HashSet<string> processedKeys = new HashSet<string>();

            try
            {
                List<string> PRServiceUniqueName = dtos.Select(x => x.PRServiceUniqueName).Where(x => x != null).Distinct().ToList();

                // Fetch existing MaterialPRs
                var existingRecords = await _repositoryWrapper.ServicePR
                    .FindByCondition(x => x.IsActive && PRServiceUniqueName.Contains(x.PRServiceUniqueName))
                    .ToListAsync();

                foreach (var dto in dtos)
                {
                    try
                    {
                        string key = $"{dto.PRServiceUniqueName}-{dto.DomainId}-{dto.LineItemNumber}";

                        if (processedKeys.Contains(key))
                            continue;

                        processedKeys.Add(key);


                        var existing = existingRecords
            .FirstOrDefault(x =>
                x.PRServiceUniqueName == dto.PRServiceUniqueName &&
                x.LineItemNumber == dto.LineItemNumber &&
                x.DomainId == dto.DomainId);


                        if (existing != null)
                        {
                            // Update only changed or updated fields
                            existing.PurchaseOrg = dto.PurchaseOrg;
                            existing.Description = dto.Description;
                            existing.CommodityCode = dto.Commodity;
                            existing.AccountType = Common.ACCOUNT_TYPE;
                            existing.Type = Common.SERVICE_TYPE;
                            existing.UOM = dto.UOM;
                            existing.Amount = dto.Amount;
                            existing.DeliveryDate = dto.DeliveryDate;
                            existing.Quantity = dto.Quantity;

                            existing.Vendor = dto.Vendor;
                            existing.Contract = dto.Contract;
                            existing.Title = dto.Title;
                            existing.Currency = dto.Currency;
                            existing.PreparerName = dto.PreparerName;
                            existing.GLAccount = dto.GLAccount;
                            existing.CostCenter = dto.CostCenter;
                            existing.ShipToPlant = dto.ShipToPlant;
                            existing.CompanyCode = dto.CompanyCode;
                            existing.PurchaseGroup = dto.PurchaseGroup;
                            existing.BillTo = dto.BillTo;
                            existing.TaxCode = dto.TaxCode;
                            existing.NeedBy = dto.NeedBy;
                            existing.DeliverTo = dto.DeliverTo;
                            existing.MaterialCode = dto.MaterialCode;

                            existing.LineItemNumber = dto.LineItemNumber;
                            if (existing.Status == Common.MATERIAL_PR_STATUS_COMPOSING)
                            {
                                existing.Status = Common.MATERIAL_PR_STATUS_PENDING;

                            }

                            existing.PreparerPasswordAdapter = dto.PreparerPasswordAdapter;
                            updateServicePRList.Add(existing);
                        }
                        else
                        {
                            ServicePR newItem = new ServicePR
                            {
                                Id = Guid.NewGuid(),
                                DomainId = dto.DomainId,
                                PRNumber = dto.PRNumber,
                                PurchaseOrg = dto.PurchaseOrg,
                                Description = dto.Description,
                                CommodityCode = dto.Commodity,

                                Amount = dto.Amount,
                                DeliveryDate = dto.DeliveryDate,
                                Quantity = dto.Quantity,

                                Vendor = dto.Vendor,
                                Contract = dto.Contract,
                                Title = dto.Title,
                                Currency = dto.Currency,
                                PreparerName = dto.PreparerName,
                                GLAccount = dto.GLAccount,
                                CostCenter = dto.CostCenter,
                                ShipToPlant = dto.ShipToPlant,
                                CompanyCode = dto.CompanyCode,
                                PurchaseGroup = dto.PurchaseGroup,
                                BillTo = dto.BillTo,
                                NeedBy = dto.NeedBy,
                                TaxCode = dto.TaxCode,
                                Status = Common.STATUS_PENDING,
                                DeliverTo = dto.DeliverTo,
                                AccountType = Common.ACCOUNT_TYPE,
                                Type = Common.SERVICE_TYPE,
                                UOM = dto.UOM,
                                PRServiceUniqueName = dto.PRServiceUniqueName,
                                MaterialCode = dto.MaterialCode,
                                PreparerPasswordAdapter = dto.PreparerPasswordAdapter,
                                LineItemNumber = dto.LineItemNumber,
                                ExpectedAmount = dto.ExpectedAmount,
                                MaxAmount = dto.MaxAmount,
                                ServiceEndDate = dto.ServiceEndDate,
                                ServiceStartDate = dto.ServiceStartDate




                            };

                            newServicePRList.Add(newItem);
                        }

                        result.SuccessfulCount++;
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount++;
                        result.Errors.Add(new ErrorResponseDto
                        {
                            StatusCode = 500,
                            Message = "Error processing row",
                            Description = $"Row {dto.RowNumber}: {ex.Message}"
                        });

                        _logger.LogError($"Error processing row {dto.RowNumber} : {ex.Message}");
                    }
                }

                // Bulk insert
                if (newServicePRList.Any())
                {
                    _logger.LogInfo($"Bulk insert {newServicePRList.Count} Service PR data");
                    await _bulkInsertHelper.BulkInsertOrUpdateSafeAsync(newServicePRList);
                }

                // Bulk update
                if (updateServicePRList.Any())
                {
                    _logger.LogInfo($"Bulk update {updateServicePRList.Count} Service PR data");
                    _repositoryWrapper.ServicePR.UpdateRange(updateServicePRList);
                }

                await _repositoryWrapper.SaveAsync();
                _logger.LogInfo($"ServicePR Batch Completed | Success: {result.SuccessfulCount}, Failed: {result.FailedCount}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Critical Batch Error: {ex.Message}");
                throw;
            }
        }

        public async Task<BatchProcessingResultDto> CreateSupplierNameBatchAsync(List<SupplierNameExcelDto> dtos)
        {
            _logger.LogInfo($"Batch creating {dtos.Count} Contract Data records");

            BatchProcessingResultDto result = new BatchProcessingResultDto();
            List<AribaSupplierName> newCompanyDataList = new List<AribaSupplierName>();
            List<AribaSupplierName> updateCompanyDataList = new List<AribaSupplierName>();

            // Track processed contracts to avoid duplicates
            HashSet<string> processedCompanyKeys = new HashSet<string>();

            try
            {
                List<string> supplierId = dtos.Select(k => k.SupplierId).Distinct().ToList();

                Dictionary<string, AribaSupplierName> existingCompanys = await _repositoryWrapper.AribaSupplierName.FindByCondition(x => x.IsActive && supplierId.Contains(x.SupplierId!))
                        .ToDictionaryAsync(x => x.SupplierId!);

                foreach (var dto in dtos)
                {
                    try
                    {
                        string companyKey = dto.SupplierId;

                        if (!processedCompanyKeys.Contains(companyKey))
                        {
                            processedCompanyKeys.Add(companyKey);

                            if (existingCompanys.TryGetValue(companyKey, out AribaSupplierName existingCompany) && existingCompany.DomainId == dto.DomainId)
                            {
                                existingCompany.SupplierName = dto.SupplierName;
                                updateCompanyDataList.Add(existingCompany);
                            }
                            else
                            {
                                AribaSupplierName newCompany = new AribaSupplierName
                                {
                                    Id = Guid.NewGuid(),
                                    DomainId = dto.DomainId,
                                    SupplierId = dto.SupplierId,
                                    SupplierName = dto.SupplierName
                                };

                                newCompanyDataList.Add(newCompany);
                            }
                        }
                        result.SuccessfulCount++;
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount++;
                        result.Errors.Add(new ErrorResponseDto
                        {
                            StatusCode = 500,
                            Message = "Error processing row",
                            Description = $"Error processing row {dto.RowNumber}: {ex.Message}"
                        });
                        _logger.LogError($"Error processing row {dto.RowNumber}: {ex.Message}");
                    }
                }

                // Step 4: Bulk insert all new orders in one transaction
                if (newCompanyDataList.Any())
                {
                    _logger.LogInfo($"Bulk inserting {newCompanyDataList.Count} new contracts");
                    await _repositoryWrapper.AribaSupplierName.CreateRangeAsync(newCompanyDataList);
                }

                if (updateCompanyDataList.Any())
                {
                    _logger.LogInfo($"Bulk updating {updateCompanyDataList.Count} contracts");
                    _repositoryWrapper.AribaSupplierName.UpdateRange(updateCompanyDataList);
                }

                await _repositoryWrapper.SaveAsync();

                _logger.LogInfo($"Batch processing completed. Success: {result.SuccessfulCount}, Failed: {result.FailedCount}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Critical error in batch processing: {ex.Message}");
                throw;
            }
        }

        public async Task<BatchProcessingResultDto> CreateVenueBatchAsync(List<VenueExcelDto> dtos)
        {
            _logger.LogInfo($"Batch creating {dtos.Count} Contract Data records");

            BatchProcessingResultDto result = new BatchProcessingResultDto();
            List<AribaVenueListMapping> newCompanyDataList = new List<AribaVenueListMapping>();
            List<AribaVenueListMapping> updateCompanyDataList = new List<AribaVenueListMapping>();

            // Track processed contracts to avoid duplicates
            HashSet<string> processedCompanyKeys = new HashSet<string>();

            try
            {
                List<string> venueName = dtos.Select(k => k.VenueName).Distinct().ToList();

                Dictionary<string, AribaVenueListMapping> existingCompanys = await _repositoryWrapper.AribaVenueListMapping.FindByCondition(x => x.IsActive && venueName.Contains(x.VenueName!))
                        .ToDictionaryAsync(x => x.VenueName!);

                foreach (var dto in dtos)
                {
                    try
                    {
                        string companyKey = dto.VenueName;

                        if (!processedCompanyKeys.Contains(companyKey))
                        {
                            processedCompanyKeys.Add(companyKey);

                            if (existingCompanys.TryGetValue(companyKey, out AribaVenueListMapping existingCompany) && existingCompany.DomainId == dto.DomainId)
                            {
                                existingCompany.VenueName = dto.VenueName;
                                updateCompanyDataList.Add(existingCompany);
                            }
                            else
                            {
                                AribaVenueListMapping newCompany = new AribaVenueListMapping
                                {
                                    Id = Guid.NewGuid(),
                                    DomainId = dto.DomainId,
                                    VenueName = dto.VenueName,
                                    CompanyCode = dto.CompanyCode,
                                    CompanyName = dto.CompanyName,
                                    CostCenter = dto.CostCenter,
                                    GLAccount = dto.GLAccount,
                                    ShipToPlant = dto.ShipToPlant,
                                    PurchaseOrg = dto.PurchaseOrg,
                                    PurchaseGroup = dto.PurchaseGroup,
                                    BillTo = dto.BillTo
                                };

                                newCompanyDataList.Add(newCompany);
                            }
                        }
                        result.SuccessfulCount++;
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount++;
                        result.Errors.Add(new ErrorResponseDto
                        {
                            StatusCode = 500,
                            Message = "Error processing row",
                            Description = $"Error processing row {dto.RowNumber}: {ex.Message}"
                        });
                        _logger.LogError($"Error processing row {dto.RowNumber}: {ex.Message}");
                    }
                }

                // Step 4: Bulk insert all new orders in one transaction
                if (newCompanyDataList.Any())
                {
                    _logger.LogInfo($"Bulk inserting {newCompanyDataList.Count} new contracts");
                    await _repositoryWrapper.AribaVenueListMapping.CreateRangeAsync(newCompanyDataList);
                }

                if (updateCompanyDataList.Any())
                {
                    _logger.LogInfo($"Bulk updating {updateCompanyDataList.Count} contracts");
                    _repositoryWrapper.AribaVenueListMapping.UpdateRange(updateCompanyDataList);
                }

                await _repositoryWrapper.SaveAsync();

                _logger.LogInfo($"Batch processing completed. Success: {result.SuccessfulCount}, Failed: {result.FailedCount}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Critical error in batch processing: {ex.Message}");
                throw;
            }
        }

            public async Task<BatchProcessingResultDto> CreateDraftMaterialPRBatchAsync(
            List<DraftMaterialPRExcelUploadDto> dtos,
            Guid userId, Guid domainId)
        {
            _logger.LogInfo($"Batch creating {dtos.Count} Draft Material PR records");

            BatchProcessingResultDto result = new();
            List<DraftMaterialPR> newPRList = new();

            try
            {
            var profile = _repositoryWrapper.Person.GetMyProfileByUserId(userId);

            if (profile == null)
            {
                _logger.LogError($"User profile not found for UserId: {userId}");
                throw new NotFoundCustomException(
                    "User profile not found",
                    $"UserId: {userId}"
                );
            }


            string? aribaUsername = profile.AribaUserName;
            
             int currentDraftNumber = _repositoryWrapper.DraftMaterialPR
            .FindByCondition(x =>
                x.IsActive &&
                x.UserId == userId &&
                x.DomainId == domainId &&
                x.DraftID != null &&
                x.DraftID.StartsWith(Common.DRAFT_ID_START_WITH_PR))
            .Select(x => x.DraftID.Substring(Common.DRAFT_ID_START_WITH_PR.Length))
            .AsEnumerable()
            .Where(x => int.TryParse(x, out _))
            .Select(int.Parse)
            .DefaultIfEmpty(0)
            .Max();   
                                     


                // 🔹 Track PRUniqueName → DraftID & LineItemNumber
                Dictionary<string, string> prDraftMap = new();
                Dictionary<string, int> prLineItemMap = new();

                foreach (var dto in dtos)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(dto.PRUniqueName))
                            throw new Exception("PRUniqueName is required");

                        if (!prDraftMap.ContainsKey(dto.PRUniqueName))
                        {
                            currentDraftNumber++;
                            string newDraftId = $"{Common.DRAFT_ID_START_WITH_PR}{currentDraftNumber:D2}";

                            prDraftMap[dto.PRUniqueName] = newDraftId;
                            prLineItemMap[dto.PRUniqueName] = 1;
                        }
                        else
                        {
                            prLineItemMap[dto.PRUniqueName]++;
                        }
                          var venueDetailsForLine = !string.IsNullOrWhiteSpace(dto.Venue)
                    ? await _repositoryWrapper.AribaVenueListMapping
                        .GetVenueDetailsByNameAsync(domainId, dto.Venue)
                    : null;
                        var restrictedEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                        {
                            Common.PR_ENTERTAINMENT_USER_EMAIL_ONE_KEY,
                            Common.PR_ENTERTAINMENT_USER_EMAIL_TWO_KEY
                        };

                        string? resolvedAribaUserId = null;

                        if (venueDetailsForLine != null &&
                            !restrictedEmails.Contains(profile.Email))
                        {
                            resolvedAribaUserId = venueDetailsForLine.AribaUserId;
                        }

                        DateTime startDate = dto.NeedBy;
                        string description = string.IsNullOrWhiteSpace(dto.Venue)
                            ? $"{dto.Vendor} {startDate:yyyy-MM-dd}"
                            : $"{dto.Vendor} {dto.Venue} {startDate:yyyy-MM-dd}";

                        DraftMaterialPR pr = new DraftMaterialPR
                        {
                            Id = Guid.NewGuid(),
                            DomainId = domainId,
                            UserId = userId,

                            DraftID = prDraftMap[dto.PRUniqueName],  
                            LineItemNumber = prLineItemMap[dto.PRUniqueName],
                            PRUniqueName = dto.PRUniqueName,
                            RequesterName=aribaUsername,
                            Venue = dto.Venue,
                            Vendor = dto.Vendor,
                            NeedBy = dto.NeedBy,
                            Quantity = dto.Quantity,
                            Amount = dto.Amount,
                            TaxInclude = dto.TaxInclude,
                            Currency = dto.Currency,
                            AutoInvoices = dto.AutoInvoices,
                            InvoiceID = dto.InvoiceID,
                            InvoiceDate = dto.InvoiceDate,
                            AutoGRN = dto.AutoGRN ?? true,
                            IsOCRAttachmentProcessed = false,
                            Description = dto.Description ?? description,
                            CompanyCode = venueDetailsForLine?.CompanyCode,
                            GLAccount = venueDetailsForLine?.GLAccount,
                            CostCenter = venueDetailsForLine?.CostCenter,
                            ShipToPlant = venueDetailsForLine?.ShipToPlant,
                            PurchaseOrg = venueDetailsForLine?.PurchaseOrg,
                            PurchaseGroup = venueDetailsForLine?.PurchaseGroup,
                            BillTo = venueDetailsForLine?.BillTo,
                            AribaUserId = resolvedAribaUserId,
                            TeamReceived = dto.TeamReceived ?? false,
                            Title = dto.Title?? description
                        };

                        newPRList.Add(pr);
                        result.SuccessfulCount++;
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount++;
                        result.Errors.Add(new ErrorResponseDto
                        {
                            StatusCode = 400,
                            Message = "Draft Material PR row failed",
                            Description = $"Row {dto.RowNumber}: {ex.Message}"
                        });

                        _logger.LogError($"Row {dto.RowNumber} failed: {ex.Message}");
                    }
                }

                if (newPRList.Any())
                {
                    await _repositoryWrapper.DraftMaterialPR.CreateRangeAsync(newPRList);
                    await _repositoryWrapper.SaveAsync();
                }

                _logger.LogInfo($"Draft Material PR batch completed. Success: {result.SuccessfulCount}, Failed: {result.FailedCount}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Critical error in Draft Material PR batch: {ex.Message}");
                throw;
            }
        }


        // Helper method to validate required columns for PR Block/Unblock
        // This can be extended in the future if more required fields are added
          public async Task<BatchProcessingResultDto> UpdatePRBlockOrUnblockPurchaseOrderBatchAsync(List<PRBlockOrUnblockDataDto> dtos)
        {
            _logger.LogInfo($"Batch Updating Block or Unblock Status for {dtos.Count} Purchase Order records");
            try
            {
                BatchProcessingResultDto result = new BatchProcessingResultDto();
                List<string> poIds = dtos.Select(k => k.POOrderId).Distinct().ToList();

                List<MaterialPR> existingPurchaseOrders = _repositoryWrapper.MaterialPR
                    .FindByCondition(x => x.IsActive && poIds.Contains(x.PONumber))
                    .ToList();

                List<MaterialPR> materialPRsToUpdate = new List<MaterialPR>();



                foreach (var dto in dtos)
                {
                    try
                    {
                        List<MaterialPR>? existingPurchaseOrder = existingPurchaseOrders
                            .Where(x => x.PONumber == dto.POOrderId).ToList();
                        if (existingPurchaseOrder.Any())
                        {
                            existingPurchaseOrder.ForEach(x =>
                            {
                                x.IsBlocked = dto.IsBlocked;

                                if (!dto.IsBlocked)
                                {
                                    x.Counter = 0;
                                }
                            });
                            materialPRsToUpdate.AddRange(existingPurchaseOrder);
                            result.SuccessfulCount++;
                        }
                        else
                        {
                            _logger.LogInfo(
                        $"POOrderId {dto.POOrderId} not found in DB. Row {dto.RowNumber} ignored.");
                    continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount++;
                        result.Errors.Add(new ErrorResponseDto
                        {
                            StatusCode = 500,
                            Message = "Error processing row",
                            Description = $"Error processing row {dto.RowNumber}: {ex.Message}"
                        });
                        _logger.LogError($"Error processing row {dto.RowNumber}: {ex.Message}");
                    }
                }

                if (materialPRsToUpdate.Any())
                {
                    _logger.LogInfo($"Bulk updating {materialPRsToUpdate.Count} Material PRs");
                    _repositoryWrapper.MaterialPR.UpdateRange(materialPRsToUpdate);
                }
                _repositoryWrapper.Save();

                _logger.LogInfo($"Batch processing completed. Success: {result.SuccessfulCount}, Failed: {result.FailedCount}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Critical error in batch processing: {ex.Message}");
                throw;
            }
        }

       public async Task<byte[]> DownloadPRBlockedPurchaseOrdersAsync(Guid domainId, bool isBlocked)
        {
            _logger.LogInfo($"[START] DownloadBlockedPurchaseOrdersAsync for Domain: {domainId}, IsBlocked: {isBlocked}");

            List<MaterialPR> purchaseOrders = _repositoryWrapper.MaterialPR
                .FindByCondition(x => x.IsActive && x.DomainId == domainId && x.IsBlocked == isBlocked)
                .ToList();

            _logger.LogInfo($"Fetched {purchaseOrders.Count} blocked purchase orders for export.");

            if (purchaseOrders == null || purchaseOrders.Count == 0)
            {
                _logger.LogError("No blocked purchase orders found for the given domain.");
                throw new NotFoundCustomException("No Data", "No blocked purchase orders found for the given domain.");
            }

            using (XLWorkbook workbook = new XLWorkbook())
            {
                IXLWorksheet ws = workbook.Worksheets.Add("BlockedPurchaseOrders");


                ws.Cell(1, 1).Value = "PurchaseOrderId";
                ws.Cell(1, 2).Value = "IsBlocked";
                ws.Cell(1, 3).Value = "Result";

                int row = 2;
                foreach (MaterialPR po in purchaseOrders)
                {
                    ws.Cell(row, 1).Value = po.PONumber ?? "";
                    ws.Cell(row, 2).Value = po.IsBlocked ? "True" : "False";
                    ws.Cell(row, 3).Value = po.ErrorComment ?? "";
                    row++;
                }

                using (MemoryStream stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    _logger.LogInfo($"[END] DownloadBlockedPurchaseOrdersAsync - Exported {purchaseOrders.Count} rows.");
                    return stream.ToArray();
                }
            }
        }
        //create or update PropertyCrossCharge in batch
        // This method will check if a PropertyCrossCharge with the same name and domain already exists:
           public async Task<BatchProcessingResultDto> CreatePropertyCrossChargeBatchAsync(List<PropertyCrossChargeExcelDto> dtos)
        {
            _logger.LogInfo($"Batch creating {dtos.Count} Contract Data records");

            BatchProcessingResultDto result = new BatchProcessingResultDto();
            List<PropertyCrossCharge> newCompanyDataList = new List<PropertyCrossCharge>();
            List<PropertyCrossCharge> updateCompanyDataList = new List<PropertyCrossCharge>();

            // Track processed contracts to avoid duplicates
            HashSet<string> processedCompanyKeys = new HashSet<string>();

            try
            {
                List<string> propertyCrossChargeName = dtos.Select(k => k.PropertyCrossChargeName).Distinct().ToList();

                Dictionary<string, PropertyCrossCharge> existingCompanys = await _repositoryWrapper.PropertyCrossCharge.FindByCondition(x => x.IsActive && propertyCrossChargeName.Contains(x.PropertyCrossChargeName!))
                        .ToDictionaryAsync(x => x.PropertyCrossChargeName!);

                foreach (var dto in dtos)
                {
                    try
                    {
                        string companyKey = dto.PropertyCrossChargeName + dto.CompanyCode + dto.CostCenter;

                        if (!processedCompanyKeys.Contains(companyKey))
                        {
                            processedCompanyKeys.Add(companyKey);

                            if (existingCompanys.TryGetValue(companyKey, out PropertyCrossCharge existingCompany) && existingCompany.DomainId == dto.DomainId)
                            {
                                existingCompany.PropertyCrossChargeName = dto.PropertyCrossChargeName;
                                updateCompanyDataList.Add(existingCompany);
                            }
                            else
                            {
                                PropertyCrossCharge newCompany = new PropertyCrossCharge
                                {
                                    Id = Guid.NewGuid(),
                                    DomainId = dto.DomainId,
                                    PropertyCrossChargeName = dto.PropertyCrossChargeName,
                                    CompanyCode = dto.CompanyCode,
                                    CompanyName = dto.CompanyName,
                                    CostCenter = dto.CostCenter,
                                    GLAccount = dto.GLAccount,
                                    ShipToPlant = dto.ShipToPlant,
                                    PurchaseOrg = dto.PurchaseOrg,
                                    PurchaseGroup = dto.PurchaseGroup,
                                    BillTo = dto.BillTo
                                };

                                newCompanyDataList.Add(newCompany);
                            }
                        }
                        result.SuccessfulCount++;
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount++;
                        result.Errors.Add(new ErrorResponseDto
                        {
                            StatusCode = 500,
                            Message = "Error processing row",
                            Description = $"Error processing row {dto.RowNumber}: {ex.Message}"
                        });
                        _logger.LogError($"Error processing row {dto.RowNumber}: {ex.Message}");
                    }
                }

                // Step 4: Bulk insert all new orders in one transaction
                if (newCompanyDataList.Any())
                {
                    _logger.LogInfo($"Bulk inserting {newCompanyDataList.Count} new contracts");
                    await _repositoryWrapper.PropertyCrossCharge.CreateRangeAsync(newCompanyDataList);
                }

                if (updateCompanyDataList.Any())
                {
                    _logger.LogInfo($"Bulk updating {updateCompanyDataList.Count} contracts");
                    _repositoryWrapper.PropertyCrossCharge.UpdateRange(updateCompanyDataList);
                }

                await _repositoryWrapper.SaveAsync();

                _logger.LogInfo($"Batch processing completed. Success: {result.SuccessfulCount}, Failed: {result.FailedCount}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Critical error in batch processing: {ex.Message}");
                throw;
            }
        }

 
    }
}



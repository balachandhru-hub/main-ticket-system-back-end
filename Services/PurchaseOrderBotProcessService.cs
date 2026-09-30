using Contracts;
using Contracts.IRepository;
using Contracts.IServices;
using Dto;
using Entities.Models;
using ExceptionHandler;
using Microsoft.EntityFrameworkCore;

namespace Services
{
    public class PurchaseOrderBotProcessService : IPurchaseOrderBotProcessService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerManager _logger;
        private readonly IBulkInsertHelper _bulkInsertHelper;

        public PurchaseOrderBotProcessService(
            IRepositoryWrapper repositoryWrapper,
            ILoggerManager logger,
            IBulkInsertHelper bulkInsertHelper)
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
            _bulkInsertHelper = bulkInsertHelper;
        }

        /// <summary>
        /// Optimized batch processing method for bulk uploads of Purchase Order Bot Process
        /// </summary>
        public async Task<BatchProcessingResultDto> CreatePurchaseOrderBotProcessBatchAsync(List<PurchaseOrderBotProcessExcelDto> dtos)
        {
            _logger.LogInfo($"Batch creating {dtos.Count} Purchase Order Bot Process records");

            BatchProcessingResultDto result = new BatchProcessingResultDto();
            List<PurchaseOrderBotProcess> newOrders = new List<PurchaseOrderBotProcess>();
            List<PurchaseOrderBotProcess> ordersToUpdate = new List<PurchaseOrderBotProcess>();

            try
            {
                var poKeys = dtos
                    .Select(t => $"{t.POOrderId}|{t.EPID}|{t.LineItem}|{t.DomainId}")
                    .Distinct()
                    .ToList();

                List<PurchaseOrderBotProcess> existingOrders = new List<PurchaseOrderBotProcess>();
                int batchSize = 1000; // tune this based on your DB performance

                for (int i = 0; i < poKeys.Count; i += batchSize)
                {
                    var batch = poKeys.Skip(i).Take(batchSize).ToList();

                    var batchQuery = _repositoryWrapper.PurchaseOrderBotProcess.FindByCondition(x =>
                        batch.Contains(x.PurchaseOrderId + "|" + x.EPID + "|" + x.LineItem + "|" + x.DomainId)
                    );

                    var batchResult = await batchQuery.ToListAsync();
                    existingOrders.AddRange(batchResult);
                }

                Dictionary<string, PurchaseOrderBotProcess> existingOrdersLookup = existingOrders
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

                        if (existingOrdersLookup.TryGetValue(lookupKey, out PurchaseOrderBotProcess existingOrder) && existingOrder.DomainId == dto.DomainId)
                        {
                            if (existingOrder.PreviousAcceptedQuantity != dto.PreviousAcceptedQuantity)
                            {
                                existingOrder.PreviousAcceptedQuantity = dto.PreviousAcceptedQuantity;
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
                            PurchaseOrderBotProcess PurchaseOrderBotProcess = new PurchaseOrderBotProcess
                            {
                                Id = Guid.NewGuid(),
                                PurchaseOrderId = dto.POOrderId,
                                EPID = dto.EPID,
                                LineItem = dto.LineItem,
                                UnitOfMeasure = dto.UnitOfMeasure,
                                MaterialCode = dto.MaterialCode,
                                DomainId = dto.DomainId,
                                Quantity = dto.Quantity,
                                PreviousAcceptedQuantity = dto.PreviousAcceptedQuantity,
                                ReceiptId = dto.ReceiptID,
                                Status = dto.Status,
                                ProcessedState = dto.ProcessedState,
                                HardReprocess = dto.HardReprocess,
                                IsActive = true,
                                DateCreated = DateTime.UtcNow
                            };
                            newOrders.Add(PurchaseOrderBotProcess);
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

                // Bulk insert all new orders in one transaction
                if (newOrders.Any())
                {
                    newOrders = newOrders.GroupBy(t => new { t.PurchaseOrderId, t.EPID, t.LineItem, t.DomainId })
                                        .Select(g => g.Last())
                                        .ToList();
                    _logger.LogInfo($"Bulk inserting {newOrders.Count} new Purchase Order Bot Process records");
                    await _bulkInsertHelper.BulkInsertOrUpdateSafeAsync(newOrders);
                }

                if (ordersToUpdate.Any())
                {
                    _logger.LogInfo($"Bulk updating {ordersToUpdate.Count} Purchase Order Bot Process records");
                    _repositoryWrapper.PurchaseOrderBotProcess.UpdateRange(ordersToUpdate);
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

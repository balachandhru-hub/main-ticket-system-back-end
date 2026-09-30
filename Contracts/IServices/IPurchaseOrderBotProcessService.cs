using Dto;
using System;

namespace Contracts.IServices
{
    public interface IPurchaseOrderBotProcessService
    {
        /// <summary>
        /// Optimized batch processing method for bulk uploads of Purchase Order Bot Process
        /// </summary>
        Task<BatchProcessingResultDto> CreatePurchaseOrderBotProcessBatchAsync(List<PurchaseOrderBotProcessExcelDto> dtos);
    }
}

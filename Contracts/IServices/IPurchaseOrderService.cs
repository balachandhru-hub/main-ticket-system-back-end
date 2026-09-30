using Dto;
using System;

namespace Contracts.IServices
{
    public interface IPurchaseOrderService
    {
        /// <summary>
        /// Get Purchase Order Ids for user.
        /// </summary>
        List<string> GetPurchaseOrderIds(Guid domainId, int index, int limit, string searchTerm, PurchaseOrderRqMode? mode = null);
        /// <summary>
        /// Creates the Purchase Order Record from Excel
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Guid CreatePurchaseOrder(POExcelExtractDto dto);

        /// <summary>
        /// Optimized batch processing method for bulk uploads
        /// </summary>
        Task<BatchProcessingResultDto> CreatePurchaseOrdersBatchAsync(List<POExcelExtractDto> dtos);

        /// <summary>
        /// Get the details of purchase order
        /// </summary>
        List<PurchaseOrderDto>? GetPurchaseOrderByIds(PurchaseOrderRqDto purchaseOrderRq, Guid domainId);
        /// <summary>
        /// Get the Reciept by Purchase Id
        /// </summary>
        /// <param name="purchaseId"></param>
        /// <returns></returns>
        List<string> GetRecieptByPurchaseId(List<string> purchaseId);
        Task PostPoDetailsToAriba(List<PuchaseOrderPostRqDto> poRequest, Guid domainId, bool isReversible, DateOnly? date);
        /// <summary>
        /// Blocks or Unblocks the Purchase Order
        /// </summary>
        /// <param name="PurchaseOrder"></param>
        /// <returns></returns>
        void BlockOrUnblockPO(List<string> purchaseOrder, Guid domainId, bool isBlocked);
        /// <summary>
        /// Fetches the blocked or unblocked Purchase Orders
        /// </summary>
        /// <param name="index"></param>
        /// <param name="limit"></param>
        /// <param name="domainId"></param>
        /// <param name="isBlocked"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundCustomException"></exception>
        List<string> GetBlockOrUnblockedPurchaseOrder(int index, int limit, Guid domainId, bool isBlocked, string? searchTerm);
        /// <summary>
        /// Fetches the List of Batch Id
        /// </summary>
        /// <param name="index"></param>
        /// <param name="limit"></param>
        /// <param name="domainId"></param>
        /// <returns></returns>
        List<int> GetBatchId(int index, int limit, Guid domainId);
        /// <summary>
        /// Fetches the List of Purchase Order Log Result by Batch Id
        /// </summary>
        /// <param name="index"></param>
        /// <param name="limit"></param>
        /// <param name="domainId"></param>
        /// <param name="batchId"></param>
        /// <returns></returns>
        List<PurchaseOrderLogResultDto> GetBatchIdResult(int index, int limit, Guid domainId, int batchId, string? purchaseOrderId);
        /// <summary>
        /// Optimized batch processing method for bulk uploads
        /// </summary>
        Task<BatchProcessingResultDto> CreatePurchaseOrdersMasterBatchAsync(List<POMasterExcelExtractDto> dtos);
        /// <summary>
        /// Create purchase order
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Guid CreatePurchaseOrderManual(List<PurchaseOrderCreateDto> dto, Guid domainId);
        /// <summary>
        /// Get Contract Id by PO Number    
        /// </summary>
        InvoicePODto? GetPODetailsByPONumber(Guid domainId, string poNumber);
        /// <summary>
        /// Optimized batch processing method for bulk uploads
        /// </summary>
        Task<BatchProcessingResultDto> CreateContractBatchAsync(List<ContractExcelExtractDto> dtos);
        /// <summary>
        /// Optimized batch processing method for bulk uploads
        /// </summary>
        Task<BatchProcessingResultDto> CreateCompanyCodeBatchAsync(List<CompanyCodeExcelDto> dtos);
        Task<byte[]> DownloadPurchaseOrderLogWithSuccessErpIdAsync(Guid domainId, DateTime fromDate, DateTime toDate);

        InvoicePODto? GetPODetailsByRequisitionsId(Guid domainId, string RequisitionsId);
        Task<byte[]> DownloadBlockedPurchaseOrdersAsync(Guid domainId);
        Task<BatchProcessingResultDto> CreateMaterialPRBatchAsync(List<MaterialPRExcelDto> dtos);

        /// <summary>
        /// Optimized batch processing method for bulk uploads
        /// </summary>
        Task<BatchProcessingResultDto> UpdateBlockOrUnblockPurchaseOrderBatchAsync(List<BlockOrUnblockDataDto> dtos);
                /// <summary>
        /// Uplaod the Material PR Excel File
        /// </summary>
        /// <param name="excelFile"></param>
        Task<BatchProcessingResultDto> CreateServicePRBatchAsync(List<ServicePRExcelDto> dtos);
       Task<BatchProcessingResultDto> CreateSupplierNameBatchAsync(List<SupplierNameExcelDto> dtos);
       Task<BatchProcessingResultDto> CreateVenueBatchAsync(List<VenueExcelDto> dtos);
       Task<BatchProcessingResultDto> CreateDraftMaterialPRBatchAsync(
    List<DraftMaterialPRExcelUploadDto> dtos,
    Guid userId, Guid domainId);

    Task<BatchProcessingResultDto> UpdatePRBlockOrUnblockPurchaseOrderBatchAsync(List<PRBlockOrUnblockDataDto> dtos);
    Task<byte[]> DownloadPRBlockedPurchaseOrdersAsync(Guid domainId,bool isBlocked);
    Task<BatchProcessingResultDto> CreatePropertyCrossChargeBatchAsync(List<PropertyCrossChargeExcelDto> dtos);
    
    }
}

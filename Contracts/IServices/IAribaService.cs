using Dto;
using Entities.Common;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.IServices
{
    public interface IAribaService
    {
        Task<ExternalReceiptImportReply> PostPoDetailsToAriba(PurchaseOrderRqToAribaDto poRequest);
        Task<List<AribaInvoiceDto>> GetPendingApprovables(DateTime fromDate, DateTime toDate, string token, string apiKey, string realm, string pendingApprovablesUrl, string requisitionDetailsUrl, Guid domainid, string TaskId);
        Task<List<AribaInvoiceDto>> GetPendingApprovablesByUserId(DateTime fromDate, DateTime toDate, string token, string apiKey, string realm, string pendingApprovablesUrl, string requisitionDetailsUrl, Guid domainid, string TaskId, string userName, string password);
        Task<List<AribaAttachmentDTO>> GetRequisitionAttachments(Guid domainId, string requisitionId, string attachmentId);
        Task<RequisitionDetailDto> GetRequisitionById(string approvableId, string token, string apiKey, string realm, string requisitionDetailsUrl);
        Task<string> PatchRequisitionAsync(Guid domainId, string requisitionId, AribaPatchRequestDto patchRequest, MyProfileDto aribauser,Guid userId);
        Task<string> GetValidToken(Guid domainId, AribaPurchaseOrderSettings config);
        Task<string> GetAribaToken(string authToken, string tokenUrl);
        Task<InvoiceContractDto> GetContractById(Guid domainId, string contractId);
        Task<byte[]> GetContractDocumentById(Guid domainId, string documentId, MyProfileDto aribauser);
        Task<AribaInvoiceDetailDto> GetContractDetailsFromReportAsync(Guid domainId, MyProfileDto aribaUser, DateTime updatedDateFrom, DateTime updatedDateTo);
        Task<string> GetContractWorkspaceId(Guid domainId, string contractId);
        Task<byte[]> GetAttachmentbyContractID(Guid domainId, string contractId, MyProfileDto aribaUser);
        Task<string?> GetAttachmentIdFromAribaAsync(Guid domainId, string workspaceId, MyProfileDto aribaUser);
        Task<string?> GetAttachmentIdByWorkspaceIdAsync(Guid domainId, string workspaceId, MyProfileDto aribaUser);

        SplitAttachmentResponseDto GetFinalAttachmentBase64(string invoiceId);
        Task DeleteAribaTaskByInvoiceAndApproverAsync(string invoiceId, string approver);
        AribaApprovalDashboardSummaryDto GetLeadershipApprovalDashboardSummary(Guid userId, Guid domainId, string type = Common.TYPE_INVOICE);
        Task SaveOrUpdateExtensionAsync(Guid domainId, ExtensionUploadDto dto, Guid userId);
        Task<ExtensionDownloadDto> GetLatestInvoiceExtensionAsync(Guid domainId);
        Task<ExtensionDownloadDto> GetExtensionByDomainIdAsync(Guid domainId);
        Task<List<AribaInvoiceDetailDto>> GetInvoicePendingApprovals(
           Guid userId,
           Guid domainId,
           int index,
           int limit,
           string? searchTerm = null,
           DateTime? fromDate = null,
           DateTime? toDate = null,
           string? status = null,
           List<string>? glAccount = null);
        Task<ExportExcelResultDto> ExportInvoicePendingApprovalsExcelAsync(
Guid userId, Guid domainId, int limit,
string? searchTerm, DateTime? fromDate, DateTime? toDate,
string? status, List<string>? glAccount);
Task<bool> UpdatePaymentStatusAsync(string invoiceId, string paymentStatus);
        Task<List<PaymentStatusDto>> GetPaymentStatusesAsync();


    }
}

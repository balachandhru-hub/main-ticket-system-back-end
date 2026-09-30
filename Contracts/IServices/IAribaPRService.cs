using Dto;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.IServices
{
    public interface IAribaPRService
    {

        Task<List<AribaPRDto>> GetPendingApprovables(DateTime fromDate, DateTime toDate, string token, string apiKey, string realm, string pendingApprovablesUrl, string RequisitionTaskUrl, Guid domainid, string TaskId);
        Task<List<AribaPRDto>> GetPendingApprovablesByUserId(DateTime fromDate, DateTime toDate, string token, string apiKey, string realm, string pendingApprovablesUrl, string RequisitionTaskUrl, string aribaUserName, string aribaPassword, Guid domainid, string TaskId);
        Task<RequisitionDto> GetRequisitionById(
       string approvableId,
       string token,
       string apiKey,
       string realm,
       string RequisitionTaskUrl);
        Task<string> PatchRequisition(Guid domainId, string requisitionId, AribaPatchRequestDto patchRequest, MyProfileDto aribauser, Guid userId);
        SplitAttachmentResponseDto GetFinalPRAttachmentBase64(string requisitionId);
        Task<List<AribaAttachmentDTO>> GetRequisitionAttachments(Guid domainId, string requisitionId, string attachmentId);
        Task<byte[]> GetAttachmentbyContractID(Guid domainId, string contractId, MyProfileDto aribaUser);
        Task<List<AribaPRDetailDto>> GetPendingApprovalsAsync(
        Guid userId,
        Guid domainId,
        int index,
        int limit,
        string? searchTerm = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? status = null,
        List<string>? glAccount = null);
        Task<ExportExcelResultDto> ExportPendingApprovalsExcelAsync(
    Guid userId, Guid domainId, int limit,
    string? searchTerm, DateTime? fromDate, DateTime? toDate,
    string? status, List<string>? glAccount);
        Task<bool> UpdatePaymentStatusAsync(string PRNumber, string paymentStatus);

    }
}

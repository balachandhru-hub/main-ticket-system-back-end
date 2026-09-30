using Dto;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.IServices
{
    public interface IContractService
    {

        Task<string> GetAribaToken(string authToken, string tokenUrl);
        Task<List<AribaContractDto>> GetPendingContracts(
    DateTime fromDate,
    DateTime toDate,
    string token,
    string apiKey,
    string realm,
    string ContractPendingApprovablesUrl,
    string ContractWorkspaceUrl,

    Guid domainId,
    string taskId);
        Task<string> PostRequisitionActionAsync(Guid domainId, string taskId, AribaActionRequestDto actionRequest, MyProfileDto aribaUser);
        
        Task<AribaAttachmentDTO> GetContractDocumentById(Guid domainId, string documentId, MyProfileDto aribauser);
        Task<List<AribaContractDto>> GetContractPendingApprovalsAsync(
    Guid userId,
    Guid domainId,
    int index,
    int limit,
    string? searchTerm = null,
    DateTime? fromDate = null,
    DateTime? toDate = null,
    string? status = null);
    SplitAttachmentResponseDto GetFinalContractAttachmentBase64(string contractTaskId);
    Task<ExportExcelResultDto> ExportContractPendingApprovalsExcelAsync(
           Guid userId, Guid domainId, int limit,
           string? searchTerm, DateTime? fromDate, DateTime? toDate,
           string? status);
           Task<List<AribaContractDto>> GetPendingContractsByUserId(
    DateTime fromDate,
    DateTime toDate,
    string token,
    string apiKey,
    string realm,
    string ContractPendingApprovablesUrl,
    string ContractWorkspaceUrl,
    string aribaPassword,
    string aribaUserName,Guid domainId,
    string taskId);

    }
}
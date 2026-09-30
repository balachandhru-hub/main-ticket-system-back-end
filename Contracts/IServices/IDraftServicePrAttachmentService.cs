using Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Contracts.IServices
{
    /// <summary>
    /// Interface for Draft Material PR Service
    /// Handles creation of draft material PRs without calling SOAP API
    /// </summary>
    public interface IDraftServicePrAttachmentService
    {
        /// <summary>
        /// Create a new draft Material PR with auto-increment DraftID
        /// Does not call SOAP API
        /// </summary>
        /// <param name="prRequests">List of draft PR requests</param>
        /// <param name="domainId">Domain ID for the draft PR</param>
        /// <param name="userId">User ID creating the draft PR</param>
        /// <param name="assetUploadDto">Attachments for the draft PR</param>
        /// <returns>List of draft material PR result DTOs with DraftID</returns>
         // =========================================================
       

       Task<List<DraftServicePRResultDto>> CreateDraftServicePRAsync(
            List<DraftAribaServicePRDto> prRequests,
            Guid domainId,
            Guid userId,
            List<AssetUploadDto>? assetUploadDto);

        Task<List<DraftServicePRDetailsDto>> GetDraftServicePRsGroupedAsync(
        Guid userId,
        Guid domainId,
        MaterialPRFilterRequestDto filter);
    }   
}
using System.Collections.Generic;
using Dto;
using System;
using Contracts.IServices;

namespace Contracts.IServices
{
    /// <summary>
    /// Interface IConciergePRCreationService
    /// </summary>
    public interface IConciergePRCreationService
    {
        Task<List<MaterialPRResultDto>> PostConciergeMaterialPRToAriba(List<AribaMaterialPRDto> prRequests, Guid domainId, MyProfileDto dto, Guid userId, List<AssetUploadDto>? assetUploadDto, bool isUpdate);
        Task<VenueDetailsDto?> GetConciergeVenueDetailsByNameAsync(Guid domainId, string venueName);
        Task<List<VenueListDto>> GetConciergeVenueNamesAsync(Guid domainId, string searchTerm, int index, int limit);
        Task<BatchProcessingResultDto> CreateConciergeVenueBatchAsync(List<VenueExcelDto> dtos);
    }
}
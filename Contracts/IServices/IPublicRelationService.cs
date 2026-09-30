using System.Collections.Generic;
using Dto;
using System;
using Contracts.IServices;
using Entities.Models;

namespace Contracts.IServices
{
    /// <summary>
    /// Interface IPublicRelationService
    /// </summary>
    public interface IPublicRelationService
    {
        Task<List<MaterialPRResultDto>> PostDigitalQuickPRToAriba(List<AribaMaterialPRDto> prRequests, Guid domainId, MyProfileDto dto, Guid userId, List<AssetUploadDto>? assetUploadDto, bool isUpdate);
        Task<List<PropertyCrossChargeDto>> GetPropertyCrossChargeForDropdownAsync(Guid domainId, string? companyCode, string? searchTerm, int index, int limit);
        Task<PropertyCrossChargeDetailsDto?> GetCrossChargeDetailsByNameAsync(
            Guid domainId,
            string venueName,string? companyCode);
        Task<VenueDetailsDto?> GetPublicVenueDetailsByNameAsync(
            Guid domainId,
            string venueName);
        Task<BatchProcessingResultDto> CreatePublicVenueBatchAsync(List<VenueExcelDto> dtos);
        Task<List<VenueListDto>> GetVenueForDropdownAsync(Guid domainId, string searchTerm, int index, int limit);
        Task<BatchProcessingResultDto> CreatePublicCategoryBatchAsync(List<CategoryExcelDto> dtos);
        Task<List<PublicCategoryListDto>> GetPublicCategoryForDropdownAsync(Guid domainId, string searchTerm, int index, int limit);


    }
}
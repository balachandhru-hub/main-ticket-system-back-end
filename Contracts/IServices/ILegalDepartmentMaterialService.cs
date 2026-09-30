using System.Collections.Generic;
using Dto;
using System;
using Contracts.IServices;
using Entities.Models;

namespace Contracts.IServices
{
    /// <summary>
    /// Interface IMaterialPRService
    /// </summary>
    public interface ILegalDepartmentMaterialService
    {
        Task<List<MaterialPRResultDto>> PostMaterialPRToAriba(List<AribaMaterialPRDto> prRequests, Guid domainId, MyProfileDto dto, Guid userId, List<AssetUploadDto>? assetUploadDto, bool isUpdate);
    Task<BatchProcessingResultDto> CreateVenueBatchAsync(List<VenueExcelDto> dtos);
    Task<List<VenueListDto>> GetVenueForDropdownAsync(Guid domainId, string searchTerm, int index, int limit);
    Task<VenueDetailsDto?> GetVenueDetailsByNameAsync(
            Guid domainId,
            string venueName);
            Task<List<LegalMaterialCodeDetailsDto>> GetLegalMaterialCodesForDropdownAsync(Guid domainId, string searchTerm, int index, int limit);
            Task<BatchProcessingResultDto> CreateLegalMaterialCodeBatchAsync(List<LegalMaterialCodeExcelDto> dtos);
            Task<LegalMaterialCodeListDto?> GetMaterialCodeDetailsByNameAsync(
            Guid domainId,
            string materialcode);

    }
}
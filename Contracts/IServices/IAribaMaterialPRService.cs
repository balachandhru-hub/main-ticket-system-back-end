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
    public interface IAribaMaterialPRService
    {
        Task<List<MaterialPRResultDto>> PostMaterialPRToAriba(List<AribaMaterialPRDto> prRequests, Guid domainId, MyProfileDto dto, Guid userId, List<AssetUploadDto>? assetUploadDto, bool isUpdate);
        Task UpdateMaterialPR(string prNumber, MaterialPRUpdateDto dto, Guid domainId);
        Task<List<GetMaterialPRDto>> GetMaterialPRs(Guid userId, Guid domainId, MaterialPRFilterRequestDto filter);
        Task<PagedResult<string>> GetMaterialCodesForDropdown(string? searchTerm, int page, int pageSize);
        Task<MaterialDetailsDto?> GetMaterialDetailsAsync(string materialCode);
        Task<MaterialDetailsDto?> GetCommodityDetailsAsync(string CommodityCode);

        Task<PRSmartDefaultsDto> GetPRSmartDefaultsAsync(string aribaUserName);
        Task<List<string>> GetPRNumbersAsync(Guid domainId, int index, int limit);
        Task<List<SupplierListDto>> GetSuppliersForDropdownAsync(
    Guid domainId,
    string searchTerm,
    int index,
    int limit);
    Task<List<VenueListDto>> GetVenueForDropdownAsync(
    Guid domainId,
    string searchTerm,
    int index,
    int limit);
    Task<VenueDetailsDto?> GetVenueDetailsByNameAsync(
    Guid domainId,
    string venueName);
    Task<List<MaterialPRResponseDto>> GetMaterialPRsGroupedAsync(
    Guid userId, 
    Guid domainId, 
    MaterialPRFilterRequestDto filter);
    

    }
}
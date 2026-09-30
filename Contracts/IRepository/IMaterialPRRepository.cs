using Entities.Models;
using Dto;

namespace Contracts.IRepository
{
    /// <summary>
    /// Interface <c>IMetadataRepository</c> used to define the methods related for refterm.
    /// </summary>
    public interface IMaterialPRRepository : IRepositoryBase<MaterialPR>
    {
        MaterialPRLineItemDto GetPRDetails(string aribaUser);
        Task<PagedResult<string>> GetMaterialCodesForDropdownAsync(string searchTerm, int page, int pageSize);
        Task<MaterialDetailsDto?> GetMaterialDetailsByMaterialCodeAsync(string materialCode);
        Task<MaterialDetailsDto?> GetMaterialDetailsByCommodityCodeAsync(string CommodityCode);
        Task<string?> GetExternalPRStatusByPRNumberAsync(string prNumber);
        string GetOriginatingReferenceIdByPRNumber(Guid domainId, string prNumber);

        Task<List<PRDetailsDto>> GetPRDetailsByPRNumberAsync(string prNumber);
        Task<string?> GetLastPRUniqueNameAsync();

    }

}
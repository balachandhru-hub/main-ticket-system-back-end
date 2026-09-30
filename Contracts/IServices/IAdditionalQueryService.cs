using Dto;
using System;
using System.Threading.Tasks;

namespace Contracts.IServices
{
    public interface IAdditionalQueryService
    {
        Task<Guid> CreateAdditionalQueryAsync(AdditionalQueryDto dto, Guid userId);
        Task UpdateStatusAsync(Guid id, string status, Guid userId);
        Task<List<AdditionalQueryListDto>> GetAdditionalQueriesAsync(AdditionalQueryFilterDto filter);
    }
}

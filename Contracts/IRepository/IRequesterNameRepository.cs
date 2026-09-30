using Entities.Models;

namespace Contracts.IRepository
{
    /// <summary>
    /// IequesterNameRepository
    /// </summary>
    public interface IRequesterNameRepository : IRepositoryBase<RequesterName>
    {
Task<string?> GetRequesterByTypeAsync(string firstType, Guid userId);
List<RequesterNameDto> GetRequesterNamesByUserId(Guid userId);
    }
}
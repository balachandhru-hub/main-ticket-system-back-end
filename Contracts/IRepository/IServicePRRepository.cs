using Entities.Models;
using Dto;

namespace Contracts.IRepository
{
    /// <summary>
    /// Interface <c>IMetadataRepository</c> used to define the methods related for refterm.
    /// </summary>
    public interface IServicePRRepository : IRepositoryBase<ServicePR>
    {
       string GetOriginatingReferenceIdByPRNumber(Guid domainId, string prNumber);
       Task<string?> GetExternalPRStatusByPRNumberAsync(string? prNumber);
       ServicePRLineItemDto GetServicePRDetails (string aribaUser);
       Task<string?> GetLastPRUniqueNameAsync();
        
    }
    
}
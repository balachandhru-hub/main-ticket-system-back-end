using Dto;
using Entities.Models;
namespace Contracts.IRepository
{
    /// <summary>
    /// IAppModelMappingRepository
    /// </summary>
    public interface IAppModelMappingRepository : IRepositoryBase<AppModelMapping>
    {
        List<AppNameDto> GetAppNamesByDomainId(Guid domainId);
        
    }
}

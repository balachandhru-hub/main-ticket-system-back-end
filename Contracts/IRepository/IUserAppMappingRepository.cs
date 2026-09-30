using Dto;
using Entities.Models;

namespace Contracts.IRepository
{
    public interface IUserAppMappingRepository : IRepositoryBase<UserAppMapping>
    {
        List<AppNameDto> GetAppNamesByUserId(Guid userId);
        
    }
}

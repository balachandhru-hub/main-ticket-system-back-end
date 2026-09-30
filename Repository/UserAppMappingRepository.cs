using Contracts.IRepository;
using Entities.DbContext;
using Entities.Models;
using Dto;

namespace Repository
{
    public class UserAppMappingRepository : RepositoryBase<UserAppMapping>, IUserAppMappingRepository
    {
        public UserAppMappingRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
        }

        public List<AppNameDto> GetAppNamesByUserId(Guid userId)
        {
            return (from mapping in RepositoryContext.AppUserMapping
                    join app in RepositoryContext.UserAppMapping
                        on mapping.AppId equals app.Id
                    where mapping.IsActive && app.IsActive && mapping.UserId == userId
                    select new AppNameDto
                    {
                        Id = app.Id,
                        Key = app.Key,
                        Name = app.Name,
                        Description = app.Description,
                        Link = app.Link
                    })
                   .ToList();
        }
        
    }
}

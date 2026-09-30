using Contracts.IRepository;
using Dto;
using Entities.DbContext;
using Entities.Models;

namespace Repository
{
    /// <summary>
    /// Class <c>AppModelMappingRepository</c> used to implement the methods related for Api Key.
    /// </summary>
    public class AppModelMappingRepository : RepositoryBase<AppModelMapping>, IAppModelMappingRepository
    {
        /// <summary>
        /// Constructor for injecting DbContext and ILogger
        /// </summary>
        /// <param name="repositoryContext"></param>
        /// <returns></returns>
        public AppModelMappingRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {

        }
         public List<AppNameDto> GetAppNamesByDomainId(Guid domainId)
        {
            return (from mapping in RepositoryContext.AppModelMapping
                    join app in RepositoryContext.UserAppMapping
                        on mapping.AppId equals app.Id
                    where mapping.DomainId == domainId
                          && app.IsActive
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

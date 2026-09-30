using Contracts.IRepository;
using Entities.DbContext;
using Entities.Models;

namespace Repository
{
    public class AppUserMappingRepository : RepositoryBase<AppUserMapping>, IAppUserMappingRepository
    {
        public AppUserMappingRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
        }
    }
}

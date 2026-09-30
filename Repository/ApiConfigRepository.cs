using Contracts.IRepository;
using Entities.Models;
using Entities.DbContext;

namespace Repository
{
    public class ApiConfigRepository : RepositoryBase<ApiConfig>, IApiConfigRepository
    {
        /// <summary>
        /// Constructor for injecting DbContext
        /// </summary>
        /// <param name="repositoryContext"></param>
        /// <returns></returns>
        public ApiConfigRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {

        }

    }
}

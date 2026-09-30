using Contracts.IRepository;
using Entities.DbContext;
using Entities.Models;

namespace Repository
{
    /// <summary>
    /// Class <c>ApiKeyRepository</c> used to implement the methods related for Api Key.
    /// </summary>
    public class ApiKeyRepository : RepositoryBase<ApiKey>, IApiKeyRepository
    {
        /// <summary>
        /// Constructor for injecting DbContext and ILogger
        /// </summary>
        /// <param name="repositoryContext"></param>
        /// <returns></returns>
        public ApiKeyRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {

        }
    }
}

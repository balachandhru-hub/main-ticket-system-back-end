using Contracts.IRepository;
using Entities.Models;
using Entities.DbContext;

namespace Repository
{
    /// <summary>
    /// Class <c>ConfigRepository</c> used to implement the methods related for Config.
    /// </summary>
    public class ConfigRepository : RepositoryBase<Config>, IConfigRepository
    {
        /// <summary>
        /// Constructor for injecting DbContext
        /// </summary>
        /// <param name="repositoryContext"></param>
        /// <returns></returns>

        public ConfigRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
        }
        
    }
}
using Contracts.IRepository;
using Entities.Models;
using Entities.DbContext;
 
namespace Repository
{
    /// <summary>
    /// Class <c>RoleRepository</c> used to implement the methods related for User.
    /// </summary>
    public class RoleRepository : RepositoryBase<Role>, IRoleRepository
    {
        /// <summary>
        /// Constructor for injecting DbContext
        /// </summary>
        /// <param name="repositoryContext"></param>
        /// <returns></returns>
        public RoleRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
 
        }
    }
}
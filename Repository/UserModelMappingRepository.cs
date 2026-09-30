using Contracts.IRepository;
using Entities.Models;
using Entities.DbContext;
 
namespace Repository
{
    /// <summary>
    /// Class <c>RoleRepository</c> used to implement the methods related for User.
    /// </summary>
    public class UserModelMappingRepository : RepositoryBase<UserModelMapping>, IUserModelMappingRepository
    {
        /// <summary>
        /// Constructor for injecting DbContext
        /// </summary>
        /// <param name="repositoryContext"></param>
        /// <returns></returns>
        public UserModelMappingRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
 
        }
    }
}
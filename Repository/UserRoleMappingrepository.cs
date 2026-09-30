using System;
using Contracts.IRepository;
using Entities.Models;
using Entities.DbContext;

namespace Repository
{
    /// <summary>
    /// Class <c>UserRoleMappingRepository</c> implements methods related to UserRoleMapping entity.
    /// </summary>
    public class UserRoleMappingRepository : RepositoryBase<UserRoleMapping>, IUserRoleMappingRepository
    {
        /// <summary>
        /// Constructor for injecting RepositoryContext.
        /// </summary>
        /// <param name="repositoryContext">The database context for the repository.</param>
        public UserRoleMappingRepository(RepositoryContext repositoryContext)
            : base(repositoryContext)
        {
        }
    }
}

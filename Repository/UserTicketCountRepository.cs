using System;
using Contracts.IRepository;
using Entities.Models;
using Entities.DbContext;

namespace Repository
{
    /// <summary>
    /// Class <c>UserTicketCountRepository</c> implements methods related to UserTicketCount entity.
    /// </summary>
    public class UserTicketCountRepository : RepositoryBase<UserTicketCount>, IUserTicketCountRepository
    {
        /// <summary>
        /// Constructor for injecting RepositoryContext.
        /// </summary>
        /// <param name="repositoryContext">The database context for the repository.</param>
        public UserTicketCountRepository(RepositoryContext repositoryContext)
            : base(repositoryContext)
        {
        }
    }
}

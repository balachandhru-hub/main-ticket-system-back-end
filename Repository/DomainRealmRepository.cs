using Contracts.IRepository;
using Entities.Models;
using Entities.DbContext;

namespace Repository
{
    /// <summary>
    /// Repository for Ticket entity, implementing ITicketRepository interface.
    /// </summary>
    public class DomainRealmRepository : RepositoryBase<DomainRealm>, IDomainRealmRepository
    {
        public DomainRealmRepository(RepositoryContext repositoryContext)
            : base(repositoryContext)
        {
        }
    }
}
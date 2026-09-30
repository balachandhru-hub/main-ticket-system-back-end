using Contracts.IRepository;
using Entities.Models;
using Entities.DbContext;

namespace Repository
{
    /// <summary>
    /// Repository for Ticket entity, implementing ITicketRepository interface.
    /// </summary>
    public class DomainModelRepository : RepositoryBase<DomainModel>, IDomainModelRepository
    {
        public DomainModelRepository(RepositoryContext repositoryContext)
            : base(repositoryContext)
        {
        }
    }
}
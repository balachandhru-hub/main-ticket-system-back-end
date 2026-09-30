using Dto;
using Entities.Models;

namespace Contracts.IRepository
{
    /// <summary>
    /// Interface for Ticket repository, extending IRepositoryBase.
    /// </summary>
    public interface ITicketRepository : IRepositoryBase<Ticket>
    {
        // Add ticket-specific methods here if needed
        TicketDetailDto? GetTicketById(Guid id);
    }
}
using Dto;
using Entities.Models;

namespace Contracts.IRepository
{
    /// <summary>
    /// ITicketMessageRepository            
    /// </summary>
    public interface ITicketMessageRepository : IRepositoryBase<TicketMessage>
    {
        /// <summary>
        /// Fetches the Ticket Message Details for the Given Ticket Id
        /// </summary>
        /// <param name="ticketId">Ticket Id</param>
        List<TicketMessageDto> GetTicketMessageById(Guid ticketId, int index, int limit);
    }
}
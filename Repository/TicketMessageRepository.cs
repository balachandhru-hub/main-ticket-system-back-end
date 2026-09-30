using Contracts.IRepository;
using Entities.Models;
using Entities.DbContext;
using Contracts;
using Dto;
using Entities.Common;

namespace Repository
{
    /// <summary>
    /// Class <c>TicketMessageRepository</c> used to implement the methods related for Ticket Messages.
    /// </summary>
    public class TicketMessageRepository : RepositoryBase<TicketMessage>, ITicketMessageRepository
    {
        private readonly ILoggerManager _logger;
        /// <summary>
        /// Constructor for injecting DbContext
        /// </summary>
        /// <param name="repositoryContext"></param>
        /// <returns></returns>
        public TicketMessageRepository(RepositoryContext repositoryContext, ILoggerManager logger) : base(repositoryContext)
        {
            _logger = logger;
        }

        /// <summary>
        /// Fetches the Ticket Message Details for the Given Ticket Id
        /// </summary>
        /// <param name="ticketId">Ticket Id</param>
        public List<TicketMessageDto> GetTicketMessageById(Guid ticketId, int index, int limit)
        {
            _logger.LogInfo($"Fetching the Ticket Message Details for the Ticket with Id : {ticketId}");
            List<TicketMessageDto> messageResult = (from _ticketMessage in RepositoryContext.TicketMessage.Where(t => t.IsActive)
                                                    join _user in RepositoryContext.User on _ticketMessage.CreatedBy equals _user.Id
                                                    join _person in RepositoryContext.Person on _user.PersonId equals _person.Id
                                                    where _ticketMessage.TicketId == ticketId
                                                    orderby _ticketMessage.DateCreated descending
                                                    select new TicketMessageDto
                                                    {
                                                        Type = Common.MESSAGE_TYPE_KEY,
                                                        MessageId = _ticketMessage.Id,
                                                        Message = _ticketMessage.Message,
                                                        UserName = _person.Name,
                                                        DateCreated = _ticketMessage.DateCreated,
                                                        UserId = _user.Id
                                                    }).ToList();

            List<TicketMessageDto> assetResult = (from _asset in RepositoryContext.Asset.Where(t => t.IsActive && t.AssetType == Common.TICKET_MESSAGE_ASSET_TYPE_ID)
                                                  join _ticket in RepositoryContext.Ticket on _asset.EntityId equals _ticket.Id
                                                  join _user in RepositoryContext.User on _asset.CreatedBy equals _user.Id
                                                  join _person in RepositoryContext.Person on _user.PersonId equals _person.Id
                                                  where _ticket.Id == ticketId
                                                  orderby _asset.DateCreated descending
                                                  select new TicketMessageDto
                                                  {
                                                      Type = Common.ASSET_TYPE_KEY,
                                                      AssetId = _asset.Id,
                                                      UserName = _person.Name,
                                                      DateCreated = _asset.DateCreated,
                                                      UserId = _user.Id
                                                  }).ToList();

            List<TicketMessageDto> combinedQuery = messageResult
                        .Union(assetResult)
                        .OrderByDescending(x => x.DateCreated)
                        .Skip(index)
                        .Take(limit).ToList();

            return combinedQuery;
        }
    }
}
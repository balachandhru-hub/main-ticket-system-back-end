using Contracts;
using Contracts.IServices;
using Entities.Common;
using Quartz;
using Dto; // ✅ Required for TicketFilterDto
using System.Threading.Tasks;

namespace Services
{
    public class DomainTicketService : IJob
    {
        private readonly IDomainService _domainService;
        private readonly ITicketService _ticketService;
        private readonly ILoggerManager _logger;

        public DomainTicketService(IDomainService domainService, ITicketService ticketService, ILoggerManager logger)
        {
            _domainService = domainService;
            _ticketService = ticketService;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                _logger.LogInfo("🔄 [DomainTicketService] Job started: Fetching all domains and tickets...");

                var allDomains = _domainService.GetAllDomains(0, int.MaxValue, null,null);
                _logger.LogInfo($"✅ Total Domains: {allDomains.Count}");

                var ticketFilter = new TicketFilterDto
                {
                    Index = 0,
                    Limit = int.MaxValue
                };

                var allTickets = _ticketService.GetAllTickets(Guid.Empty, ticketFilter);
                _logger.LogInfo($"✅ Total Tickets: {allTickets.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ [DomainTicketService] Job failed: {ex.Message}");
            }
        }
    }
}

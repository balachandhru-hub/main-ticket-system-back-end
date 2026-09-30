using System.Security.Cryptography;
using Contracts;
using Contracts.IRepository;
using Entities.DbContext;
using Entities.Models;

namespace Repository
{
    public class PlanDetailsRepository : RepositoryBase<PlanDetails>, IPlanDetailsRepository
    {
        private readonly ILoggerManager _logger;
        public PlanDetailsRepository(RepositoryContext context, ILoggerManager logger) : base(context)
        {
            _logger = logger;
        }

        public PlanDetails GetPlanDetailsByTicketId(Guid ticketId)
        {
            _logger.LogInfo($"Fetching PlanDetails using TicketId: {ticketId}");

            return (
                from ticket in RepositoryContext.Ticket.Where(t => t.IsActive && t.Id == ticketId)
                join domainPlan in RepositoryContext.DomainPlan.Where(dp => dp.IsActive)
                    on ticket.DomainId equals domainPlan.DomainId
                join planDetails in RepositoryContext.PlanDetails.Where(pd => pd.IsActive)
                    on domainPlan.Id equals planDetails.DomainPlanId
                select planDetails
            ).FirstOrDefault();
        }
            public PlanDetails GetPlanDetailsByDomainId(Guid domainId)
{
    _logger.LogInfo($"Fetching PlanDetails using DomainId: {domainId}");

    return (
        from domainPlan in RepositoryContext.DomainPlan.Where(dp => dp.IsActive && dp.DomainId == domainId)
        join planDetails in RepositoryContext.PlanDetails.Where(pd => pd.IsActive)
            on domainPlan.Id equals planDetails.DomainPlanId
        select planDetails
    ).FirstOrDefault();
}

    }
}
using Entities.Models;
using Contracts.IRepository;
using Entities.DbContext;
using Contracts;

namespace Repository
{

    public class DomainPlanCreditRepository : RepositoryBase<DomainPlanCredit>, IDomainPlanCreditRepository
    {
        private readonly ILoggerManager _logger;
        public DomainPlanCreditRepository(RepositoryContext repositoryContext, ILoggerManager logger) : base(repositoryContext)
        {
            _logger = logger;
        }
        public DomainPlanCredit GetDomainPlanCredit(Guid domainId)
        {
            _logger.LogInfo($"Fetching Domain Plan Credit for Domain with Id : {domainId}");
            return (from _domainPlan in RepositoryContext.DomainPlan.Where(x => x.IsActive && x.DomainId == domainId)
                    join _domainPlanCredit in RepositoryContext.DomainPlanCredit on _domainPlan.Id equals _domainPlanCredit.DomainPlanId
                    select _domainPlanCredit).FirstOrDefault();
        }
    }
}
using Entities.DbContext;
using Entities.Common;
using Dto;
using Contracts.IRepository;
using Entities.Models;  
using System.Collections.Generic;
using System.Linq;


namespace Repository
{
    /// <summary>
    /// PlanConfigRepository
    /// </summary>
    public class PlanConfigRepository : RepositoryBase<PlanConfig>, IPlanConfigRepository
    {
        private readonly RepositoryContext _context;

        public PlanConfigRepository(RepositoryContext context) : base(context)
        {
            _context = context;
        }

        public List<PlanConfigDto> GetByPlanAndEngagementType(string plan, string engagementType)
        {
            var grouped = _context.PlanConfig
                .Where(x => x.Plan == plan && x.EngagementType == engagementType)
                .GroupBy(x => new { x.Plan, x.EngagementType })
                .Select(g => new PlanConfigDto
                {
                    Id = g.Select(x => x.Id).FirstOrDefault(), // Take one of the IDs
                    Plan = g.Key.Plan,
                    EngagementType = g.Key.EngagementType,

                    ChargePerTicket = g.Where(x => x.Key == Common.CHARGE_PER_TICKET).Select(x => x.Value).FirstOrDefault(),
                    CrBeyond8Hours = g.Where(x => x.Key == Common.CR_BEYOND_8_HOURS).Select(x => x.Value).FirstOrDefault(),
                    MonthlyHours = g.Where(x => x.Key == Common.MONTHLY_HOURS).Select(x => x.Value).FirstOrDefault(),
                    PaymentType = g.Where(x => x.Key == Common.PAYMENT_TYPE).Select(x => x.Value).FirstOrDefault(),
                    ResourceCost = g.Where(x => x.Key == Common.RESOURCE_COST).Select(x => x.Value).FirstOrDefault(),
                    NoOfConsultants = g.Where(x => x.Key == Common.NO_OF_CONSULTANTS).Select(x => x.Value).FirstOrDefault(),
                    CrChargesForAdditionalHours = g.Where(x => x.Key == Common.CR_CHARGES_FOR_ADDITIONAL_HOURS).Select(x => x.Value).FirstOrDefault(),
                    Manager = g.Where(x => x.Key == Common.MANAGER).Select(x => x.Value).FirstOrDefault(),
                    Resources = g.Where(x => x.Key == Common.RESOURCES).Select(x => x.Value).FirstOrDefault(),
                    CrHours = g.Where(x => x.Key == Common.CR_HOURS).Select(x => x.Value).FirstOrDefault(),
                    ManagerCost = g.Where(x => x.Key == Common.MANAGER_COST).Select(x => x.Value).FirstOrDefault()
                })
                .ToList();

            return grouped;
        }
 


    }
}

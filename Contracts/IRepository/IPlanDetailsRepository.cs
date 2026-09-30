using Entities.Models;

namespace Contracts.IRepository
{
    public interface IPlanDetailsRepository : IRepositoryBase<PlanDetails>
    {


        PlanDetails GetPlanDetailsByTicketId(Guid ticketId);
        PlanDetails GetPlanDetailsByDomainId(Guid domainId);


    }
}
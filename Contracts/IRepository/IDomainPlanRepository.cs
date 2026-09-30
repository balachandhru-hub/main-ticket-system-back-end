using Entities.Models;

namespace Contracts.IRepository
{
    public interface IDomainPlanRepository : IRepositoryBase<DomainPlan>
    {
        DomainPlan? GetDomainPlanByUserId(Guid userId); // <-- ADD THIS
    }
}

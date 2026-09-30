using System.Collections.Generic;
using Dto;
using Entities.Models;
namespace Contracts.IRepository
{
    /// <summary>
    /// IPlanConfigRepository
    /// </summary>

    public interface IPlanConfigRepository : IRepositoryBase<PlanConfig>
    {
        List<PlanConfigDto> GetByPlanAndEngagementType(string plan, string engagementType);
       
           
    }
}
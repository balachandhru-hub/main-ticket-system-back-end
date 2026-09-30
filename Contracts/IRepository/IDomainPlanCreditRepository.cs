using Contracts.IRepository;
using Dto;
using System;
using System.Collections.Generic;
using Entities.Models;


namespace Contracts.IRepository
{
   
    public interface IDomainPlanCreditRepository : IRepositoryBase<DomainPlanCredit>
    {
        DomainPlanCredit GetDomainPlanCredit(Guid domainId);
    }
}
using Dto;
using Entities.Models;
using System;
using System.Collections.Generic;

namespace Contracts.IRepository
{
    public interface IFinalPurchaseOrderRepository : IRepositoryBase<FinalPurchaseOrder>
    {
        List<string> GetDistinctPoNumbersByFilters(Guid domainId, PurchaseOrderFilterDto filter, int index = 0, int limit = 10);
        List<FinalPurchaseOrderDto> GetFinalPurchaseOrdersByPoNumbers(Guid domainId, List<string> poNumbers);
    }
}
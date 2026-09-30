using Dto;
using System;
using System.Collections.Generic;

namespace Contracts.IServices
{
    public interface IFinalPurchaseOrderService
    {
        List<string> GetPoNumbersByFilters(Guid domainId, PurchaseOrderFilterDto filter, int index = 0, int limit = 10);
        List<FinalPurchaseOrderDto> GetFinalPurchaseOrdersByPoNumbers(Guid domainId, List<string> poNumbers);
    }
}
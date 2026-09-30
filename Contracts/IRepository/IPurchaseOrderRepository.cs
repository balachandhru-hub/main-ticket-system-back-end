using Dto;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.IRepository
{
    public interface IPurchaseOrderRepository : IRepositoryBase<PurchaseOrder>
    {
        List<string> GetPurchaseOrderIds(Guid domainId, int index, int limit, string? searchTerm, PurchaseOrderRqMode? mode = null);
        /// <summary>
        /// Get Reciept by Purchase Ids
        /// </summary>
        /// <param name="purchaseIds"></param>
        /// <returns></returns>
        List<string>? GetRecieptByPurchaseIds(List<string> purchaseIds);
        List<PurchaseOrderDto>? GetPurchaseOrderByIds(PurchaseOrderRqDto purchaseOrderRq, Guid domainId);
    }
}

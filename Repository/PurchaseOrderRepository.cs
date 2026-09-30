using Contracts;
using Contracts.IRepository;
using Dto;
using Entities.Common;
using Entities.DbContext;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class PurchaseOrderRepository : RepositoryBase<PurchaseOrder>, IPurchaseOrderRepository
    {
        private ILoggerManager _logger;

        public PurchaseOrderRepository(RepositoryContext repositoryContext, ILoggerManager logger)
            : base(repositoryContext)
        {
            _logger = logger;

        }

        public List<string> GetPurchaseOrderIds(Guid domainId, int index, int limit, string? searchTerm, PurchaseOrderRqMode? mode = null)
        {
            _logger.LogInfo($"Fetching the List of PurchaseOrderIds SearchTerm: {searchTerm}");

            var baseQuery = RepositoryContext.PurchaseOrder
           .Where(x => x.DomainId == domainId && x.IsActive);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                baseQuery = baseQuery.Where(x => x.PurchaseOrderId.ToLower().Contains(searchTerm));
            }

            var all = baseQuery.ToList();

            var query = all.Where(t => t.PreviousAcceptedQuantity < t.Quantity).Select(t => t);
            var query2 = all.Where(t => t.Quantity >= t.PreviousAcceptedQuantity).Select(t => t);
            var query3 = all.Where(t => t.ProcessedReceiptId == null).Select(t => t);

            List<PurchaseOrder> ret = new List<PurchaseOrder>();

            if (mode == PurchaseOrderRqMode.PARTIAL)
            {
                ret.AddRange(query);
                ret.AddRange(query2);
                ret.AddRange(query3);
            }
            else if (mode == PurchaseOrderRqMode.COMPLETED)
            {
                ret.AddRange(query2);
                ret.AddRange(query);
                ret.AddRange(query3);
            }
            else // NEW
            {
                ret.AddRange(query3);
                ret.AddRange(query);
                ret.AddRange(query2);
            }


            var groupedQuery = ret.Select(x => x.PurchaseOrderId).Distinct().Skip(index).Take(limit).ToList();

            return groupedQuery.ToList();
        }

        public List<PurchaseOrderDto>? GetPurchaseOrderByIds(PurchaseOrderRqDto purchaseOrderRq, Guid domainId)
        {
            _logger.LogInfo($"Fetching the List of PurchaseOrderId : {string.Join(",", purchaseOrderRq.PurchaseOrderIds)}");

            var query = RepositoryContext.PurchaseOrder.Where(x => x.DomainId == domainId).AsQueryable();

            if (purchaseOrderRq.PurchaseOrderIds?.Any() == true && purchaseOrderRq.ReceiptIds?.Any() == true)
            {
                query = query.Where(po => purchaseOrderRq.PurchaseOrderIds.Contains(po.PurchaseOrderId)
                                       && purchaseOrderRq.ReceiptIds.Contains(po.ReceiptId));
            }
            else if (purchaseOrderRq.PurchaseOrderIds?.Any() == true)
            {
                query = query.Where(po => purchaseOrderRq.PurchaseOrderIds.Contains(po.PurchaseOrderId));
            }
            else if (purchaseOrderRq.ReceiptIds?.Any() == true)
            {
                query = query.Where(po => purchaseOrderRq.ReceiptIds.Contains(po.ReceiptId));
            }

            var results = query.ToList();


            var grouped = results
                .GroupBy(x => x.PurchaseOrderId)
                .Select(g => new PurchaseOrderDto
                {
                    PurchaseOrderId = g.Key,
                    RecievableId = g.Select(t => t.EPID).FirstOrDefault(),
                    LineItems = g.Select(t => new PurchaseOrderLineItem
                    {
                        AcceptedQuantity = t.PreviousAcceptedQuantity,
                        LineItem = t.LineItem,
                        MaterialCode = t.MaterialCode,
                        PreviousAcceptedQuantity = t.PreviousAcceptedQuantity,
                        Quantity = t.Quantity,
                        UnitOfMeasure = t.UnitOfMeasure,
                        ReceiptId = t.ReceiptId,
                        ERPReceiptId = t.ERPReceiptId,
                        ProcessedReceiptId = t.ProcessedReceiptId,
                        ErrorComment = t.ErrorComment
                    }).ToList()
                })
                .ToList();

            return grouped;
        }

        /// <summary>
        /// Get Reciept by Purchase Ids
        /// </summary>
        /// <param name="purchaseIds"></param>
        /// <returns></returns>
        public List<string>? GetRecieptByPurchaseIds(List<string> purchaseIds)
        {
            _logger.LogInfo($"Fetching the Reciept Details...");
            return (from r in RepositoryContext.PurchaseOrder
                    where purchaseIds.Contains(r.PurchaseOrderId)
                    select r.ReceiptId)
                    .Distinct().ToList();
        }

    }
}

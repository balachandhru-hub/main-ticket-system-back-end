using Contracts;
using Contracts.IRepository;
using Dto;
using Entities.DbContext;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Repository
{
    public class FinalPurchaseOrderRepository : RepositoryBase<FinalPurchaseOrder>, IFinalPurchaseOrderRepository
    {
        private readonly ILoggerManager _logger;

        public FinalPurchaseOrderRepository(RepositoryContext repositoryContext, ILoggerManager logger) : base(repositoryContext)
        {
            _logger = logger;
        }

        public List<string> GetDistinctPoNumbersByFilters(Guid domainId, PurchaseOrderFilterDto filter, int index = 0, int limit = 10)
        {
            _logger.LogInfo($"Fetching distinct PO numbers for Domain: {domainId} with index: {index}, limit: {limit}");

            IQueryable<FinalPurchaseOrder> query = RepositoryContext.FinalPurchaseOrder.AsQueryable()
                .Where(x => x.IsActive && x.DomainId == domainId);

            if (filter != null)
            {
                if (filter.GlAccount?.Any() == true)
                {
                    query = query.Where(x => !string.IsNullOrEmpty(x.GlAccount) && filter.GlAccount.Any(g => g == x.GlAccount));
                }
                if (filter.Supplier?.Any() == true)
                {
                    query = query.Where(x => !string.IsNullOrEmpty(x.Supplier) && filter.Supplier.Any(s => s == x.Supplier));
                }
                if (filter.CostCenter?.Any() == true)
                {
                    query = query.Where(x => !string.IsNullOrEmpty(x.CostCenter) && filter.CostCenter.Any(cc => cc == x.CostCenter));
                }
                if (filter.Property?.Any() == true)
                {
                    query = query.Where(x => !string.IsNullOrEmpty(x.Property) && filter.Property.Any(p => p == x.Property));
                }
                if (filter.Requester?.Any() == true)
                {
                    query = query.Where(x => !string.IsNullOrEmpty(x.Requester) && filter.Requester.Any(r => r == x.Requester));
                }
                if (filter.Department?.Any() == true)
                {
                    query = query.Where(x => !string.IsNullOrEmpty(x.Department) && filter.Department.Any(d => d == x.Department));
                }
                if (filter.PoNumber?.Any() == true)
                {
                    query = query.Where(x => !string.IsNullOrEmpty(x.PoNumber) && filter.PoNumber.Any(po => po == x.PoNumber));
                }
            }

            return query.Select(x => x.PoNumber).Distinct().Skip(index).Take(limit).ToList();
        }

        public List<FinalPurchaseOrderDto> GetFinalPurchaseOrdersByPoNumbers(Guid domainId, List<string> poNumbers)
        {
            _logger.LogInfo($"Fetching FinalPurchaseOrder details for Domain: {domainId} and PO Numbers: {string.Join(",", poNumbers)}");

            IQueryable<FinalPurchaseOrder> query = RepositoryContext.FinalPurchaseOrder.AsQueryable()
                .Where(x => x.IsActive && x.DomainId == domainId && poNumbers.Any(po => po == x.PoNumber));

            List<FinalPurchaseOrder> data = query.ToList();

            List<FinalPurchaseOrderDto> results = data
                .GroupBy(x => x.PoNumber)
                .Select(g =>
                {
                    FinalPurchaseOrder firstRecord = g.First();
                    return new FinalPurchaseOrderDto
                    {
                        PoNumber = g.Key,
                        Epid = firstRecord.Epid,
                        LineItems = g.Select(x => new FinalPurchaseOrderLineItemDto
                        {
                            LineItem = x.LineItem,
                            Supplier=x.Supplier,
                            Department=x.Department,
                            Property=x.Property,
                            GlAccount=x.GlAccount,
                            CostCenter=x.CostCenter,
                            Requester=x.Requester,
                            Quantity = x.Quantity,
                            Amount = x.Amount,
                            Accepted = x.Accepted,
                            PreviousAccepted = x.PreviousAccepted,
                            AribaReceiptId = x.AribaReceiptId,
                            S4MaterialDoc = x.S4MaterialDoc,
                            ErrorComments = x.ErrorComments,
                            Result = x.Result
                        }).ToList()
                    };
                }).ToList();

            return results;
        }
    }
}
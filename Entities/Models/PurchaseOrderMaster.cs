using System.ComponentModel.DataAnnotations.Schema;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Entities
{
    [Index(nameof(PurchaseOrderId), nameof(LineItem), nameof(DomainId), nameof(EPID), IsUnique = true)]
    public class PurchaseOrderMaster : BaseModel
    {
        public Guid Id { get; set; }
        [ForeignKey("Domain")]
        public Guid DomainId { get; set; }
        public Domain Domain { get; set; }
        public string PurchaseOrderId { get; set; }
        public string EPID { get; set; }
        public string LineItem { get; set; }
        public Double Quantity { get; set; }
        public string? UnitOfMeasure { get; set; }
        public string? MaterialCode { get; set; }
        public string? Description { get; set; }
        public string? CompanyCode { get; set; }
        public string? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string? RequisitionRequester { get; set; }
        public int? ERPCommodityCode { get; set; }
        public double? Price { get; set; }
        public string? PriceCurrency { get; set; }
        public double? UnitPrice { get; set; }
        public string? UnitPriceCurrency { get; set; }
        public string? RequisitionId { get; set; }
        public int? PurchaseGroup { get; set; }
        public string? PurchaseOrg { get; set; }
        public double? UnitPriceInBasePrice { get; set; }
        public int? CommodityCode { get; set; }
        public string? ExternalLineNumber { get; set; }
        public int? Version { get; set; }
        public DateTime? NeedByDate { get; set; }
        public string? AccountCategory { get; set; }
        public string? ItemCategory { get; set; }
        public int? ParentLineNumber { get; set; }
        public string? TaxCode { get; set; }
        public int? TaxPercentage { get; set; }
        public double? TaxAmountInBaseCurrency { get; set; }
        public string? LineTypeCategory { get; set; }
        public string? GLAccount { get; set; }
        public string? CostCenter { get; set; }
        public string? AssetCode { get; set; }
        public string? ContractId { get; set; }
        public double? ServiceMaxAmount { get; set; }
        public string? HeaderTittle { get; set; }
        public DateTime? ServiceStartDate { get; set; }
        public DateTime? ServiceEndDate { get; set; }
        public DateTime? OrderDate { get; set; }
        public string? Status { get; set; }
    }
}
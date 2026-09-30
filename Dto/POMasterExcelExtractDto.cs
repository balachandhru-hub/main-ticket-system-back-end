namespace Dto
{
    public class POMasterExcelExtractDto
    {
        public string POOrderId { get; set; }
        public string EPID { get; set; }
        public string LineItem { get; set; }
        public string? UnitOfMeasure { get; set; }
        public string? MaterialCode { get; set; }
        public double Quantity { get; set; }
        public Guid DomainId { get; set; }
        public string? RequisitionRequester { get; set; }
        public int? ERPCommodityCode { get; set; }
        public string? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public double Price { get; set; }
        public string? PriceCurrency { get; set; }
        public double? UnitPrice { get; set; }
        public string? RequisitionId { get; set; }
        public string? CompanyCode { get; set; }
        public string? UnitPriceCurrency { get; set; }
        public int? PurchaseGroup { get; set; }
        public string? PurchaseOrg { get; set; }
        public string? Description { get; set; }
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
        public DateTime? ServiceStartDate { get; set; }
        public DateTime? ServiceEndDate { get; set; }
        public int RowNumber { get; set; }
        public string? HeaderTitle { get; set; }
        public DateTime? OrderedDate { get; set; }
        public string? POStatus { get; set; }

    }
}
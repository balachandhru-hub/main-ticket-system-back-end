namespace Dto
{
    public class PRDetailsDto
    {
        public string PRNumber { get; set; }
        public string? PRTitle { get; set; }
        public string? Requester { get; set; }
        public string? CompanyCode { get; set; }
        public string? PurchaseOrg { get; set; }
        public string? Currency { get; set; }
        public int? LineNumber { get; set; }
        public string? Description { get; set; }
        public string? MaterialCode { get; set; }
        public string? CommodityCode { get; set; }
        public decimal? Quantity { get; set; }
        public string? UOM { get; set; }
        public DateTime? NeedByDate { get; set; }
        public string? SupplierId { get; set; }
        public string? CostCenter { get; set; }
        public string? GLAccount { get; set; }
        public string? DeliverTo { get; set; }
        public decimal? Price { get; set; }
        public string? PRType { get; set; }
        public DateTime? ServiceStartDate { get; set; }
        public DateTime? ServiceEndDate { get; set; }
    }
}
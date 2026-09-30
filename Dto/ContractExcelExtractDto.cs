namespace Dto
{
    public class ContractExcelExtractDto
    {
        public Guid DomainId { get; set; }
        public string? ContractName { get; set; }
        public string ContractId { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? TotalAmount { get; set; }
        public string? WorkSpaceId { get; set; }
        public string? AttachmentId { get; set; }
        public string? MaterialCode { get; set; }
        public string? CommodityCode { get; set; }
        public string? Description { get; set; }
        public string? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string? AmountSpend { get; set; }
        public string? BalanceAmount { get; set; }
        public int RowNumber { get; set; }
        public string? Currency { get; set; }
    }
}
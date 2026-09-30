namespace Dto
{
    public class PurchaseOrderCreateDto
    {
        public string PurchaseOrderId { get; set; }
        public string LineItem { get; set; }
        public string EPID { get; set; }
        public double Quantity { get; set; }
        public double? PrevQunatity { get; set; }
        public string? RecieptId { get; set; }
        public string? UnitOfMeasure { get; set; }
        public string? MaterialCode { get; set; }
    }
}
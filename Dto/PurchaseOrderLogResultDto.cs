namespace Dto
{
    public class PurchaseOrderLogResultDto
    {
        public Guid Id { get; set; }
        public string PurchaseOrderId { get; set; }
        public string LineItemId { get; set; }
        public string Result { get; set; }
    }
}
namespace Entities.Models
{
    public class AribaSupplierName : BaseModel
    {
        public Guid Id { get; set; }
        public string SupplierName { get; set; }
        public string SupplierId { get; set; }
        public Guid DomainId { get; set; }
          public AribaSupplierName()
        { }
    }
}
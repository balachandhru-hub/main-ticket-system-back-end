namespace Entities.Models
{
    public class PublicProperty : BaseModel
    {
        public Guid Id { get; set; }
        public string VenueName { get; set; }
        public string CompanyName { get; set; }
        public Guid DomainId { get; set; }
        public string CompanyCode { get; set; }
        public string CostCenter { get; set; }
        public string GLAccount { get; set; }
        public string ShipToPlant { get; set; }
        public string PurchaseOrg { get; set; }
        public string PurchaseGroup { get; set; }
        public string BillTo { get; set; }
          public PublicProperty()
        { }

    }
}
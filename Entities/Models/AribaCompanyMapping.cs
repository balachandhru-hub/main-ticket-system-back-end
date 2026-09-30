namespace Entities.Models
{
    public class AribaCompanyMapping : BaseModel
    {
        public Guid Id { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public Guid DomainId { get; set; }
    }
}
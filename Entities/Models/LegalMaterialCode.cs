namespace Entities.Models
{
    public class LegalMaterialCode : BaseModel
    {
        public Guid Id { get; set; }
 
        public Guid DomainId { get; set; }
        public string MaterialCode { get; set; }
        public string Description { get; set; }
        public string GLAccount { get; set; }
   
          public LegalMaterialCode()
        { }

    }
}
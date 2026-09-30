using System.ComponentModel.DataAnnotations;

namespace Entities.Models
{
    public class AribaInvoiceUpload: BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
      
        public string InvoiceNumber{ get; set; }
        public string Approver { get; set; }
        public Guid? AssetId { get; set; }
        public AribaInvoiceUpload()
        { }
    }
}
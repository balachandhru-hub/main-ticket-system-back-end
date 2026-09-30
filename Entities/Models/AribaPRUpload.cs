using System.ComponentModel.DataAnnotations;

namespace Entities.Models
{
    public class AribaPRUpload: BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
      
        public string PRNumber{ get; set; }
        public string Approver { get; set; }
        public Guid? AssetId { get; set; }
        public AribaPRUpload()
        { }
    }
}
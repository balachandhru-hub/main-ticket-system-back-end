using System.ComponentModel.DataAnnotations;

namespace Entities.Models
{
    public class AribaContractUpload: BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
      
        public string ContractTaskId{ get; set; }
        public string Approver { get; set; }
        public Guid? AssetId { get; set; }
        public AribaContractUpload()
        { }
    }
}
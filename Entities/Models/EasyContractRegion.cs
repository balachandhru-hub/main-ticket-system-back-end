using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    public class EasyContractRegion: BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("Domain")]
        public Guid DomainId { get; set; }
        public Domain Domain { get; set; }
        public string ContractId { get; set; }
        public string UniqueName { get; set; }
        public string Name { get; set; }
       

        public EasyContractRegion() { }
    }
}
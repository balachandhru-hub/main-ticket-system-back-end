using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
 
namespace Entities.Models
{
    public class DomainModel : BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
 
        [Required]
        [ForeignKey("Domain")]
        public Guid DomainId { get; set; }
        public Domain Domain { get; set; }
        public string ModelName { get; set; }
        public DomainModel()
        { }
    }
}
 
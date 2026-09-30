using System.ComponentModel.DataAnnotations;

namespace Entities.Models
{
    public class EmailFailedDetail : BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string? Email { get; set; }
        public string? EntityType { get; set; }
        public Guid? EntityId { get; set; }
        [Required]
        public string? EmailType { get; set; }
        public int TriggerCount { get; set; } 
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public EmailFailedDetail() { }
    }
}
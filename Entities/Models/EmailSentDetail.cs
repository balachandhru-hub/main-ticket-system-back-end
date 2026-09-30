using System.ComponentModel.DataAnnotations;

namespace Entities.Models
{
    public class EmailSentDetail : BaseModel
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
        public EmailSentDetail() { }
    }
}
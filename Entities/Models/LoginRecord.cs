using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    public class LoginRecord : BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; } 

        [Required]
        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public string? AttributeId { get; set; }

        [Required]
        public Guid DomainId { get; set; }

        public DateTime LoginTime { get; set; }

       public string? EntityType { get; set; }
        public string? IpAddress { get; set; }
        public string? UserName { get; set; }

        public LoginRecord() { }
    }
}

using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
namespace Entities.Models
{
    public class RequesterName : BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [ForeignKey("Domain")]
        public Guid DomainId { get; set; }
        public Guid UserId { get; set; }
        public string? Department { get; set; }
        public string? Name { get; set; }


        public RequesterName()
        {}
    }
}
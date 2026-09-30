using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
namespace Entities.Models
{
    public class AppModelMapping : BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("Domain")]
        public Guid DomainId { get; set; }

        public Guid AppId { get; set; }
        public AppModelMapping()
        {}
    }
}
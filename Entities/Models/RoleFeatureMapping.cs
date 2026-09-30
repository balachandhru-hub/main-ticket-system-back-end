using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
namespace Entities.Models
{
    public class RoleFeatureMapping : BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        public Guid FeatureId { get; set; }

        public Guid RoleId { get; set; }


        public RoleFeatureMapping()
        {}
    }
}
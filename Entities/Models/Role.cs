using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
namespace Entities.Models
{
    public class Role : BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        public string UserRole{ get; set; }


        public Role()
        {}
    }
}
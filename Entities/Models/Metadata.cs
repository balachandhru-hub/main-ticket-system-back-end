using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
namespace Entities.Models
{
    public class Metadata : BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        public string Key { get; set; }
        public string Type {get; set; }
        public string? Description { get; set; }

        public Metadata()
        { }
    }
}
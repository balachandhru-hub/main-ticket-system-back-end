using System;
using System.ComponentModel.DataAnnotations;
 
namespace Entities.Models
{
    public class Link : BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
 
        public string LinkUrl { get; set; }
 
        public string Type { get; set; }
 
        public Link() { }
    }
}
 
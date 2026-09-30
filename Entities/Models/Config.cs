using System.ComponentModel.DataAnnotations;

namespace Entities.Models
{
    public class Config : BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        public Config() { }
    }
}
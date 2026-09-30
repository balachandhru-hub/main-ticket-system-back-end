using System.ComponentModel.DataAnnotations;

namespace Entities.Models
{
    public class ApiKey : BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string ApiKeyValue { get; set; }
        public ApiKey()
        { }
    }
}
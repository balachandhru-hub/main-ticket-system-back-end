using System.ComponentModel.DataAnnotations;
namespace Entities.Models
{
    public class ApiConfig : BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string? Description { get; set; }

        [Required]
        public string BaseUrl { get; set; }

        [Required]
        public string? Username { get; set; }

        [Required]
        public string? Password { get; set; }

        public ApiConfig()
        {}

    }
}
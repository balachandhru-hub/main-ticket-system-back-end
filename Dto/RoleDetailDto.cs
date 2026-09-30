using System.ComponentModel.DataAnnotations;

namespace Dto
{
    public class RoleDetailDto
    {
        public Guid Id { get; set; }
        [Required]
        public string? UserRole { get; set; }
        public Guid? DomainId { get; set; }
    }
}
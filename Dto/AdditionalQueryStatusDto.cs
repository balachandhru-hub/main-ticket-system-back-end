using System.ComponentModel.DataAnnotations;

namespace Dto
{
    public class AdditionalQueryStatusDto
    {
        [Required]
        public string Status { get; set; }
    }
}

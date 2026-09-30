using System.ComponentModel.DataAnnotations;

namespace Dto
{
    public class TicketMessageRequestDto
    {
        [Required]
        public Guid TicketId { get; set; }
        [Required]
        public string Message { get; set; }
    }
}
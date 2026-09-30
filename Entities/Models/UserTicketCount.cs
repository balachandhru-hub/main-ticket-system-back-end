using System.ComponentModel.DataAnnotations;

namespace Entities.Models
{
    public class UserTicketCount : BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        public string Domain { get; set; }
        public int TotalHours { get; set; }
        public UserTicketCount() { }
    }
}
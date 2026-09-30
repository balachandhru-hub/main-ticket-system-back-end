using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    public class Ticket : BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public User User { get; set; }
        public Guid DomainId { get; set; }

        [Required]
        public string Title { get; set; }
        public string IPAddress { get; set; }
        public string Description { get; set; }
        [Required]
        public string Status { get; set; }
        public string Plan { get; set; }
        public string Category { get; set; }

        public Guid? AssignedTo { get; set; }
        public Guid? AssignedBy { get; set; }


        public decimal Amount { get; set; }
        public double TotalHours { get; set; }
        public bool IsApproved { get; set; }
        public string Priority { get; set; }
        public DateTime EndDate { get; set; }
        public string Type { get; set; }
        public bool IsExceed{ get; set; }
        public string? AdminDescription { get; set; }
        public bool IsPayable { get; set; }     
        public bool IsPaid { get; set; }  
        public bool IsCredited { get; set; }
        public string? TicketCode { get; set; }

        public Ticket()
        {
        }
    }
}
using System.ComponentModel.DataAnnotations;

namespace Dto
{
    public class TicketDetailDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        [Required]
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? Category { get; set; }
        public string? Priority { get; set; }
        public Guid? AssignedTo { get; set; }
        public Guid? AssignedBy { get; set; }
        public DateTime DateCreated { get; set; }
        public string? Name { get; set; }
        public double TotalHours { get; set; }
        public List<AssetDto>? Asset { get; set; }
        public List<TicketPRDetailDto>? PRDetail { get; set; }
        public string? AdminDescription { get; set; }
        public string? Email { get; set; }
        public string? Organization { get; set; }
        public string? AssignedToName { get; set; }
        public string? TicketCode { get; set; }
    }
}
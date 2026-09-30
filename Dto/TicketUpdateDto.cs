using System;
using System.ComponentModel.DataAnnotations;
using Entities.Common;

namespace Dto
{
    /// <summary>
    /// DTO for creating or updating a Ticket.
    /// </summary>
    public class TicketUpdateDto
    {
        [Required]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public Guid? AssignedTo { get; set; }
        public Guid? AssignedBy { get; set; }
        public string? Priority { get; set; }
        public string? Category { get; set; }
        public Guid DomainId { get; set; }
        public string? AdminDescription { get; set; }
        public List<AssetUploadDto>? Asset { get; set; }
 
    }
}
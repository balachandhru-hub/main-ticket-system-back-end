using System;
using System.ComponentModel.DataAnnotations;
using Entities.Common;

namespace Dto
{
    /// <summary>
    /// DTO for creating or updating a Ticket.
    /// </summary>
    public class TicketRequestDto
    {
        public Guid? UserId { get; set; }
        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; }
        public string? IPAddress { get; set; }
        public string Priority { get; set; }
        public string Category { get; set; } 
        public bool IsExceed { get; set; } = false;
        
        public  AssetUploadDto? Asset { get; set; }
    }
}
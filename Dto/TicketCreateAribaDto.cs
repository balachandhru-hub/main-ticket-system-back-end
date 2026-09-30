using System;
using System.ComponentModel.DataAnnotations;
using Entities.Common;

namespace Dto
{
    /// <summary>
    /// DTO for creating a Ticket.
    /// </summary>
    public class TicketCreateAribaDto
    {
        public Guid? UserId { get; set; }
        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; }
        public string Status { get; set; } = Common.INITIAL_STATUS_FOR_TICKET;
        [Required]
        [RegularExpression(@"^[\w\.-]+@(?!gmail\.com$)(?!gmail\.in$)[\w\.-]+\.(com|in)$",
        ErrorMessage = "Only company or organization emails are allowed. Gmail is not accepted.")]
        public string Email { get; set; }
        public string AribaUserName { get; set; }
        public string? IPAddress { get; set; }   
        public AssetUploadDto? Asset { get; set; }
    }
}
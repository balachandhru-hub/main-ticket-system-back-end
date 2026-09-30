using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
namespace Entities.Models
{
    public class AribaPRAttachment : BaseModel
    {
        [Key]
        public Guid Id { get; set; }
        public string RequisitionsId { get; set; }
        public string AribaTaskId { get; set; }
        public string AttachmentId { get; set; }
    }
}
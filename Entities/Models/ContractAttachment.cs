using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
namespace Entities.Models

{
    public class ContractAttachment : BaseModel
    {
        [Key]
        public Guid Id { get; set; }
        public string TaskId { get; set; }
   
        public string AttachmentId { get; set; }
    }
}
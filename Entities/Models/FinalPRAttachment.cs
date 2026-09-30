using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
namespace Entities.Models

{
    public class FinalPRAttachment : BaseModel
    {
        public Guid Id { get; set; }
        public string AribaId { get; set; }
        [Column(TypeName = "VARBINARY(MAX)")]
        public byte[] Attachment { get; set; }
        public string type { get; set; }
        public FinalPRAttachment() { }

    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
namespace Entities.Models
{
    public class ServicePRAttachment : BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        public string PRServiceUniqueName {get;set;}
        public Guid AssetId { get; set; }
        public string? Type { get; set; } 
        public string? InvoiceID { get; set; }
        public int? LineItemNumber { get; set; }
        public ServicePRAttachment()
        { }
    }
}
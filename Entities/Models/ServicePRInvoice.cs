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
    public class ServicePRInvoice : BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        public string? PRNumber { get; set; }   
        public string? InvoiceID { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public int? LineItemNumber { get; set; }
        public bool IsOCRAttachmentProcessed { get; set;}
        public string? InvoiceComment { get; set; }    
        public bool IsHeader { get; set; }          
        public ServicePRInvoice()
        { }
    }
}
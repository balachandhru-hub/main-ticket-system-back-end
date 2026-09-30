using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
namespace Entities.Models
{
    public class AribaInvoice : BaseModel
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string InvoiceId { get; set; }
        [ForeignKey("Domain")]
        public Guid DomainId { get; set; }
        public Domain Domain { get; set; }
        public string? InvoiceNumber { get; set; }
        public string? SupplierName { get; set; }
        public string? Currency { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? InvoiceDate { get; set; } 
        public decimal? Amount { get; set; } = 0;
        public string? Description { get; set; }
        public string? Department { get; set; }
        public string AribaTaskId { get; set; }
        public string? PONumber { get; set; }
        public string? ContractId { get; set; }       
        public string? CompanyUniqueName { get; set; }  
        public bool IsAttachmentProcessed { get; set; } 
        public decimal? InvoiceNetAmount { get; set; }
        public decimal? InvoiceTaxAmount { get; set; }
        public string? RequesterUser { get; set; }
        public ICollection<AribaTasks> Tasks { get; set; } = new List<AribaTasks>();
    }

    public class AribaTasks : BaseModel
    {
        [Key]
        public Guid Id { get; set; }

       
        public string ApprovalId { get; set; }
        [ForeignKey("Domain")]
        public Guid DomainId { get; set; }
        public Domain Domain { get; set; }

        [MaxLength(255)]
        public string Approver { get; set; }

        public string? Description { get; set; }
        public DateTime? AssignedDate { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public string FullUrl { get; set; }
        public DateTime? DueDate { get; set; }
        public string ApprovableType { get; set; }
        [Required]
        public string ApprovableUniqueName { get; set; }
        public string? PaymentStatus {get; set;}
        
      

        public ICollection<AribaAttachment> Attachments { get; set; } = new List<AribaAttachment>();
    }

    public class AribaAttachment : BaseModel
    {
        [Key]
        public Guid Id { get; set; }
        public string InvoiceID { get; set; }
        public string AribaTaskId { get; set; }
        
        public string AttachmentId { get; set; }
    }
    public class AribaInvoiceApprovalHistory : BaseModel
    {
        [Key]
        public Guid Id { get; set; }
        public string InvoiceID { get; set; }
        public string AribaTaskId { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string? AribaUser { get; set; }
        public string? Comments { get; set; }
        public string? Level { get; set; }
        public string? Status { get; set; }
    }
}
using Entities.Models;

namespace Dto
{
    public class AribaInvoiceDto
    {
        public string InvoiceId { get; set; }

        // Invoice number or reference from Ariba
        public string InvoiceNumber { get; set; }

        // Name of the supplier for this invoice
        public string SupplierName { get; set; }
        public string? RequesterName { get; set; }
       // public string? requester { get; set; }

        // Due date of the invoice
        public DateTime? DueDate { get; set; }
        public DateTime? InvoiceDate { get; set; }

        public string PONumber { get; set; }
        public string ContractId { get; set; }
        public decimal? Amount { get; set; }


        public string AribaTaskId { get; set; }
        public string Description { get; set; }
        public string Department { get; set; }
        public string Currency { get; set; }
        public string? Status { get; set; }
        public decimal? ApprovedInvoiceAmount { get; set; } = 0;
        public string? CompanyUniqueName { get; set; }
        public decimal? InvoiceNetAmount { get; set; }
        public decimal? InvoiceTaxAmount { get; set; }
public string? PaymentStatus {get; set;}
        public string WorkSpaceId { get; set; }
        public string AttachmentId { get; set; }
        public DateTime? DateCreated { get; set; }
        public List<AribaTask> Tasks { get; set; } = new List<AribaTask>();


        public List<string> Attachments { get; set; } = new List<string>();
        public List<AribaInvoiceApprovalHistory> ApprovalHistory { get; set; } = new List<AribaInvoiceApprovalHistory>();
        public InvoiceContractDto Contract { get; set; } = new InvoiceContractDto();
        public bool HasAttachment { get; set; }
    }
    public class AribaInvoiceDetailDto
    {
        public string InvoiceId { get; set; }

        // Invoice number or reference from Ariba
        public string InvoiceNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }

        // Name of the supplier for this invoice
        public string SupplierName { get; set; }
        public string CompanyUniqueName { get; set; }
        public decimal? InvoiceAmount { get; set; }
        public string? Currency { get; set; }
        public string AribaTaskId { get; set; }
        public string ContractId { get; set; }
        public string WorkSpaceId { get; set; }
        public string AttachmentId { get; set; }
        public string Description { get; set; }
        public string Department { get; set; }
        public string? TotalAmount { get; set; }
        public string? ContractValue { get; set; }
        public string? ValueAlreadyPaid { get; set; }
        public string? RemainingBalance { get; set; }
        public decimal? InvoiceTaxAmount { get; set; }
        public decimal? InvoiceNetAmount { get; set; }
        public string? PONumber { get; set; }
        public string? EPID { get; set; }
        // public string? Epid { get; set; }
        public string? PurchaseOrderId { get; set; }
        public DateTime? DueDate { get; set; }
        public bool HasAttachment { get; set; }
        public string? ContractCurrency { get; set; }
        public string Approver { get; set; }
        public List<AribaInvoiceApprovalHistory> ApprovalHistory { get; set; } = new List<AribaInvoiceApprovalHistory>();
        public string? Status { get; set; }
        public string? FullUrl { get; set; }
        public string?GLAccount { get; set; }
        public string?CostCenter { get; set; }
        public string? Entity { get; set; }
        public string? PaymentStatus {get; set;}
       
     
    }
}
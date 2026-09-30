using Entities.Models;

namespace Dto
{
  public class AribaPRDto
  {

    public string RequisitionsId { get; set; }

    public string? SupplierName { get; set; }
    public string? Currency { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? RequisitionsDate { get; set; }
    public decimal? Amount { get; set; } = 0;
    public DateTime? NeedBYDate { get; set; }
    public string? Description { get; set; }
    public string? Department { get; set; }
    public string AribaTaskId { get; set; }
    public double? PRTaxAmount { get; set; }
    public double? PRNetAmount { get; set; }

    public string? ContractId { get; set; }
    public string? CompanyUniqueName { get; set; }
    public string? RequesterName { get; set; }


    public List<AribaPRTask> Tasks { get; set; } = new List<AribaPRTask>();
    public List<AribaPRHistory> ApprovalHistory { get; set; } = new List<AribaPRHistory>();
    public InvoiceContractDto Contract { get; set; } = new InvoiceContractDto();
    public List<string> Attachments { get; set; } = new List<string>();
    public string? ContractCurrency { get; set; }
    public string? ContractValue { get; set; }

    public string? ValueAlreadyPaid { get; set; }
    public string? RemainingBalance { get; set; }
    public string? WorkSpaceId { get; set; }
    public string? AttachmentId { get; set; }
    public bool HasAttachment { get; set; }
  }
       public class AribaPRDetailDto
    {
      
      public string RequisitionsId { get; set; }
      public DateTime? RequisitionsDate { get; set; }
      public string? RequesterName { get; set; }

      // Name of the supplier for this invoice
      public string SupplierName { get; set; }
      public string CompanyUniqueName { get; set; }
      public decimal? Amount { get; set; }
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
      public double? PRTaxAmount { get; set; }
      public double? PRNetAmount { get; set; }
     
 public DateTime? NeedBYDate { get; set; }
      public DateTime? DueDate { get; set; }
      public bool HasAttachment { get; set; }
      public string? ContractCurrency { get; set; }
      public string Approver { get; set; }
      public List<AribaPRHistory> ApprovalHistory { get; set; } = new List<AribaPRHistory>();
      public string? Status { get; set; }
      public string? FullUrl { get; set; }
      public string? GLAccount { get; set; }
      public string? CostCenter { get; set; }
      public string? PaymentStatus {get;set;}

    }
    public class ExportExcelResultDto
{
    public byte[] FileContent { get; set; }
    public string FileName { get; set; }
}



    }

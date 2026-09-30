using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;   
using Newtonsoft.Json;
using Dto;

namespace Dto
{


    public class RequisitionDetailDto
    {
        public bool IsNonPO { get; set; }
        public string ApprovedState { get; set; }
        public string InitialUniqueName { get; set; }
        public DateTime SubmitDate { get; set; }
        public SupplierLocationDto SupplierLocation { get; set; }
        public DateTime CreateDate { get; set; }
        public string Name { get; set; }
     //   public string DomainId { get; set; }
     //   public string poNumber { get; set; }
    //    public string ContractId { get; set; }
        public SupplierLocationDto RemitToAddress { get; set; }
        public DateTime LastModified { get; set; }
        public DateTime TimeCreated { get; set; }
        public AmountDto TotalCost { get; set; }
      
        public RequesterDto Requester { get; set; }
        public string InvoiceNumber { get; set; }
        public AmountDto TotalInvoicedLessTax { get; set; }
        public AmountDto TaxAmount { get; set; }
        public string RemitToID { get; set; }
        public string InvoiceExternalStatusString { get; set; }
        public OrderDto Order { get; set; }
        public List<LineItemDto> LineItems { get; set; }
        public AmountDto TotalWithholdingTax { get; set; }
     

        public string InvoiceOrigin { get; set; }
        public bool Signed { get; set; }
        public int PreviousApprovalRequestsVersion { get; set; }
        public PurchaseOrgDto PurchaseOrg { get; set; }
        public bool IsTaxInLine { get; set; }
        public List<ApprovalRequestDto> ApprovalRequests { get; set; }
        public bool Active { get; set; }
        public string InvoiceStatusString { get; set; }
        public string InvoiceSourceDocument { get; set; }
        public decimal TaxExchangeRate { get; set; }
        public bool HeaderLevel { get; set; }
        public string UniqueName { get; set; }
        public string CompanyUniqueName { get; set; }
        public AmountDto TotalInvoiced { get; set; }
        public int Category { get; set; }
        public string InvoiceSubmissionMethod { get; set; }
        public string ProjectID { get; set; }
        public string InvoicePurpose { get; set; }
        public PaymentTermsDto PaymentTerms { get; set; }
        public List<AttachmentDto> Attachments { get; set; }
        public string AccountReceivableID { get; set; }
        public AmountDto TotalTax { get; set; }
        public SupplierDto Supplier { get; set; }
        public DateTime InvoiceDate { get; set; }
           public DateTime RequisitionsDate { get; set; }
        public string StatusString { get; set; }
        public string FullURL { get; set; }
        public int ApprovalRequestsVersion { get; set; }
        public string UpperCaseInvoiceNumber { get; set; }
        public DateTime TimeUpdated { get; set; }
        public int VersionNumber { get; set; }
        public string Type { get; set; }
        public string ProjectTitle { get; set; }
        public int InvoiceOriginType { get; set; }
        public string SupplierName { get; set; }
        public string ApprovableId { get; set; }
        public string InvoiceState { get; set; }
        public RequesterDto Preparer { get; set; }
        public CompanyCodeDto CompanyCode { get; set; }
    }

 
    public class SupplierLocationDto
    {
        public string State { get; set; }
        public string Phone { get; set; }
        public CountryDto Country { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string Fax { get; set; }
        public string UniqueName { get; set; }
        public string Name { get; set; }
        public string Lines { get; set; }
    }
 
    public class CountryDto
    {
        public string UniqueName { get; set; }
    }

    public class AmountDto
    {
        public decimal AmountInReportingCurrency { get; set; }
        public decimal ApproxAmountInBaseCurrency { get; set; }
        public decimal Amount { get; set; }
        public DateTime ConversionDate { get; set; }
        public CurrencyDto Currency { get; set; }
        
    }

    public class CurrencyDto
    {
        public string UniqueName { get; set; }
        
    }
 
    public class RequesterDto
    {
        public string UniqueName { get; set; }
        public string Name { get; set; }
        public string PasswordAdapter { get; set; }
    }
 
    public class OrderDto
    {
        public string UniqueName { get; set; }
    }
 
    public class LineItemDto
    {
        public int InvoiceLineNumber { get; set; }
        public string Description { get; set; }
        public AmountDto Amount { get; set; }
        public decimal Quantity { get; set; }
    }
 
    public class ApprovalRequestDto
    {
        public string State { get; set; }
        public RequesterDto Approver { get; set; }
        public RequesterDto ApprovedBy { get; set; }
        public string RuleName { get; set; }
        public string ApproverComment { get; set; }
        public DateTime? ActivationDate { get; set; }
        public DateTime? LastModified { get; set; }
    }
 
    public class PurchaseOrgDto
    {
        public string UniqueName { get; set; }
    }
 
    public class PaymentTermsDto
    {
        public string UniqueName { get; set; }
    }
 
    public class AttachmentDto
    {
       
        public string Id { get; set; }
     
    }
 
    public class SupplierDto
    {
        public string UniqueName { get; set; }
        public string Name { get; set; }
        public string smVendorID { get; set; }
    }

    public class CompanyCodeDto
    {
        public string UniqueName { get; set; }
    }

}
   


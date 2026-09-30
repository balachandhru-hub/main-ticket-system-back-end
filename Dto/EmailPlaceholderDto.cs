using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dto
{
  public class EmailPlaceholderDto
  {
    public Guid? EntityId { get; set; }
    public string? EntityType { get; set; }
    public string? EmailType { get; set; }
    public string? ToEmail { get; set; }
    public string? Subject { get; set; }
    public string? EmailBody { get; set; }
    public string? PersonName { get; set; }
    public string? TicketId { get; set; }
    public DateOnly? Date { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? CompanyName { get; set; }
    public string? SupportEmail { get; set; }
    public string? PortalLink { get; set; }
    public string? SupportAdminEmail { get; set; }
    public string? DaysLeft { get; set; }
    public string? ExpiryDate { get; set; }

    public string? CompanyLogo { get; set; }
    public string? DomainCompanyName { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? CompanyLocation { get; set; }
    public string? CinNo { get; set; }
    public string? Country { get; set; }
    public string? BusinessType { get; set; }
    public string? IndustryType { get; set; }
    public string? BillingType { get; set; }
    public string? UserName { get; set; }
    public string? UserSecret { get; set; }
    public string? Password => UserSecret;
    public string? BillingAddress { get; set; }
    public string? InvoiceId { get; set; }
    public string? SupplierName { get; set; }
    public string? SupplierId { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? DueDate { get; set; }

    public string? RequisitionId { get; set; }
    public string? RequesterName { get; set; }


    public string? ContractTaskId { get; set; }

    public decimal? ContractTotalAmount { get; set; }
    public DateTime? ContractBeginDate { get; set; }
    public DateTime? ContractEndDate { get; set; }
    public string? ContractCurrency { get; set; }
    public decimal? ContractAmountSpend { get; set; }
    public decimal? ContractBalanceAmount { get; set; }

    //----- Domain Plan -----
    public string? DomainPlanName { get; set; }
    public string? DomainPlanEngagementType { get; set; }
    public DateTime DomainPlanStartDate { get; set; }
    public DateTime DomainPlanEndDate { get; set; }
    public string? DomainPlanPaymentType { get; set; }
    public Guid? DomainPlanDiscountCouponId { get; set; }
    public DateTime DomainPlanContractStartDate { get; set; }
    public DateTime DomainPlanContractEndDate { get; set; }
    public int DomainPlanNumberOfDues { get; set; }

    //----- Material PR -----
    public string? PRNumber { get; set; }
    public string? Vendor { get; set; }
    public string? VendorNamePR { get; set; }
    public string? InvoiceIdPR { get; set; }
    public string? ReceiptId { get; set; }
    public string? StatusPR { get; set; }
    public string? PONumber { get; set; }
    
  }
}

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
    public class ServicePR : BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [ForeignKey("Domain")]
        public Guid DomainId { get; set; }
        public Domain Domain { get; set; }
        public string? PRNumber { get; set; }
        public string? PurchaseOrg { get; set; }
        public string? Description { get; set; }
        public string? CommodityCode { get; set; }
        public string? AccountType { get; set; }
        public string? Type { get; set; }
        public decimal Amount { get; set; }
        public DateTime DeliveryDate { get; set; }
        public decimal Quantity { get; set; }
        public string? UOM { get; set; }
        public string Vendor { get; set; }
        public string? Contract { get; set; }
        public string? MaterialCode { get; set; }

        public DateTime ServiceStartDate { get; set; }
        public DateTime ServiceEndDate { get; set; }
        public decimal MaxAmount { get; set; }
        public decimal ExpectedAmount { get; set; }
        public string Title { get; set; }
        public string? Currency { get; set; }
        public string? PreparerName { get; set; }
        public string? GLAccount { get; set; }
        public string? CostCenter { get; set; }
        public string? ShipToPlant { get; set; }
        public string? CompanyCode { get; set; }
        public string PurchaseGroup { get; set; }
        public string? BillTo { get; set; }
        public string? TaxCode { get; set; }
        public string DeliverTo { get; set; }
        public string? ErrorComment { get; set; }
        public string? Status { get; set; }
        public DateTime NeedBy { get; set; }
        public bool IsBlocked { get; set; }
        public int? ProcessedState { get; set; }
        public int? LineItemNumber { get; set; }
        public string PRServiceUniqueName { get; set; }
        public int Counter { get; set; }
        public string? PreparerPasswordAdapter { get; set; }
        public string? VendorName { get; set; }
        public int? ParentLineItemNumber { get; set; }
        public string? Category { get; set; }
         public string? PONumber { get; set; }
        public bool AutoSES { get; set; }
        public bool AutoInvoices { get; set; }
        public bool TaxInclude { get; set; }
        public string? Venue { get; set; }
        public string? HeaderComment { get; set; }
        public string? InvoiceID { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public bool IsOCRAttachmentProcessed { get; set; }
        public string? InvoiceComment { get; set; }
        public string? SSID { get; set; }
        public bool IsSESProcessed {get;set;}
        public bool? TeamReceived { get; set; }
        public ServicePR()
        { }
    }
}


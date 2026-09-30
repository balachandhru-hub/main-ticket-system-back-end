using System;
using System.Collections.Generic;

namespace Dto
{
    public class GetMaterialPRDto
     {
       
     

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
        public string? Vendor { get; set; }
        public string? Contract { get; set; }
        public string? MaterialCode { get; set; }
        public string? Title { get; set; }
        public string? Currency { get; set; }
        public string? RequesterName { get; set; }
        public string? GLAccount { get; set; }
        public string? CostCenter { get; set; }
        public string? ShipToPlant { get; set; }
        public string? CompanyCode { get; set; }
        public string? PurchaseGroup { get; set; }
        public string? BillTo { get; set; }
        public string? TaxCode { get; set; }
        public string? DeliverTo { get; set; }
        public string? HeaderComment { get; set; }

        public string? ErrorComment { get; set; }
        public string? Status { get; set; }
        public DateTime NeedBy { get; set; }
        public bool IsBlocked { get; set; }
        public int? ProcessedState { get; set; }
        public int? LineItemNumber { get; set; }
        public string? PRUniqueName { get; set; }
        public int Counter { get; set; }

       public bool AutoGRN { get; set; }
        public bool AutoInvoices { get; set; }
        public string? Venue { get; set; }
        public string? ReciptID { get; set; }
        public string? InvoiceID { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string? PONumber { get; set; }
        public bool TaxInclude { get; set; }
        public string? VendorName { get; set; }
    }

}
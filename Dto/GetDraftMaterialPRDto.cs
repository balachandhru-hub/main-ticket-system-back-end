using System;
using System.Collections.Generic;

namespace Dto
{
    public class DraftMaterialPRDetailsDto
    {
        public string DraftID { get; set; }
        public string? PRUniqueName { get; set; }
        public string? Title { get; set; }
        public DateTime? NeedBy { get; set; }
        public string? HeaderComment { get; set; }
        public string? DeliverTo { get; set; }
        public string? RequesterName { get; set; }

        public int LineItemCount { get; set; }

        public List<AssetDto>? HeaderAttachments { get; set; } = new();
        public List<DraftMaterialPRLineItemDto>? LineItems { get; set; } = new();
    }

    public class DraftMaterialPRLineItemDto
    {
        public int? LineItemNumber { get; set; }
        public string? Description { get; set; }
        public decimal? Quantity { get; set; }
        public string? UOM { get; set; }
        public decimal? Amount { get; set; }
        public string? Currency { get; set; }

        public string? MaterialCode { get; set; }
        public string? Vendor { get; set; }
        public string? VendorName { get; set; }

        public string? GLAccount { get; set; }
        public string? CostCenter { get; set; }

        public bool? AutoGRN { get; set; }
        public bool? AutoInvoices { get; set; }
        public string? TaxCode { get; set; }
        public string? CommdityCode {get ; set;}
        public string? CompanyCode {get;set;}
        public string? InvoiceComment{get ; set;}
        public DateTime? InvoiceDate{get;set;}
        public string? InvoiceID{get;set;}
        public string? PurchaseGroup{get;set;}
        public string? PurchaseOrg{get;set;}
        public string? Type{get;set;}
        public string? AccountType{get; set;}
        public string? ShipToPlant{get;set;}
        public string? BillTo{get;set;}
        public string? Contract{get;set;}
        public string? DeliverTo{get;set;}
        public bool? IsOCRAttachmentProcessed{get; set;}
        public bool? TaxInclude{get;set;}
        public string? ErrorComment{get; set;}
        public int? ParentLineItem { get; set; }
        public double? Percentage { get; set; }
        public List<AssetDto>? InvoiceAttachments { get; set; } = new();
        public string? Venue {get; set;}
        public string? Category {get; set;}
        public string? AribaUserId { get; set; }
        public bool? TeamReceived { get; set; }
        public string? Title { get; set; }
    }


}

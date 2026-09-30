
using System;
using System.Collections.Generic;

namespace Dto
{

    public class ServicePRResponseDto
    {
        public string PRNumber { get; set; }
        public string PRServiceUniqueName { get; set; }
        public string Name { get; set; }
        public DateTime NeedBy { get; set; }
        public string HeaderComment { get; set; }
        public string Status { get; set; }
       public bool? IsHeader {get;set;}
        public List<ServicePRLineItemResponseDto> LineItems { get; set; } = new();
        public List<AssetDto> Attachments { get; set; } = new(); // Header-level attachments
    }

    public class ServicePRLineItemResponseDto
    {
        public int? LineItemNumber { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public decimal Quantity { get; set; }
        public string UOM { get; set; }
        public string Vendor { get; set; }
        public string MaterialCode { get; set; }
        public string GLAccount { get; set; }
        public string CostCenter { get; set; }
        public string PurchaseOrg { get; set; }
        public string PurchaseGroup { get; set; }
        public string CompanyCode { get; set; }
        public string ShipToPlant { get; set; }
        public string DeliverTo { get; set; }
        public string BillTo { get; set; }
        public string TaxCode { get; set; }
        public string Contract { get; set; }
        public string AccountType { get; set; }
        public string Type { get; set; }
        public bool AutoSES { get; set; }
        public bool AutoInvoices { get; set; }
        public bool TaxInclude { get; set; }
        public string VendorName { get; set; }
        public string? InvoiceID { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string InvoiceComment { get; set; }
        public string? ReciptID { get; set; }
        public string? PONumber { get; set; }
        public bool IsOCRAttachmentProcessed { get; set; }
        public string CommodityCode {get;set;}
        public string Venue {get;set;}
        public string Currency {get;set;}
        public string? ExtractedInvoiceNumber{get;set;}
        
        public string? Category { get; set; }
        public DateTime ServiceStartDate { get; set; }
        public DateTime ServiceEndDate { get; set; }
        public decimal MaxAmount { get; set; }
        public decimal ExpectedAmount { get; set; }
        public string SSID { get; set; }
        public List<AssetDto> InvoiceAttachments { get; set; } = new();

    }

 



}
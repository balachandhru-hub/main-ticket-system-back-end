using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;


namespace Dto
{
    public class MaterialPRDto
    {
        public Guid DomainId { get; set; }
        public string? PRNumber { get; set; }
        public string? Server { get; set; }
        public string? Variant { get; set; }
        public Guid UserId { get; set; }
        public string? Partition { get; set; }
        public string? PRUniqueName { get; set; }

        // Header level info
        public string? UniqueName { get; set; }
        public string? Name { get; set; }
        public string? Preparer { get; set; }
        public string? PasswordAdapter { get; set; }
        public string? AribaUserName { get; set; }
        public string? Requester { get; set; }
        public string? Description { get; set; }
        public string? RequesterPasswordAdapter { get; set; }
        public string? CompanyCode { get; set; }
        public string? Title { get; set; }
        public string? PurchaseGroup { get; set; }
        public string? ShipToPlant { get; set; }
        public string? DeliverTo { get; set; }
        public string? HeaderComment { get; set; }


        public bool IsHeader { get; set; }
        // Default line item
        public string? DefaultDeliverTo { get; set; }
        public DateTime NeedBy { get; set; }
        public string? Venue { get; set; }



        // Ariba credentials
        public string? AribaEndPoint { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Operation { get; set; }

        // List of line items
        public List<MaterialPRLineItemDto> LineItems { get; set; }
        public List<LineItemAttachmentDto>? Attachments { get; set; }
        public List<AssetUploadDto> InvoiceAttachment { get; set; }
        public string? SystemReferenceID { get; set; }
        public string? PRType { get; set; }
        public bool? TeamReceived { get; set; }
        public string? Type { get; set; }


    }

    public class MaterialPRLineItemDto
    {
        public int? NumberInCollection { get; set; }
        public decimal Quantity { get; set; }
        public int? LineItemNumber { get; set; }
        public string? DeliverTo { get; set; }
        public DateTime NeedBy { get; set; }
        public string? Type { get; set; }
        public string? Venue { get; set; }

        // Description block
        public string? Description { get; set; }
        public string? ManPartNumber { get; set; }
        public string? Vendor { get; set; }
        public string? Contract { get; set; }
        public string? SupplierPartAuxiliaryID { get; set; }
        public string? UOM { get; set; } // e.g., KG, EA
        public decimal Price { get; set; }
        public bool AutoGRN { get; set; }
        public bool AutoInvoices { get; set; }

        public string? Currency { get; set; } // e.g., AED
        public string? CommodityDomain { get; set; }
        public string? CommodityCode { get; set; }
        public string? PreparerPasswordAdapter { get; set; }
        public string? RequesterPasswordAdapter { get; set; }
        // Custom
        public string? MaterialCode { get; set; }

        // Other attributes
        public string? ItemCategory { get; set; }
        public string? PurchaseOrg { get; set; }
        public string? PurchaseGroup { get; set; }
        public string? ShipTo { get; set; }
        public string? BillingAddress { get; set; }
        public string? Supplier { get; set; }
        public string? SupplierLocation { get; set; }

        // Accounting
        public string? CostCenter { get; set; }
        public string? CompanyCode { get; set; }
        public string? GLAccount { get; set; }
        public string? ProcurementUnit { get; set; }

        public string? GLAccountCompanyCode { get; set; }  // <-- Add this
        public string? CostCenterCompanyCode { get; set; }  // <-- Add this
        public string? AccountType { get; set; }
        public decimal Amount { get; set; }
        public string? Title { get; set; }
        public string? ShipToPlant { get; set; }
        public string? BillTo { get; set; }
        public string? TaxCode { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public bool TaxInclude { get; set; }
        public string? VendorName { get; set; }
        public string? InvoiceID { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string? InvoiceComment { get; set; }
        public string? ErrorComment { get; set; }
        public int? ParentLineItem { get; set; }
        public double? Percentage { get; set; }

        public List<AssetUploadDto>? InvoiceAttachment { get; set; }
        public int? ParentLineItemNumber { get; set; }
        public string? CategoryName { get; set; }
        public string? AribaUserId { get; set; }

    }
    public class LineItemAttachmentDto
    {
        public string FileName { get; set; }
        public string Extension { get; set; }
        public string ContentType { get; set; }
        public string? Base64Content { get; set; }
        public string UploadedBy { get; set; }
        public string PasswordAdapter { get; set; }
        public byte[] FileBytes { get; set; }
        public string? type { get; set; }

    }






}



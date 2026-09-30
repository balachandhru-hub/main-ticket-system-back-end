using System;
using System.Collections.Generic;

namespace Dto
{

    public class ServicePRDto
    {

        public Guid DomainId { get; set; }
        public Guid UserId { get; set; }
        public string? PRNumber { get; set; }
        public string? PRServiceUniqueName { get; set; }
        public string? Server { get; set; }
        public string? Variant { get; set; }
        public string? Partition { get; set; }
        public string? Operation { get; set; }
        public string? OriginatingSystem { get; set; }
        public string? OriginatingSystemReferenceID { get; set; }
        public string? UniqueName { get; set; }
        public string? Name { get; set; }
        public string? Title { get; set; }

        public string? Preparer { get; set; }
        public string? PasswordAdapter { get; set; }
        public string? AribaUserName { get; set; }
        public string? Venue { get; set; }
        public bool IsHeader { get; set; }
        public string? HeaderComment { get; set; }


        public string? CompanyCode { get; set; }
        public string? DeliverTo { get; set; }
        public DateTime NeedBy { get; set; }
        public string? ImportedHeaderCommentStaging { get; set; }
        public bool ImportedHeaderExternalCommentStaging { get; set; }
        public string? AribaEndPoint { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? ServicePartition { get; set; }
        public string? ServiceVariant { get; set; }
        public DateTime ServiceStartDate {get;set;}
        public string? RequesterName { get; set; }



        // List of line items
        public List<ServicePRLineItemDto> LineItems { get; set; }
        public List<LineItemAttachmentDto>? Attachments { get; set; }
        public List<AssetUploadDto> InvoiceAttachment { get; set; }
        public string? SystemReferenceID { get; set; }
        public string? PRType { get; set; }
        public bool? TeamReceived { get; set; }
        public string? Type { get; set; }
    }

    public class ServicePRLineItemDto
    {

        public int? NumberInCollection { get; set; }
        public int? OriginatingSystemLineNumber { get; set; }
        public decimal Quantity { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string? BillingAddress { get; set; }
        public string? ShipToPlant { get; set; }
        public string? DeliverTo { get; set; }
        public string? CommodityCode { get; set; }
        public string? CommodityDomain { get; set; }
        public string? Description { get; set; }
        public decimal ConversionFactor { get; set; } = 1;
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public string? PriceBasisQuantityDesc { get; set; }
        public string? PriceBasisQuantityUOM { get; set; }
        public string? UOM { get; set; }
        public string? PurchaseOrg { get; set; }
        public string? PurchaseGroup { get; set; }
        public string? CostCenter { get; set; }
        public string? GLAccount { get; set; }
        public decimal ExpectedAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public string? ImportedItemCategoryStaging { get; set; }
        public string? Type { get; set; }
        public string? ImportedAccountCategoryStaging { get; set; }

        public DateTime NeedBy { get; set; }
        public string? CompanyCode { get; set; }

        public bool IsServiceChild { get; set; } = false;
        public DateTime ServiceStartDate { get; set; }
        public DateTime ServiceEndDate { get; set; }

        public string? Vendor { get; set; }
        public string? Contract { get; set; }
        public string? MaterialCode { get; set; }
        public string? TaxCode { get; set; }
        public string? VendorName { get; set; }
        public bool AutoSES { get; set; }
        public bool AutoInvoices { get; set; }
        public bool TaxInclude { get; set; }
        public string? ErrorComment { get; set; }
        public string? PONumber { get; set; }
        public int? LineItemNumber { get; set; }
        public string? Category { get; set; }
        public string? BillTo { get; set; }
        public string? Venue { get; set; }
        public string? AccountType { get; set; }
        public string? Title { get; set; }
        public List<AssetUploadDto>? InvoiceAttachment { get; set; }
        public bool IsOCRAttachmentProcessed { get; set; }
        public string? InvoiceComment { get; set; }
        public string? InvoiceID { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public bool? AutoGRN { get; set; }
        public int? ParentLineItemNumber { get; set; }

        public decimal? Percentage { get; set; }

        public string? CategoryName { get; set; }



    }






}

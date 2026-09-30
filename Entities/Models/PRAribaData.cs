using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    public class PRAribaData : BaseModel
    {
        public Guid Id { get; set; }
        [ForeignKey("Domain")]
        public Guid DomainId { get; set; }
        public Domain Domain { get; set; }
        public string PRNumber { get; set; }
        public string? PRTitle { get; set; }
        public string? Status { get; set; }
        public DateTime? AribaCreatedDate { get; set; }
        public DateTime? AribaSubmitDate { get; set; }
        public DateTime? AribaApprovedDate { get; set; }
        public DateTime? AribaOrderedDate { get; set; }
        public DateTime? AribaLastModifiedDate { get; set; }
        public string? Preparer { get; set; }
        public string? Requester { get; set; }
        public string? CompanyCode { get; set; }
        public string? ProcurementUnit { get; set; }
        public string? PurchaseOrg { get; set; }
        public string? ErpPoNumber { get; set; }
        public double? TotalCost { get; set; }
        public string? Currency { get; set; }
        public double? ReportingCost { get; set; }
        public double? TaxAmount { get; set; }
        public double? TaxReporting { get; set; }
        public int? LineNumber { get; set; }
        public string? SapLineNumber { get; set; }
        public string? Description { get; set; }
        public string? ItemDescription { get; set; }
        public string? MaterialCode { get; set; }
        public string? CommodityCode { get; set; }
        public string? CommodityName { get; set; }
        public double? Quantity { get; set; }
        public string? UOM { get; set; }
        public DateTime? NeededBy { get; set; }
        public double? UnitPrice { get; set; }
        public double? LineTotal { get; set; }
        public double? LineReporting { get; set; }
        public double? Tax { get; set; }
        public string? Supplier { get; set; }
        public string? SupplierPartNumber { get; set; }
        public string? CostCenter { get; set; }
        public string? CostCenterDesc { get; set; }
        public string? GlAccount { get; set; }
        public string? GlDescription { get; set; }
        public string? DeliveryTo { get; set; }
        public string? ShippedTo { get; set; }
        public string? Approver { get; set; }
        public string? ApproverRole { get; set; }
        public string? ApprovalState { get; set; }
        public string? ApprovalDate { get; set; }
        public string? PendingApprovalWith { get; set; }
        public string? ApprovedBy { get; set; } 

    }
}
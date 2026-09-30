using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dto
{
    public class RequisitionDto
    {
        public string ApprovableId { get; set; }
        public MoneyDto TotalCost { get; set; }
        public string FullUrl { get; set; }
      
        public string SupplierName { get; set; }
        public string CompanyUniqueName { get; set; }
        public DateTime RequisitionsDate { get; set; }  // use DateTime if
        public string Currency { get; set; }
        public DateTime InvoiceDate { get; set; }  // use DateTime if
        public List<LineItemsDto> LineItems { get; set; }
        public string Realm { get; set; }
        public int Version { get; set; }
        public List<CommentDto> Comments { get; set; }
        public UserDto Requester { get; set; }
        public string FirstActiveApproverName { get; set; }
        public long TimeUpdated { get; set; }   // use DateTime if you’ll convert epoch
        public string Title { get; set; }
        public int ActiveApproverCount { get; set; }
        public string Site { get; set; }
        public List<RecordDto> Records { get; set; }
        public string RequesterName { get; set; }
        public string ActiveApprovalRequestIds { get; set; }
        public string StatusString { get; set; }
        public long LastModified { get; set; }
        public string UniqueName { get; set; }
        public List<ApprovalRequestsDto> ApprovalRequests { get; set; }
        public int LineItemCount { get; set; }
        public long SubmitDate { get; set; }
         public List<AttachmentDto> Attachments { get; set; }
        public List<object> Messages { get; set; }
        public bool IsPinned { get; set; }
    }

    public class MoneyDto
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }
    }
    public class CommentDto
{
    public long Date { get; set; }            // in milliseconds (Unix)
    public string RequisitionId { get; set; }
    public string Id { get; set; }
    public string Text { get; set; }
    public int AttachedToLine { get; set; }
}


    public class LineItemsDto
    {
        public SuppliersDto Supplier { get; set; }
        public NamedDto ShipTo { get; set; }
        public NamedDto SupplierLocation { get; set; }
        public MoneyDto Amount { get; set; }
        public DescriptionDto Description { get; set; }
        public CommodityExportMapEntryDto CommodityExportMapEntry { get; set; }
        public string RequisitionId { get; set; }
        public long NeedBy { get; set; }
        public NamedDto BillingAddress { get; set; }
        public decimal Quantity { get; set; }
        public string SplitAccountingType { get; set; }
        public int NumberInCollection { get; set; }
        public string DeliverTo { get; set; }
        public string CommonCommodityCode { get; set; }
        public List<AccountingDto> Accountings { get; set; }
        public AccountCategoryDto AccountCategory { get; set; }
        public string ItemCategory { get; set; }
        public NamedDto PurchaseGroup { get; set; }
        public NamedDto PurchaseOrg { get; set; }
    }

    public class SuppliersDto
    {
        public NamedDto CommonSupplier { get; set; }
        public string Name { get; set; }
        public string UniqueName { get; set; }
    }

    public class NamedDto
    {
        public string Name { get; set; }
        public string UniqueName { get; set; }
    }

    public class DescriptionDto
    {
        public MoneyDto Price { get; set; }
        public string ShortName { get; set; }
        public string SupplierPartAuxiliaryID { get; set; }
        public string UnitOfMeasure { get; set; }
        public string SupplierPartNumber { get; set; }
        public string Description { get; set; }
    }

    public class CommodityExportMapEntryDto
    {
        public AccountTypeDto AccountType { get; set; }
    }
    

    public class AccountTypeDto
    {
        public string UniqueName { get; set; }
    }

    public class AccountingDto
    {
        public DepartmentDto Department { get; set; }
        public object BusinessUnit { get; set; }
        public MoneyDto Amount { get; set; }
        public object Account { get; set; }
        public CostCenterDto CostCenter { get; set; }
        public object GlBusinessUnit { get; set; }
        public object StatisticsCode { get; set; }
        public string RequisitionId { get; set; }
        public object Location { get; set; }
        public decimal Percentage { get; set; }
        public object Project { get; set; }
        public GeneralLedgerDto GeneralLedger { get; set; }
        public decimal Quantity { get; set; }
        public List<object> CustomFields { get; set; }
        public int LineItem { get; set; }
        public int NumberInCollection { get; set; }
        public object Product { get; set; }
        public object Asset { get; set; }
        public string ErpSplitValue { get; set; }
        public object InternalOrder { get; set; }
        public object WBSElement { get; set; }
    }

    public class DepartmentDto
    {
        public string Name{ get; set; }
        public string UniqueName { get; set; }
    }

    public class CostCenterDto
    {
        public string CostCenterDescription { get; set; }
        public string UniqueName { get; set; }
    }

    public class GeneralLedgerDto
    {
        public string UniqueName { get; set; }
        public string GeneralLedgerDescription { get; set; }
    }

    public class AccountCategoryDto
    {
        public string UniqueName { get; set; }
        public string Description { get; set; }
    }

    public class UserDto
    {
        public string EmailAddress { get; set; }
        public string Phone { get; set; }
        public string Name { get; set; }
        public string UniqueName { get; set; }
        public string PasswordAdapter { get; set; }
        public string Realm { get; set; }
    }

    public class RecordDto
    {
        public string Summary { get; set; }
        public string RequisitionId { get; set; }
        public long Date { get; set; }
        public string RealUser { get; set; }
        public string Action { get; set; }
        public string Id { get; set; }
        public string Type { get; set; }
        public UserDto User { get; set; }
    }

    public class ApprovalRequestsDto
    {
        public string Approver { get; set; }
        public List<ApproverDto> Approvers { get; set; }
       public bool ApprovalRequired { get; set; }
        public string RequisitionId { get; set; }
        public string Id { get; set; }
         public DateTime? ActivationDate { get; set; }  
        public string ApprovalState { get; set; }
        public string ApproverComments { get; set; }
        public List<string> DependencyApprovalRequestsUID { get; set; }
        public string Reason { get; set; }
        public string State { get; set; }
    }

    public class ApproverDto
    {
        public GroupDto Group { get; set; }
        public string Type { get; set; }
        public string Id { get; set; }
        public string RequisitionId { get; set; }
        public string ApprovalRequestId { get; set; }
    }

    public class GroupDto
    {
        public string Id { get; set; }
        public string UniqueName { get; set; }
    }
}
using Entities.Models;

namespace Dto
{
    public class ContractDetailsDto
    {
        public string? Name { get; set; }
    }
    public class ContractDto
    {
        public string? ContractId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Name { get; set; }
        public decimal? Amount { get; set; }
        public string? Currency { get; set; }
        public string? SupplierName { get; set; }
        public string? Status { get; set; }
        public string? ContractValue { get; set; }
        public string? AmountAlreadyPaid { get; set; }
        public string? BalanceAfterCurrentPayment { get; set; }
        public string? OutletName { get; set; }
        public List<ContractLineItemDto> LineItems { get; set; }
    }


    public class ContractLineItemDto
    {
        public ContractDescriptionDto Description { get; set; }
        public ContractAmountDto Amount { get; set; }
    }

    public class ContractDescriptionDto
    {
        public string Description { get; set; }
    }

    public class ContractAmountDto
    {
        public decimal Amount { get; set; }
        public ContractCurrencyDto Currency { get; set; }
    }

    public class ContractCurrencyDto
    {
        public string UniqueName { get; set; }
        public string Name { get; set; }
    }

    public class InvoiceContractDto
    {
        public string? ContractName { get; set; }
        public string? Description { get; set; }
        public string? ContractId { get; set; }
        public string? WorkspaceID { get; set; }
        public string? AttachmentID { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? TotalAmount { get; set; }
        public string? Amount { get; set; }
        public string? AmountSpend { get; set; }
        public string? BalanceAmount { get; set; }
    }
    public class InvoicePODto
    {
        public string? ContractID { get; set; }
        public string? Description { get; set; }
        public string? Department { get; set; }
        public string?HeaderTittle { get; set; }
    }
    //public class InvoiceContractDetailDto
    //{
    //    public string? ContractId { get; set; }
    //    public string? MaterialCode { get; set; }
    //    public string? CommodityCode { get; set; }
    //    public string? Description { get; set; }
    //    public string? SupplierID { get; set; }
    //    public string? SupplierName { get; set; }
    //    public string? Amount { get; set; }
    //    public string? AmountSpend { get; set; }
    //    public string? BalanceAmount { get; set; }
    //}
}

namespace Dto
{
    public class DraftServicePRExcelUploadDto
    {
        public Guid DomainId { get; set; }
        public int RowNumber { get; set; }
     public string? Venue { get; set; }
    public string? Vendor { get; set; }         

    public decimal Quantity { get; set; }
    public decimal Amount { get; set; }
    public bool TaxInclude { get; set; }          
    public string? Currency { get; set; }
    public bool AutoInvoices { get; set; }      
    public string? InvoiceID { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public string? PRServiceUniqueName { get; set; }
    public bool? AutoSES { get; set; }
    public DateTime? ServiceStartDate { get; set; }
    public DateTime? ServiceEndDate { get; set; }
    public string? Description { get; set; }
    public string? Title { get; set; }
    public bool? TeamReceived { get; set; }
    

    
    }
}
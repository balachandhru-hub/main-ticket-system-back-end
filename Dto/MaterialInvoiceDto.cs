using System;

namespace Dto
{
    public class MaterialInvoiceDto
    {
        public Guid Id { get; set; }
        public string? PRNumber { get; set; }
        public string? InvoiceID { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string? InvoiceComment { get; set; }
        public bool IsHeader { get; set; }
    }
}

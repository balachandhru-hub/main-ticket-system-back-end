using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dto
{
    public class FinalPurchaseOrderDto
    {
        public string PoNumber { get; set; }
        public string Epid { get; set; }
        public List<FinalPurchaseOrderLineItemDto> LineItems { get; set; } = new List<FinalPurchaseOrderLineItemDto>();
    }

    public class FinalPurchaseOrderLineItemDto
    {
        public string? Supplier { get; set; }
        public string? Department { get; set; }
        public string? Property { get; set; }
        public string? GlAccount { get; set; }
        public string? CostCenter { get; set; }
        public string? Requester { get; set; }
        public string LineItem { get; set; }
        public double? Quantity { get; set; }
        public double? Amount { get; set; }
        public double? Accepted { get; set; }
        public double? PreviousAccepted { get; set; }
        public string? AribaReceiptId { get; set; }
        public string? S4MaterialDoc { get; set; }
        public string? ErrorComments { get; set; }
        public string? Result { get; set; }
    }
}

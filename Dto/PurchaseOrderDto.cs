using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dto
{
    public class PurchaseOrderDto
    {
        public string? PurchaseOrderId { get; set; }
        public List<PurchaseOrderLineItem>? LineItems { get; set; }
        public string? RecievableId { get; set; }
    }

    public class PurchaseOrderLineItem
    {
        public string? LineItem { get; set; }
        public string? UnitOfMeasure { get; set; }
        public string? MaterialCode { get; set; }
        public Double Quantity { get; set; }
        public Double? AcceptedQuantity { get; set; }
        public Double? PreviousAcceptedQuantity { get; set; }
        public string? ReceiptId { get; set; }
        public string? ProcessedReceiptId { get; set; }
        public string? ERPReceiptId { get; set; }
        public string? SuccessCloudERPId { get; set; }
        public string? ErrorComment { get; set; }
    }
}

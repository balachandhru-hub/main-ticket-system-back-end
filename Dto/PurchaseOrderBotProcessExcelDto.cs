using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dto
{
    public class PurchaseOrderBotProcessExcelDto
    {
        public string POOrderId { get; set; }
        public string EPID { get; set; }
        public string LineItem { get; set; }
        public string UnitOfMeasure { get; set; }
        public string MaterialCode { get; set; }
        public Guid DomainId { get; set; }
        public int RowNumber { get; set; }
        public double Quantity { get; set; }
        public Double PreviousAcceptedQuantity { get; set; }
        public string ReceiptID { get; set; }
        public string? Status { get; set; }
        public int? ProcessedState { get; set; }
        public bool HardReprocess { get; set; }
    }
}

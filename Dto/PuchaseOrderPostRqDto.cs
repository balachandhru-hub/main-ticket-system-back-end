using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dto
{
    public class PuchaseOrderPostRqDto
    {
        public string? PurchaseOrderId { get; set; }

        public List<PurchaseOrderLineItemRq>? LineItems { get; set; }
    }

    public class PurchaseOrderLineItemRq
    {
        public string? LineItem { get; set; }
        public Double? AcceptedQuantity { get; set; }
    }

    public class PurchaseOrderRqToAribaDto
    {
        public string? PurchaseOrderId { get; set; }
        public string? ReceivableId { get; set; }
        public string? Variant { get; set; }
        public string? Partition { get; set; }
        public string? AribaEndPoint { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Server { get; set; }

        public List<PurchaseOrderLineItemRq>? LineItems { get; set; }
        public DateOnly? Date { get; set; }
    }
}

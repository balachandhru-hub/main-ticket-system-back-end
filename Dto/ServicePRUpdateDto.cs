using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dto
{
    public class ServicePRUpdateDto
    {
        public string? PurchaseOrg { get; set; }
        public string? Vendor { get; set; }
        public string? Description { get; set; }
        public string? Commodity { get; set; }
        public string? Contract { get; set; }
        public string? PurchaseGroup { get; set; }
        public decimal? Quantity { get; set; }
        public string? UOM { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string? DeliverTo { get; set; }
        public string? GLAccount { get; set; }
        public string? CostCenter { get; set; }
        public string? MaterialCode { get; set; }
        public decimal? ExpectedAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public DateTime? ServiceStartDate { get; set; }
        public DateTime? ServiceEndDate { get; set; }

    }
}

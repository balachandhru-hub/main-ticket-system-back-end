using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dto
{
    public class PurchaseOrderRqDto
    {
        public List<string> PurchaseOrderIds { get; set; }
        public List<string> ReceiptIds { get; set; }
    }

    public enum PurchaseOrderRqMode
    {
        PARTIAL = 0,
        COMPLETED = 1,
        NEW = 3
    }
}

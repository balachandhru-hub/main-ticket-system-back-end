using System.Collections.Generic;

namespace Dto
{
    public class PurchaseOrderFilterDto
    {
        public List<string>? GlAccount { get; set; }
        public List<string>? Supplier { get; set; }
        public List<string>? CostCenter { get; set; }
        public List<string>? Property { get; set; }
        public List<string>? Requester { get; set; }
        public List<string>? Department { get; set; }
        public List<string>? PoNumber { get; set; }
    }
}
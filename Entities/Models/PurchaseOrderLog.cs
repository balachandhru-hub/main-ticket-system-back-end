using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class PurchaseOrderLog : BaseModel
    {
        public Guid Id { get; set; }
        public int BatchId { get; set; }
        [ForeignKey("Domain")]
        public Guid DomainId { get; set; }
        public Domain Domain { get; set; }
        public string PurchaseOrderId { get; set; }
        public string LineItemId { get; set; }
        public double Quantity { get; set; }
        public string Result { get; set; }
        public PurchaseOrderLog() { }
    }
}

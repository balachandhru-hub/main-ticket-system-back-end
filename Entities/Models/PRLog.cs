using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class PRLog : BaseModel
    {
        public Guid Id { get; set; }
        public int BatchId { get; set; }
        [ForeignKey("Domain")]
        public Guid DomainId { get; set; }
        public Domain Domain { get; set; }
        public string? PurchaseOrderId { get; set; }
        public string? PRNumber { get; set; }
        public string LineItemId { get; set; }
        public string Result { get; set; }
        public double Quantity { get; set; }
        public string? ERPId { get; set; }

        public PRLog() { }
    }
}

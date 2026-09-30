using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.Migrations;
using Microsoft.Identity.Client;

namespace Entities.Models
{
    public class ServicePRLog : BaseModel
    {
        public Guid Id { get; set; }
        public int BatchId { get; set; }
        [ForeignKey("Domain")]
        public Guid DomainId { get; set; }
        public Domain Domain { get; set; }
        public string PRNumber { get; set; }
        public string Status { get; set; }
        public string Result { get; set; }
        public int? LineItemNumber { get; set; }
        public string PRServiceUniqueName { get; set; }
        public decimal Quantity { get; set; }
        public ServicePRLog() { }
    }
}

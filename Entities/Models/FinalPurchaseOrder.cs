using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    public class FinalPurchaseOrder : BaseModel
    {

        [Key]
        [Required]
        public Guid Id { get; set; }

        [ForeignKey("Domain")]
        public Guid DomainId { get; set; }
        public string PoNumber { get; set; }
        public string Epid { get; set; }
        public string LineItem { get; set; }
        public double? Quantity { get; set; }
        public double? Amount { get; set; }
        public double? Accepted { get; set; }
        public double? PreviousAccepted { get; set; }
        public string? AribaReceiptId { get; set; }
        public string? S4MaterialDoc { get; set; }
        public string? ErrorComments { get; set; }
        public string? Supplier { get; set; }
        public string? Department { get; set; }
        public string? Property { get; set; }
        public string? GlAccount { get; set; }
        public string? CostCenter { get; set; }
        public string? Requester { get; set; }
        public string? Result { get; set; }
        public FinalPurchaseOrder() { }
    }
}
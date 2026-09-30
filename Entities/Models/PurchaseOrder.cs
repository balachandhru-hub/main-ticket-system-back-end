using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Entities.Models
{
    [Index(nameof(PurchaseOrderId), nameof(LineItem), nameof(DomainId), nameof(EPID), IsUnique = true)]
    public class PurchaseOrder : BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [ForeignKey("Domain")]
        public Guid DomainId { get; set; }
        public Domain Domain { get; set; }
        public string PurchaseOrderId { get; set; }
        public string EPID { get; set; }
        public string LineItem { get; set; }
        public string? UnitOfMeasure { get; set; }
        public string? MaterialCode { get; set; }
        public Double Quantity { get; set; }
        public Double? PreviousAcceptedQuantity { get; set; }
        public string? ReceiptId { get; set; }
        public string? ProcessedReceiptId { get; set; }
        public string? ERPReceiptId { get; set; }
        public string? AdditionalData { get; set; }
        public string? ErrorComment { get; set; }
        public bool IsBlocked { get; set; }
        public string? SuccessCloudERPId { get; set; }
        public int Counter { get; set; }
        public string? Status { get; set; }
        public int? ProcessedState { get; set; }
        public bool HardReprocess { get; set; }

        public PurchaseOrder()
        { }
    }
}

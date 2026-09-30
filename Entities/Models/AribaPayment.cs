using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class AribaPayment : BaseModel
    {
        public Guid Id { get; set; }
        [ForeignKey("Domain")]
        public Guid DomainId { get; set; }
        public Domain Domain { get; set; }
        public string? PRNumber { get; set; }   
        public string? PONumber { get; set; }
        public string? InvoiceNumber { get; set; }
        public double? InvoiceAmount { get; set; }
        public string? AribaPaymentId { get; set; }
        public string? ERPInvoiceNumber { get; set; }
        public string? AdvancePaymentId { get; set; }
        public double? AdvanceAmount { get; set; }
        public string? AdvancePaymentStatus { get; set; }
        public string? PaymentStatus { get; set; }
        public string? PaymentId { get; set; }
        public double? PaidAmount { get; set; }
        public DateTime? PaidDate { get; set; }
        public string? Approver { get; set; }
        public string? BankClearance { get; set; }
        public DateTime? BankClearanceDate { get; set; }
        public string? SupplierName { get; set; }
        public string? Description { get; set; }
        public string? ContractId { get; set; }       
        public string? CompanyUniqueName { get; set; }
        public string? ContractValue { get; set; }
        public string? ValueAlreadyPaid { get; set; }
        public string? RemainingBalance { get; set; }
        public string? EPID { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string? InvoiceId { get; set; }

    }
}

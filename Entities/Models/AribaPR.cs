using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
namespace Entities.Models
{
    public class AribaPR : BaseModel
    {

        public Guid Id { get; set; }

        [Required]
        public string RequisitionsId { get; set; }
        [ForeignKey("Domain")]
        public Guid DomainId { get; set; }
        public Domain Domain { get; set; }

        public string? SupplierName { get; set; }
        public string? Currency { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? NeedBYDate { get; set; }
        public DateTime? RequisitionsDate { get; set; }
        public decimal? Amount { get; set; } = 0;
        public string? Description { get; set; }
        public string? Department { get; set; }
        public string AribaTaskId { get; set; }

        public string? ContractId { get; set; }
        public string? CompanyUniqueName { get; set; }
        public bool IsAttachmentProcessed { get; set; }
        public double? PRTaxAmount { get; set; } 
        public double? PRNetAmount { get; set; }
        public string? RequesterName { get; set; }
        public ICollection<AribaPRTasks> Tasks { get; set; } = new List<AribaPRTasks>();

    }
}
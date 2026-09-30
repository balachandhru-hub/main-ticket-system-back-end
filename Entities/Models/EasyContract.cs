using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    public class EasyContract: BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("Domain")]
        public Guid DomainId { get; set; }
        public Domain Domain { get; set; }
        public string Name { get; set; }
        public string  HierarchicalType { get; set; }
        public string TermType { get; set; }
        public string Description { get; set; }
        public string Supplier { get; set; }
        public string TemplateName { get; set; }
        public string Email { get; set; }
   
        public string ContactPerson { get; set; }
        public decimal ContractValue { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
   
        public string Currency { get; set; }
        public bool TestProject { get; set; }
        public string ContractId { get; set; }
        public string COntractStatus { get; set; }
        public string? MaximumNumberOfRenewals { get; set; }
     public string? AutoRenewalInterval { get; set; }
     public string? ParentAgreement { get; set; }

        public EasyContract() { }
    }
}
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    public class DomainPlan : BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        [ForeignKey("Domain")]
        public Guid DomainId { get; set; }
        public Domain Domain { get; set; }
        public string PlanName { get; set; }
        public string EngagementType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string PaymentType { get; set; }
        public Guid? DiscountCouponId { get; set; }
        public bool IsRunning { get; set; }
        public DateTime ContractStartDate { get; set; }
        public DateTime ContractEndDate { get; set; }
        public int NumberOfDues { get; set; }
        public DomainPlan()
        { }
    } 
}
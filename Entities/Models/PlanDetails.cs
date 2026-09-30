using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    public class PlanDetails : BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [ForeignKey("DomainPlan")]
        public Guid DomainPlanId { get; set; }
        public DomainPlan DomainPlan { get; set; }

        public int CRHoursBeyondLimit { get; set; }
        public int NumberOfConsult { get; set; }
        public int NumberOfManager { get; set; }
        public decimal TicketCharge { get; set; }
        public int MonthlyHours { get; set; }
        public decimal AdditionalCRHoursCharge { get; set; }
        public int NumberOfResources { get; set; }
        public decimal CRCost { get; set; }
        public decimal ResourceCost { get; set; }
        public decimal ManagerCost { get; set; }
        public decimal TotalChargeOfConsult { get; set; }
        public decimal TotalChargeOfManager { get; set; }
        public decimal TotalCost { get; set; }
        public PlanDetails() { }
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    public class DomainPlanCredit : BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        [ForeignKey("DomainPlan")]
        [Required]
        public Guid DomainPlanId { get; set; }
        public DomainPlan DomainPlan { get; set; }
        public double MonthlyHourCredit { get; set; }
        public double TotalHourCredit { get; set; } 
        public double ActualMonthlyHourCredit { get; set; } 
        public DomainPlanCredit()
        { }
    }
}
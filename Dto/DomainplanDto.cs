using System;
using System.ComponentModel.DataAnnotations;
using Entities.Common;

namespace Dto
{
    public class DomainPlanDto
    {
        [Required]
        public Guid DomainId { get; set; }


        [Required]
        public string PlanName { get; set; }

        [Required]
        public string EngagementType { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public Guid? DiscountCouponId { get; set; }
        public string PaymentType { get; set; }
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
        public DateTime ContractStartDate { get; set; }
        public DateTime ContractEndDate { get; set; }
        public AssetUploadDto Asset { get; set; }
        public int ExpiryTime { get; set; }

    }
}
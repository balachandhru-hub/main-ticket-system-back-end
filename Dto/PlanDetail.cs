public class PlanDetailsDto
{
    public string PlanType { get; set; }
    public string EngagementType { get; set; }
    public int NoOfConsultants { get; set; }
    public int MonthlyHours { get; set; }
    public decimal CrChargesForAdditionalHours { get; set; }
    public string PaymentType { get; set; }
    public DateTime PlanStartDate { get; set; }
    public DateTime PlanEndDateOrRenewalDate { get; set; }
    public decimal PaymentAmount { get; set; }
    public string? PaymentStatus { get; set; }
    public string PlanStatus { get; set; }
    public DateTime ContractStartDate { get; set; }
    public DateTime ContractEndDate { get; set; }
    public int CRHoursBeyondLimit { get; set; }
    public decimal TicketCharge { get; set; }
    
}
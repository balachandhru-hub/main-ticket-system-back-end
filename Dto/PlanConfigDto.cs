namespace Dto
{
    public class PlanConfigDto
    {
        public Guid Id { get; set; }
        public string Plan { get; set; }
        public string EngagementType { get; set; }
        public string? ChargePerTicket { get; set; }
        public string? CrBeyond8Hours { get; set; }
        public string? MonthlyHours { get; set; }
        public string? PaymentType { get; set; }
        public string? ResourceCost { get; set; }
        public string? NoOfConsultants { get; set; }
        public string? CrChargesForAdditionalHours { get; set; }
        public string? Manager { get; set; }
        public string? Resources { get; set; }
        public string? CrHours { get; set; }
        public string? ManagerCost { get; set; }
     
    }
}
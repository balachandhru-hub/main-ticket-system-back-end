namespace Dto
{
    public class MyProfileDto
    {
        public string PersonName { get; set; }
        public string Email { get; set; }
        public string Domain { get; set; }
        public string PlanType { get; set; }
        public double? MonthlyCreditRemaining { get; set; }
        public double? TotalCreditRemaining { get; set; }
        public string Address { get; set; }
        public string Role { get; set; }
        public string? Country { get; set; }
        public string? MobilePhone { get; set; }
        public List<AssetDto>? Asset { get; set; }
        public string? AribaUserName { get; set; }
        public string? AribaPassword { get; set; }
        public double MonthlyExceededHours { get; set; }
        public string? orgANId { get; set; }
    }
}
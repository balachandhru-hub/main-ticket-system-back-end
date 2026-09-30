namespace Dto
{
    public class DomainPreviewDto
    {
        public Guid Id { get; set; }
        public string GST_VAT { get; set; }
        public string CurrentPlan { get; set; }
        public string ActiveSince { get; set; }
        public string DomainName { get; set; }
        public string DomainStatus { get; set; }
        public bool? IsActive { get; set; }
        public string AribaNetworkId { get; set; }
        public string? LUTNumber { get; set; }
    }
}
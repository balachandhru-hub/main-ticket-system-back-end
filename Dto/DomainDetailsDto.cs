using Dto;
public class DomainDetailsDto
{
    public CompanyDetailsDto CompanyDetails { get; set; }
    public PlanDetailsDto PlanDetails { get; set; }
    public TaxDetailsDto TaxDetails { get; set; }
    public AssetDto? Asset { get; set; }
    public List<AppNameDto>? AppNames { get; set; } 
}
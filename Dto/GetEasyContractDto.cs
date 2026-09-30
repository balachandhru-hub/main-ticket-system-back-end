public class EasyContractDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string HierarchicalType { get; set; }
    public string TermType { get; set; }

    public List<EasyContractCommodityDto> Commodities { get; set; } = new();
    public List<LookupDto> Regions { get; set; } = new();
    public List<LookupDto> Departments { get; set; } = new();

    public string Supplier { get; set; }
    public string ContractId { get; set; }
    public string COntractStatus { get; set; }
    public decimal ContractValue { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Currency { get; set; }
    public bool TestProject { get; set; }
    public string Description { get; set; }
    public string? ParentAgreement { get; set; }
}

public class LookupDto
{
    public string UniqueName { get; set; }
    public string Name { get; set; }

}
public class EasyContractCommodityDto
{
    public string UniqueName { get; set; }
    public string Name { get; set; }
    public string? ContractDomain { get; set; }
}

public class EasyContractFilterRequestDto
{
    public int Index { get; set; } = 0;
    public int Limit { get; set; } = 10;
    public string? SearchTerm { get; set; }
}
public class ContractDocumentDto
{
    public List<ContractChildDocumentDto> ChildrenDocument { get; set; } = new();
}

public class ContractChildDocumentDto
{
    public string Id { get; set; }
    

}

 public class GetVenueSupplierRequestDto
    {
        public string Type { get; set; }        // "VENUE" or "SUPPLIER"
        public string? Identifier { get; set; } // VenueName OR SupplierId
        public string? SearchTerm { get; set; }
        public int Index { get; set; } = 0;
        public int Limit { get; set; } = 10;
    }

     public class MasterContractDto
    {
        public Guid Id { get; set; }
        public Guid DomainId { get; set; }
        public string UniqueName { get; set; }
        public string Name { get; set; }
        public string? ContractDomain { get; set; }
    }
     public class ContractSupplierDto
{
    public string Name { get; set; }
    public string SmVendorId { get; set; }
}
public class ContractCustomeTypeDto
{
    public string CustomType { get; set; }
    public string Value { get; set; }
}
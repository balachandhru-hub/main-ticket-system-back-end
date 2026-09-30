public class LineItemDto
{
    public SupplierDto Supplier { get; set; }
    public ShipToDto ShipTo { get; set; }       // ✅ Must be object
    public SupplierLocationDto SupplierLocation { get; set; }
    public AmountDto Amount { get; set; }
    public DescriptionDto Description { get; set; }
    public string DeliverTo { get; set; }       // string is ok
    // ... other properties as needed
}

public class ShipToDto
{
    public string Name { get; set; }
    public string UniqueName { get; set; }
}

public class SupplierDto
{
    public CommonSupplierDto CommonSupplier { get; set; }
    public string Name { get; set; }
    public string UniqueName { get; set; }
}

public class CommonSupplierDto
{
    public string Name { get; set; }
}

public class SupplierLocationDto
{
    public string Name { get; set; }
    public string UniqueName { get; set; }
}

public class AmountDto
{
    public decimal Amount { get; set; }
    public string Currency { get; set; }
}

public class DescriptionDto
{
    public AmountDto Price { get; set; }
    public string ShortName { get; set; }
    public string SupplierPartAuxiliaryID { get; set; }
    public string UnitOfMeasure { get; set; }
    public string SupplierPartNumber { get; set; }
    public string Description { get; set; }
}

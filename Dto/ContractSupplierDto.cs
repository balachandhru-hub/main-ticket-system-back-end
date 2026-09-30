    public class ContractSupplierNameExcelDto
    {
        public Guid DomainId { get; set; }
        public string Name { get; set; }
        public string SmVendorId { get; set; }
        public int RowNumber { get; set; }
    }

        public class ContractDepartmentExcelDto
    {
        public Guid DomainId { get; set; }
        public string UniqueName { get; set; }
        public string Name { get; set; }
     
        public int RowNumber { get; set; }
    }
        public class ContractRegionExcelDto
    {
        public Guid DomainId { get; set; }
        public string UniqueName { get; set; }
        public string Name { get; set; }
     
        public int RowNumber { get; set; }
    }
        public class ContractComoditiesExcelDto
    {
        public Guid DomainId { get; set; }
        public string UniqueName { get; set; }
        public string Name { get; set; }
        public string ContractDomain { get; set; }
        public int RowNumber { get; set; }  
        }
          public class LegalMaterialCodeExcelDto
    {
        public Guid DomainId { get; set; }
        public string MaterialCode { get; set; }
        public string Description { get; set; }
        public string GLAccount { get; set; }
    
        public int RowNumber { get; set; }  
        }
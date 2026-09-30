namespace Dto
{
    public class CompanyCodeExcelDto
    {
        public Guid DomainId { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public int RowNumber { get; set; }
    }
      public class SupplierNameExcelDto
    {
        public Guid DomainId { get; set; }
        public string SupplierName { get; set; }
        public string SupplierId { get; set; }
        public int RowNumber { get; set; }
    }
    
        public class VenueExcelDto
        {
            public Guid DomainId { get; set; }
            public string VenueName { get; set; }
            public string CompanyCode { get; set; }
            public string CompanyName { get; set; }
            public string CostCenter { get; set; }
            public string GLAccount { get; set; }
            public string ShipToPlant { get; set; }
            public string PurchaseOrg { get; set; }
            public string PurchaseGroup { get; set; }
            public string BillTo { get; set; }
            public string Type {get; set;}
            public int RowNumber { get; set; }
        }
         public class PropertyCrossChargeExcelDto
        {
            public Guid DomainId { get; set; }
            public string PropertyCrossChargeName { get; set; }
            public string CompanyCode { get; set; }
            public string CompanyName { get; set; }
            public string CostCenter { get; set; }
            public string GLAccount { get; set; }
            public string ShipToPlant { get; set; }
            public string PurchaseOrg { get; set; }
            public string PurchaseGroup { get; set; }
            public string BillTo { get; set; }
            public int RowNumber { get; set; }
        }
          public class CategoryExcelDto
        {
            public Guid DomainId { get; set; }
            public string CategoryName { get; set; }
            public int RowNumber { get; set; }
        }


    }


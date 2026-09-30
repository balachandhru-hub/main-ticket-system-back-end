using System.Text;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Dto
{
 public class SupplierListDto
{
    public string SupplierId { get; set; }
    public string SupplierName { get; set; }
}
public class VenueListDto
{
public string VenueName { get; set; }

}
public class VenueDetailsDto
{
    public string VenueName { get; set; }
    public string CompanyCode { get; set; }
    public string CompanyName { get; set; }
    public string CostCenter { get; set; }
    public string GLAccount { get; set; }
    public string ShipToPlant { get; set; }
    public string PurchaseOrg { get; set; }
    public string PurchaseGroup { get; set; }
    public string BillTo { get; set; }
    public string? AribaUserId { get; set; }
    public string? MaterialCode { get; set; }
    public string? Type {get; set;}
    public string? UOM { get; set; }
}
public class PropertyCrossChargeDetailsDto
{

   
    public string PropertyCrossChargeName { get; set; }
    public string CompanyCode { get; set; }
    public string CompanyName { get; set; }
    public string CostCenter { get; set; }
    public string GLAccount { get; set; }
    public string ShipToPlant { get; set; }
    public string PurchaseOrg { get; set; }
    public string PurchaseGroup { get; set; }
    public string BillTo { get; set; }
}
public class PropertyCrossChargeDto
{
    public string PropertyCrossChargeName { get; set; }
    public string CostCenter { get; set; }
}

public class PublicCategoryListDto
{
   
    public string CategoryName { get; set; }

 
}
public class LegalMaterialCodeListDto
{
    public string GLAccount { get; set; }
}
public class LegalMaterialCodeDetailsDto
{
    public string MaterialCode { get; set; }
    public string Description { get; set; }
  
}
public class ContractListDto
{
public string ContractId { get; set; }

}
}
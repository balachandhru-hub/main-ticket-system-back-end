using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Dto
{
    public class AribaWorkspaceResponseDto
    {
       
        public List<AribaWorkspaceItem> Value { get; set; }
    }

    public class AribaWorkspaceItem
    {
        
        public string Title { get; set; }

     
        public string Description { get; set; }

     
        public string ContractId { get; set; }

 
        public string ContractStatus { get; set; }

     
        public AribaSupplier Supplier { get; set; }

  
        public List<AribaDepartment> Departments { get; set; }

   
        public List<AribaRegion> Regions { get; set; }


        public List<AribaCommodity> Commodities { get; set; }

 
        public AribaOwner Owner { get; set; }

        public DateTime? EffectiveDate { get; set; }

        public DateTime? AgreementDate { get; set; }


        public DateTime? ExpirationDate { get; set; }

        public DateTime? CreationDate { get; set; }

    
        public AribaContractAmount ContractAmount { get; set; }

     
        public AribaBusinessSystem BusinessSystem { get; set; }

      
        public int? Version { get; set; }

 
        public string TemplateId { get; set; }

 
        public bool? IsTestProject { get; set; }

       
        public List<AribaCustomField> CustomFields { get; set; }
    }

    public class AribaSupplier
    {
        
        public string Name { get; set; }

       
        public string SystemID { get; set; }

      
        public string SmVendorID { get; set; }

   
        public bool? IsBlocked { get; set; }

        public AribaAddress Address { get; set; }
    }

    public class AribaAddress
    {
      
        public string Name { get; set; }

       
        public string UniqueName { get; set; }

    
        public string City { get; set; }

       
        public string PostalCode { get; set; }


        public List<string> Lines { get; set; }
    }

    public class AribaDepartment
    {
        
        public string UniqueName { get; set; }

        
        public string Name { get; set; }
    }

    public class AribaRegion
    {
       
        public string UniqueName { get; set; }

        
        public string Name { get; set; }
    }

    public class AribaCommodity
    {
       
        public string UniqueName { get; set; }

       
        public string Domain { get; set; }

        
        public string Name { get; set; }
    }

    public class AribaOwner
    {
        
        public string UniqueName { get; set; }

        
        public string Name { get; set; }

       
        public string EmailAddress { get; set; }

 
        public string Organization { get; set; }

        public string OrgName { get; set; }
    }

    public class AribaContractAmount
    {
       
        public decimal Amount { get; set; }

        public string Currency { get; set; }
    }

    public class AribaBusinessSystem
    {
        
        public string BusinessSystemId { get; set; }

    
        public string BusinessSystemName { get; set; }
    }

    public class AribaCustomField
    {
      
        public string FieldId { get; set; }

 
        public string FieldName { get; set; }

       
        public string FieldDataType { get; set; }

        public List<string> TextMultiSelectValue { get; set; }

       
        public List<string> TextValue { get; set; }
    }
}

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Dto
{
    public class ContractWorkspaceRequestDto
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
   

        [JsonProperty("commodities")]
        public List<CommodityDto> Commodities { get; set; }

        [JsonProperty("regions")]
        public List<RegionDto> Regions { get; set; }

        [JsonProperty("departments")]
        public List<ContractDepartmentDto> Departments { get; set; }

        [JsonProperty("owner")]
        public OwnerDto Owner { get; set; }

        [JsonProperty("supplier")]
        public ContractSupplierDto Supplier { get; set; }

        [JsonProperty("effectiveDate")]
        public DateTime EffectiveDate { get; set; }   

        [JsonProperty("expirationDate")]
        public DateTime ExpirationDate { get; set; }  

        [JsonProperty("contractAmount")]
        public EasyContractAmountDto ContractAmount { get; set; }

        [JsonProperty("templateId")]
        public string TemplateId { get; set; }

        [JsonProperty("hierarchicalType")]
        public string HierarchicalType { get; set; }

        [JsonProperty("expirationTermType")]
        public string ExpirationTermType { get; set; }

        [JsonProperty("isTestProject")]
        public bool IsTestProject { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }
        [JsonProperty("maximumNumberOfRenewals")]
        public string? MaximumNumberOfRenewals { get; set; }

        [JsonProperty("autoRenewalInterval")]
        public string? AutoRenewalInterval { get; set; }
        [JsonProperty("parentAgreement")]
        public string? ParentAgreement { get; set; }
    }

    public class CommodityDto
    {
        [JsonProperty("uniqueName")]
        public string UniqueName { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class RegionDto
    {
        [JsonProperty("uniqueName")]
        public string UniqueName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class ContractDepartmentDto
    {
        [JsonProperty("uniqueName")]
        public string UniqueName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class OwnerDto
    {
        [JsonProperty("uniqueName")]
        public string UniqueName { get; set; }

        [JsonProperty("passwordAdapter")]
        public string PasswordAdapter { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("emailAddress")]
        public string EmailAddress { get; set; }

     

        [JsonProperty("orgANId")]
        public string OrgANId { get; set; }
    }

    public class ContractSupplierDto
    {
        [JsonProperty("name")]
        public string Name { get; set; }

         [JsonProperty("smVendorID")]
        public string SmVendorID { get; set; }

     
    }

    public class EasyContractAmountDto
    {
        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }
    }
    public class AribaContractResponse
{
    [JsonProperty("value")]
    public List<AribaContractItem> Value { get; set; }
}

public class AribaContractItem
{
    [JsonProperty("contractId")]
    public string ContractId { get; set; }

    [JsonProperty("contractStatus")]
    public string ContractStatus { get; set; }
}
public class EasyContractResponseDto
{
    public bool IsSuccess { get; set; }

    public string Message { get; set; }

    public string ContractId { get; set; }

    public string ContractStatus { get; set; }
}
}
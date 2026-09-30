using System;
using System.ComponentModel.DataAnnotations;

namespace Dto
{
    public class DomainDto
    {
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string WebsiteUrl { get; set; }
        [Required]
        public string RegistrationNumber { get; set; }
        [Required]
        public string HeadquartersLocation { get; set; }
        public string? CountriesOfOperation { get; set; }
        [Required]
        public string BusinessType { get; set; }
        [Required]
        public string IndustryType { get; set; }
        public string BillingAddress { get; set; }
        public string? GST { get; set; }
        public bool TaxApplicable { get; set; }
        public int NumberOfUser { get; set; }
        public string SupportAdminName { get; set; }
        public string SupportAdminEmail { get; set; }
        public string SupportAdminTimeZone { get; set; }
        public string SupportAdminPhoneNo { get; set; }
        public string ConnectERPSystem { get; set; }
        public string Designation { get; set; }
        public string AribaNetworkId { get; set; }
        public string? VAT { get; set; }
        public string? TAN { get; set; }
        public string? PAN { get; set; }
        public string? TAXRegistration { get; set; }
        public string? OtherLocalTaxId { get; set; }
        public string? LUTNumber { get; set; }
        public bool NDA { get; set; }
        public List<DomainRealmDto>? DomainRealms { get; set; } 
        public List<DomainModelDto> DomainModels { get; set; }
    }
}

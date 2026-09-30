using System;
using System.ComponentModel.DataAnnotations;

namespace Entities.Models
{
    public class Domain : BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string WebsiteUrl { get; set; }
        public string RegistrationNumber { get; set; }
        public string HeadquartersLocation { get; set; }
        public string? CountriesOfOperation { get; set; }
        public string BusinessType { get; set; }
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
        public bool NDA { get; set; }
        public string? CINNumber { get; set; }
        public string? LUTNumber { get; set; }
        public Domain() { }
    }
}
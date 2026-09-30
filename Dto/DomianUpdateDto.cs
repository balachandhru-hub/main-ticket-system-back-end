using System;
using System.ComponentModel.DataAnnotations;

namespace Dto
{
    public class DomainUpdateDto
    {
        public Guid Id { get; set; }
        public int? NumberOfUser { get; set; }
        public string? SupportAdminName { get; set; }
        public string? SupportAdminEmail { get; set; }
        public string? SupportAdminTimeZone { get; set; }
        public string? SupportAdminPhoneNo { get; set; }
        public string? ConnectERPSystem { get; set; }
        public string? Designation { get; set; }
        public List<DomainModelDto>? DomainModel { get; set; } 
        public List<DomainRealmDto>? DomainRealm { get; set; } 

    }
}

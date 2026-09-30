using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    public class Person : BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("Domain")]
        public Guid DomainId { get; set; }
        public Domain Domain { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string MobilePhone { get; set; }
        public string? TenantId { get; set; }
        public string? RealmId { get; set; }
        public string Address { get; set; }
        public Guid? TempTenantId { get; set; }
        public string? AribaUserName { get; set; }
        public string? Designation { get; set; }
        public string? EmployeeId { get; set; }
        public string? Department { get; set; }
        public string? Country { get; set; }
        public string? AribaPassword{get;set;}
        public string? orgANId { get; set; }
        public Person() { }
    }
}
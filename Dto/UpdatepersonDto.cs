using System;
using System.ComponentModel.DataAnnotations;

namespace Dto
{
    public class UpdatePersonDto
    {
        public string? Name { get; set; }

        public string? Address { get; set; }

        public string? MobilePhone { get; set; }
        public string? Country { get; set; }
        public string? Designation { get; set; }
        public string? Department { get; set; }
        public string? EmployeeId { get; set; }
        public string? AribaUserName { get; set; }
        public string? AribaPassword { get; set; }
        public List<Guid>? ModelIds { get; set; }
        public List<Guid>? AppIds { get; set; }
        public List<RequesterNameDto>? RequesterNames { get; set; }
        public string? orgANId { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using Entities.Common;

namespace Dto
{
    /// <summary>
    /// DTO for Person entity, used for create, update, delete, and remove operations.
    /// </summary>
    public class PersonDto
    {
        /// <summary>
        /// Person's name (nullable for partial updates or delete/remove)
        /// </summary>
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        /// <summary>
        /// Person's email (nullable for partial updates or delete/remove)
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [RegularExpression(@"^[\w\.-]+@(?!gmail\.com$)(?!gmail\.in$)[\w\.-]+\.(com|in|co|design)$",
        ErrorMessage = "Only company or organization emails are allowed. Gmail is not accepted.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }
        // Add this to PersonDto.cs
        public string MobilePhone { get; set; }
        public string? RealmId { get; set; }

        /// <summary>
        /// Tenant Id
        /// </summary>
        public string? TenantId { get; set; }

        /// <summary>
        /// Temporary Tenant Id
        /// </summary>
        public Guid? TempTenantId { get; set; }

        /// <summary>
        /// Ariba User Name
        /// </summary>
        public string? AribaUserName { get; set; }

        /// <summary>
        /// Ariba User Name
        /// </summary>
        public Guid RoleId { get; set; } = Common.DEFAULT_ROLE_ID;

        /// <summary>
        /// Ariba User Name
        /// </summary>
        [Required]
        public string UserName { get; set; }

        /// <summary>
        /// Ariba User Name
        /// </summary>
        [Required]
        public string UserSecret { get; set; }

        /// <summary>
        /// Domain Id
        /// </summary>
        [Required]
        public Guid DomainId { get; set; }
        /// <summary>
        /// Designation of the person
        ///     </summary>
        public string? Designation { get; set; }
        /// <summary>
        /// Employee ID of the person
        /// </summary>
        public string? EmployeeId { get; set; }
        /// <summary>
        /// Department of the person
        /// </summary>
        /// 
        public string? Department { get; set; }
        /// <summary>
        /// Country of the person
        /// </summary>
        ///     
        public string? Country { get; set; }
        /// <summary>
        /// Ariba Password
        /// </summary>
        public string? AribaPassword { get; set; }
        public List<Guid>? ModelIds { get; set; }
        public List<Guid>? AppIds { get; set; }
        public List<RequesterNameDto>? RequesterNames { get; set; }
        public string? orgANId { get; set; }
    }
}
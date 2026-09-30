using System;
using System.ComponentModel.DataAnnotations;

namespace Dto
{

    public class UpdateDomainStatus
    {
        [Required]
        public Guid DomainId { get; set; }

        [Required]

        public string ActiveStatus { get; set; }
    }
}

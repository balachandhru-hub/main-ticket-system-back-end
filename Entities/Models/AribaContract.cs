using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Entities.Models
{
    [Index(nameof(ContractId), nameof(DomainId), IsUnique = true)]
    public class AribaContract : BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [ForeignKey("Domain")]
        public Guid DomainId { get; set; }
        public Domain Domain { get; set; }
        public string? ContractName { get; set; }
        public string? ContractId { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? TotalAmount { get; set; }
        public string? Currency { get; set; }
        public AribaContract()
        { }
    }
}

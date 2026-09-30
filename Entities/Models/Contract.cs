using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class ContractDetails : BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [ForeignKey("Domain")]
        public Guid DomainId { get; set; }
        public Domain Domain { get; set; }
        public string? ContractTaskId { get; set; }
        public string? ContractId { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? TotalAmount { get; set; }
        public string? Currency { get; set; }
        public string? WorkspaceID { get; set; }
        public string? AttachmentID { get; set; }
        public string? MaterialCode { get; set; }
        public string? CommodityCode { get; set; }
        public string? Description { get; set; }
        public string? SupplierID { get; set; }
        public string? SupplierName { get; set; }
        public string? AmountSpend { get; set; }
        public string? BalanceAmount { get; set; }
        public string? Department { get; set; }
        public string? CompanyUniqueName { get; set; }
          public ICollection<ContractPending> Tasks { get; set; } = new List<ContractPending>();
        public bool IsAttachmentProcessed { get; set; } 
  
        public ContractDetails()
        
        { }
    }
}

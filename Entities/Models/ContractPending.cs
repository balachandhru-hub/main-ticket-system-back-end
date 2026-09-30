using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
namespace Entities.Models;
    public class ContractPending : BaseModel
    {
        [Key]
        public Guid Id { get; set; }
         public string UniqueName { get; set; }
        [ForeignKey("Domain")]
        public Guid DomainId { get; set; }
        public Domain Domain { get; set; }

        [MaxLength(255)]
        public string Approver { get; set; }

        public string? Description { get; set; }
        public DateTime? AssignedDate { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public string FullUrl { get; set; }
        public string DocumentType { get; set; }
     
      
    }


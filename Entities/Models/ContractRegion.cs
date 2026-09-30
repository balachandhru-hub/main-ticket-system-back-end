using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
namespace Entities.Models
{
    public class ContractRegion : BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        [ForeignKey("Domain")]
        public Guid DomainId { get; set; }
        public string UniqueName { get; set; }
        public string Name { get; set; }
 
       
        public ContractRegion()
        { }
    }
}
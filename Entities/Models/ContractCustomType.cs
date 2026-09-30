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
    public class ContractCustomType : BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        public string CustomType { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
 
       
        public ContractCustomType()
        { }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class AribaCurrency : BaseModel
    {
        public Guid Id { get; set; }
      
 
        public string? CurrencyName { get; set; }
        public int? SortNumber { get; set; }
         public AribaCurrency() 
          { }
 

    }
}

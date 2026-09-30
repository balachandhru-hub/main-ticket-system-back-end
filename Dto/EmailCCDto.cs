using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dto
{
    public class EmailCCDto
    {
        [Required]
        public string EntityType { get; set; }
        [Required]
        public string EmailAddress { get; set; }
    }
}

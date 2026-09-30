using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dto
{
    public class EmailCCDetailDto
    {
        public Guid Id { get; set; }
        public string EntityType { get; set; }
        public string EmailAddress { get; set; }
    }
}

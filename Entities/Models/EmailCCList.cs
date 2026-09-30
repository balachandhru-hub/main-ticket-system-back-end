using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class EmailCCList : BaseModel
    {
        public Guid Id { get; set; }
        public string EntityType { get; set; }
        public string Email { get; set; }
        public EmailCCList()
        { }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dto
{
    public class PRBlockOrUnblockDataDto
    {
        public string POOrderId { get; set; }
        public bool IsBlocked { get; set; }
        public Guid DomainId { get; set; }
        public int RowNumber { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace Dto
{
    public class AdditionalQueryFilterDto
    {
        public string? SearchQuery { get; set; }
        public string? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? Index { get; set; }
        public int? Limit { get; set; }
    }

}

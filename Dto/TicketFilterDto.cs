using System;

namespace Dto
{
    public class TicketFilterDto
    {
        public int Index { get; set; }
        public int Limit { get; set; }

        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public string? Category { get; set; }
        public string? Priority { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}

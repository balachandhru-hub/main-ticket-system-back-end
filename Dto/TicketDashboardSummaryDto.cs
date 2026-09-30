public class TicketDashboardSummaryDto
{
    public int? OpenTickets { get; set; }
    public int? Approved { get; set; }
    public int InProgress { get; set; }
    public int Resolved { get; set; }
    public int Rejected { get; set; } 
    public int OnHold { get; set; }
}
namespace Dto
{
    public class TicketMessageDto
    {
        public string Type { get; set; }
        public DateTime DateCreated { get; set; }
        public Guid? MessageId { get; set; }
        public Guid? AssetId { get; set; }
        public string? Message { get; set; }
        public string? UserName { get; set; }
        public Guid UserId { get; set; }
    }
}
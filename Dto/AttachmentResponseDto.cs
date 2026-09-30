namespace Dto
{
    public class SplitAttachmentResponseDto
    {
        public List<string> InvoiceAttachments { get; set; } = new();
        public List<string> ContractAttachments { get; set; } = new();
        public List<string> PRAttachments { get; set; } = new();
    }
}
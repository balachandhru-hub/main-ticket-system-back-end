public class AribaAttachmentDTO
{

    public string Id { get; set; }
    public string attachmentId { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public string? Base64File{get;set;}
    public byte[] FileBytes { get; set; }

 
}

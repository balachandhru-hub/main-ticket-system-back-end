namespace Dto
{
    public class ContractTaskDto
    {
          public string Approver { get; set; }
           public string UniqueName { get; set; }

        public string? Description { get; set; }
        public DateTime? AssignedDate { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public string FullUrl { get; set; }
        public string DocumentType { get; set; }
        public List<string> attachments { get; set; } 

      
    }
}
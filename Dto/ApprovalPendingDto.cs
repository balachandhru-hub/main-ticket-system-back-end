namespace Dto
{
    public class AribaTask
    {
        public string approvableId { get; set; }
        public string approver { get; set; }
        public string description { get; set; }
        public DateTime? assignedDate { get; set; }
        public string email { get; set; }
        public string status { get; set; }
        public string fullUrl { get; set; }
        public DateTime? dueDate { get; set; }
        public string approvableType { get; set; }
        public string approvableUniqueName { get; set; }
        public List<string> attachments { get; set; } 

      
    }
}
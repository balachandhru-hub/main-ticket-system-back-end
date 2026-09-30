using System; using System.ComponentModel.DataAnnotations; 
namespace Entities.Models 
{
     public class AdditionalQuery : BaseModel 
     { 
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Organization { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public AdditionalQuery() { } 
    }
               
}
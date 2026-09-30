using System;

namespace Entities.Models
{
    
    public class AribaExtension : BaseModel
    {
        public Guid Id { get; set; }

        
        public Guid DomainId { get; set; }

       
        public byte[]? Extension { get; set; }

       
        public string? ExtensionFileName { get; set; }

      
        public string? ExtensionContentType { get; set; }
        public AribaExtension() { }
    }
}

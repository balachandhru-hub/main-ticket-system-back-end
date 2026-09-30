using System;

namespace Dto
{
    public class DomainModelDto
    { 
         public Guid DomainId { get; set; }
        public string ModelName { get; set; }  
        public bool? IsActive { get; set; }
    }
}

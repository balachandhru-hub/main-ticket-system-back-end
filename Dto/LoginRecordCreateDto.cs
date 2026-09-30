using System;

namespace Dto
{
    public class LoginRecordCreateDto
    {
        // optional id string, if omitted server will generate
public string? AttributeId { get; set; }
        public Guid UserId { get; set; }
        public Guid DomainId { get; set; }
        public string? IpAddress { get; set; }
        public string? Attribute { get; set; }
        public DateTime? LoginTime { get; set; }
    }
}

namespace Dto
{
    public class TokenClaimDto
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public Guid DomainId { get; set; }
        public Guid PersonId { get; set; }
    }
}
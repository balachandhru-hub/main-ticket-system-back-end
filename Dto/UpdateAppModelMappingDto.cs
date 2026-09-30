namespace Dto
{
    public class UpdateAppModelMappingDto
    {
        public Guid DomainId { get; set; }

        public List<Guid> AppIds { get; set; } = new();
    }
}
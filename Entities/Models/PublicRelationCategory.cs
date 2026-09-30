namespace Entities.Models
{
    public class PublicRelationCategory : BaseModel
    {
        public Guid Id { get; set; }
        public string Category { get; set; }
        public Guid DomainId { get; set; }

        public PublicRelationCategory()
        { }

    }
}
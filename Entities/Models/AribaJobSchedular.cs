using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    public partial class AribaJobSchedular : BaseModel
    {
        public Guid Id { get; set; }
        [ForeignKey("Domain")]
        public Guid DomainId { get; set; }
        public Domain Domain { get; set; }
        public string JobId { get; set; }
        public string Status { get; set; }
        public string JobType { get; set; }
    }
}
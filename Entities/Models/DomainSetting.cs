using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class DomainSetting : BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        [ForeignKey("Domain")]
        public Guid DomainId { get; set; }
        public Domain Domain { get; set; }
        public string SettingsKey { get; set; }
        public string SettingsValue { get; set; }

        public DomainSetting()
        {

        }
    }
}

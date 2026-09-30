using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
namespace Entities.Models
{
    public class Feature : BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        public string Key { get; set; }


        public Feature()
        {}
    }
}
using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
namespace Entities.Models
{
    public class UserModelName: BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        public string ModelName{ get; set; }


        public UserModelName()
        {}
    }
}
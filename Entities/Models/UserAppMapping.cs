using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
namespace Entities.Models
{
    public class UserAppMapping : BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        public String Key{get;set;}
        public String Name{get;set;}
        public String Description{get;set;}
        public String Link{get;set;}

        public UserAppMapping()
        {}
    }
}
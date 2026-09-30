using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
namespace Entities.Models
{
    public class CountryList: BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        public string CountryName{ get; set; }
        public string CountryCode{ get; set; }
        public string MobileCountryCode { get; set; }


        public CountryList()
        {}
    }
}
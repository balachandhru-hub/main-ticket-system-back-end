    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading.Tasks;

    namespace Entities.Models
    {
        public class EmailContent : BaseModel
        {
            [Key]
            [Required]
            public Guid Id { get; set; }

            [Required]
            public string? Key { get; set; }

            [Required]
            public string? Subject { get; set; } 

            [Required]
            public string? Body { get; set; } 
            public EmailContent()
            { }


    }
    }
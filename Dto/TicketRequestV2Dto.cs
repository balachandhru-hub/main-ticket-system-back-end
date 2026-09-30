using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dto
{
    public class TicketRequestV2Dto
    {
        public Guid? UserId { get; set; }
        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; }
        public string? IPAddress { get; set; }
        public string Priority { get; set; }
        public string Category { get; set; }
        public bool IsExceed { get; set; } = false;

        public List<AssetUploadDto>? Asset { get; set; }
    }
}

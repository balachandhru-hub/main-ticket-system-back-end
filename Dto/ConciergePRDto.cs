using System.ComponentModel.DataAnnotations;
namespace Dto
{
    public class ConciergePRRequest
    {
        public bool IsUpdate { get; set; }
        public List<AribaMaterialPRDto> PRRequests { get; set; }
        public List<AssetUploadDto>? Asset { get; set; } // base64 file
    }
    public class ConciergePRResultDto
    {
        public string PRNumber { get; set; }
        public string Status { get; set; }
    }


}

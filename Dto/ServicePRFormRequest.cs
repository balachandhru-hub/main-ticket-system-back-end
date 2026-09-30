using System.ComponentModel.DataAnnotations;
namespace Dto
{
public class ServicePRFormRequest
{
    public bool IsUpdate { get; set; }
    public List<AribaServicePRDto> PRRequests { get; set; }
    public List<AssetUploadDto>? Asset { get; set; } 
}



}

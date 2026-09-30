using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
namespace Dto
{
public class MaterialPRFormRequest
{
    public bool IsUpdate { get; set; }
    public List<AribaMaterialPRDto> PRRequests { get; set; }
    public List<AssetUploadDto>? Asset { get; set; } 
    public String? Type { get; set; }
}
public class MaterialPRResultDto
{
    public string PRNumber { get; set; }
    public string Status { get; set; }
}


}

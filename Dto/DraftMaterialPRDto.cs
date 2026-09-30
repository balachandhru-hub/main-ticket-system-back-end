using System;
using System.Collections.Generic;


namespace Dto
{
    /// <summary>
    /// Request DTO for Draft Material PR Creation
    /// </summary>
    public class DraftMaterialPRFormRequest
    {
        public List<DraftAribaMaterialPRDto> PRRequests { get; set; }
        public List<AssetUploadDto>? Asset { get; set; } 
    }

    /// <summary>
    /// Result DTO for Draft Material PR Creation
    /// </summary>
    public class DraftMaterialPRResultDto
    {
        public string DraftID { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// Request DTO for individual Draft Material PR from Ariba (without PR Number)
    /// </summary>
    public class DraftAribaMaterialPRDto
    {
        public string? UniqueName { get; set; }
        public string? Name { get; set; }
        public string? CompanyCode { get; set; }
        public string? Title { get; set; }
        public string? PurchaseGroup { get; set; }
        public string? ShipToPlant { get; set; }
        public string? DeliverTo { get; set; }
        public string? HeaderComment { get; set; }
        public string? DefaultDeliverTo { get; set; }
        public DateTime NeedBy { get; set; }
        public string? Venue { get; set; }
        public List<MaterialPRLineItemDto> LineItems { get; set; }
        public List<LineItemAttachmentDto>? Attachments { get; set; }
    }
}

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;


namespace Dto
{
    public class AribaMaterialPRDto
    {
        public string? UniqueName { get; set; }
        public string? PRNumber { get; set; }
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
        public bool IsUpdate { get; set; }
        public bool IsHeader { get; set; }
        public bool? TeamReceived { get; set; }
        public string? Type { get; set; }
    }

  





}



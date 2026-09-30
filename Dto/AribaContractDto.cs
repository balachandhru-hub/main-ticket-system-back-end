using Entities.Models;

namespace Dto
{
    public class AribaContractDto
    {

        public string? ContractTaskId { get; set; }
        public string? ContractId { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? TotalAmount { get; set; }
        public string? Currency { get; set; }
        public string? WorkspaceID { get; set; }
        public List<string>? AttachmentID { get; set; }

    public string? Status { get; set; }
        public string? Description { get; set; }
        public string? SupplierID { get; set; }
        public string? SupplierName { get; set; }
        public string? Department { get; set; }
        public string? Approver { get; set; }
        public string?FullUrl { get; set; }
        public bool HasAttachment { get; set; }

public string? CompanyUniqueName { get; set; }
        public List<ContractTaskDto> Tasks { get; set; } = new List<ContractTaskDto>();
        public List<AribaContractHistory> ApprovalHistory { get; set; } = new List<AribaContractHistory>();

        public List<string> Attachments { get; set; } = new List<string>();


    }
}
     
using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
namespace Entities.Models
{
public class AribaContractHistory : BaseModel
{
    [Key]
    public Guid Id { get; set; }
    public string? WorkspaceId { get; set; }
    public string? ContractId { get; set; }
    public string TaskId { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string? AribaUser { get; set; }
    public string? Comments { get; set; }
    public string? Level { get; set; }
    public string? Status { get; set; }
         public AribaContractHistory()
        { }
}
}
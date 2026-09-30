using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
namespace Entities.Models
{
public class AribaPRHistory : BaseModel
{
    [Key]
    public Guid Id { get; set; }
    public string RequisitionsId { get; set; }
    public string AribaTaskId { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string? AribaUser { get; set; }
    public string? Comments { get; set; }
    public string? Level { get; set; }
    public string? Status { get; set; }
}
}
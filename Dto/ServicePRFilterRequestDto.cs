
using System;
using System.Collections.Generic;

namespace Dto
{
public class ServicePRFilterRequestDto
{
    public int Index { get; set; } = 0;
    public int Limit { get; set; } = 10;

    public string? SearchTerm { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public DateTime? ServiceStartDate {get; set;}
    public DateTime? ServiceEndDate {get;set;}

    public string? Status { get; set; }
    public List<string>? GLAccount { get; set; }

    public List<string>? MaterialCode { get; set; }
    public List<string>? CostCenter { get; set; }

}
}

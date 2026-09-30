
using System;
using System.ComponentModel.DataAnnotations;

namespace Dto
{
public class GetUserDto
{

    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? MobilePhone { get; set; }
    public string? Address { get; set; }
    public string? Designation { get; set; }
    public string? EmployeeId { get; set; }
    public string? Department { get; set; }
    public string? Country { get; set; }
    public string? UserRole { get; set; }
    public string? Name { get; set; }
    public string? AribaUserName { get; set; }
    public string? AribaPassword { get; set; }
       public List<UserModelNameDto> Models { get; set; }
       public List<AppNameDto> Apps { get; set; }  
       public List<RequesterNameDto>? RequesterNames { get; set; }
}
}
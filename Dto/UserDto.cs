using Entities.Common;

public class UserDto
{
    public Guid PersonId { get; set; }
    public string? UserName { get; set; }
    public string? UserSecret { get; set; }
    public Guid? UserType { get; set; } = Common.BUYER_USER_TYPE;
}
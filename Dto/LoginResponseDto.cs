namespace Dto
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public Guid RefreshToken { get; set; }

    }
}
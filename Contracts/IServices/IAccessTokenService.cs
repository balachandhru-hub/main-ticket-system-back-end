using Entities.Models;

namespace Contracts.IServices
{
    public interface IAccessTokenService
    {
        void CreateToken(
           Guid userId,
           string accessToken,
           DateTime expiredTime,
           bool expirePreviousTokens 
       );

        AccessToken GetToken(string token);
    }
}
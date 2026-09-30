using Contracts.IRepository;
using Contracts.IServices;
using Dto;
using Entities.Models;
using Contracts;

namespace Services
{
    /// <summary>
    /// Service for managing AccessToken.
    /// </summary>
    public class AccessTokenService : IAccessTokenService
    {
        private readonly IRepositoryWrapper _repoWrapper;

        private readonly ILoggerManager _logger;

        /// <summary>
        /// Constructor for ApiConfig.
        /// </summary>
        /// <param name="repoWrapper">Repository wrapper.</param>
        /// <param name="logger">Logger manager.</param>

        public AccessTokenService(IRepositoryWrapper repoWrapper, ILoggerManager logger)
        {
            _repoWrapper = repoWrapper;
            _logger = logger;
        }

        public void CreateToken(
           Guid userId,
           string accessToken,
           DateTime expiredTime,
           bool expirePreviousTokens //ask user before expiring the tokens
       )
        {
            _logger.LogInfo($"Adding token details for User Id");

            if (expirePreviousTokens)
            {
                IQueryable<AccessToken> existingTokens = _repoWrapper.AccessToken.FindByCondition(r => r.IsActive && r.UserId == userId);

                foreach (AccessToken existingToken in existingTokens)
                {
                    existingToken.ExpiredTime = DateTime.UtcNow;
                    existingToken.IsActive = false;
                    _repoWrapper.AccessToken.Update(existingToken);
                }
            }
            AccessToken newAccessToken = new AccessToken()
            {
                Id = Guid.NewGuid(),
                AccessTokenValue = accessToken,
                ExpiredTime = expiredTime,
                UserId = userId,
            };
            _repoWrapper.AccessToken.Create(newAccessToken);
            _repoWrapper.Save();
            _logger.LogInfo($"Token successfully created for User");
        }



        /// <summary>
        /// To get the token details of the user
        /// </summary>
        /// <param name="token">The token of the logged user</param>
        /// <returns></returns>
        public AccessToken GetToken(string token)
        {
            _logger.LogInfo("Fetching the Access Token");
            AccessToken accessToken = _repoWrapper.AccessToken.FindFirstByCondition(r => r.IsActive && r.AccessTokenValue == token);
            _logger.LogInfo($"Token fetched successfully");
            return accessToken;
        }


    }
}
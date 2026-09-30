using Contracts.IServices;
using Entities.Common;
using ExceptionHandler;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Contracts;

namespace Services
{
    /// <summary>
    /// Service class for UserIdentity Service
    /// </summary>
    public class UserIdentityService : IUserIdentityService
    {
        private readonly IConfiguration _configuration;

        private readonly ILoggerManager _logger;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserContext _userContext;

        /// <summary>
        /// Contructor used for injecting dependencies.
        /// </summary>
        /// <param name="httpContextAccessor">The HTTP context accessor for accessing the current HTTP context.</param>
        /// <param name="configuration">The configuration for accessing application settings.</param>
        /// <param name = "logger" > The logger for logging messages.</param>
        public UserIdentityService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, ILoggerManager logger, IUserContext userContext)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _userContext = userContext;
        }

        /// <summary>
        /// Gets the current user's ID from the HTTP context.
        /// </summary>
        /// <returns>The current user's ID as a GUID, or Guid.Empty if the user is not authenticated or the ID cannot be parsed.</returns>
        public Guid GetCurrentUser()
        {
            _logger.LogInfo("Fetching current user");
            Guid userId;
            if (_httpContextAccessor.HttpContext != null)
            {
                ClaimsPrincipal user = _httpContextAccessor.HttpContext.User;
                if (user.Identity != null && user.Identity.IsAuthenticated)
                {
                    Claim? userIdClaim = user.FindFirst("UserId");
                    if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out userId))
                    {
                        _logger.LogInfo($"Current user ID retrieved successfully: {userId}");
                        return userId;
                    }
                }
            }

            userId = _userContext.GetCurrentUserId();
            if (userId != Guid.Empty)
            {
                return userId;
            }

            return Guid.Empty;
        }


        /// <summary>
        /// Extracts the identity claims from a JWT token.
        /// </summary>
        /// <param name="token">The JWT token.</param>
        /// <returns>The claims extracted from the token.</returns>
        public ClaimsIdentity GetIdentityFromJwtToken(string token)
        {
            _logger.LogInfo("Retrieving identity from the JWT token ");
            token = token.Replace("Bearer ", string.Empty);
            TokenValidationParameters? tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration[Common.TOKEN_KEY]!)),
                ValidateLifetime = false
            };
            JwtSecurityTokenHandler? tokenHandler = new JwtSecurityTokenHandler();

            ClaimsPrincipal? principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (!(securityToken is JwtSecurityToken jwtSecurityToken) || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogError("The token is either not valid or expired");
                throw new UnAuthorizedCustomException("Invalid Token", "The token is either not valid or expired");
            }
            ClaimsIdentity? identity = principal.Identities.FirstOrDefault();
            _logger.LogInfo("JWT token fetched successfully and identity retrieved.");
            return identity!;
        }
    }
}
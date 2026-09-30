using Entities.Models;
using Contracts.IServices;
using ExceptionHandler;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using Entities.Common;


namespace TicketSystemAPI.Attributes
{
    /// <summary>
    /// Class <c>ApiAuthorizationAttribute</c> used to validate the authorization of API 
    /// by reading the Authorize attribute and check against user identity.
    /// </summary>
    public class ApiAuthorizationAttribute : Attribute, IAuthorizationFilter
    {
        /// <summary>
        /// Name string to get from API controller
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Check whether the user is Authorized to access the API or not
        /// </summary>
        /// <param name="context"></param>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (System.String.IsNullOrEmpty(Name))
            {
                throw new ForBiddenCustomException("Not authorized", "Not authorized to access the API");
            }
            IAccessTokenService? _accessTokenService = context.HttpContext.RequestServices.GetService(typeof(IAccessTokenService)) as IAccessTokenService;
            string authHeader = context.HttpContext.Request.Headers.Authorization.ToString();

            HttpContext httpContext = context.HttpContext;

            string token = null;

            // Try reading from cookie first
            if (!httpContext.Request.Cookies.TryGetValue(Common.COOKIE_ACCESS_TOKEN_KEY, out token) || string.IsNullOrWhiteSpace(token) || token.Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                // If cookie not found, try reading Bearer token from header
                if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    token = authHeader.Substring("Bearer ".Length).Trim();
                }
            }

            // If still no token → unauthorized
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new UnAuthorizedCustomException("Not authorized", "Missing or invalid Authorization header");
            }

            AccessToken accessToken = _accessTokenService!.GetToken(token);
            if (accessToken == null || accessToken.ExpiredTime <= DateTime.UtcNow)
            {
                throw new UnAuthorizedCustomException("Not authorized", "User Logged Out");
            }

            IFeatureService? _featureService = context.HttpContext.RequestServices.GetService(typeof(IFeatureService)) as IFeatureService;
            Guid featureId = _featureService!.GetFeatureIdByKey(Name);

            var roleClaim = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            if (roleClaim == null)
            {
                throw new ForBiddenCustomException("Not authorized", "Role not found in token");
            }

            string userRoleId = roleClaim.Value;

            if (context.HttpContext.RequestServices.GetService(typeof(IRoleFeatureMappingService)) is IRoleFeatureMappingService _roleFeatureMappingService && _roleFeatureMappingService.IsRoleFeatureMapped(Guid.Parse(userRoleId), featureId))
            {
                return; //User Authorized. Without setting any result value and just returning is sufficent for authorizing user
            }

            throw new ForBiddenCustomException("Not authorized", "Not authorized to access the API");

        }
    }


}
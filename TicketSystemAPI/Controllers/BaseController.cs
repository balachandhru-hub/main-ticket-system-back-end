using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace TicketSystemAPI.Controllers
{
    /// <summary>
    /// Base controller for other controllers in the API.
    /// </summary>
    public class BaseController : ControllerBase
    {
        /// <summary>
        /// Gets the ID of the currently logged-in user.
        /// </summary>
        /// <remarks>
        /// This method retrieves the ID of the user from the claims in the current user's identity.
        /// </remarks>
        /// <returns>The ID of the currently logged-in user.</returns>
        protected Guid getLoggedUser()
        {
            Guid userId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserId")!.Value);
            return userId;
        }

        protected Guid getUserDomainId()
        {
            Guid userDomainId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "DomainId")!.Value);
            return userDomainId;
        }

        protected List<string> GetRolesFromClaims()
        {
            var claims = User.Claims;
            // Find all claims with the type "role" (or "ClaimTypes.Role")
            var roleClaims = claims.Where(c => c.Type == ClaimTypes.Role);

            // Extract and return the role values as a list of strings
            return roleClaims.Select(c => c.Value).ToList();
        }

        protected string GetIPAddress()
        {
            // 1️⃣ Prefer X-Forwarded-For (real client IP behind proxy)
            if (Request.Headers.TryGetValue("X-Forwarded-For", out var xff) &&
                !string.IsNullOrWhiteSpace(xff))
            {
                return xff.ToString().Split(',')[0].Trim();
            }

            // 2️⃣ Fallback to RemoteIpAddress
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            if (ipAddress == null)
                return "Unknown";

            if (ipAddress.IsIPv4MappedToIPv6)
                ipAddress = ipAddress.MapToIPv4();

            return ipAddress.ToString();
        }
    }
}

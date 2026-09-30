using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TicketSystemAPI.Attributes;
using Dto;
using Contracts;
using ExceptionHandler;
using System.ComponentModel.DataAnnotations;
using Entities.Common;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace TicketSystemAPI.Controllers
{
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly ILoggerManager _logger;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthService authService, ILoggerManager logger, IConfiguration configuration)
        {
            _authService = authService;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Returns the Token to Access the API's.
        /// </summary>
        /// <response code="200">Successfully Login Successfull, token Returened.</response>
        /// <response code="400">Bad Request - User not authenticated.</response>
        [EnableRateLimiting("LoginPolicy")]
        [HttpPost]
        [Route("api/v1/auth/login")]
        [SwaggerOperation(Summary = "Authenticate user and generate token.")]
        [SwaggerResponse(statusCode: 200, type: typeof(LoginResponseDto), description: "Login successful, token returned.")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad Request")]
        public IActionResult GetToken([FromBody] LoginRequestDto request)
        {
            _logger.LogDebug($"Attempting login for user: {request.Username}");

            LoginResponseDto response = _authService.GetAuthToken(request);

            Response.Cookies.Append(Common.COOKIE_ACCESS_TOKEN_KEY, response.Token, new CookieOptions
            {
                Domain = _configuration[Common.DOMAIN_COOKIE_NAME],
                Path = "/",
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true
            });

            Response.Cookies.Append(Common.COOKIE_REFRESH_TOKEN_KEY, response.RefreshToken.ToString(), new CookieOptions
            {
                Domain = _configuration[Common.DOMAIN_COOKIE_NAME],
                Path = "/",
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddMinutes(20),
                IsEssential = true
            });


            _logger.LogInfo($"Token generated successfully for user: {request.Username}, {Request.Host},  Path: {Request.Path}");
            return Ok(new SuccessResponseDto { StatusCode = 200, Message = "Token Generated Successfully", Description = "Successfully Created Token and Refresh Token" });
        }

        /// <summary>
        /// Returns the Role for Current Logged User.
        /// </summary>
        /// <response code="200">Successfully returned the feature list.</response>
        /// <response code="400">Bad Request - User not authenticated.</response>
        [HttpGet]
        [Route("api/v1/auth/role")]
        [SwaggerOperation(Summary = "GetRole")]
        [SwaggerResponse(statusCode: 200, type: typeof(RoleDetailDto), description: "Returns the Role Details")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad Request")]
        [SwaggerResponse(statusCode: 401, "Unauthorized", typeof(ErrorResponseDto))]
        public IActionResult GetRole()
        {
            Guid userId = getLoggedUser();
            _logger.LogDebug($"Fetching the Role for Logged User with Id : {userId}");

            RoleDetailDto response = _authService.GetRole(userId);

            _logger.LogDebug($"Fetched the Role for Logged User with Id : {userId}");
            return Ok(response);
        }

        /// <summary>
        /// Returns the New Refresh Token Id.
        /// </summary>
        /// <response code="200">Successfully returned the New Refresh Token Id.</response>
        /// <response code="400">Bad Request - User not Found.</response>
        [HttpPost]
        [Route("api/v1/auth/refresh-token")]
        [ApiAuthorization(Name = "REFRESH_TOKEN")]
        [SwaggerOperation(Summary = "RefreshToken")]
        [SwaggerResponse(statusCode: 200, type: typeof(RoleDetailDto), description: "Returns the New Refresh Token Id")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad Request")]
        [SwaggerResponse(statusCode: 401, "Unauthorized", typeof(ErrorResponseDto))]
        public IActionResult RefreshToken()
        {
            Guid userId = getLoggedUser();
            Guid refreshToken = Guid.Parse(Request.Cookies[Common.COOKIE_REFRESH_TOKEN_KEY]!);
            _logger.LogDebug($"Updating the Refresh Token for the User With Id : {userId}");

            LoginResponseDto refreshTokenNew = _authService.UpdateTokenExpiry(refreshToken, userId);

            Response.Cookies.Append(Common.COOKIE_ACCESS_TOKEN_KEY, refreshTokenNew.Token, new CookieOptions
            {
                Domain = _configuration[Common.DOMAIN_COOKIE_NAME],
                Path = "/",
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true
            });


            Response.Cookies.Append(Common.COOKIE_REFRESH_TOKEN_KEY, refreshTokenNew.RefreshToken.ToString(), new CookieOptions
            {
                Domain = _configuration[Common.DOMAIN_COOKIE_NAME],
                Path = "/",
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddMinutes(20)
            });

            _logger.LogDebug($"Updated the Refresh Token for the User With Id : {userId}");
            return Ok(new SuccessResponseDto { StatusCode = 200, Message = "Updated Refresh Token", Description = "Updated Token Expiry and Sending the New Refresh Token" });
        }

        /// <summary>
        /// Refreshes the token for SSO without cookies.
        /// </summary>
        /// <response code="200">Token refreshed successfully.</response>
        /// <response code="401">Unauthorized - User not authenticated.</response>
        [HttpPost]
        [Route("api/v1/auth/sso-refresh-token")]
        [ApiAuthorization(Name = "REFRESH_TOKEN")]
        [SwaggerOperation(Summary = "SSO Refresh Token")]
        [SwaggerResponse(statusCode: 200, type: typeof(LoginResponseDto), description: "Token refreshed successfully")]
        [SwaggerResponse(statusCode: 401, "Unauthorized", typeof(ErrorResponseDto))]
        public IActionResult SSORefreshToken([FromHeader][Required] Guid refreshToken)
        {
            Guid userId = getLoggedUser();
            _logger.LogDebug($"SSO Token refresh for User With Id : {userId}");

            LoginResponseDto refreshTokenNew = _authService.UpdateTokenExpiry(refreshToken, userId);

            _logger.LogDebug($"SSO Token refreshed successfully for User With Id : {userId}");
            return Ok(refreshTokenNew);
        }


        /// <summary>
        /// Resets the password for a user.
        /// </summary>
        /// <response code="200">Password reset successfully.</response>
        /// <response code="400">Bad Request - Missing or invalid data.</response>
        /// <response code="404">User not found.</response>
        [HttpPost]
        [Route("api/v1/auth/reset-password")]
        [ValidateModelState]
        [ApiAuthorization(Name = "RESET_PASSWORD")]
        [SwaggerOperation(Summary = "Reset user password")]
        [SwaggerResponse(statusCode: 200, type: typeof(ResetPasswordDto), description: "Password reset successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad Request")]
        [SwaggerResponse(statusCode: 401, "Unauthorized", typeof(ErrorResponseDto))]
        public IActionResult ResetPassword([FromBody] ResetPasswordDto dto)
        {
            _logger.LogDebug($"Resetting password for user: {dto?.UserId}");
            _authService.ResetPassword(dto);
            _logger.LogDebug($"Password reset successfully for user: {dto?.UserId}");
            return Ok(new SuccessResponseDto { StatusCode = 200, Message = "Password reset successfully", Description = "User password has been updated." });
        }



        /// <summary>
        /// Logs out the user.
        /// </summary>
        /// <response code="200">Logged out successfully.</response>
        /// <response code="401">Unauthorized - User not authenticated.</response>
        [HttpPut]
        [Route("api/v1/auth/logout")]
        [ValidateModelState]
        [ApiAuthorization(Name = "LOGOUT")]
        [SwaggerOperation("Logout")]
        [SwaggerResponse(statusCode: 200, type: typeof(SuccessResponseDto), description: "Logged out successfully")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized")]
        public virtual IActionResult Logout()
        {
            Guid userId = getLoggedUser();

            _logger.LogDebug($"Logging out the user with Id: {userId}");
            _authService.Logout(userId);

            _logger.LogDebug("Person logged out successfully.");
            return Ok(new SuccessResponseDto { StatusCode = 200, Message = "Logged out successfully.", Id = userId.ToString(), Description = "User Logged Out Successfully" });
        }

        /// <summary>
        /// Fetches the List of Available Roles.
        /// </summary>
        /// <param name="roleCategory">The Role type Category</param>
        /// <response code="200">Fetches the List of User Roles.</response>
        /// <response code="401">Unauthorized - User not authenticated.</response>
        [HttpGet]
        [Route("api/v1/auth/role/all/{roleCategory}")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GET_ALL_ROLES")]
        [SwaggerOperation("GetAllRoles")]
        [SwaggerResponse(statusCode: 200, type: typeof(List<RoleDetailDto>), description: "Fetched the List of User Roles")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized")]
        public virtual IActionResult GetAllRole([FromRoute][Required] string roleCategory)
        {
            Guid userId = getLoggedUser();
            _logger.LogDebug($"Fetching all the User Roles for the User with : {userId}");
            List<RoleDetailDto> result = _authService.GetAllRole(roleCategory);
            _logger.LogDebug($"Fetched all the User Roles for the User with : {userId}");
            return Ok(result);
        }

        /// <summary>
        /// Fetches the Claim Detail for Logged In User.
        /// </summary>
        /// <response code="200">Fetches the List of User Roles.</response>
        /// <response code="401">Unauthorized - User not authenticated.</response>
        [HttpGet]
        [Route("api/v1/auth/token-claim")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GET_ALL_CLAIMS")]
        [SwaggerOperation("GetAllClaim")]
        [SwaggerResponse(statusCode: 200, type: typeof(TokenClaimDto), description: "Fetches the Claim Detail for Logged In User")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized")]
        public virtual IActionResult GetAllClaim()
        {
            Guid userId = getLoggedUser();
            string token = Request.Cookies[Common.COOKIE_ACCESS_TOKEN_KEY]!;
            _logger.LogDebug($"Fetching all the User Claim for the User with : {userId}");
            TokenClaimDto result = _authService.GetClaim(token, userId);
            _logger.LogDebug($"Fetched all the User Claim for the User with : {userId}");
            return Ok(result);
        }

        /// <summary>
        /// Fetches the Claim Detail for Logged In User.
        /// </summary>
        /// <response code="200">Fetches the List of User Roles.</response>
        /// <response code="401">Unauthorized - User not authenticated.</response>
        [HttpGet]
        [Route("api/v1/auth/microsoft-login")]
        [ValidateModelState]
        [SwaggerOperation("GetToken")]
        [SwaggerResponse(statusCode: 200, type: typeof(TokenClaimDto), description: "Fetches the Claim Detail for Logged In User")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized")]
        public virtual IActionResult GetToken([FromHeader][Required] string email)
        {
            _logger.LogDebug($"Extracting the Email and Validating the User from Microsoft");

            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized("Email not found in token");

            LoginResponseDto response = _authService.GetAuthTokenMicrosoft(email);
            Response.Cookies.Append(Common.COOKIE_ACCESS_TOKEN_KEY, response.Token, new CookieOptions
            {
                Domain = _configuration[Common.DOMAIN_COOKIE_NAME],
                Path = "/",
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true
            });

            Response.Cookies.Append(Common.COOKIE_REFRESH_TOKEN_KEY, response.RefreshToken.ToString(), new CookieOptions
            {
                Domain = _configuration[Common.DOMAIN_COOKIE_NAME],
                Path = "/",
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddMinutes(20),
                IsEssential = true
            });
            _logger.LogDebug($"Extracted and Validated the User from Microsoft");
            return Ok(new SuccessResponseDto { StatusCode = 200, Message = "Token Generated Successfully", Description = "Successfully Created Token and Refresh Token" });
        }

        /// <summary>
        /// Authenticates user via SSO and returns token without cookies.
        /// </summary>
        /// <response code="200">Successfully authenticated, token returned.</response>
        /// <response code="401">Unauthorized - Email not found.</response>
        [HttpPost]
        [Route("api/v1/auth/sso-login")]
        [ValidateModelState]
        [ApiKeyAuthorization(Name = "SSO_LOGIN_EXTERNAL_SYSTEM")]
        [SwaggerOperation("SSOLogin")]
        [SwaggerResponse(statusCode: 200, type: typeof(LoginResponseDto), description: "SSO authentication successful, token returned")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized")]
        public virtual IActionResult SSOLogin([FromHeader][Required] string email)
        {
            _logger.LogDebug($"SSO Login attempt with email: {email}");

            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized("Email not found in token");

            LoginResponseDto response = _authService.GetAuthTokenMicrosoft(email);
            _logger.LogDebug($"SSO Login successful for email: {email}");
            return Ok(response);
        }

        /// <summary>
        /// Impersonates a user (BDU only)
        /// </summary>
        /// <param name="targetUserId">The ID of the user to impersonate</param>
        /// <response code="200">Impersonation successful, token returned.</response>
        /// <response code="400">Bad Request - Invalid user or unauthorized.</response>
        /// <response code="401">Unauthorized - User not authenticated or not BDU.</response>
        /// <response code="404">User not found.</response>
        [HttpPost]
        [Route("api/v1/auth/impersonate/{targetUserId}")]
        [ApiAuthorization(Name = "IMPERSONATE_USER")]
        [SwaggerOperation(Summary = "Impersonate a user.")]
        [SwaggerResponse(statusCode: 200, type: typeof(LoginResponseDto), description: "Impersonation successful, token returned.")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad Request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorResponseDto), description: "User not found")]
        public IActionResult ImpersonateUser([FromRoute] Guid targetUserId)
        {
            _logger.LogDebug($"Impersonation request: wants to impersonate {targetUserId}");

            LoginResponseDto response = _authService.ImpersonateUser(targetUserId);
            Response.Cookies.Append(Common.COOKIE_ACCESS_TOKEN_KEY, response.Token, new CookieOptions
            {
                Domain = _configuration[Common.DOMAIN_COOKIE_NAME],
                Path = "/",
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true
            });

            Response.Cookies.Append(Common.COOKIE_REFRESH_TOKEN_KEY, response.RefreshToken.ToString(), new CookieOptions
            {
                Domain = _configuration[Common.DOMAIN_COOKIE_NAME],
                Path = "/",
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddMinutes(20),
                IsEssential = true
            });
            _logger.LogInfo($"Impersonation token generated successfully for target user: {targetUserId}");
            return Ok(response);
        }
    }
}

using ExceptionHandler;
using Entities.Models;
using Contracts.IRepository;
using Dto;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Contracts;
using Entities.Common;
using Microsoft.Extensions.Configuration;
using HashingSystem;
using Microsoft.AspNetCore.Http;

namespace Services
{
    public class AuthService : IAuthService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IConfiguration _configuration;
        private readonly ILoggerManager _logger;
        private readonly IBcryptHashing _hashing;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(IRepositoryWrapper repositoryWrapper, IConfiguration configuration, ILoggerManager logger, IBcryptHashing hashing, IHttpContextAccessor httpContextAccessor)
        {
            _repositoryWrapper = repositoryWrapper;
            _configuration = configuration;
            _logger = logger;
            _hashing = hashing;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Generates the Token
        /// </summary>
        /// <param name="request"></param>
      public LoginResponseDto GetAuthToken(LoginRequestDto request)
        {
            int maxAttempts = int.TryParse(_configuration["LoginSecurity:MaxFailedAttempts"], out int m) ? m : 5;
            int lockoutMinutes = int.TryParse(_configuration["LoginSecurity:LockoutMinutes"], out int l) ? l : 2;
            int maxActiveSessions = int.TryParse(_configuration["TokenSecurity:MaxActiveSessions"], out int s) ? s : 3;

            _logger.LogInfo($"Authentication request received for user: {request.Username}");

            string ipAddress = _httpContextAccessor.HttpContext?.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim()
                ?? _httpContextAccessor.HttpContext?.Request.Headers["X-Real-IP"].FirstOrDefault()
                ?? _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.MapToIPv4()?.ToString();

            User? user = _repositoryWrapper.User
                .FindFirstByCondition(u => u.UserName == request.Username);

             if (user == null)
            {
                _logger.LogError($"Invalid username or password for user: {request.Username}");
                throw new UnAuthorizedCustomException(
                    "Invalid username or password",
                    "Invalid username or password.");
            }
            else if (user != null && user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            {
                throw new TooManyRequestsCustomException(
                    "Maximum limit reached. Please try again later.",
                    $"Try again after {user.LockoutEnd} UTC");
            }
         
            else if (!_hashing.VerifyHash(request.Password, user.UserSecret))
            {
                user.FailedLoginAttempts++;

                if (user.FailedLoginAttempts >= maxAttempts)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(lockoutMinutes);
                }

                user.LastFailedLogin = DateTime.UtcNow;
                _repositoryWrapper.User.Update(user);

                _repositoryWrapper.Save(); 

                _logger.LogError($"Invalid password for user: {request.Username}");

                if (user.LockoutEnd != null && user.LockoutEnd > DateTime.UtcNow)
                {
                    throw new TooManyRequestsCustomException(
                        "Maximum limit reached",
                        $"Try again after {user.LockoutEnd}");
                }

                throw new UnAuthorizedCustomException(
                    "Invalid username or password",
                    "Invalid username or password");
            }

         
            else
            {
                user.FailedLoginAttempts = 0;
                user.LockoutEnd = null;
                _repositoryWrapper.User.Update(user);

                UserRoleMapping? userRoleMapping = _repositoryWrapper.UserRoleMapping
                    .FindByCondition(rm => rm.UserId == user.Id).FirstOrDefault();

                if (userRoleMapping == null)
                {
                    _logger.LogError($"UserRoleMapping not found for userId: {user.Id}");
                    throw new UnAuthorizedCustomException("Unauthorized", "User role not found.");
                }

                Person? person = _repositoryWrapper.Person
                    .FindFirstByCondition(p => p.IsActive && p.Id == user.PersonId);

                if (person == null)
                {
                    _logger.LogError($"Person Not Found for userId : {user.Id}");
                    throw new UnAuthorizedCustomException("Unauthorized", "Person Not Found.");
                }

                var claims = new[]
                {
                    new Claim(ClaimTypes.Role, userRoleMapping.RoleId.ToString()),
                    new Claim("PersonId", user.PersonId.ToString()),
                    new Claim("UserId", user.Id.ToString()),
                    new Claim("DomainId", person.DomainId.ToString())
                };

                int number = int.TryParse(_configuration[Common.TOKEN_EXPIRY], out int result)
                    ? result : Common.TOKEN_EXPIRY_TIME_DEFAULT;

                DateTime expirationTime = DateTime.UtcNow.AddSeconds(number);
                string jwtToken = GenerateToken(claims, expirationTime);

                 var activeTokens = _repositoryWrapper.AccessToken
                    .FindByCondition(x => x.UserId == user.Id && x.IsActive)
                    .OrderByDescending(x => x.ExpiredTime)
                    .ToList();

                if (activeTokens.Count > maxActiveSessions)
                {
                    var tokensToDeactivate = activeTokens.Skip(maxActiveSessions).ToList();

                    foreach (var token in tokensToDeactivate)
                    {
                        token.IsActive = false;
                       
                    }
                    _repositoryWrapper.AccessToken.UpdateRange(tokensToDeactivate);
                }
                AccessToken accessToken = new AccessToken
                {
                    Id = Guid.NewGuid(),
                    AccessTokenValue = jwtToken,
                    ExpiredTime = expirationTime,
                    UserId = user.Id,
                    RefreshToken = Guid.NewGuid(),
                };

                _repositoryWrapper.AccessToken.Create(accessToken);
               

                LoginRecord loginRecord = new LoginRecord
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    DomainId = person.DomainId,
                    LoginTime = DateTime.UtcNow,
                    EntityType = Common.LOGIN_ATTRIBUTE_LOGIN,
                    IpAddress = ipAddress,
                    AttributeId = user.Id.ToString()
                };

                _repositoryWrapper.LoginRecord.Create(loginRecord);

                _repositoryWrapper.Save(); 

                _logger.LogInfo($"Access token created for user: {user.Id}");

                return new LoginResponseDto
                {
                    Token = jwtToken,
                    RefreshToken = accessToken.RefreshToken
                };
            }
        }

        private string GenerateToken(Claim[] claims, DateTime expirationTime)
        {
            string? tokenKey = _configuration[Common.TOKEN_KEY];
            if (string.IsNullOrEmpty(tokenKey))
            {
                _logger.LogError("Token key is not configured.");
                throw new InvalidOperationException("Token key is not configured.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var issuer = _configuration[Common.TOKEN_ISSUER];
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expirationTime,
                Issuer = issuer,
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Gets The Current Role for Logged User
        /// </summary>
        /// <param name="userId">user id</param>
        public RoleDetailDto GetRole(Guid userId)
        {
            _logger.LogInfo($"Fetching the Current Logged User with Id : {userId}");
            RoleDetailDto result = _repositoryWrapper.User.GetUserRole(userId);
            _logger.LogInfo($"Fetched the Current Logged User with Id : {userId}");
            return result;
        }

        /// <summary>
        /// Updates the Token Expiry Time
        /// </summary>
        /// <param name="refreshToken">refreshToken</param>
        /// <param name="userId">user Id </param>
        public LoginResponseDto UpdateTokenExpiry(Guid refreshToken, Guid userId)
        {
            _logger.LogInfo($"Updating the Token Expiry Time for the User with Id : {userId}");
            if (refreshToken == null || refreshToken == Guid.Empty)
            {
                _logger.LogError($"Unable to find the Refresh Token for the Given User : {userId}");
                throw new PreConditionFailedCustomException("Unable to Find the Given Refresh Token for the Session", $"Unable to find the Refresh Token {refreshToken} for the Session");
            }
            AccessToken accessToken = _repositoryWrapper.AccessToken.FindFirstByCondition(x => x.IsActive && x.RefreshToken == refreshToken);
            if (accessToken == null)
            {
                _logger.LogError($"Unable to Find the Refresh Token with Id : {refreshToken}");
                throw new NotFoundCustomException($"Unable to Find the Refresh Token ", $"Unable to Find the Refresh Token with Id : {refreshToken}");
            }

            TokenClaimDto claim = GetClaim(accessToken.AccessTokenValue, userId);

            var newExpiry = DateTime.UtcNow.AddMinutes(20);

            var claims = new[]
            {
            new Claim(ClaimTypes.Role, claim.RoleId.ToString()),
            new Claim("PersonId", claim.PersonId.ToString()),
            new Claim("UserId", claim.UserId.ToString()),
            new Claim("DomainId", claim.DomainId.ToString())
        };
            var newJwt = GenerateToken(claims, newExpiry);

            int number = int.TryParse(_configuration[Common.REFRESH_TOKEN_EXPIRY], out int result) ? result : Common.REFRESH_TOKEN_EXPIRY_TIME_DEFAULT;
            accessToken.AccessTokenValue = newJwt;
            accessToken.ExpiredTime = DateTime.UtcNow.AddSeconds(number);
            accessToken.RefreshToken = Guid.NewGuid();
            _repositoryWrapper.AccessToken.Update(accessToken);
            _repositoryWrapper.Save();
            _logger.LogInfo($"Updated the Token Expiry Time for the User with Id : {userId}");
            return new LoginResponseDto
            {
                Token = newJwt,
                RefreshToken = accessToken.RefreshToken
            }; ;
        }



        /// <summary>
        /// Resets the password for a user using their user ID.
        /// </summary>
        public void ResetPassword(ResetPasswordDto dto)
        {
            _logger.LogInfo($"Resetting password for user: {dto.UserId}");

            User? user = _repositoryWrapper.User.FindFirstByCondition(u => u.IsActive && u.Id == dto.UserId);
            if (user == null)
            {
                _logger.LogError($"User not found for ID: {dto.UserId}");
                throw new NotFoundCustomException("User not found", $"No user exists with ID: {dto.UserId}");
            }
            bool isSamePassword = _hashing.VerifyHash(dto.Password, user.UserSecret);
            if (isSamePassword)
            {
                _logger.LogError($"New password cannot be the same as the old password for user: {dto.UserId}");
                throw new BadRequestCustomException(
                    "Same Password",
                    "The new password cannot be the same as the current password."
                );
            }

            user.UserSecret = _hashing.HashStringWithSalt(dto.Password);
            _repositoryWrapper.User.Update(user);
            _repositoryWrapper.Save();

            _logger.LogInfo($"Password reset successfully for user: {dto.UserId}");
        }

        /// <summary>
        /// Logs out the user by deactivating the access token associated with the current session.
        /// </summary>
        /// <param name="userId">User Id of the user</param>
        public void Logout(Guid userId)
        {
            string? token = _httpContextAccessor.HttpContext?.Request.Cookies[Common.COOKIE_ACCESS_TOKEN_KEY];
            _logger.LogInfo("Fetching Active access token");
            AccessToken? accessToken = _repositoryWrapper.AccessToken.FindFirstByCondition(r => r.IsActive && r.AccessTokenValue == token);
            if (accessToken == null)
            {
                _logger.LogError($"Unable to find the Access Token for the User : {userId} with Token : {token}");
                throw new NotFoundCustomException("Unable to Find the Access Token for the User", $"Unable to Find the Access Token for the User : {userId} with Token : {token}, Please Try Again");
            }
            _logger.LogInfo($"Logging out user id: {accessToken!.Id}");
            accessToken.ExpiredTime = DateTime.UtcNow;
            _repositoryWrapper.AccessToken.Update(accessToken);
            _repositoryWrapper.Save();
            _httpContextAccessor.HttpContext?.Response.Cookies.Delete(Common.COOKIE_ACCESS_TOKEN_KEY);
            _httpContextAccessor.HttpContext?.Response.Cookies.Delete(Common.COOKIE_REFRESH_TOKEN_KEY);
            _logger.LogInfo($"User logged out successfully with access token Id: {accessToken.Id}");
        }

        /// <summary>
        /// Gets the list of available roles
        /// </summary>
        /// <param name="roleCategory">for which type the person belongs to</param>
        public List<RoleDetailDto> GetAllRole(string roleCategory)
        {
            _logger.LogInfo($"Fetching the Available roles for the Category : {roleCategory}");
            List<RoleDetailDto> result = new List<RoleDetailDto>();
            if (roleCategory == Common.BUSINESS_ROLE_CATEGORY_KEY)
            {
                List<Role> roles = _repositoryWrapper.Role.FindByCondition(x => x.IsActive && (x.UserRole == Common.BUSINESS_USER_KEY || x.UserRole == Common.BUSINESS_ADMIN_USER_KEY || x.UserRole == Common.LEADERSHIP_USER_KEY || x.UserRole == Common.SUPPLIER_USER_KEY || x.UserRole == Common.OPERATIONS_USER_KEY || x.UserRole == Common.FINANCE_OPERATION_USER_KEY)).ToList();
                result = roles.Select(x => new RoleDetailDto
                {
                    Id = x.Id,
                    UserRole = x.UserRole
                }).ToList();
            }
            else
            {
                Domain? domain = _repositoryWrapper.Domain.FindFirstByCondition(x => x.IsActive && x.Id == Common.GLOBAL_DOMAIN_ID);
                List<Role> roles = _repositoryWrapper.Role.FindByCondition(x => x.IsActive && x.UserRole != Common.BUSINESS_USER_KEY && x.UserRole != Common.BUSINESS_ADMIN_USER_KEY && x.UserRole != Common.LEADERSHIP_USER_KEY && x.UserRole != Common.SUPPLIER_USER_KEY || x.UserRole != Common.OPERATIONS_USER_KEY || x.UserRole != Common.FINANCE_OPERATION_USER_KEY).ToList();
                result = roles.Select(x => new RoleDetailDto
                {
                    Id = x.Id,
                    UserRole = x.UserRole,
                    DomainId = domain.Id
                }).ToList();
            }
            _logger.LogInfo($"Fetched the Available roles for the Category : {roleCategory}");
            return result;
        }

        /// <summary>
        /// Gets the Logged In User Claim
        /// </summary>
        /// <param name="token"></param>
        /// <param name="userId"></param>
        public TokenClaimDto GetClaim(string token, Guid userId)
        {
            _logger.LogInfo($"Fetching the Claim Details");
            if (token == null)
            {
                _logger.LogError($"Unable to Find the Token for  the User : {userId}");
                throw new PreConditionFailedCustomException("Unable to Find the Claim for the Token", $"Unable to Find the Claim for the Token for User : {userId}");
            }
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = handler.ReadJwtToken(token);

            Dictionary<string, string> claims = jwtToken.Claims.ToDictionary(c => c.Type, c => c.Value);

            TokenClaimDto dto = new TokenClaimDto
            {
                UserId = Guid.Parse(claims["UserId"]),
                RoleId = Guid.Parse(claims["role"]),
                DomainId = Guid.Parse(claims["DomainId"]),
                PersonId = Guid.Parse(claims["PersonId"])
            };

            _logger.LogInfo($"Fetched the Claim Details");
            return dto;
        }

        /// <summary>
        /// Generates the Token
        /// </summary>
        /// <param name="email"></param>
        public LoginResponseDto GetAuthTokenMicrosoft(string email)
        {
            _logger.LogInfo($"Authentication request received for user with email: {email}");

            Person? person = _repositoryWrapper.Person.FindFirstByCondition(p => p.IsActive && p.Email == email);
            if (person == null)
            {
                _logger.LogError($"Person Not Found for Email: {email}");
                throw new UnAuthorizedCustomException("Unauthorized", "Person Not Found.");
            }

            User? user = _repositoryWrapper.User.FindFirstByCondition(x => x.IsActive && x.PersonId == person.Id);
            if (user == null)
            {
                _logger.LogError($"Unable to find the Email : {email}");
                throw new UnAuthorizedCustomException("Invalid Email Address", "Invalid Email Address.");
            }

            UserRoleMapping? userRoleMapping = _repositoryWrapper.UserRoleMapping
                .FindByCondition(rm => rm.UserId == user.Id).FirstOrDefault();

            if (userRoleMapping == null)
            {
                _logger.LogError($"UserRoleMapping not found for userId: {user.Id}");
                throw new UnAuthorizedCustomException("Unauthorized", "User role not found.");
            }

            var claims = new[]
            {
            new Claim(ClaimTypes.Role, userRoleMapping.RoleId.ToString()),
            new Claim("PersonId", person.Id.ToString()),
            new Claim("UserId", user.Id.ToString()),
            new Claim("DomainId", person.DomainId.ToString())
        };

            int number = int.TryParse(_configuration[Common.TOKEN_EXPIRY], out int result) ? result : Common.TOKEN_EXPIRY_TIME_DEFAULT;
            DateTime expirationTime = DateTime.UtcNow.AddSeconds(number);
            string jwtToken = GenerateToken(claims, expirationTime);

            AccessToken accessToken = new AccessToken
            {
                Id = Guid.NewGuid(),
                AccessTokenValue = jwtToken,
                ExpiredTime = expirationTime,
                UserId = user.Id,
                RefreshToken = Guid.NewGuid(),
            };

            _repositoryWrapper.AccessToken.Create(accessToken);
            _repositoryWrapper.Save();

            _logger.LogInfo($"Access token created for user: {user.Id}");

            return new LoginResponseDto
            {
                Token = jwtToken,
                RefreshToken = accessToken.RefreshToken
            };
        }

        /// <summary>
        /// Generates an impersonation token for a user
        /// <summary>
        /// Generates an impersonation token for a user
        /// </summary>
        /// <param name="targetUserId">The user ID to impersonate</param>
        public LoginResponseDto ImpersonateUser(Guid targetUserId)
        {
            Guid impersonatorUserId = Guid.Parse(_httpContextAccessor.HttpContext!.User.Claims.FirstOrDefault(c => c.Type == "UserId")!.Value);
            _logger.LogInfo($"Impersonation request: Impersonator {impersonatorUserId} wants to impersonate {targetUserId}");


            UserRoleMapping? impersonatorRoleMapping = _repositoryWrapper.UserRoleMapping
                .FindByCondition(rm => rm.UserId == impersonatorUserId).FirstOrDefault();

            if (impersonatorRoleMapping == null || impersonatorRoleMapping.RoleId != Common.BUSINESS_ADMIN_ROLE_ID)
            {
                _logger.LogError($"Impersonation denied: User {impersonatorUserId} does not have BDU role");
                throw new UnAuthorizedCustomException("Unauthorized", "Only BDU users can impersonate others.");
            }


            User? targetUser = _repositoryWrapper.User.FindFirstByCondition(u => u.IsActive && u.Id == targetUserId);
            if (targetUser == null)
            {
                _logger.LogError($"Target user not found: {targetUserId}");
                throw new NotFoundCustomException("User not found", $"No user exists with ID: {targetUserId}");
            }

            Person? targetPerson = _repositoryWrapper.Person.FindFirstByCondition(p => p.IsActive && p.Id == targetUser.PersonId);
            if (targetPerson == null)
            {
                _logger.LogError($"Person not found for target user: {targetUserId}");
                throw new NotFoundCustomException("Person not found", $"No person found for user: {targetUserId}");
            }

            UserRoleMapping? targetUserRoleMapping = _repositoryWrapper.UserRoleMapping
                .FindByCondition(rm => rm.UserId == targetUser.Id).FirstOrDefault();

            if (targetUserRoleMapping == null)
            {
                _logger.LogError($"UserRoleMapping not found for target user: {targetUser.Id}");
                throw new UnAuthorizedCustomException("Unauthorized", "Target user role not found.");
            }


            var claims = new[]
            {
                new Claim(ClaimTypes.Role, targetUserRoleMapping.RoleId.ToString()),
                new Claim("PersonId", targetUser.PersonId.ToString()),
                new Claim("UserId", targetUser.Id.ToString()),
                new Claim("DomainId", targetPerson.DomainId.ToString())
            };

            int number = int.TryParse(_configuration[Common.TOKEN_EXPIRY], out int result) ? result : Common.TOKEN_EXPIRY_TIME_DEFAULT;
            DateTime expirationTime = DateTime.UtcNow.AddSeconds(number);
            string jwtToken = GenerateToken(claims, expirationTime);

            AccessToken accessToken = new AccessToken
            {
                Id = Guid.NewGuid(),
                AccessTokenValue = jwtToken,
                ExpiredTime = expirationTime,
                UserId = targetUser.Id,
                RefreshToken = Guid.NewGuid(),
            };

            _repositoryWrapper.AccessToken.Create(accessToken);
            _repositoryWrapper.Save();

            _logger.LogInfo($"Impersonation token created: Impersonator {impersonatorUserId} impersonating {targetUser.Id}");

            return new LoginResponseDto
            {
                Token = jwtToken,
                RefreshToken = accessToken.RefreshToken
            };
        }
    }
}
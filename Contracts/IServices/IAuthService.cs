using Dto;

public interface IAuthService
{
    /// <summary>
    /// Generates the Token
    /// </summary>
    /// <param name="request"></param>
    LoginResponseDto GetAuthToken(LoginRequestDto request);

    /// <summary>
    /// Gets The Current Role for Logged User
    /// </summary>
    /// <param name="userId">user id</param>
    RoleDetailDto GetRole(Guid userId);

    /// <summary>
    /// Updates the Token Expiry Time
    /// </summary>
    /// <param name="refreshToken">refreshToken</param>
    /// <param name="userId">user Id </param>
    LoginResponseDto UpdateTokenExpiry(Guid refreshToken, Guid userId);

    /// <summary>
    /// The function "Logout" is used to log out a user.
    /// </summary>
    /// <param name="userId">User Id of the user</param
    void Logout(Guid userId);

    /// <summary>
    /// Gets the list of available roles
    /// </summary>
    /// <param name="roleCategory">for which type the person belongs to</param>
    List<RoleDetailDto> GetAllRole(string roleCategory);

    /// <summary>
    /// Gets the Logged In User Claim
    /// </summary>
    /// /// <param name="token"></param>
    /// <param name="userId"></param>
    TokenClaimDto GetClaim(string token, Guid userId);
    /// <summary>
    /// Resets the password for a user.
    ///     
    /// </summary>
    /// <param name="resetPasswordDto">ResetPasswordDto containing UserId and NewPassword
    /// </param>            
    void ResetPassword(ResetPasswordDto dto);

    /// <summary>
    /// Generates the Token from the microsoft token
    /// </summary>
    /// <param name="request"></param>
    LoginResponseDto GetAuthTokenMicrosoft(string email);

    /// <summary>
    /// Generates an impersonation token for a user
    /// </summary>
    /// <param name="impersonatorUserId">The user ID of the impersonator (BDU)</param>
    /// <param name="targetUserId">The user ID to impersonate</param>
    LoginResponseDto ImpersonateUser( Guid targetUserId);
}
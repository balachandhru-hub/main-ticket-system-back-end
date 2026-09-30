using Dto;
using System;
using System.Collections.Generic;
using System.Security.Claims;
 
namespace Contracts.IServices
{
    /// <summary>
    /// Interface <c>IUserIdentityService</c> contains the definition of user authentication methods.
    /// </summary>
    public interface IUserIdentityService
    {
        /// <summary>
        /// Gets the current user's ID from the HTTP context.
        /// </summary>
        /// <returns>The current user's ID as a GUID, or Guid.Empty if the user is not authenticated or the ID cannot be parsed.</returns>
        Guid GetCurrentUser();
 
        /// <summary>
        /// Extracts the identity claims from a JWT token.
        /// </summary>
        /// <param name="token">The JWT token.</param>
        /// <returns>The claims extracted from the token.</returns>
        ClaimsIdentity GetIdentityFromJwtToken(string token);
    }
}
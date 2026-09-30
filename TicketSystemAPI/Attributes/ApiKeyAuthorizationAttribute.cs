using Entities.Models;
using Contracts.IServices;
using ExceptionHandler;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;


namespace TicketSystemAPI.Attributes
{
    /// <summary>
    /// Class <c>ApiKeyAuthorizationAttribute</c> used to validate the authorization of API 
    /// by reading the Authorize attribute and checking with the api key.
    /// </summary>
    public class ApiKeyAuthorizationAttribute : Attribute, IAuthorizationFilter
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
            
            if (context.HttpContext.Request.Headers["X-API-KEY"].FirstOrDefault() != null)
            {
                string? apiKey = context.HttpContext.Request.Headers["X-API-KEY"].FirstOrDefault();
                if (context.HttpContext.RequestServices.GetService(typeof(IApiKeyService)) is IApiKeyService _apiKeyService && _apiKeyService.IsApiKeyValid(apiKey!))
                {
                    return; //User Authorized. With the Api Key
                }
            }
            else
            {
                throw new UnAuthorizedCustomException("Not authorized", "Invalid Data");
            }

        }
    }


}
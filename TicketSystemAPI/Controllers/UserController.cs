using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Contracts.IServices;
using Contracts;
using System.Collections.Generic;
using TicketSystemAPI.Attributes;
using System.ComponentModel.DataAnnotations;
using Dto;
using Microsoft.Extensions.Logging;


namespace TicketSystemAPI.Controllers
{
    /// <summary>
    /// Controller for managing users.
    /// </summary>
    [ApiController]
    public class UserController : BaseController
    {
        private readonly ILoggerManager _logger;
        private readonly IUserService _userService;

        /// <summary>
        /// Constructor for User Controller.
        /// </summary>
        /// <param name="logger">The logger manager.</param>
        /// <param name="userService">The user service.</param>
        public UserController(ILoggerManager logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }


        [HttpGet]
        [Route("api/v1/user/domain-user/{domain}")]
        [ApiAuthorization(Name = "GET_USER_BY_DOMAIN")]
        [SwaggerOperation("GetUserByDomainName")]

        [SwaggerResponse(statusCode: 200, type: typeof(List<DomainUserDto>), description: "List of all domain user")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorResponseDto), description: "user Not Found")]

        public IActionResult GetUserByDomainName([FromRoute] string domain)
        {
            _logger.LogDebug($"Fetching users for domain: {domain}");
            List<DomainUserDto> users = _userService.GetUserByDomain(domain);
            _logger.LogDebug($"Fetched users for domain: {domain}");
            return Ok(users);
        }

       [HttpGet]
        [Route("api/v1/user/{userId}")]
        [ApiAuthorization(Name = "GET_USER_BY_ID")]
        [SwaggerOperation("GetUserById")]
        [SwaggerResponse(statusCode: 200, type: typeof(GetUserDto), description: "User details")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorResponseDto), description: "User Not Found")]
        public IActionResult GetUserById([FromRoute] Guid userId)
        {
            _logger.LogDebug($"Fetching user by ID: {userId}");
            GetUserDto user = _userService.GetUserById(userId);
            _logger.LogDebug($"Fetched user for ID: {userId}");
            return Ok(user);
        }
        }
    }


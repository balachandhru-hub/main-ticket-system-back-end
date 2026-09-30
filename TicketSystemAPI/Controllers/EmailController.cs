using Contracts;
using Contracts.IServices;
using Dto;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using TicketSystemAPI.Attributes;

namespace TicketSystemAPI.Controllers
{
    [ApiController]
    public class EmailController : BaseController
    {
        private readonly ILoggerManager _logger;
        private readonly IEmailService _emailService;
        public EmailController(ILoggerManager logger, IEmailService emailService)
        {
            _logger = logger;
            _emailService = emailService;
        }

        /// <summary>
        /// Create a new Email CC Address
        /// </summary>
        [HttpPost]
        [Route("api/v1/email-cc")]
        [ValidateModelState]
        [ApiAuthorization(Name = "CREATE_EMAIL_CC")]
        [SwaggerOperation("CreateCCEmail")]
        [SwaggerResponse(statusCode: 201, type: typeof(SuccessResponseDto), description: "Email created successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(statusCode: 412, type: typeof(ErrorResponseDto), description: "Email Exists")]
        public IActionResult CreateCCEmail([Required][FromBody] EmailCCDto dto)
        {
            _logger.LogDebug($"Creating CC Email for given Email Address : {dto.EmailAddress}");
            Guid result = _emailService.CreateCCEmail(dto);
            _logger.LogInfo($"CC Email Created with Id : {result}");
            return Ok(new SuccessResponseDto { Id = result.ToString(), StatusCode = 201, Message = "Email Created Successfully", Description = "Email Created Successfully" });
        }

        /// <summary>
        /// Delete Email CC Address
        /// </summary>
        [HttpDelete]
        [Route("api/v1/email-cc/{id}")]
        [ValidateModelState]
        [ApiAuthorization(Name = "DELETE_EMAIL_CC")]
        [SwaggerOperation("DeleteCCEmail")]
        [SwaggerResponse(statusCode: 201, type: typeof(SuccessResponseDto), description: "Email deleted successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorResponseDto), description: "Email Not Found")]
        public IActionResult DeleteCCEmail([FromRoute][Required] Guid id)
        {
            _logger.LogDebug($"Deleting CC Email with Id : {id}");
            _emailService.DeleteCCEmail(id);
            _logger.LogInfo($"CC Email Deleted with Id : {id}");
            return Ok(new SuccessResponseDto { Id = id.ToString(), StatusCode = 201, Message = "Email Deleted Successfully", Description = "Email Deleted Successfully" });
        }
        
        /// <summary>
        /// Get Email CC Address
        /// </summary>
        [HttpGet]
        [Route("api/v1/email-cc")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GET_EMAIL_CC")]
        [SwaggerOperation("GetCCEmail")]
        [SwaggerResponse(statusCode: 200, type: typeof(EmailCCDetailDto), description: "Fetches the List of Email Details")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorResponseDto), description: "Email Not Found")]
        public IActionResult GetCCEmail([FromQuery][Required] string entityType)
        {
            _logger.LogDebug($"Fetching CC Email Details");
            List<EmailCCDetailDto> result = _emailService.GetCCEmail(entityType);
            _logger.LogInfo($"CC Email Details Fetched");
            return Ok(result);
        }
    }
}

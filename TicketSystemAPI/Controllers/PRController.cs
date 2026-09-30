using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Contracts.IServices;
using Dto;
using Contracts;
using TicketSystemAPI.Attributes;
using Contracts.IRepository;

namespace TicketSystemAPI.Controllers
{
    /// <summary>
    /// Controller for PR assets.
    /// </summary>
    [ApiController]
    public class PRController : BaseController
    {
        private readonly ILoggerManager _logger;
        private readonly IPRService _prService;


        /// <summary>
        /// Constructor for PR Controller.
        /// </summary>
        /// <param name="_metadataService">The asset service.</param>
        /// <param name="logger">The logger manager.</param>
        public PRController(ILoggerManager logger, IPRService prService)
        {
            _logger = logger;
            _prService = prService;
        }
        [HttpPut]
        [Route("api/v1/PR")]
        [ValidateModelState]
        [ApiAuthorization(Name = "UPDATE_PR")]
        [SwaggerOperation("UpdatePR")]
        [SwaggerResponse(statusCode: 201, type: typeof(ServicePRDto), description: "PR updated successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]

        public async Task<IActionResult> UpdatePR([FromBody] PRUpdateDto body)
        {
            _logger.LogDebug("Updating PR details to Ariba");

            Guid domainId = getUserDomainId();
            Guid userId = getLoggedUser();
            
            var result = await _prService.UpdatePRDetailInAriba(body, domainId, userId);

            _logger.LogDebug($"SuccessFully Updated PR Details");
            return Ok(new
            {
                StatusCode = 200,
                Message = "PR updated successfully",
                Data = result
            });
        }


    }
}
using Microsoft.AspNetCore.Mvc;
using Contracts.IServices;
using Contracts;
using System.ComponentModel.DataAnnotations;
using Dto;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;
using TicketSystemAPI.Attributes;

namespace TicketSystemAPI.Controllers
{
    [ApiController]
   
    public class LinkController : BaseController
    {
        private readonly ILinkService _linkService;
        private readonly ILoggerManager _logger;

        public LinkController(ILoggerManager logger, ILinkService linkService)
        {
            _logger = logger;
            _linkService = linkService;
        }

        /// <summary>
        /// Get link by type
        /// </summary>
        /// <param name="type">Link type (e.g., Document, Video)</param>
        /// <returns>Link details</returns>
        [HttpGet]
        [Route("api/v1/link")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GET_LINK_BY_TYPE")]
        [SwaggerOperation("GetLinkByType")]
        [SwaggerResponse(statusCode: 200, type: typeof(LinkDto), description: "Link fetched successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorResponseDto), description: "Link not found for the given type")]
        public IActionResult GetLinkByType([Required][FromQuery] string type)
        {
             _logger.LogDebug("Start: Fetching Link.");
            LinkDto link = _linkService.GetLinkByTypeAsync(type).GetAwaiter().GetResult();
            _logger.LogDebug("End: Fetched Link.");
            return Ok(link);
        }
    }
}

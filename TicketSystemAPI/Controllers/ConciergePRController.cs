using System.ComponentModel.DataAnnotations;
using Contracts;
using Contracts.IRepository;
using Contracts.IServices;
using Dto;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TicketSystemAPI.Attributes;

namespace TicketSystemAPI.Controllers
{
    /// <summary>
    /// Controller for managing Concierge PR Creation.
    /// </summary>
    [ApiController]
    public class ConciergePRController : BaseController
    {
        private readonly ILoggerManager _logger;
        private readonly IRepositoryWrapper _repoWrapper;
        private readonly IConciergePRCreationService _conciergeService;

        public ConciergePRController(ILoggerManager logger, IRepositoryWrapper repoWrapper, IConciergePRCreationService conciergeService)
        {
            _logger = logger;
            _repoWrapper = repoWrapper;
            _conciergeService = conciergeService;
        }


        /// <summary>
        /// Creates a new Concierge PR.
        /// </summary>
        [HttpPost]
        [Route("api/v1/concierge-pr")]
        [ValidateModelState]
        [ApiAuthorization(Name = "CREATE_CONCIERGE_PR")]
        [SwaggerOperation("CreateConciergePR")]
        [SwaggerResponse(statusCode: 201, type: typeof(MaterialPRDto), description: "Concierge Materia PR  created successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> CreateConciergePR([FromBody] ConciergePRRequest request, [FromQuery] DateOnly? date)
        {
            _logger.LogDebug("Posting Concierge PR to Ariba");

            Guid domainId = getUserDomainId();
            Guid userId = getLoggedUser();

            var profileDto = _repoWrapper.Person.GetMyProfileByUserId(userId);
            var PRRequests = request.PRRequests;
            var result = await _conciergeService.PostConciergeMaterialPRToAriba(PRRequests, domainId, profileDto, userId, request.Asset, request.IsUpdate);

            _logger.LogDebug($"Posted PO Details SuccessFully");
            return Ok(new
            {
                StatusCode = 200,
                Message = request.IsUpdate ? "Concierge Material PR updated successfully" : "Concierge Material PR created successfully",
                Data = result
            });
        }

        [HttpGet]
        [Route("api/v1/concierge-venue-details")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GET_CONCIERGE_VENUE_NAME_DETAILS")]
        [SwaggerOperation("GET CONCIERGE VENUE NAME DETAILS")]
        [SwaggerResponse(statusCode: 200, type: typeof(SuccessResponseDto), description: "Concierge venue details retrieved successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> GetConciergeVenueDetails([FromQuery][Required] string venueName)
        {
            _logger.LogDebug($"Retrieving Concierge Venue Details with search term: {venueName}");
            Guid domainId = getUserDomainId();

            var result = await _conciergeService.GetConciergeVenueDetailsByNameAsync(domainId, venueName);
            _logger.LogDebug($"Retrieved Concierge Venue Details with search term: {venueName}");

            return Ok(result);
        }

        [HttpGet]
        [Route("api/v1/concierge-venue-name")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GET_CONCIERGE_VENUE_NAME")]
        [SwaggerOperation("GET CONCIERGE VENUE NAME")]
        [SwaggerResponse(statusCode: 200, type: typeof(SuccessResponseDto), description: "Concierge venue names retrieved successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> GetConciergeVenueName([FromQuery] string? searchTerm, [FromQuery] int index = 0, [FromQuery] int limit = 10)
        {
            _logger.LogDebug($"Retrieving Concierge Venue Names with search term: {searchTerm}");
            Guid domainId = getUserDomainId();

            var result = await _conciergeService.GetConciergeVenueNamesAsync(domainId, searchTerm, index, limit);
            _logger.LogDebug($"Retrieved Concierge Venue Names with search term: {searchTerm}");

            return Ok(result);
        }
    }
}
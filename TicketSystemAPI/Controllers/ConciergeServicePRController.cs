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
    public class ConciergeServicePRController : BaseController
    {
        private readonly ILoggerManager _logger;
        private readonly IRepositoryWrapper _repoWrapper;
        private readonly IConciergeServicePRService _conciergeService;

        public ConciergeServicePRController(ILoggerManager logger, IRepositoryWrapper repoWrapper, IConciergeServicePRService conciergeService)
        {
            _logger = logger;
            _repoWrapper = repoWrapper;
            _conciergeService = conciergeService;
        }


        /// <summary>
        /// Creates a new Concierge PR.
        /// </summary>
        [HttpPost]
        [Route("api/v1/concierge-service-pr")]
        [ValidateModelState]
        [ApiAuthorization(Name = "CREATE_CONCIERGE_SERVICE_PR")]
        [SwaggerOperation("CreateConciergeServicePR")]
        [SwaggerResponse(statusCode: 201, type: typeof(MaterialPRDto), description: "Concierge Service PR created successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> CreateConciergeServicePR([FromBody] ServicePRFormRequest request, [FromQuery] DateOnly? date)
        {
            _logger.LogDebug("Posting Concierge PR to Ariba");

            Guid domainId = getUserDomainId();
            Guid userId = getLoggedUser();

            var profileDto = _repoWrapper.Person.GetMyProfileByUserId(userId);
            var PRRequests = request.PRRequests;
            var result = await _conciergeService.PostConciergeServicePRToAriba(PRRequests, domainId, profileDto, userId, request.Asset, request.IsUpdate);

            _logger.LogDebug($"Posted PO Details SuccessFully");
            return Ok(new
            {
                StatusCode = 200,
                Message = request.IsUpdate ? "Concierge Material PR updated successfully" : "Concierge Material PR created successfully",
                Data = result
            });
        }
    }
}
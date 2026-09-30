using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Contracts;
using Contracts.IServices;
using Contracts.IRepository;
using Dto;
using TicketSystemAPI.Attributes;

namespace TicketSystemAPI.Controllers
{
    /// <summary>
    /// Controller for managing Draft Service PRs.
    /// </summary>
    [ApiController]
    public class DraftServicePRController : BaseController
    {
        private readonly ILoggerManager _logger;
        private readonly IDraftServicePrService _draftServicePRService;
        private readonly IRepositoryWrapper _repoWrapper;

        /// <summary>
        /// Constructor for Draft Service PR Controller.
        /// </summary>
        public DraftServicePRController(
            ILoggerManager logger,
            IDraftServicePrService draftServicePrService,
            IRepositoryWrapper repositoryWrapper)
        {
            _logger = logger;
            _draftServicePRService = draftServicePrService;
            _repoWrapper = repositoryWrapper;
        }

        // =========================================================
        // CREATE DRAFT SERVICE PR
        // =========================================================
        /// <summary>
        /// Create a new Draft Service PR without calling SOAP API
        /// </summary>
        [HttpPost]
        [Route("api/v1/DraftServicePR")]
        [ValidateModelState]
        [ApiAuthorization(Name = "CREATE_DRAFT_SERVICE_PR")]
        [SwaggerOperation("CreateDraftServicePR")]
        [SwaggerResponse(statusCode: 201, type: typeof(List<DraftServicePRResultDto>), description: "Draft Service PR created successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized")]
        public async Task<IActionResult> PostDraftServicePR([FromBody] DraftServicePRFormRequest request)
        {
            _logger.LogDebug("Creating Draft Service PR");

            Guid domainId = getUserDomainId();
            Guid userId = getLoggedUser();

            var result = await _draftServicePRService.CreateDraftServicePRAsync(
                request.PRRequests,
                domainId,
                userId,
                request.Asset);

            _logger.LogDebug("Draft Service PR created successfully");

            return StatusCode(201, new
            {
                StatusCode = 201,
                Message = "Draft Service PR created successfully",
                Data = result
            });
        }

        // =========================================================
        // GET ALL DRAFT SERVICE PRs
        // =========================================================
        [HttpPost]
        [Route("api/v1/GetAllDraftServicePR")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GET_ALL_DRAFT_SERVICE_PR")]
        [SwaggerOperation("GET_ALL_DRAFT_SERVICE_PR")]
        [SwaggerResponse(statusCode: 200, type: typeof(List<ServicePRDto>), description: " Service PR details retrieved successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized- JWT token is missing or invalid.")]
        public async Task<IActionResult> GetAllDraftServicePRDetails([FromBody] MaterialPRFilterRequestDto filterRequest = null)
        {
            _logger.LogDebug("Retrieving Draft Service PR details with filters");

            Guid domainId = getUserDomainId();
            Guid userId = getLoggedUser();

            var result = await _draftServicePRService
                .GetDraftServicePRsGroupedAsync(userId, domainId, filterRequest);

            return Ok(result);
        }
    }
}
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
    /// Controller for managing Draft Material PRs.
    /// </summary>
    [ApiController]
    public class DraftMaterialPRController : BaseController
    {
        private readonly ILoggerManager _logger;
        private readonly IDraftMaterialPRService _draftMaterialPRService;
        private readonly IRepositoryWrapper _repoWrapper;

        /// <summary>
        /// Constructor for Draft Material PR Controller.
        /// </summary>
        public DraftMaterialPRController(
            ILoggerManager logger,
            IDraftMaterialPRService draftMaterialPRService,
            IRepositoryWrapper repositoryWrapper)
        {
            _logger = logger;
            _draftMaterialPRService = draftMaterialPRService;
            _repoWrapper = repositoryWrapper;
        }

        /// <summary>
        /// Create a new Draft Material PR without calling SOAP API
        /// </summary>
        [HttpPost]
        [Route("api/v1/DraftMaterialPR")]
        [ValidateModelState]
        [ApiAuthorization(Name = "CREATE_DRAFT_MATERIAL_PR")]
        [SwaggerOperation("CreateDraftMaterialPR")]
        [SwaggerResponse(statusCode: 201, type: typeof(List<DraftMaterialPRResultDto>), description: "Draft Material PR created successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> PostDraftMaterialPR([FromBody] DraftMaterialPRFormRequest request)
        {
            _logger.LogDebug("Creating Draft Material PR");

            Guid domainId = getUserDomainId();
            Guid userId = getLoggedUser();

            var result = await _draftMaterialPRService.CreateDraftMaterialPRAsync(
                request.PRRequests,
                domainId,
                userId,
                request.Asset);

            _logger.LogDebug($"Draft Material PR created successfully");

            return StatusCode(201, new
            {
                StatusCode = 201,
                Message = "Draft Material PR created successfully",
                Data = result
            });
        }
      
            
   
      [HttpPost]
        [Route("api/v1/GetAllDraftMaterialPR")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GETALLDRAFTMATERIALPR")]
        [SwaggerOperation("GET_ALL_DRAFT_MATERIAL_PR")]
        [SwaggerResponse(statusCode: 200, type: typeof(List<MaterialPRDto>), description: "Material PR details retrieved successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> GetAllDraftMaterialPRDetails([FromBody] MaterialPRFilterRequestDto filterRequest = null)
        {
            _logger.LogDebug($"Retrieving Material PR details with filters");
            Guid domainId = getUserDomainId();
            Guid userId = getLoggedUser();

            var result = await _draftMaterialPRService.GetDraftMaterialPRsGroupedAsync(userId, domainId, filterRequest);

            return Ok(result);
        }
            

    }
}

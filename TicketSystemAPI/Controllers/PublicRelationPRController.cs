using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Contracts.IServices;
using Dto;
using Contracts;
using TicketSystemAPI.Attributes;
using TicketSystemAPI.Controllers;
using Contracts.IRepository;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TicketSystemAPI.Controllers
{
    /// <summary>
    /// Controller for managing assets.
    /// </summary>
    [ApiController]
    public class DigitalQuickPRController : BaseController
    {
        private readonly ILoggerManager _logger;
        private readonly IPublicRelationService _publicRelationService;
        private readonly IRepositoryWrapper _repoWrapper;


        /// <summary>
        /// Constructor for Metadata Controller.
        /// </summary>
        /// <param name="_metadataService">The asset service.</param>
        /// <param name="logger">The logger manager.</param>
        public DigitalQuickPRController(ILoggerManager logger, IPublicRelationService publicRelationService, IRepositoryWrapper repositoryWrapper)
        {
            _logger = logger;
            _publicRelationService = publicRelationService;
            _repoWrapper = repositoryWrapper;
        }
        [HttpPost]
        [Route("api/v2/DigitalQuickPR")]
        [ValidateModelState]
        [ApiAuthorization(Name = "CREATE_DIGITAL_QUICK_PR")]
        [SwaggerOperation("DIGITAL_QUICK_PR")]
        [SwaggerResponse(statusCode: 201, type: typeof(MaterialPRDto), description: "Digital Quick PR created successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> PostDigitalQuickPR([FromBody] MaterialPRFormRequest request, [FromQuery] DateOnly? date)
        {
            _logger.LogDebug("Posting Digital Quick PR details to Ariba");

            Guid domainId = getUserDomainId();
            Guid userId = getLoggedUser();

            var profileDto = _repoWrapper.Person.GetMyProfileByUserId(userId);
            var PRRequests = request.PRRequests;
           var result= await _publicRelationService.PostDigitalQuickPRToAriba(PRRequests,domainId,profileDto,userId, request.Asset,request.IsUpdate );

            _logger.LogDebug($"Posted PO Details SuccessFully");
           return Ok(new
            {
                StatusCode = 200,
                Message = request.IsUpdate ? "Material PR updated successfully" : "Material PR created successfully",
                Data = result   
            });
        }

           [HttpGet]
        [Route("api/v1/GetPropertyCrossChargeName")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GET_PROPERTY_CROSS_CHARGE_NAME")]
        [SwaggerOperation("GET PROPERTY CROSS CHARGE NAME")]
        [SwaggerResponse(statusCode: 200, type: typeof(SuccessResponseDto), description: "Material details retrieved successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> GetPropertyCrossChargeName([FromQuery] string? searchTerm, [FromQuery] int index = 0, [FromQuery] int limit = 10, [FromQuery] string? companyCode=null)
        {
            _logger.LogDebug($"Retrieving Supplier Names with search term: {searchTerm}");
            Guid domainId = getUserDomainId();

            var result = await _publicRelationService.GetPropertyCrossChargeForDropdownAsync(domainId, companyCode, searchTerm, index, limit);

            return Ok(result);
        }

        [HttpGet]
        [Route("api/v2/GetCrossChargeDetailsByName")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GET_CROSS_CHARGE_DETAILS_BY_NAME")]
        [SwaggerOperation("GET CROSS CHARGE DETAILS BY NAME")]
        [SwaggerResponse(statusCode: 200, type: typeof(SuccessResponseDto), description: "Material details retrieved successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> GetCrossChargeDetailsByName([FromQuery][Required] string venueName, [FromQuery] string? companyCode=null)
        {
            _logger.LogDebug($"Retrieving Cross Charge Details with name: {venueName}");
            Guid domainId = getUserDomainId();

            var result = await _publicRelationService.GetCrossChargeDetailsByNameAsync(domainId, venueName, companyCode);

            return Ok(result);
        }

        [HttpGet]
        [Route("api/v1/GetPublicVenueDetails")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GET_PUBLIC_VENUE_NAME_DETAILS")]
        [SwaggerOperation("GET PUBLIC VENUE NAME DETAILS")]
        [SwaggerResponse(statusCode: 200, type: typeof(SuccessResponseDto), description: "Material details retrieved successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> GetPublicVenueDetails([FromQuery][Required] string venueName)
        {
            _logger.LogDebug($"Retrieving Supplier Names with search term: {venueName}");
            Guid domainId = getUserDomainId();

            var result = await _publicRelationService.GetPublicVenueDetailsByNameAsync(domainId, venueName);

            return Ok(result);
        }

        [HttpGet]
        [Route("api/v1/GetPublicVenueName")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GET_PUBLIC_VENUE_NAME")]
        [SwaggerOperation("GET PUBLIC VENUE NAME")]
        [SwaggerResponse(statusCode: 200, type: typeof(SuccessResponseDto), description: "Material details retrieved successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> GetPublicVenueName([FromQuery] string? searchTerm, [FromQuery] int index = 0, [FromQuery] int limit = 10)
        {
            _logger.LogDebug($"Retrieving Supplier Names with search term: {searchTerm}");
            Guid domainId = getUserDomainId();

            var result = await _publicRelationService.GetVenueForDropdownAsync(domainId, searchTerm, index, limit);

            return Ok(result);
        }
        [HttpGet]
        [Route("api/v1/GetPublicCategoryName")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GET_PUBLIC_CATEGORY_NAME")]
        [SwaggerOperation("GET PUBLIC CATEGORY NAME")]
        [SwaggerResponse(statusCode: 200, type: typeof(SuccessResponseDto), description: "Material details retrieved successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> GetPublicCategoryName([FromQuery] string? searchTerm, [FromQuery] int index = 0, [FromQuery] int limit = 10, [FromQuery] string? companyCode=null)
        {
            _logger.LogDebug($"Retrieving Supplier Names with search term: {searchTerm}");
            Guid domainId = getUserDomainId();

            var result = await _publicRelationService.GetPublicCategoryForDropdownAsync(domainId, searchTerm, index, limit);

            return Ok(result);
        }
    }
}
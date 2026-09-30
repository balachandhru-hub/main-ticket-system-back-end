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
    /// Controller for managing PR Creation.
    /// </summary>
    [ApiController]
    public class LegalDepartmentMaterialController : BaseController
    {
        private readonly ILoggerManager _logger;
        private readonly ILegalDepartmentMaterialService _legalDepartmentMaterialService;
        private readonly IRepositoryWrapper _repoWrapper;


        /// <summary>
        /// Constructor for Metadata Controller.
        /// </summary>
        /// <param name="_metadataService">The asset service.</param>
        /// <param name="logger">The logger manager.</param>
        public LegalDepartmentMaterialController(ILoggerManager logger, ILegalDepartmentMaterialService legalDepartmentMaterialService, IRepositoryWrapper repositoryWrapper)
        {
            _logger = logger;
            _legalDepartmentMaterialService = legalDepartmentMaterialService;
            _repoWrapper = repositoryWrapper;
        }
        [HttpPost]
        [Route("api/v1/LegalDepartmentMaterialPR")]
        [ValidateModelState]
        [ApiAuthorization(Name = "CREATE_LEGAL_MATERIAL_PR")]
        [SwaggerOperation("LEGALMATERIALPR")]
        [SwaggerResponse(statusCode: 201, type: typeof(MaterialPRDto), description: "Materia PR  created successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> PostMaterialPR([FromBody] MaterialPRFormRequest request, [FromQuery] DateOnly? date)
        {
            _logger.LogDebug("Posting Material PR details to Ariba");

            Guid domainId = getUserDomainId();
            Guid userId = getLoggedUser();

            var profileDto = _repoWrapper.Person.GetMyProfileByUserId(userId);
            var PRRequests = request.PRRequests;
            var result = await _legalDepartmentMaterialService.PostMaterialPRToAriba(PRRequests, domainId, profileDto, userId, request.Asset, request.IsUpdate);

            _logger.LogDebug($"Posted PO Details SuccessFully");
            return Ok(new
            {
                StatusCode = 200,
                Message = request.IsUpdate ? "Material PR updated successfully" : "Material PR created successfully",
                Data = result
            });
        }

        [HttpGet]
        [Route("api/v1/GetAllVenueName")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GET_LEGAL_VENUE_NAME")]
        [SwaggerOperation("GET LEGAL VENUE NAME")]
        [SwaggerResponse(statusCode: 200, type: typeof(SuccessResponseDto), description: "Material details retrieved successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> GetVenueName([FromQuery] string? searchTerm, [FromQuery] int index = 0, [FromQuery] int limit = 10)
        {
            _logger.LogDebug($"Retrieving Supplier Names with search term: {searchTerm}");
            Guid domainId = getUserDomainId();

            var result = await _legalDepartmentMaterialService.GetVenueForDropdownAsync(domainId, searchTerm, index, limit);

            return Ok(result);
        }

         [HttpGet]
        [Route("api/v1/GetLegalVenueDetails")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GET_LEGAL_VENUE_NAME_DETAILS")]
        [SwaggerOperation("GET LEGAL VENUE NAME DETAILS")]
        [SwaggerResponse(statusCode: 200, type: typeof(SuccessResponseDto), description: "Material details retrieved successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> GetVenueDetails([FromQuery][Required] string venueName)
        {
            _logger.LogDebug($"Retrieving Supplier Names with search term: {venueName}");
            Guid domainId = getUserDomainId();

            var result = await _legalDepartmentMaterialService.GetVenueDetailsByNameAsync(domainId, venueName);

            return Ok(result);
        }

         [HttpGet]
        [Route("api/v1/GetAllLegalMaterialCodes")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GET_LEGAL_MATERIAL_CODES")]
        [SwaggerOperation("GET LEGAL MATERIAL CODES")]
        [SwaggerResponse(statusCode: 200, type: typeof(SuccessResponseDto), description: "Material details retrieved successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> GetLegalMaterialCodes([FromQuery] string? searchTerm, [FromQuery] int index = 0, [FromQuery] int limit = 10)
        {
            _logger.LogDebug($"Retrieving Supplier Names with search term: {searchTerm}");
            Guid domainId = getUserDomainId();

            var result = await _legalDepartmentMaterialService.GetLegalMaterialCodesForDropdownAsync(domainId, searchTerm, index, limit);

            return Ok(result);
        }
          [HttpGet]
        [Route("api/v1/GetAllDetailsLegalMaterialCodes")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GET_ALL_DETAILS_MATERIALCODE")]
        [SwaggerOperation("GETALLDETAILSMATERIALCODE")]
        [SwaggerResponse(statusCode: 200, type: typeof(SuccessResponseDto), description: "Material details retrieved successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> GetdetailsLegalMaterialCodes([FromQuery] string? materialcode)
        {
            _logger.LogDebug($"Retrieving  materialcode");
            Guid domainId = getUserDomainId();

            var result = await _legalDepartmentMaterialService.GetMaterialCodeDetailsByNameAsync(domainId,materialcode);

            return Ok(result);
        }



    }

}

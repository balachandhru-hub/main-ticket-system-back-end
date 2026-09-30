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
    public class MaterialPRController : BaseController
    {
        private readonly ILoggerManager _logger;
        private readonly IAribaMaterialPRService _aribaMaterialPRService;
        private readonly IRepositoryWrapper _repoWrapper;


        /// <summary>
        /// Constructor for Metadata Controller.
        /// </summary>
        /// <param name="_metadataService">The asset service.</param>
        /// <param name="logger">The logger manager.</param>
        public MaterialPRController(ILoggerManager logger, IAribaMaterialPRService aribaMaterialPRService, IRepositoryWrapper repositoryWrapper)
        {
            _logger = logger;
            _aribaMaterialPRService = aribaMaterialPRService;
            _repoWrapper = repositoryWrapper;
        }
        [HttpPost]
        [Route("api/v1/MaterialPR")]
        [ValidateModelState]
        [ApiAuthorization(Name = "CREATEMATERIALPR")]
        [SwaggerOperation("MATERIALPR")]
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
           var result= await _aribaMaterialPRService.PostMaterialPRToAriba(PRRequests,domainId,profileDto,userId, request.Asset,request.IsUpdate );

            _logger.LogDebug($"Posted PO Details SuccessFully");
           return Ok(new
{
    StatusCode = 200,
    Message = request.IsUpdate ? "Material PR updated successfully" : "Material PR created successfully",
    Data = result   
});
        }

        [HttpPut]
        [Route("api/v1/MaterialPR/{prNumber}")]
        [ValidateModelState]
        [ApiAuthorization(Name = "UPDATEMATERIALPR")]
        [SwaggerOperation("UPDATE MATERIAL PR")]
        [SwaggerResponse(statusCode: 200, type: typeof(SuccessResponseDto), description: "Material PR updated successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> UpdateMaterialPR(string prNumber, [FromBody] MaterialPRUpdateDto dto)
        {
            _logger.LogDebug($"Updating Material PR: {prNumber}");

            Guid domainId = getUserDomainId();

            await _aribaMaterialPRService.UpdateMaterialPR(prNumber, dto, domainId);

            return Ok(new SuccessResponseDto
            {
                StatusCode = 200,
                Message = "Material PR updated successfully",
                Description = $"Material PR {prNumber} updated."
            });
        }
        //Summary:
        //Get Material PR details by PR number
        //Summary:
        [HttpPost]
        [Route("api/v1/GetMaterialPR")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GETMATERIALPR")]
        [SwaggerOperation("GET MATERIAL PR")]
        [SwaggerResponse(statusCode: 200, type: typeof(List<MaterialPRDto>), description: "Material PR details retrieved successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> GetMaterialPRDetails([FromBody] MaterialPRFilterRequestDto filterRequest = null)
        {
            _logger.LogDebug($"Retrieving Material PR details with filters");
            Guid domainId = getUserDomainId();
            Guid userId = getLoggedUser();

            var result = await _aribaMaterialPRService.GetMaterialPRs(userId, domainId, filterRequest);

            return Ok(result);
        }
        [HttpGet]
        [Route("api/v1/GetMaterialCodes")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GETMATERIALCODES")]
        [SwaggerOperation("GET MATERIAL CODES")]
        [SwaggerResponse(statusCode: 200, type: typeof(List<string>), description: "Material codes retrieved successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> GetMaterialCodes([FromQuery] string? searchTerm, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            _logger.LogDebug($"Retrieving Material Codes with search term: {searchTerm}");
            Guid domainId = getUserDomainId();

            var result = await _aribaMaterialPRService.GetMaterialCodesForDropdown(searchTerm, page, pageSize);

            return Ok(result);
        }
        [HttpGet]
        [Route("api/v1/GetMaterialDetails/{materialCode}")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GETMATERIALDETAILS")]
        [SwaggerOperation("GET MATERIAL DETAILS")]
        [SwaggerResponse(statusCode: 200, type: typeof(MaterialDetailsDto), description: "Material details retrieved successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> GetMaterialDetails([FromRoute] string materialCode)
        {
            _logger.LogDebug($"Retrieving Material Details for material code: {materialCode}");
            Guid domainId = getUserDomainId();

            var result = await _aribaMaterialPRService.GetMaterialDetailsAsync(materialCode);



            return Ok(result);
        }
        [HttpGet]
        [Route("api/v1/GetCommodityDetails/{CommodityCode}")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GETCOMMODITYDETAILS")]
        [SwaggerOperation("GET COMMODITY DETAILS")]
        [SwaggerResponse(statusCode: 200, type: typeof(MaterialDetailsDto), description: "Material details retrieved successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> GetCommodityDetails([FromRoute] string CommodityCode)
        {
            _logger.LogDebug($"Retrieving Material Details for material code: {CommodityCode}");
            Guid domainId = getUserDomainId();

            var result = await _aribaMaterialPRService.GetCommodityDetailsAsync(CommodityCode);



            return Ok(result);
        }

        [HttpGet]
        [Route("api/v1/GET_PR_SMART_DEFAULTS")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GET_PR_SMART_DEFAULTS")]
        [SwaggerOperation("GET_PR_SMART_DEFAULTS")]
        [SwaggerResponse(statusCode: 200, type: typeof(PRSmartDefaultsDto), description: "PR Smart Defaults retrieved successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> GetPRSmartDefaults()
        {
            Guid userId = getLoggedUser();
            var profile = _repoWrapper.Person.GetMyProfileByUserId(userId);
            _logger.LogDebug($"Retrieving PR Smart Defaults for Ariba user name: {profile.AribaUserName}");
            var result = await _aribaMaterialPRService.GetPRSmartDefaultsAsync(profile.AribaUserName);
            return Ok(result);
        }


        [HttpGet]
        [Route("api/v1/GETALLPRMaterialPR")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GET_ALL_MATERIAL_PR")]
        [SwaggerOperation("GET_ALL_PR_MATERIAL_PR")]
        [SwaggerResponse(statusCode: 200, type: typeof(SuccessResponseDto), description: "Material PRs retrieved successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> GetPRNumbers([FromQuery][Required] int index, [FromQuery][Required] int limit = 50)
        {
            _logger.LogDebug($"Retrieving all PR Numbers with pagination: Index={index}, Limit={limit}");
            Guid domainId = getUserDomainId();

            var prNumbers = await _aribaMaterialPRService.GetPRNumbersAsync(domainId, index, limit);
            return Ok(prNumbers);
        }
        [HttpGet]
        [Route("api/v1/GetSupplierName")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GET_SUPPLIER_NAME")]
        [SwaggerOperation("GET SUPPLIER NAME")]
        [SwaggerResponse(statusCode: 200, type: typeof(MaterialDetailsDto), description: "Material details retrieved successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> GetSupplierName([FromQuery] string? searchTerm, [FromQuery] int index = 0, [FromQuery] int limit = 10)
        {
            _logger.LogDebug($"Retrieving Supplier Names with search term: {searchTerm}");
            Guid domainId = getUserDomainId();

            var result = await _aribaMaterialPRService.GetSuppliersForDropdownAsync(domainId, searchTerm, index, limit);

            return Ok(result);
        }


        [HttpGet]
        [Route("api/v1/GetVenueName")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GET_VENUE_NAME")]
        [SwaggerOperation("GET VENUE NAME")]
        [SwaggerResponse(statusCode: 200, type: typeof(SuccessResponseDto), description: "Material details retrieved successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> GetVenueName([FromQuery] string? searchTerm, [FromQuery] int index = 0, [FromQuery] int limit = 10)
        {
            _logger.LogDebug($"Retrieving Supplier Names with search term: {searchTerm}");
            Guid domainId = getUserDomainId();

            var result = await _aribaMaterialPRService.GetVenueForDropdownAsync(domainId, searchTerm, index, limit);

            return Ok(result);
        }
        [HttpGet]
        [Route("api/v1/GetVenueDetails")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GET_VENUE_NAME_DETAILS")]
        [SwaggerOperation("GET VENUE NAME DETAILS")]
        [SwaggerResponse(statusCode: 200, type: typeof(SuccessResponseDto), description: "Material details retrieved successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> GetVenueDetails([FromQuery][Required] string venueName)
        {
            _logger.LogDebug($"Retrieving Supplier Names with search term: {venueName}");
            Guid domainId = getUserDomainId();

            var result = await _aribaMaterialPRService.GetVenueDetailsByNameAsync(domainId, venueName);

            return Ok(result);
        }
          [HttpPost]
        [Route("api/v2/GetAllMaterialPR")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GETALLMATERIALPR")]
        [SwaggerOperation("GET_ALL_MATERIAL_PR")]
        [SwaggerResponse(statusCode: 200, type: typeof(List<MaterialPRDto>), description: "Material PR details retrieved successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> GetAllMaterialPRDetails([FromBody] MaterialPRFilterRequestDto filterRequest = null)
        {
            _logger.LogDebug($"Retrieving Material PR details with filters");
            Guid domainId = getUserDomainId();
            Guid userId = getLoggedUser();

            var result = await _aribaMaterialPRService.GetMaterialPRsGroupedAsync(userId, domainId, filterRequest);

            return Ok(result);
        }



    }
}
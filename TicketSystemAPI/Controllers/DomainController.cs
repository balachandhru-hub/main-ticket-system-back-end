using Microsoft.AspNetCore.Mvc;
using Entities.Models;
using Dto;
using Swashbuckle.AspNetCore.Annotations;
using Contracts.IServices;
using Contracts;
using TicketSystemAPI.Attributes;
using System.ComponentModel.DataAnnotations;

namespace TicketSystemAPI.Controllers
{
    /// <summary>
    /// Controller for managing domains.
    /// </summary>
    [ApiController]
    public class DomainController : BaseController
    {
        private readonly ILoggerManager _logger;
        private readonly IDomainService _domainService;

        public DomainController(ILoggerManager logger, IDomainService domainService)
        {
            _logger = logger;
            _domainService = domainService;
        }

        /// <summary>
        /// Create a new domain
        /// </summary>
        [HttpPost]
        [Route("api/v1/domain")]
        [ValidateModelState]
        [ApiAuthorization(Name = "CREATE_DOMAIN_USER")]
        [SwaggerOperation("CreateDomain")]
        [SwaggerResponse(201, type: typeof(SuccessResponseDto), description: "Domain created successfully")]
        [SwaggerResponse(400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Domain Not Found")]
        public IActionResult CreateDomain([FromBody] DomainDto request)
        {
            _logger.LogDebug("Start: Creating a new domain.");

            DomainDto domain = _domainService.CreateDomain(request);

            _logger.LogDebug($"End: Domain created successfully with ID: {domain.Id}");

            return StatusCode(201, new SuccessResponseDto
            {
                StatusCode = 201,
                Id = domain.Id.ToString(),
                Message = "Domain created successfully",
                Description = "Domain has been created and stored."
            });
        }

        /// <summary>
        /// Get domain by Id
        /// </summary>
        [HttpGet]
        [Route("api/v1/domain/{id}")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GET_DOMAIN_USER")]
        [SwaggerOperation("GetDomainById")]
        [SwaggerResponse(201, type: typeof(SuccessResponseDto), description: "Domain created successfully")]
        [SwaggerResponse(400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Domain Not Found")]
        public IActionResult GetDomainById([FromRoute][Required] Guid id)
        {
            _logger.LogDebug($"Start: Fetching domain with Id: {id}");
            DomainDto domain = _domainService.GetDomainById(id);
            _logger.LogDebug($"End: Fetched domain with Id: {id}");
            if (domain == null)
                return NotFound(new { message = "Domain not found" });
            return Ok(domain);
        }
        /// <summary>
        /// Update Domain
        /// </summary>
        [HttpPut]
        [Route("api/v1/domain/{id}")]
        [ValidateModelState]
        [ApiAuthorization(Name = "UPDATE_DOMIAN")]
        [SwaggerOperation("UpdatePerson")]
        [SwaggerResponse(statusCode: 200, type: typeof(SuccessResponseDto), description: "Domain updated successfully")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorResponseDto), description: "Domain Not Found")]
        public IActionResult UpdateDomain([FromRoute][Required] Guid id, [FromBody] DomainUpdateDto updateDto)
        {
            updateDto.Id = id;
            _domainService.UpdateDomain(updateDto);
            return Ok(new SuccessResponseDto
            {
                StatusCode = 200,
                Message = "Domain updated successfully",
                Description = "Domain Details Updated successfully",
            });
        }
        /// <summary>
        /// Create a plan for a domain
        /// </summary>
        [HttpPost]
        [Route("api/v1/domain/domain-plan")]
        [ValidateModelState]
        [ApiAuthorization(Name = "CREATE_DOMAIN_PLAN")]
        [SwaggerOperation("CreateDomainPlan")]
        [SwaggerResponse(201, type: typeof(SuccessResponseDto), description: "Domain plan created successfully")]
        [SwaggerResponse(400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Domain Not Found")]
        public IActionResult CreateDomainPlan([FromBody] DomainPlanDto request)
        {
            _logger.LogDebug($"Start: Creating plan for domain {request.DomainId}");

            Guid result = _domainService.CreateDomainPlan(request);

            _logger.LogDebug($"End: Domain plan created successfully for domain {request.DomainId}");
            return StatusCode(201, new SuccessResponseDto
            {
                StatusCode = 201,
                Id = result.ToString(),
                Message = "Domainplan created successfully",
                Description = "Domainplan has been created and stored."
            });
        }

        /// <summary>
        /// Create a setings for a domain
        /// </summary>
        [HttpPost]
        [Route("api/v1/domain/domain-setting")]
        [ValidateModelState]
        [ApiAuthorization(Name = "CREATE_DOMAIN_SETTING")]
        [SwaggerOperation("CreateDomainSetting")]
        [SwaggerResponse(201, type: typeof(SuccessResponseDto), description: "Domain plan created successfully")]
        [SwaggerResponse(400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Domain Not Found")]
        public IActionResult CreateDomainSetting([FromBody] DomainSettingPurchaseOrderDto request, [FromQuery][Required] string settingType)
        {

            Guid domainId = getUserDomainId();
            _logger.LogDebug($"Start: Creating Settings for domain {domainId}");

            _domainService.CreateDomainSettings(request, domainId, settingType);

            _logger.LogDebug($"End: Domain plan created successfully for domain {domainId}");
            return StatusCode(201, new SuccessResponseDto
            {
                StatusCode = 201,
                Message = "DomainSettings created successfully",
                Description = "DomainSettings has been created and stored."
            });
        }
        /// <summary>
        /// Get All Domains
        /// </summary>
        [HttpGet]
        [Route("api/v1/domain")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GET_ALL_DOMAIN")]
        [SwaggerOperation("GetDomain")]
        [SwaggerResponse(200, type: typeof(SuccessResponseDto), description: "Fetched All Domain")]
        [SwaggerResponse(400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public IActionResult GetDomain([FromQuery][Required] int index, [FromQuery][Required] int limit, [FromQuery] string? aribaNetworkId, [FromQuery] string? searchTerm)
        {
            Guid userId = getLoggedUser();
            _logger.LogDebug($"Start: Fetching domain for User :{userId}");
            List<DomainPreviewDto> result = _domainService.GetAllDomains(index, limit, aribaNetworkId, searchTerm);
            _logger.LogDebug($"End: Fetched domain for User :{userId}");
            return Ok(result);
        }

        /// <summary>
        ///     Get All Domains
        /// </summary>
        [HttpGet]
        [Route("api/v1/domain/expired-domains")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GET_ALL_EXPIRED_DOMAIN")]
        [SwaggerOperation("GetExpiredDomains")]
        [SwaggerResponse(200, type: typeof(SuccessResponseDto), description: "Fetched All Expired Domains")]
        [SwaggerResponse(400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public IActionResult GetExpiredDomains([FromQuery][Required] int index, [FromQuery][Required] int limit, [FromQuery] string? searchTerm)
        {
            Guid userId = getLoggedUser();
            _logger.LogDebug($"Start: Fetching expired domains for User :{userId}");
            List<DomainPreviewDto> result = _domainService.GetAllExpiredDomain(index, limit, searchTerm);
            _logger.LogDebug($"End: Fetched expired domains for User :{userId}");
            return Ok(result);
        }




        /// <summary>
        /// Get domain details (company, plan, tax) by domain Id
        /// </summary>
        [HttpGet]
        [Route("api/v1/domain/domain-details/{id}")]
        [ApiAuthorization(Name = "GET_DOMAIN_DETAILS")]
        [SwaggerOperation("GetDomainDetails")]
        [SwaggerResponse(200, "Domain details fetched successfully")]
        [SwaggerResponse(404, "Domain not found")]
        public IActionResult GetDomainDetails([FromRoute] Guid id)
        {
            _logger.LogDebug($"Fetching the Domain Details for the Domain : {id}");
            DomainDetailsDto details = _domainService.GetDomainDetails(id);
            _logger.LogDebug($"Fetched the Domain Details for the Domain : {id}");
            return Ok(details);
        }

        /// <summary>
        /// Get domain details (company, plan, tax) by domain Id
        /// </summary>
        [HttpGet]
        [Route("api/v1/domain/domain-logo")]
        [ApiAuthorization(Name = "GET_DOMAAIN_LOGO")]
        [SwaggerOperation("GetDomainLogo")]
        [SwaggerResponse(200, "Domain details fetched successfully")]
        [SwaggerResponse(404, "Domain not found")]
        public IActionResult GetDomainLogo()
        {
            Guid userId = getLoggedUser();
            _logger.LogDebug($"Fetching the Domain Logo for the User : {userId}");
            AssetDownloadDto details = _domainService.GetDomainLogo(userId);
            _logger.LogDebug($"Fetched the Domain Details for the Domain : {userId}");
            return Ok(details);
        }

        [HttpPut]
        [Route("api/v1/domain/domain-status")]
        [ValidateModelState]
        [ApiAuthorization(Name = "UPDATE_DOMAIN_STATUS")]
        [SwaggerOperation("UpdateDomainStatus")]
        [SwaggerResponse(200, type: typeof(SuccessResponseDto), description: "Domain status updated successfully")]
        [SwaggerResponse(400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Domain Not Found")]
        public IActionResult UpdateDomainStatus([FromBody] UpdateDomainStatus request)
        {
            Guid userId = getLoggedUser();
            _logger.LogDebug($"Start: Updating payment status for DomainId: {request.DomainId} by User: {userId}");

            _domainService.UpdateDomainStatus(request.DomainId, request.ActiveStatus);

            _logger.LogDebug($"End: Updated payment status for DomainId: {request.DomainId} by User: {userId}");
            return Ok(new SuccessResponseDto
            {
                Message = $"Domain status updated to {request.ActiveStatus} successfully",
                Description = "Domain status has been updated successfully.",
                StatusCode = 200,
            });
        }   


    }
}       
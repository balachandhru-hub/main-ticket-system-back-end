using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Contracts.IServices;
using Dto;
using Contracts;
using TicketSystemAPI.Attributes;
using TicketSystemAPI.Controllers;

namespace TicketSystemAPI.Controllers
{
    /// <summary>
    /// Controller for managing assets.
    /// </summary>
    [ApiController]
    public class MetadataController : BaseController
    {
        private readonly ILoggerManager _logger;
        private readonly IMetadataService _metadataService;


        /// <summary>
        /// Constructor for Metadata Controller.
        /// </summary>
        /// <param name="_metadataService">The asset service.</param>
        /// <param name="logger">The logger manager.</param>
        public MetadataController(ILoggerManager logger, IMetadataService metadataService)
        {
            _logger = logger;
            _metadataService = metadataService;
        }

        /// <summary>
        /// List of Reference Keys, Id's.
        /// </summary>
        /// <param name="type">List of Reference Types</param>
        /// <response code="200">Successfully Fetched the Reference Types</response>
        /// <response code="400">Invalid file.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpPost("api/v1/metadata/reference-list")]
        [SwaggerOperation("GetReferenceList")]
        [SwaggerResponse(statusCode: 200, "Fetched List of Reference Type", typeof(RefTermDto))]
        [SwaggerResponse(statusCode: 400, "Invalid file.", typeof(ErrorResponseDto))]
        [SwaggerResponse(statusCode: 401, "Unauthorized", typeof(ErrorResponseDto))]
        public IActionResult GetReferenceList([FromBody] List<string> type)
        {
            _logger.LogDebug($"Fetching the List of Reference Type");
            List<RefTermDto> result = _metadataService.GetRefTermListByRefSetKey(type);

            _logger.LogDebug($"Fetched the List of Reference Type");
            return Ok(result);
        }

        /// <summary>
        /// List of Available Roles.
        /// </summary>
        /// <response code="200">Successfully Fetched the List of Roles</response>
        /// <response code="400">Invalid file.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpGet("api/v1/metadata/role-list")]
        [ApiAuthorization(Name = "GET_ROLE_LIST")]
        [SwaggerOperation("GetRoleList")]
        [SwaggerResponse(statusCode: 200, "Fetched List of Roles", typeof(List<RoleDetailDto>))]
        [SwaggerResponse(statusCode: 404, "Not Found", typeof(ErrorResponseDto))]
        [SwaggerResponse(statusCode: 401, "Unauthorized", typeof(ErrorResponseDto))]
        [SwaggerResponse(statusCode: 403, "Forbidden", typeof(ErrorResponseDto))]
        public IActionResult GetRoleList()
        {
            _logger.LogDebug($"Fetching the List of Available Roles");
            List<RoleDetailDto> result = _metadataService.GetRoleList();

            _logger.LogDebug($"Fetched the List of Available Roles");
            return Ok(result);
        }
        /// <summary>
        /// Create a new Reference Term (RefTerm).
        /// </summary>

        [HttpPost("api/v1/metadata/create-refterm")]
        [ApiAuthorization(Name = "CREATE_REFTERM")]
        [SwaggerOperation("CreateRefTerm")]
        [SwaggerResponse(statusCode: 200, "RefTerm Created", typeof(Guid))]
        [SwaggerResponse(statusCode: 400, "Invalid input.", typeof(ErrorResponseDto))]
        [SwaggerResponse(statusCode: 401, "Unauthorized", typeof(ErrorResponseDto))]
        public IActionResult CreateRefTerm([FromBody] CreateRefTermDto dto)
        {
            _logger.LogDebug($"Creating new RefTerm for module: {dto.Description}");
            Guid id = _metadataService.CreateRefTerm(dto);
            _logger.LogDebug($"Created RefTerm with Id: {id}");
            return StatusCode(201, new SuccessResponseDto
            {
                StatusCode = 201,
                Id = id.ToString(),
                Message = "RefTerm created successfully",
                Description = "RefTerm has been created and stored."
            });
        }
        /// <summary>
        /// Retrieves the reference term ID corresponding to the specified reference term key. 
        ///  </summary>
        [HttpGet("api/v1/metadata/module-list")]
        [ApiAuthorization(Name = "GET_MODULE_LIST")]
        [SwaggerOperation("GetModuleList")]
        [SwaggerResponse(statusCode: 200, "Fetched List of Modules", typeof(List<ModulePreviewDto>))]
        [SwaggerResponse(statusCode: 401, "Unauthorized", typeof(ErrorResponseDto))]
        public IActionResult GetModuleList()
        {
            _logger.LogDebug($"Fetching all modules (active and inactive)");
            List<ModulePreviewDto> result = _metadataService.GetAllModules();
            _logger.LogDebug($"Fetched all modules");    
            return Ok(result);
        }
        /// <summary>
        /// Updates the status of a Ariba Model.
        /// </summary>
        [HttpPut("api/v1/metadata/update-model")]
        [ApiAuthorization(Name = "UPDATE_MODULE_STATUS")]
        [SwaggerOperation("UpdateModelStatus")]
        [SwaggerResponse(statusCode: 200, "Ariba Model status updated successfully", typeof(SuccessResponseDto))]
        [SwaggerResponse(statusCode: 404, "Ariba Model not found", typeof(ErrorResponseDto))]
        public IActionResult UpdateModelStatus([FromBody] UpdateModelStatusDto dto)
        {
            _metadataService.UpdateModelStatus(dto);
            return Ok(new SuccessResponseDto
            {
                StatusCode = 200,
                Id = dto.Id.ToString(),
                Message = "Ariba Model status updated successfully",
                Description = $"Ariba Model IsActive set to {dto.IsActive}"
            });
}
    }
}
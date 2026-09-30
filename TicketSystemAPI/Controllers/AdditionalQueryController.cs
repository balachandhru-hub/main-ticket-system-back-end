using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Dto;
using Contracts.IServices;
using Contracts;
using System;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Contracts.IRepository;
using TicketSystemAPI.Attributes;



namespace TicketSystemAPI.Controllers
{
  
    public class AdditionalQueryController : BaseController
    {
        private readonly IAdditionalQueryService _additionalQueryService;
        private readonly ILoggerManager _logger;

        public AdditionalQueryController(IAdditionalQueryService additionalQueryService, ILoggerManager logger)
        {
            _additionalQueryService = additionalQueryService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new additional query
        /// </summary>
        /// <param name="dto">Additional query details</param>
        /// <returns>Query ID if created successfully</returns>
        [HttpPost]
        [Route("api/v1/additional-query")]
        [ValidateModelState]
        [SwaggerOperation("CreateAdditionalQuery")]
        [SwaggerResponse(201, type: typeof(SuccessResponseDto), description: "Additional query created successfully")]
        [SwaggerResponse(400, type: typeof(ErrorResponseDto), description: "Bad request")]
        public async Task<IActionResult> CreateAdditionalQuery([FromBody] AdditionalQueryDto body)
        {
            _logger.LogDebug("Creating a new additional query.");
            Guid result = await _additionalQueryService.CreateAdditionalQueryAsync(body,getLoggedUser());
            return StatusCode(201, new SuccessResponseDto
            {
                StatusCode = 201,
                Id = result.ToString(),
                Message = "Additional query created successfully",
                Description = "Additional query created successfully",
            });
        }

        /// <summary>
        /// Get all additional queries (simple list) with optional filters
        /// </summary>
        /// <param name="filters">Filter parameters (search by organization, status, date range, limit)</param>
        /// <returns>List of additional queries</returns>
        [HttpGet]
        [Route("api/v1/additional-query")]
        [ApiAuthorization(Name = "GET_ALL_ADDITIONAL_QUERIES")]
        [SwaggerOperation("GetAllAdditionalQueries")]
        [SwaggerResponse(statusCode: 200, type: typeof(List<AdditionalQueryListDto>), description: "List of additional queries")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public IActionResult GetAllAdditionalQueries([FromQuery] AdditionalQueryFilterDto filters)
        {
            Guid userId=getLoggedUser();
            _logger.LogDebug("Start: Fetching all additional queries.");

         
            List<AdditionalQueryListDto> additionalQueries = _additionalQueryService.GetAdditionalQueriesAsync(filters ?? new AdditionalQueryFilterDto()).GetAwaiter().GetResult();

            _logger.LogDebug("End: Fetched additional queries.");
            return Ok(additionalQueries);
        }
 

        /// <summary>
        /// Update the status of an additional query
        /// </summary>
        /// <param name="id">Query ID</param>
        /// <param name="body">Status DTO</param>
        /// <returns>Success or error response</returns>
        [HttpPut]
        [Route("api/v1/additional-query/{id}/status")]
        [ApiAuthorization(Name = "UPDATE_ADDITIONAL_QUERY_STATUS")]
        [SwaggerOperation("UpdateAdditionalQueryStatus")]
        [SwaggerResponse(200, type: typeof(SuccessResponseDto), description: "Status updated successfully")]
        [SwaggerResponse(400, type: typeof(ErrorResponseDto), description: "Bad request")]
        public async Task<IActionResult> UpdateAdditionalQueryStatus([FromRoute] Guid id, [FromBody] AdditionalQueryStatusDto body)
        {
            _logger.LogDebug($"Updating status for additional query {id} to {body.Status}");
            await _additionalQueryService.UpdateStatusAsync(id, body.Status, getLoggedUser());
            return Ok(new SuccessResponseDto
            {
                StatusCode = 200,
                Id = id.ToString(),
                Message = "Status updated successfully",
                Description = "Additional query status updated successfully",
            });
        }
    }
}

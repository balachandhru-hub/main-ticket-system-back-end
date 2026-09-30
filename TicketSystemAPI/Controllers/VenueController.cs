using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Contracts;
using Contracts.IServices;
using Dto;
using Entities.Common;
using ExceptionHandler;
using TicketSystemAPI.Attributes;
using Entities.Models;

namespace TicketSystemAPI.Controllers
{


    /// <summary>
    /// Controller for managing Ariba Invoice Uploads
    /// </summary>
    public class VenueController : BaseController
    {
        private readonly IVenueService _venueService;
        private readonly ILoggerManager _logger;

        public VenueController(IVenueService venueService, ILoggerManager logger)
        {
            _venueService = venueService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new venue
        /// </summary>
        [HttpPost]
        [Route("api/v1/ariba/venue")]
        [ApiAuthorization(Name = "CREATE_VENUE")]
        [SwaggerOperation("CreateVenue")]
        [SwaggerResponse(statusCode: 200, type: typeof(SuccessResponseDto), description: "File Upload successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorResponseDto), description: "File Not Found")]
        public async Task<ActionResult> CreateVenueOrSupplier(
    [FromBody] CreateVenueSupplierDto dto)
        {
            Guid domainId = getUserDomainId();
            Guid userId = getLoggedUser();

            _logger.LogInfo($"Creating {dto.Type} for domain: {domainId}");

            await _venueService.CreateVenueSupplierAsync(domainId, dto);

            return Ok(new SuccessResponseDto { Message = "Successfully created Data", Description = "Success", StatusCode = 200 });
        }
        

        /// <summary>
        /// Get all venues for a domain
        /// </summary>
        [HttpGet]
        [Route("api/v1/ariba/GetAllVenues")]
        [ApiAuthorization(Name = "GET_VENUES")]
        [SwaggerOperation("GetAllVenues")]
        [SwaggerResponse(statusCode: 200, type: typeof(SuccessResponseDto), description: "File Upload successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorResponseDto), description: "File Not Found")]
        public async Task<ActionResult> GetVenueSupplier(
    [FromQuery] string type,          // VENUE | SUPPLIER
    [FromQuery] string? searchTerm,
    [FromQuery] int index = 0,
    [FromQuery] int limit = 10)
        {
            Guid domainId = getUserDomainId();

            var request = new GetVenueSupplierRequestDto
            {
                Type = type,
                SearchTerm = searchTerm,
                Index = index,
                Limit = limit
            };

            var result = await _venueService.GetVenueSupplierAsync(domainId, request);

            return Ok(result);
        
        }
        /// <summary>
        /// Update a venue
        /// </summary>
        [HttpPut]
        [Route("api/v1/ariba/UpdateVenueSupplier")]

        [ApiAuthorization(Name = "UPDATE_VENUE")]
        [SwaggerOperation("UpdateVenue")]
        [SwaggerResponse(statusCode: 200, type: typeof(SuccessResponseDto), description: "Venue updated successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorResponseDto), description: "Venue Not Found")]
        public async Task<ActionResult> UpdateVenueSupplier(
    [FromBody] UpdateVenueSupplierDto dto)
        {
            Guid domainId = getUserDomainId();

            var result = await _venueService.UpdateVenueSupplierAsync(domainId, dto);

            return Ok(new SuccessResponseDto { Message = "Successfully Updated Data", Description = "Success", StatusCode = 200 });
        }
        

        /// <summary>
        /// Delete a venue
        /// </summary>
        [HttpDelete]
        [Route("api/v1/ariba/DeleteVenueSupplier")]
        [ApiAuthorization(Name = "DELETE_VENUE")]
        [SwaggerOperation("DeleteVenue")]
        [SwaggerResponse(statusCode: 200, type: typeof(SuccessResponseDto), description: "Venue deleted successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorResponseDto), description: "Venue Not Found")]
        public async Task<ActionResult> DeleteVenueSupplier(
    [FromQuery] string type,        // VENUE | SUPPLIER
    [FromQuery] string identifier)  // VenueName | SupplierId
        {
            Guid domainId = getUserDomainId();

            var result = await _venueService.DeleteVenueSupplierAsync(
                domainId,
                type,
                identifier);

              return Ok(new SuccessResponseDto
           { Message = "Successfully Deleted Data", Description = "Success", StatusCode = 200 , Id=identifier});
        
        }
    }
}

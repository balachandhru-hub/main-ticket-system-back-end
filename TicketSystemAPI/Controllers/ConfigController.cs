using Microsoft.AspNetCore.Mvc;
using Contracts;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using Dto;
using Dto;
using TicketSystemAPI.Attributes;

namespace TicketSystemAPI.Controllers
{

    public class ConfigController : BaseController
    {
        private readonly ILoggerManager _logger;
        private readonly IPlanConfigService _planConfigService;

        public ConfigController(ILoggerManager logger, IPlanConfigService planConfigService)
        {
            _logger = logger;
            _planConfigService = planConfigService;
        }

        [HttpGet]
        [Route("api/v1/config/plan")]
        [ValidateModelState]
        [ApiAuthorization(Name = "PLAN_CONFIG")]
        [SwaggerOperation("GetPlanConfig")]
        [SwaggerResponse(statusCode: 200, type: typeof(PlanConfigDto), description: "Fetches the Plan Config Details for the given plan and engagement type")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorResponseDto), description: "Plan Config Not Found")]
        public IActionResult GetPlanConfig([FromQuery][Required] string plan, [FromQuery][Required] string engagementType)
        {
            Guid userId = getLoggedUser();
            _logger.LogInfo($"Start: Fetching plan config for plan: {plan}, engagementType: {engagementType}, user: {userId}");

            PlanConfigDto config = _planConfigService.GetPlanConfig(plan, engagementType);

            _logger.LogInfo($"End: Fetched plan config for plan: {plan}, engagementType: {engagementType}, user: {userId}");

            return Ok(config);
        }
          /// <summary>
          /// getallConfig
          /// </summary>

        [HttpGet]
        [Route("api/v1/config/plan-details/all")]
        [ValidateModelState]
        [ApiAuthorization(Name = "PLAN_CONFIG_ALL")]
        [SwaggerOperation("GetAllPlans")]
        [SwaggerResponse(statusCode: 200, type: typeof(List<PlanConfigDto>), description: "Fetches all Plan Config Details")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorResponseDto), description: "No Plan Config Found")]
        public IActionResult GetAllPlans()
        {
            var result = _planConfigService.GetAllPlanConfigs();


            if (result == null || !result.Any())
                return NotFound(new ErrorResponseDto { Message = "No Plan Config found" });

            return Ok(result);
        }
            /// <summary>
          /// Update configPlan
          /// </summary>


        [HttpPut]
        [Route("api/v1/config/plan")]
        [ValidateModelState]
        [ApiAuthorization(Name = "PLAN_CONFIG_UPDATE")]
        [SwaggerOperation("UpdatePlanConfig")]
        [SwaggerResponse(statusCode: 200, type: typeof(bool), description: "Updates the Plan Config Details for the given plan and engagement type")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorResponseDto), description: "Plan Config Not Found")]
        public IActionResult UpdatePlanConfig([FromBody] UpdatePlanConfigDto dto)
        {
            Guid userId = getLoggedUser();
            _logger.LogInfo($"Start: Updating plan config with Id: {dto.Id}, user: {userId}");

            bool isUpdated = _planConfigService.UpdatePlanConfig(dto);
            _logger.LogInfo($"End: Updated plan config with Id: {dto.Id}, user: {userId}");
            return Ok(new SuccessResponseDto
    {
        StatusCode = 200,
        Id = dto.Id.ToString(),
        Message = "Plan Config updated successfully",
        Description = "Plan Config fields updated based on user role."
    });
        }



    }
}

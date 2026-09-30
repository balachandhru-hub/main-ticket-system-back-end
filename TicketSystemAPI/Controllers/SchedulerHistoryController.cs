using Contracts.IServices;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Entities.Common;
using Microsoft.AspNetCore.Authorization;
using TicketSystemAPI.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace TicketSystemAPI.Controllers
{
    
    public class SchedulerHistoryController :  BaseController
    {
        private readonly ISchedulerHistoryService _schedulerHistoryService;

        public SchedulerHistoryController(ISchedulerHistoryService schedulerHistoryService)
        {
            _schedulerHistoryService = schedulerHistoryService;
        }
        [HttpGet]
        [Route("api/scheduler-history/last-end-date")]
        [ApiAuthorization(Name = "GET_SCHEDULER_HISTORY")]
        [SwaggerOperation("Get Last Scheduler End Date")]
        [SwaggerResponse(200, type: typeof(Dto.SchedulerHistoryResponseDto), description: "Scheduler history fetched successfully")]
        [SwaggerResponse(400, type: typeof(Dto.ErrorResponseDto), description: "Bad Request - Invalid scheduler key.")]
        [SwaggerResponse(404, type: typeof(Dto.ErrorResponseDto), description: "No scheduler history found for the given key.")]

        public async Task<IActionResult> GetLastSchedulerEndDate([FromQuery] string schedulerKey)
        {
            if (string.IsNullOrWhiteSpace(schedulerKey))
            {
                return BadRequest(new { message = "Scheduler key is required." });
            }

            try
            {
                var result = await _schedulerHistoryService.GetLastSchedulerEndDate(schedulerKey);

                if (result == null)
                    return NotFound(new { message = "No scheduler history found for the given key." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching scheduler history.", error = ex.Message });
            }
        }
    }
}
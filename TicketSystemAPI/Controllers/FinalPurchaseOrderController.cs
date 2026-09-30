using Contracts;
using Contracts.IServices;
using Dto;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TicketSystemAPI.Attributes;

namespace TicketSystemAPI.Controllers
{
    public class FinalPurchaseOrderController : BaseController
    {
        private readonly ILoggerManager _logger;
        private readonly IFinalPurchaseOrderService _finalPurchaseOrderService;

        public FinalPurchaseOrderController(ILoggerManager logger, IFinalPurchaseOrderService finalPurchaseOrderService)
        {
            _logger = logger;
            _finalPurchaseOrderService = finalPurchaseOrderService;
        }

        [HttpPost]
        [Route("api/v1/final-purchase-order/filter-po-numbers")]
        [ApiAuthorization(Name = "GET_FINALPURCHASE_ORDER")]
        [SwaggerOperation("GetPoNumbersByFilters")]
        [SwaggerResponse(200, type: typeof(List<string>), description: "Distinct PO numbers matching filters")]
        [SwaggerResponse(400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "PO Numbers Not Found")]
        public IActionResult GetPoNumbersByFilters([FromBody] PurchaseOrderFilterDto filter, [FromQuery] int index = 0, [FromQuery] int limit =10)
        {
            _logger.LogDebug($"Fetching distinct PO numbers by filters with index: {index}, limit: {limit}");

            Guid userDomainId = getUserDomainId();

            List<string> result = _finalPurchaseOrderService.GetPoNumbersByFilters(userDomainId, filter, index, limit);

            _logger.LogDebug($"Fetched {result.Count} distinct PO numbers");
            return Ok(result);
        }

        [HttpPost]
        [Route("api/v1/final-purchase-order/get-details")]
        [ApiAuthorization(Name = "GET_FINALPURCHASE_ORDER_DETAILS")]
        [SwaggerOperation("GetFinalPurchaseOrdersByPoNumbers")]
        [SwaggerResponse(200, type: typeof(List<FinalPurchaseOrderDto>), description: "FinalPurchaseOrder details for provided PO numbers")]
        [SwaggerResponse(400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: " Details Not Found for provided PO numbers")]
        public IActionResult GetFinalPurchaseOrdersByPoNumbers([FromBody] GetFinalPurchaseOrderRqDto request)
        {
            _logger.LogDebug($"Fetching FinalPurchaseOrder details for PO numbers: {string.Join(",", request.PoNumbers)}");

            Guid userDomainId = getUserDomainId();

            List<FinalPurchaseOrderDto> results = _finalPurchaseOrderService.GetFinalPurchaseOrdersByPoNumbers(userDomainId, request.PoNumbers);

            _logger.LogDebug($"Fetched {results.Count} FinalPurchaseOrder records");
            return Ok(results);
        }
    }
}

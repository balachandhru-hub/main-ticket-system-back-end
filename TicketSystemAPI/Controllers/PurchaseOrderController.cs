using Contracts;
using Contracts.IServices;
using Dto;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using TicketSystemAPI.Attributes;

namespace TicketSystemAPI.Controllers
{
    public class PurchaseOrderController : BaseController
    {
        private readonly ILoggerManager _logger;
        private readonly IPurchaseOrderService _purchaseOrderService;

        public PurchaseOrderController(ILoggerManager logger, IPurchaseOrderService purchaseOrderService)
        {
            _logger = logger;
            _purchaseOrderService = purchaseOrderService;
        }

        /// <summary>
        /// Retrieves the list of available PurchaseOrderIds.
        /// </summary>
        [HttpGet]
        [Route("api/v1/purchase-order/ids")]
        [ApiAuthorization(Name = "GET_PURCHASE_ORDER")]
        [SwaggerOperation("GetConsultantsWithTicketCount")]
        [SwaggerResponse(200, type: typeof(SuccessResponseDto), description: "Fetching the List of Consultants with Ticket Count")]
        [SwaggerResponse(400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Consultants Not Found")]
        public IActionResult GetPurchaseOrderIds(
            [FromQuery][Required] int index,
            [FromQuery][Required] int limit,
            [FromQuery] string? searchTerm = null,
            [FromQuery] PurchaseOrderRqMode? mode = null)
        {
            _logger.LogDebug($"Fetching purchaseOrderIds - Page: {index}, Limit: {limit}, Search: {searchTerm}");

            Guid userDomainId = getUserDomainId();

            List<string> result = _purchaseOrderService.GetPurchaseOrderIds(userDomainId, index, limit, searchTerm, mode);
            _logger.LogDebug($"Fetched {result.Count} purchaseOrders count");
            return Ok(result);
        }

        /// <summary>
        /// Retrieves the Details of the specified purchase order.
        /// </summary>
        [HttpPost]
        [Route("api/v1/purchase-order")]
        [ApiAuthorization(Name = "GET_PURCHASE_ORDER")]
        [SwaggerOperation("GetConsultantsWithTicketCount")]
        [SwaggerResponse(200, type: typeof(SuccessResponseDto), description: "Fetching the List of Consultants with Ticket Count")]
        [SwaggerResponse(400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Consultants Not Found")]
        public IActionResult GetPurchaseOrderById([FromBody] PurchaseOrderRqDto purchaseOrderRqDto)
        {
            _logger.LogDebug($"Fetching purchaseOrderId - {purchaseOrderRqDto.ToString()}");

            Guid userDomainId = getUserDomainId();

            List<PurchaseOrderDto>? result = _purchaseOrderService.GetPurchaseOrderByIds(purchaseOrderRqDto, userDomainId);

            _logger.LogDebug($"Fetched purchaseOrder SuccessFully");
            return Ok(result);
        }

        /// <summary>
        /// Retrieves the Details of the specified purchase order.
        /// </summary>
        [HttpPost]
        [Route("api/v1/reciept")]
        [ApiAuthorization(Name = "GET_RECIEPT_ID")]
        [SwaggerOperation("GetRecieptByPurchaseId")]
        [SwaggerResponse(200, type: typeof(List<string>), description: "Fetching the List of Reciept")]
        [SwaggerResponse(400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Reciept Not Found")]
        public IActionResult GetRecieptByPurchaseId([FromBody] List<string> purchaseId)
        {
            _logger.LogDebug($"Fetching the Reciept Id for the given Purchase Id");

            List<string>? result = _purchaseOrderService.GetRecieptByPurchaseId(purchaseId);

            _logger.LogDebug($"Fetched Reciept SuccessFully");
            return Ok(result);
        }

        /// <summary>
        /// Post the Data rq to Ariba for specific POs.
        /// </summary>
        [HttpPost]
        [Route("api/v1/purchase-order-ariba")]
        [ApiAuthorization(Name = "CREATE_PURCHASE_ORDER_ARIBA")]
        [SwaggerOperation("CreatePurchaseOrderAriba")]
        [SwaggerResponse(200, type: typeof(SuccessResponseDto), description: "Purchase Order Created Successfully")]
        [SwaggerResponse(400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Reciept Not Found")]
        public async Task<IActionResult> CreatePurchaseOrderAriba([FromBody] List<PuchaseOrderPostRqDto> postRequest, [FromQuery] bool isReversible, [FromQuery] DateOnly? date)
        {
            _logger.LogDebug($"Posting the PO details to ariba");

            var domainId = getUserDomainId();

            await _purchaseOrderService.PostPoDetailsToAriba(postRequest, domainId, isReversible, date);

            _logger.LogDebug($"Posted PO Details SuccessFully");
            return Ok(new SuccessResponseDto { StatusCode = 200, Message = "Created the Purchase Order in Ariba", Description = "Created the Purchase Order in Ariba" });
        }

        /// <summary>
        /// Blocks or Unblocks the Purchase Orders.
        /// </summary>
        [HttpPut]
        [Route("api/v1/purchase-order/block-unblock")]
        [ApiAuthorization(Name = "UPDATE_PURCHASE_ORDER")]
        [SwaggerOperation("UpdateBlockOrUnblockPO")]
        [SwaggerResponse(200, type: typeof(SuccessResponseDto), description: "Purchase Order Updated Successfully")]
        [SwaggerResponse(400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Reciept Not Found")]
        public IActionResult UpdateBlockOrUnblockPO([FromBody] List<string> purchaseOrder, [FromQuery] bool isBlocked)
        {
            _logger.LogDebug($"Updating the PO details");

            Guid domainId = getUserDomainId();

            _purchaseOrderService.BlockOrUnblockPO(purchaseOrder, domainId, isBlocked);

            _logger.LogDebug($"Updated PO Details SuccessFully");
            return Ok(new SuccessResponseDto { StatusCode = 200, Message = "Updated the Purchase Order", Description = "Updated the Purchase Order" });
        }

        /// <summary>
        /// Get the Blocked or Unblocked Purchase Order.
        /// </summary>
        [HttpGet]
        [Route("api/v1/purchase-order/block-unblock")]
        [ApiAuthorization(Name = "GET_BLOCKED_UNBLOCKED_PURCHASE_ORDER")]
        [SwaggerOperation("GetBlockedUnblockedPO")]
        [SwaggerResponse(200, type: typeof(List<string>), description: "Fetches List of Blocked or Unblocked Purchase Order")]
        [SwaggerResponse(400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Reciept Not Found")]
        public IActionResult GetBlockedUnblockedPO([FromQuery][Required] int index, [FromQuery][Required] int limit, [FromQuery] bool isBlocked, [FromQuery] string? searchTerm = null)
        {
            _logger.LogDebug($"Fetches the List of Blocked or Unblocked PO");

            Guid domainId = getUserDomainId();
            List<string> result = _purchaseOrderService.GetBlockOrUnblockedPurchaseOrder(index, limit, domainId, isBlocked, searchTerm);

            _logger.LogDebug($"Fetches the List of Blocked or Unblocked PO");
            return Ok(result);
        }

        /// <summary>
        /// Get the Batch Ids.
        /// </summary>
        [HttpGet]
        [Route("api/v1/purchase-order/batch-ids")]
        [ApiAuthorization(Name = "GET_BATCH_ID")]
        [SwaggerOperation("GetBatchId")]
        [SwaggerResponse(200, type: typeof(List<int>), description: "Fetches List of Batch Ids")]
        [SwaggerResponse(400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Reciept Not Found")]
        public IActionResult GetBatchId([FromQuery][Required] int index, [FromQuery][Required] int limit)
        {
            _logger.LogDebug($"Fetches the List of Batch Id");

            Guid domainId = getUserDomainId();
            List<int> result = _purchaseOrderService.GetBatchId(index, limit, domainId);

            _logger.LogDebug($"Fetched the List of Batch Id");
            return Ok(result);
        }

        /// <summary>
        /// Get the Blocked or Unblocked Purchase Order.
        /// </summary>
        [HttpGet]
        [Route("api/v1/purchase-order/batch-id-result")]
        [ApiAuthorization(Name = "GET_BATCH_ID_RESULT")]
        [SwaggerOperation("GetBatchIdResult")]
        [SwaggerResponse(200, type: typeof(List<PurchaseOrderLogResultDto>), description: "Fetches Result for the Batch Id")]
        [SwaggerResponse(400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Reciept Not Found")]
        public IActionResult GetBatchIdResult([FromQuery][Required] int index, [FromQuery][Required] int limit, [FromQuery] int batchId, [FromQuery] string? purchaseOrderId)
        {
            _logger.LogDebug($"Fetches the Result of Batch Id: {batchId}");

            Guid domainId = getUserDomainId();
            List<PurchaseOrderLogResultDto> result = _purchaseOrderService.GetBatchIdResult(index, limit, domainId, batchId, purchaseOrderId);

            _logger.LogDebug($"Fetched the Result of Batch Id: {batchId}");
            return Ok(result);
        }

        /// <summary>
        /// Create Purchase Order.
        /// </summary>
        [HttpPost]
        [Route("api/v1/purchase-order/create")]
        [ApiAuthorization(Name = "CREATE_PURCHASE_ORDER")]
        [SwaggerOperation("CreatePurchaseOrder")]
        [SwaggerResponse(200, type: typeof(SuccessResponseDto), description: "Created Purchase Order Successfully")]
        [SwaggerResponse(400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Reciept Not Found")]
        public IActionResult CreatePurchaseOrder([FromBody] List<PurchaseOrderCreateDto> dto)
        {
            Guid domainId = getUserDomainId();
            _logger.LogDebug($"Creating the Purchase Order for Domain with Id : {domainId}");
            Guid result = _purchaseOrderService.CreatePurchaseOrderManual(dto, domainId);

            _logger.LogDebug($"Created the Purchase Order for Domain with Id : {domainId}");
            return Ok(new SuccessResponseDto { Id = result.ToString(), StatusCode = 201, Message = "Purchase Order Created successfully", Description = "Purchase Order Created successfully" });
        }
        [HttpGet]
        [Route("api/v1/purchase-order/logs/download")]
        [ApiAuthorization(Name = "DOWNLOAD_PURCHASE_ORDER_LOG")]
        [SwaggerOperation("DownloadPurchaseOrderLogWithSuccessErpId")]
        [SwaggerResponse(200, description: "Excel file with PO logs and SuccessCloudERPId")]
        [SwaggerResponse(400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "No purchase order logs found")]
        public async Task<IActionResult> DownloadPurchaseOrderLogWithSuccessErpId(
            [FromQuery][Required] DateTime fromDate,
            [FromQuery][Required] DateTime toDate)
        {
            Guid domainId = getUserDomainId();
            _logger.LogInfo($"DownloadPurchaseOrderLogWithSuccessErpId called for Domain: {domainId}, From: {fromDate}, To: {toDate}");
            byte[] fileBytes = await _purchaseOrderService.DownloadPurchaseOrderLogWithSuccessErpIdAsync(domainId, fromDate, toDate);
            string fileName = $"PO_Log_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        [HttpGet]
        [Route("api/v1/purchase-order/blocked/download")]
        [ApiAuthorization(Name = "DOWNLOAD_BLOCKED_PURCHASE_ORDER")]
        [SwaggerOperation("DownloadBlockedPurchaseOrders")]
        [SwaggerResponse(200, description: "Excel file with blocked purchase orders and error comments")]
        [SwaggerResponse(400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "No blocked purchase orders found")]
        public async Task<IActionResult> DownloadBlockedPurchaseOrders()
        {
            Guid domainId = getUserDomainId();
            _logger.LogInfo($"DownloadBlockedPurchaseOrders called for Domain: {domainId}");
            byte[] fileBytes = await _purchaseOrderService.DownloadBlockedPurchaseOrdersAsync(domainId);
            string fileName = $"Blocked_PO_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

          [HttpGet]
        [Route("api/v1/purchase-order/blocked/download/PR")]
        [ApiAuthorization(Name = "DOWNLOAD_BLOCKED_PURCHASE_ORDER_PR")]
        [SwaggerOperation("DownloadPRBlockedPurchaseOrders")]
        [SwaggerResponse(200, description: "Excel file with blocked purchase orders and error comments")]
        [SwaggerResponse(400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "No blocked purchase orders found")]
        public async Task<IActionResult> DownloadPRBlockedPurchaseOrders([FromQuery] bool isBlocked)
        {
            Guid domainId = getUserDomainId();
            _logger.LogInfo($"DownloadPRBlockedPurchaseOrders called for Domain: {domainId}");
            byte[] fileBytes = await _purchaseOrderService.DownloadPRBlockedPurchaseOrdersAsync(domainId, isBlocked);
            string fileName = $"Blocked_PO_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}

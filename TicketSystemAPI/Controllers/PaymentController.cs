using System.ComponentModel.DataAnnotations;
using Contracts;
using Contracts.IServices;
using Dto;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TicketSystemAPI.Attributes;

namespace TicketSystemAPI.Controllers
{
    public class PaymentController : BaseController
    {
        private readonly ILoggerManager _logger;
        private readonly IAribaPaymentService _aribaPaymentService;

        public PaymentController(ILoggerManager logger, IAribaPaymentService aribaPaymentService)
        {
            _logger = logger;
            _aribaPaymentService = aribaPaymentService;
        }

        /// <summary>
        /// Get the Payment Related Combined Information
        /// </summary>
        [HttpGet]
        [Route("api/v1/payment")]
        [ApiAuthorization(Name = "GET_PAYMENT")]
        [SwaggerOperation("GetPayments")]
        [SwaggerResponse(200, type: typeof(List<PaymentResponseDto>), description: "Payments fetched successfully")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Domain Not Found")]
        public async Task<IActionResult> GetPayment([FromQuery] int index = 0, [FromQuery] int limit = 10, [FromQuery] string? searchTerm = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null, [FromQuery] string? status = null)
        {
            _logger.LogDebug("Fetching payment information");
            Guid userId = getLoggedUser();
            Guid domaindId = getUserDomainId();
            List<PaymentResponseDto> payments = await _aribaPaymentService.GetAllPayment(index, limit, userId, domaindId, searchTerm, fromDate, toDate, status);
            return Ok(payments);
        }

        /// <summary>
        /// Get Final Attachment for Invoice
        /// </summary>
        [HttpGet]
        [Route("api/v1/ariba/final-payment-attachment/{invoiceId}")]
        [ApiAuthorization(Name = "GET_FINAL_ATTACHMENT")]
        [SwaggerOperation("GetFinalAttachment")]
        [SwaggerResponse(200, type: typeof(FileContentResult), description: "Final Payment attachment fetched successfully")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Final Attachment Not Found")]
        public IActionResult GetFinalAttachment([FromRoute][Required] string invoiceId)
        {
            _logger.LogDebug($"Fetching the Attachments for the Invoice with Id : {invoiceId}");
            invoiceId = Uri.UnescapeDataString(invoiceId);
            SplitAttachmentResponseDto result = _aribaPaymentService.GetFinalAttachmentBase64(invoiceId);
            _logger.LogDebug($"Fetched the Attachments for the Invoice with Id : {invoiceId}");
            return Ok(result);
        }
    }
}

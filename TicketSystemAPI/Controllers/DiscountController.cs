using Microsoft.AspNetCore.Mvc;
using Dto;
using TicketSystemAPI.Attributes;
using Swashbuckle.AspNetCore.Annotations;
using Contracts.IServices;

namespace TicketSystemAPI.Controllers
{
    public class DiscountController : ControllerBase
    {
        private readonly IDiscountService _discountService;

        public DiscountController(IDiscountService discountService)
        {
            _discountService = discountService;
        }

        /// <summary>
        /// Creates a new discount coupon.
        /// </summary>
        [HttpPost]
        [Route("api/v1/discount")]
        [ValidateModelState]
        [ApiAuthorization(Name = "CREATE_DISCOUNT")]
        [SwaggerOperation("CreateDiscount")]
        [SwaggerResponse(statusCode: 201, type: typeof(SuccessResponseDto), description: "Discount coupon created successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public IActionResult CreateDiscount([FromBody] DiscountCouponDto dto)
        {
            string couponCode = _discountService.CreateDiscountCoupon(dto);
            return StatusCode(201, new SuccessResponseDto
            {
                StatusCode = 201,
                Id = couponCode,
                Message = "Discount coupon created successfully",
                Description = "Discount coupon has been created and stored."
            });
        }
    }
}
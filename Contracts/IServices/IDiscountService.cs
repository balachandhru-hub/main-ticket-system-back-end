using Dto;

namespace Contracts.IServices
{
    public interface IDiscountService
    {
        /// <summary>
        /// Creates the Discount Coupon.
        /// </summary>
        string CreateDiscountCoupon(DiscountCouponDto dto);
    }
}
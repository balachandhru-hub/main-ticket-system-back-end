using Dto;
using Entities.Models;
using Contracts.IRepository;
using Contracts.IServices;
using Entities.Common;
using System;
using System.Linq;
using Contracts;

namespace Services
{
    public class DiscountService : IDiscountService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerManager _logger;

        public DiscountService(IRepositoryWrapper repositoryWrapper, ILoggerManager logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new discount coupon with a unique alphanumeric code.
        /// Only allows discount percentage between configured min and max values.
        /// </summary>
        public string CreateDiscountCoupon(DiscountCouponDto dto)
        {
            _logger.LogInfo("Starting discount coupon creation.");

            int minDiscount = int.TryParse(
                _repositoryWrapper.Config.FindFirstByCondition(c => c.Key == Common.DISCOUNT_MIN_KEY)?.Value,
                out int minVal) ? minVal : Common.DISCOUNT_MIN;
            int maxDiscount = int.TryParse(
                _repositoryWrapper.Config.FindFirstByCondition(c => c.Key == Common.DISCOUNT_MAX_KEY)?.Value,
                out int maxVal) ? maxVal : Common.DISCOUNT_MAX;

            if (dto.DiscountPercentage < minDiscount || dto.DiscountPercentage > maxDiscount)
            {
                _logger.LogError($"Invalid discount percentage: {dto.DiscountPercentage}");
                throw new ArgumentException($"Discount percentage must be between {minDiscount} and {maxDiscount}.");
            }

            string couponCode = string.Empty;
            bool isUnique = false;
            int codeLength = Common.COUPON_CODE_LENGTH;

            while (!isUnique)
            {
                couponCode = GenerateCouponCode(codeLength);
                isUnique = !_repositoryWrapper.DiscountCoupon
                    .FindByCondition(d => d.CouponCode == couponCode)
                    .Select(d => d.CouponCode)
                    .Any();
            }

            DiscountCoupon discountCoupon = new DiscountCoupon
            {
                Id = Guid.NewGuid(),
                CouponCode = couponCode,
                DiscountPercentage = dto.DiscountPercentage
            };

            _repositoryWrapper.DiscountCoupon.Create(discountCoupon);
            _repositoryWrapper.Save();

            _logger.LogInfo($"Discount coupon created: {couponCode} with {dto.DiscountPercentage}% discount.");

            return discountCoupon.CouponCode;
        }

        private string GenerateCouponCode(int length)
        {
            Random random = new Random();
            return new string(Enumerable.Repeat(Common.COUPON_CODE_CHARS, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
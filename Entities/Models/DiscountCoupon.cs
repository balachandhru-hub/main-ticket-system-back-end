using System;
using System.ComponentModel.DataAnnotations;

namespace Entities.Models
{
    public class DiscountCoupon : BaseModel
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        public string CouponCode { get; set; }
        public int DiscountPercentage { get; set; }
        public DiscountCoupon() { }
    }
}
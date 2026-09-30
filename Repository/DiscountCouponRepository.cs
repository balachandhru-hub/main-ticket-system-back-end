using Contracts.IRepository;
using Entities.Models;
using Entities.DbContext;

namespace Repository
{
    /// <summary>
    /// Class <c>DiscountCouponRepository</c> used to implement the methods related for discount coupon.
    /// </summary>
    public class DiscountCoupenRepository : RepositoryBase<DiscountCoupon>, IDiscountCouponRepository
    {
        /// <summary>
        /// Constructor for injecting DbContext
        /// </summary>
        /// <param name="repositoryContext"></param>
        /// <returns></returns>

        public DiscountCoupenRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
        }
        
    }
}
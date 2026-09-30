using Contracts.IRepository;
using Entities.Models;
using Entities.DbContext;

namespace Repository
{
    /// <summary>
    /// Class <c>FeatureRepository</c> used to implement the methods related for User.
    /// </summary>
    public class FeatureRepository : RepositoryBase<Feature>, IFeatureRepository
    {
        /// <summary>
        /// Constructor for injecting DbContext
        /// </summary>
        /// <param name="repositoryContext"></param>
        /// <returns></returns>
        public FeatureRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {

        }
    }
}

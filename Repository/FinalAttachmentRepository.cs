using Contracts.IRepository;
using Entities.Models;
using Entities.DbContext;

namespace Repository
{
    /// <summary>
    /// Class <c>FeatureRepository</c> used to implement the methods related for User.
    /// </summary>
    public class FinalAttachmentRepository : RepositoryBase<FinalAttachment>, IFinalAttachmentRepository
    {
        /// <summary>
        /// Constructor for injecting DbContext
        /// </summary>
        /// <param name="repositoryContext"></param>
        /// <returns></returns>
        public FinalAttachmentRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {

        }
    }
}

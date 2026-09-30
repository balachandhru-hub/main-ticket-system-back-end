using Contracts.IRepository;
using Entities.Models;
using Entities.DbContext;

namespace Repository
{
    /// <summary>
    /// Class <c>FeatureRepository</c> used to implement the methods related for User.
    /// </summary>
    public class FinalPRAttachmentRepository : RepositoryBase<FinalPRAttachment>, IFinalPRAttachmentRepository
    {
        /// <summary>
        /// Constructor for injecting DbContext
        /// </summary>
        /// <param name="repositoryContext"></param>
        /// <returns></returns>
        public FinalPRAttachmentRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {

        }
    }
}

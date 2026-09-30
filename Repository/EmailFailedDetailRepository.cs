using Contracts.IRepository;
using Entities.DbContext;
using Entities.Models;

namespace Repository
{
    public class EmailFailedDetailRepository : RepositoryBase<EmailFailedDetail>, IEmailFailedDetailRepository
    {
        /// <summary>
        /// Constructor for injecting DbContext
        /// </summary>
        /// <param name="repositoryContext"></param>
        /// <returns></returns>
        public EmailFailedDetailRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {

        }
    }
}
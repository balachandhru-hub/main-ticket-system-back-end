using Contracts.IRepository;
using Entities.Models;
using Entities.DbContext;

namespace Repository
{
    /// <summary>
    /// Repository for AribaInvoiceUpload entity
    /// </summary>
    public class AribaPRUploadRepository : RepositoryBase<AribaPRUpload>, IAribaPRUploadRepository
    {
        /// <summary>
        /// Constructor for injecting DbContext
        /// </summary>
        /// <param name="repositoryContext">Database context</param>
        public AribaPRUploadRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
        }
    }
}

using Contracts.IRepository;
using Entities.Models;
using Entities.DbContext;

namespace Repository
{
    /// <summary>
    /// Repository for AribaInvoiceUpload entity
    /// </summary>
    public class AribaInvoiceUploadRepository : RepositoryBase<AribaInvoiceUpload>, IAribaInvoiceUploadRepository
    {
        /// <summary>
        /// Constructor for injecting DbContext
        /// </summary>
        /// <param name="repositoryContext">Database context</param>
        public AribaInvoiceUploadRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
        }
    }
}

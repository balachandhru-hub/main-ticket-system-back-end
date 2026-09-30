using Contracts.IRepository;
using Entities.Models;
using Entities.DbContext;

namespace Repository
{
    /// <summary>
    /// Repository for AribaInvoiceUpload entity
    /// </summary>
    public class AribaContractUploadRepository : RepositoryBase<AribaContractUpload>, IAribaContractUploadRepository
    {
        /// <summary>
        /// Constructor for injecting DbContext
        /// </summary>
        /// <param name="repositoryContext">Database context</param>
        public AribaContractUploadRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
        }
    }
}

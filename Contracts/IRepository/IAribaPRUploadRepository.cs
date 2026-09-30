using Entities.Models;

namespace Contracts.IRepository
{
    /// <summary>
    /// Interface for Ariba PR Upload Repository
    /// </summary>
    public interface IAribaPRUploadRepository : IRepositoryBase<AribaPRUpload>
    {
        /// <summary>
        /// Get PR upload record by approver and PR number
        /// </summary>

    }
}

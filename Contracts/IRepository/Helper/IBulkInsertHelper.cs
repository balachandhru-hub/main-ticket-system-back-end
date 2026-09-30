using Entities.Models;
namespace Contracts.IRepository
{
    /// <summary>
    /// BulkInsertHelper
    /// </summary>
    public interface IBulkInsertHelper
    {
        Task BulkInsertOrUpdateSafeAsync<T>(List<T> entities) where T : class;
    }
}

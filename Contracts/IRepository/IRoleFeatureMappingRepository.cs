using Entities.Models;
using System.Linq;
namespace Contracts.IRepository
{
    /// <summary>
    /// IRoleFeatureMappingRepository
    /// </summary>
    public interface IRoleFeatureMappingRepository : IRepositoryBase<RoleFeatureMapping>
    {
                /// <summary>
        /// Retrieves a list of feature names for a specified role.
        /// </summary>
        /// <param name="roleId">The role ID as a string (GUID).</param>
        /// <returns>A list of feature names associated with the given role.</returns>
        List<string> GetFeaturesByRole(string roleId);
    }
}

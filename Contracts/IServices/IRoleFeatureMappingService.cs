namespace Contracts.IServices
{
    public interface IRoleFeatureMappingService
    {
        /// <summary>
        /// Checks if a role-feature mapping exists.
        /// </summary>
        /// <param name="roleId">The ID of the role.</param>
        /// <param name="featureId">The ID of the feature.</param>
        /// <returns>True if the mapping exists, false otherwise.</returns>
        bool IsRoleFeatureMapped(Guid roleId, Guid featureId);

        List<string> GetFeatureByRoles(string roles);
    }
}
using Contracts.IRepository;
using Contracts.IServices;
using Dto;
using Entities.Models;
using Contracts;
using Microsoft.Extensions.Caching.Memory;
using Entities.Common;


namespace Services
{
    /// <summary>
    /// Service for managing RoleFeatureMapping.
    /// </summary>
    public class RoleFeatureMappingService : IRoleFeatureMappingService
    {
        private readonly IRepositoryWrapper _repoWrapper;

        private readonly ILoggerManager _logger;

        private readonly IMemoryCache _cache;


        /// <summary>
        /// Constructor for UserRoleMapping.
        /// </summary>
        /// <param name="repoWrapper">Repository wrapper.</param>
        /// <param name="logger">Logger manager.</param>

        public RoleFeatureMappingService(IRepositoryWrapper repoWrapper, ILoggerManager logger, IMemoryCache cache)
        {
            _repoWrapper = repoWrapper;
            _logger = logger;
            _cache = cache;

        }



        /// <summary>
        /// Checks if a role-feature mapping exists.
        /// </summary>
        /// <param name="roleId">The ID of the role.</param>
        /// <param name="featureId">The ID of the feature.</param>
        /// <returns>True if the mapping exists, false otherwise.</returns>
        public bool IsRoleFeatureMapped(Guid roleId, Guid featureId)
        {

            _logger.LogInfo($"Checking if role-feature mapping exists for role ID: {roleId}, feature ID: {featureId}");

            var roleFeatureMappings = _repoWrapper.RoleFeatureMapping.FindByCondition(r => r.IsActive).ToList();
            _logger.LogInfo($"Retrieved {roleFeatureMappings.Count} role-feature mappings from database.");

            bool mappingExists = roleFeatureMappings.Exists(r => r.RoleId == roleId && r.FeatureId == featureId);
            _logger.LogInfo($"Role-feature mapping exists: {mappingExists}");

            return mappingExists;
        }



        public List<string> GetFeatureByRoles(string roles)
        {
            _logger.LogInfo($"Fetching feature list for roles with ids: {roles}");
            List<string> roleFeatureIdsMap = _repoWrapper.RoleFeatureMapping.GetFeaturesByRole(roles);
            _logger.LogInfo($"Fetched feature list for roles with ids: {roles} successfully");

            return roleFeatureIdsMap;
        }


    }
}
using Contracts.IRepository;
using Entities.Models;
using Entities.DbContext;
using Contracts;
using Dto;
using Microsoft.Extensions.Configuration;
using Entities.Common;

namespace Repository
{
    /// <summary>
    /// Class <c>RoleFeatureMappingRepository</c> used to implement the methods related for User.
    /// Excluding from code coverage because currently not using, in future if use add in code coverage
    /// </summary>
    public class RoleFeatureMappingRepository : RepositoryBase<RoleFeatureMapping>, IRoleFeatureMappingRepository
    {

        private readonly string _dbConnectionString;

        private readonly ILoggerManager _logger;

        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor for injecting DbContext
        /// </summary>
        /// <param name="repositoryContext"></param>
        /// <returns></returns>
        public RoleFeatureMappingRepository(RepositoryContext repositoryContext, ILoggerManager logger, IConfiguration configuration, string dbConnectionString) : base(repositoryContext)
        {
            _logger = logger;
            _configuration = configuration;
            _dbConnectionString = dbConnectionString;
        }

        /// <summary>
        /// Retrieves a list of feature names for a specified role.
        /// </summary>
        /// <param name="roleId">The role ID as a string (GUID).</param>
        /// <returns>A list of feature names associated with the given role.</returns>
        public List<string> GetFeaturesByRole(string roleId)
        {
            _logger.LogInfo($"Fetching features for role ID: {roleId}");

            if (!Guid.TryParse(roleId, out Guid roleGuid))
            {
                _logger.LogError($"Invalid role ID format: {roleId}");
                return new List<string>();
            }

            List<string> featureNames = (from rfm in RepositoryContext.RoleFeatureMapping
                                         join f in RepositoryContext.Feature on rfm.FeatureId equals f.Id
                                         where rfm.IsActive && rfm.RoleId == roleGuid
                                         select f.Key).ToList();

            _logger.LogInfo($"Fetched {featureNames.Count} features for role ID: {roleId}");

            return featureNames;
        }

    }
}

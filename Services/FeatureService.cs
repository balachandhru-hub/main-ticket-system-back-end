using Contracts.IRepository;
using Contracts.IServices;
using Dto;
using Entities.Models;
using Contracts;

namespace Services
{
    /// <summary>
    /// Service for managing RoleFeatureMapping.
    /// </summary>
    public class FeatureService : IFeatureService
    {
        private readonly IRepositoryWrapper _repoWrapper;

        private readonly ILoggerManager _logger;


        /// <summary>
        /// Constructor for UserRoleMapping.
        /// </summary>
        /// <param name="repoWrapper">Repository wrapper.</param>
        /// <param name="logger">Logger manager.</param>

        public FeatureService(IRepositoryWrapper repoWrapper, ILoggerManager logger)
        {
            _repoWrapper = repoWrapper;
            _logger = logger;
        }



        public Guid GetFeatureIdByKey(string feature)
        {
            _logger.LogInfo($"Fetching the Feature Key with Id : {feature}");
            Guid featureId = _repoWrapper.Feature.FindFirstByCondition(f => f.IsActive && (f.Key!).ToLower().Equals(feature.ToLower())).Id;
            _logger.LogInfo($"Fetched the Feature Key with Id : {feature}");
            return featureId;
        }

    }
}
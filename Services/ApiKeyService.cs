using Contracts.IRepository;
using Contracts.IServices;
using Dto;
using Entities.Models;
using Contracts;

namespace Services
{
    /// <summary>
    /// Service for managing Api Key.
    /// </summary>
    public class ApiKeyService : IApiKeyService
    {
        private readonly IRepositoryWrapper _repoWrapper;

        private readonly ILoggerManager _logger;

        /// <summary>
        /// Constructor for Api Key.
        /// </summary>
        /// <param name="repoWrapper">Repository wrapper.</param>
        /// <param name="logger">Logger manager.</param>

        public ApiKeyService(IRepositoryWrapper repoWrapper, ILoggerManager logger)
        {
            _repoWrapper = repoWrapper;
            _logger = logger;
        }

        /// <summary>
        /// Checks weather given api key is valid or not
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>bool value</returns>
        public bool IsApiKeyValid(string key)
        {
            _logger.LogInfo("Validating the API Key Value");
            string result = _repoWrapper.ApiKey.FindFirstByCondition(x => x.IsActive).ApiKeyValue;
            if (result == key)
            {
                return true;
            }
            return false;
        }
    }
}
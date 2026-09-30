using Contracts;
using Contracts.IRepository;
using Entities.Models;
using Entities.DbContext;
using Microsoft.Extensions.Configuration;
using Entities.Common;
using Dto;

namespace Repository
{
    /// <summary>
    /// Class <c>AssetRepository</c> used to implement the methods related for Asset.
    /// </summary>
    public class AssetRepository : RepositoryBase<Asset>, IAssetRepository
    {
        /// <summary>
        /// Constructor for injecting DbContext
        /// </summary>
        /// <param name="repositoryContext"></param>
        /// <returns></returns>

        private readonly IConfiguration _configuration;

        private readonly string _dbConnectionString;

        private readonly ILoggerManager _logger;

        public AssetRepository(RepositoryContext repositoryContext, IConfiguration configuration, ILoggerManager logger, string dbConnectionString) : base(repositoryContext)
        {

            _configuration = configuration;
            _logger = logger;
            _dbConnectionString = dbConnectionString;
        }

    }
}
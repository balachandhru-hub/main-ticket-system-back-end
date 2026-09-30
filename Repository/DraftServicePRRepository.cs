using Contracts;
using Contracts.IRepository;
using Entities.DbContext;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Repository
{
    /// <summary>
    /// Repository for Draft Material PR entity, implementing IDraftMaterialPRRepository interface.
    /// </summary>
    public class DraftServicePRRepository : RepositoryBase<DraftServicePR>, IDraftServicePRRepository
    {
        private readonly ILoggerManager _logger;
        private readonly IConfiguration _configuration;

        public DraftServicePRRepository(RepositoryContext repositoryContext, ILoggerManager logger, IConfiguration configuration)
            : base(repositoryContext)
        {
            _logger = logger;
            _configuration = configuration;
        }

    
    }
}
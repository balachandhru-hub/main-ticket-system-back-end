using Contracts;
using Contracts.IRepository;
using Entities.DbContext;
using Entities.Models;
using Microsoft.Extensions.Configuration;

namespace Repository
{
    /// <summary>
    /// Repository for Draft Material PR Attachment entity, implementing IDraftMaterialPRAttachmentRepository interface.
    /// </summary>
    public class DraftMaterialPRAttachmentRepository : RepositoryBase<DraftMaterialPRAttachment>, IDraftMaterialPRAttachmentRepository
    {
        private readonly ILoggerManager _logger;
        private readonly IConfiguration _configuration;

        public DraftMaterialPRAttachmentRepository(RepositoryContext repositoryContext, ILoggerManager logger, IConfiguration configuration)
            : base(repositoryContext)
        {
            _logger = logger;
            _configuration = configuration;
        }
    }
}

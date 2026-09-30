using Contracts;
using Contracts.IRepository;
using Entities.DbContext;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class PurchaseOrderBotReprocessingLogRepository : RepositoryBase<PurchaseOrderBotReprocessingLog>, IPurchaseOrderBotReprocessingLogRepository
    {
        private ILoggerManager _logger;

        public PurchaseOrderBotReprocessingLogRepository(RepositoryContext repositoryContext, ILoggerManager logger)
            : base(repositoryContext)
        {
            _logger = logger;
        }
    }
}

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
    public class SchedularHistoryRepository : RepositoryBase<SchedularHistory>, ISchedularHistoryRepository
    {
        private ILoggerManager _logger;

        public SchedularHistoryRepository(RepositoryContext repositoryContext, ILoggerManager logger)
            : base(repositoryContext)
        {
            _logger = logger;
        }
    }

}
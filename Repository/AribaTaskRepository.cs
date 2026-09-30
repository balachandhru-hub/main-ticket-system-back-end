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
    public class AribaTaskRepository : RepositoryBase<AribaTasks>, IAribaTaskRepository
    {
        private readonly RepositoryContext _context;
        
        public AribaTaskRepository(RepositoryContext context) : base(context)
        {
            _context = context;
        }
    }
}

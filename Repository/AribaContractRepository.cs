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
    public class AribaContractRepository : RepositoryBase<AribaContract>, IAribaContractRepository
    {
        private readonly RepositoryContext _context;

        public AribaContractRepository(RepositoryContext context) : base(context)
        {
            _context = context;
        }
    }
}

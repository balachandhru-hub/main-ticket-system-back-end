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
    
    public class AribaContractDetailsRepository : RepositoryBase<AribaContractDetails>, IAribaContractDetailsRepository
    {
        private readonly RepositoryContext _context;

        public AribaContractDetailsRepository(RepositoryContext context) : base(context)
        {
            _context = context;
        }
    }
}

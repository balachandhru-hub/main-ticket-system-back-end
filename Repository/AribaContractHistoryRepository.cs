using Contracts.IRepository;
using Entities.DbContext;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    public class AribaContractHistoryRepository : RepositoryBase<AribaContractHistory>, IAribaContractHistoryRepository 
    {
        private readonly RepositoryContext _context;

        public AribaContractHistoryRepository(RepositoryContext context) : base(context)
        {
            _context = context;
        }
    }
}

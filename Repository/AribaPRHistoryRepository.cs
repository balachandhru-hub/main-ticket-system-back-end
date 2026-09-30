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
    public class AribaPRHistoryRepository : RepositoryBase<AribaPRHistory>, IAribaPRHistoryRepository
    {
        private readonly RepositoryContext _context;

        public AribaPRHistoryRepository(RepositoryContext context) : base(context)
        {
            _context = context;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.IRepository;
using Entities.DbContext;
using Entities.Models;


namespace Repository
{
    public class TicketPRRepository : RepositoryBase<TicketPR>, ITicketPRRepository
    {
        public TicketPRRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
        }

        
    }
}

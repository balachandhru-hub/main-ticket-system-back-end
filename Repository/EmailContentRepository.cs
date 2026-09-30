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
     public class EmailContentRepository : RepositoryBase<EmailContent>, IEmailContentRepository
    {
        public EmailContentRepository(RepositoryContext repositoryContext)
            : base(repositoryContext)
        {
        }
    }
}

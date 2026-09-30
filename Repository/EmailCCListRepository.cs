using Contracts.IRepository;
using Entities.DbContext;
using Entities.Migrations;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class EmailCCListRepository : RepositoryBase<EmailCCList>, IEmailCCListRepository
    {
        public EmailCCListRepository(RepositoryContext repositoryContext)
            : base(repositoryContext)
        {
        }
    }
}

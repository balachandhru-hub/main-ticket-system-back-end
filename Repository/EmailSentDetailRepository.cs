using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.IRepository;
using Entities.DbContext;
using Entities.Models;

namespace Repository
{
    public class EmailSentDetailRepository : RepositoryBase<EmailSentDetail>, IEmailSentDetailRepository
    {
        /// <summary>
        /// Constructor for injecting DbContext
        /// </summary>
        /// <param name="repositoryContext"></param>
        /// <returns></returns>
        public EmailSentDetailRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {

        }
    }
}
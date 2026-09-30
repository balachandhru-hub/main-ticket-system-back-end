using Contracts.IRepository;
using Entities.DbContext;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.Models;
namespace Repository
{
    public class AribaAttachmentRepository : RepositoryBase<AribaAttachment>, IAribaAttachmentRepository
    {
        private readonly RepositoryContext _context;

        public AribaAttachmentRepository(RepositoryContext context) : base(context)
        {
            _context = context;
        }
    }
    
}

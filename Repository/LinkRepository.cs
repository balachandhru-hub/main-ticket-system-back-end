using Contracts.IRepository;
using Entities.DbContext;
using Entities.Models;

namespace Repository
{
    public class LinkRepository : RepositoryBase<Link>, ILinkRepository
    {
        private readonly RepositoryContext _context;

        public LinkRepository(RepositoryContext context) : base(context)
        {
            _context = context;
        }
    }
}

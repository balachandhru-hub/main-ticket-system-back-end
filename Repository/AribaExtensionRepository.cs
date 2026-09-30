using Contracts.IRepository;
using Entities.DbContext;
using Entities.Models;

namespace Repository
{
    public class AribaExtensionRepository : RepositoryBase<AribaExtension>, IAribaExtensionRepository
    {
        private readonly RepositoryContext _context;

        public AribaExtensionRepository(RepositoryContext context) : base(context)
        {
            _context = context;
        }
    }
}

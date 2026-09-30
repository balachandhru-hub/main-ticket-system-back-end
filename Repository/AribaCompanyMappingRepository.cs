using Contracts.IRepository;
using Entities.DbContext;
using Entities.Models;

namespace Repository
{
    public class AribaCompanyMappingRepository : RepositoryBase<AribaCompanyMapping>, IAribaCompanyMappingRepository
    {
        private readonly RepositoryContext _context;

        public AribaCompanyMappingRepository(RepositoryContext context) : base(context)
        {
            _context = context;
        }
    }
    
}
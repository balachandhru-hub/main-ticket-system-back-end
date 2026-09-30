using Contracts.IRepository;
using Entities.DbContext;
using Entities.Models;

namespace Repository
{
    public class AribaSupplierMappingRepository : RepositoryBase<AribaSupplierName>, IAribaSupplierMappingRepository
    {
        private readonly RepositoryContext _context;

        public AribaSupplierMappingRepository(RepositoryContext context) : base(context)
        {
            _context = context;
        }
    }
    
}
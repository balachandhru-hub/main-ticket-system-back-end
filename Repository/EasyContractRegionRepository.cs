using Contracts.IRepository;
using Entities.DbContext;
using Entities.Models;

namespace Repository
{
    public class EasyContractRegionRepository : RepositoryBase<EasyContractRegion>, IEasyContractRegionRepository
    {
        private readonly RepositoryContext _context;

        public EasyContractRegionRepository(RepositoryContext context) : base(context)
        {
            _context = context;
        }
    }
    
}
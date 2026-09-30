using Contracts.IRepository;
using Entities.DbContext;
using Entities.Models;

namespace Repository
{
    public class ContractRegionRepository : RepositoryBase<ContractRegion>, IContractRegionRepository
    {
        private readonly RepositoryContext _context;

        public ContractRegionRepository(RepositoryContext context) : base(context)
        {
            _context = context;
        }
    }
    
}
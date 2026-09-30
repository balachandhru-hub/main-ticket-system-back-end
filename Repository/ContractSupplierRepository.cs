using Contracts.IRepository;
using Entities.DbContext;
using Entities.Models;

namespace Repository
{
    public class ContractSupplierRepository : RepositoryBase<ContractSupplier>, IContractSupplierRepository
    {
        private readonly RepositoryContext _context;

        public ContractSupplierRepository(RepositoryContext context) : base(context)
        {
            _context = context;
        }
    }
    
}
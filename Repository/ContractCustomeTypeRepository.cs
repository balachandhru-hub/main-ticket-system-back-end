using Contracts.IRepository;
using Entities.DbContext;
using Entities.Models;

namespace Repository
{
    public class ContractCustomTypeRepository : RepositoryBase<ContractCustomType>, IContractCustomTypeRepository
    {
        private readonly RepositoryContext _context;

        public ContractCustomTypeRepository(RepositoryContext context) : base(context)
        {
            _context = context;
        }
    }
    
}
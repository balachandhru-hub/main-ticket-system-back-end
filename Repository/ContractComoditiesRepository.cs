using Contracts.IRepository;
using Entities.DbContext;
using Entities.Models;

namespace Repository
{
    public class ContractComoditiesRepository : RepositoryBase<ContractComodities>, IContractComoditiesRepository
    {
        private readonly RepositoryContext _context;

        public ContractComoditiesRepository(RepositoryContext context) : base(context)
        {
            _context = context;
        }
    }
    
}
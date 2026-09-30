using Contracts.IRepository;
using Entities.DbContext;
using Entities.Models;

namespace Repository
{
    public class ContractDepartmentRepository : RepositoryBase<ContractDepartment>, IContractDepartmentRepository
    {
        private readonly RepositoryContext _context;

        public ContractDepartmentRepository(RepositoryContext context) : base(context)
        {
            _context = context;
        }
    }
    
}
using Contracts.IRepository;
using Entities.DbContext;
using Entities.Models;

namespace Repository
{
    public class EasyContractDepartmentRepository : RepositoryBase<EasyContractDepartment>, IEasyContractDepartmentRepository
    {
        private readonly RepositoryContext _context;

        public EasyContractDepartmentRepository(RepositoryContext context) : base(context)
        {
            _context = context;
        }
    }
    
}
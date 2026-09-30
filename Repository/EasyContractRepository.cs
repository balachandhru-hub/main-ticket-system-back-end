using Contracts.IRepository;
using Entities.DbContext;
using Entities.Models;

namespace Repository
{
    public class EasyContractRepository : RepositoryBase<EasyContract>, IEasyContractRepository
    {
        private readonly RepositoryContext _context;

        public EasyContractRepository(RepositoryContext context) : base(context)
        {
            _context = context;
        }
    }
    
}
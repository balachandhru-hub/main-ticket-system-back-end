using Contracts.IRepository;
using Entities.DbContext;
using Entities.Models;

namespace Repository
{
    public class EasyContractComoditiesRepository : RepositoryBase<EasyContractComoditity>, IEasyContractComoditiesRepository
    {
        private readonly RepositoryContext _context;

        public EasyContractComoditiesRepository(RepositoryContext context) : base(context)
        {
            _context = context;
        }
    }
    
}
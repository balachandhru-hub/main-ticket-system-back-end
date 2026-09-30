using Contracts.IRepository;
using Entities.DbContext;
using Entities.Models;

namespace Repository
{
    public class LoginRecordRepository : RepositoryBase<LoginRecord>, ILoginRecordRepository
    {
        private readonly RepositoryContext _context;

        public LoginRecordRepository(RepositoryContext context) : base(context)
        {
            _context = context;
        }
    }
}

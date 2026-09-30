using Contracts.IRepository;
using Entities.DbContext;
using Entities.Models;

namespace Repository
{
    public class AdditionalQueryRepository : RepositoryBase<AdditionalQuery>, IAdditionalQueryRepository
    {
        private readonly RepositoryContext _context;

        public AdditionalQueryRepository(RepositoryContext context) : base(context)
        {
            _context = context;
        }
    }
}

using Contracts.IRepository;
using Entities.DbContext;
using Entities.Models;
using Dto;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    public class PublicRelationCategoryRepository : RepositoryBase<PublicRelationCategory>, IPublicRelationCategoryRepository
    {
        private readonly RepositoryContext _context;

        public PublicRelationCategoryRepository(RepositoryContext context) : base(context)
        {
            _context = context;
        }
         

      

    }
    
}
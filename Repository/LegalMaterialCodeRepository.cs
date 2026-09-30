using Contracts.IRepository;
using Entities.DbContext;
using Entities.Models;
using Dto;
using Microsoft.EntityFrameworkCore;
using Entities.Common;

namespace Repository
{
    public class LegalMaterialCodeRepository : RepositoryBase<LegalMaterialCode>, ILegalMaterialCodeRepository
    {
        private readonly RepositoryContext _context;

        public LegalMaterialCodeRepository(RepositoryContext context) : base(context)
        {
            _context = context;
        }
         public async Task<LegalMaterialCodeListDto?> GetLegalMaterialCodeDetailsByNameAsync(
    Guid domainId,
    string materialCode)
{
    return await _context.LegalMaterialCode
        .AsNoTracking()
        .Where(v =>
            v.DomainId == domainId &&
           v.MaterialCode == materialCode)
        .Select(v => new LegalMaterialCodeListDto
        
        {
           GLAccount = v.GLAccount
           

        })
        .FirstOrDefaultAsync();
}
    }
}
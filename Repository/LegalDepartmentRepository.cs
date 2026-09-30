using Contracts.IRepository;
using Entities.DbContext;
using Entities.Models;
using Dto;
using Microsoft.EntityFrameworkCore;
using Entities.Common;

namespace Repository
{
    public class LegalDepartmentMaterialRepository : RepositoryBase<LegalVenueList>, ILegalDepartmentMaterialRepository
    {
        private readonly RepositoryContext _context;

        public LegalDepartmentMaterialRepository(RepositoryContext context) : base(context)
        {
            _context = context;
        }
        public async Task<VenueDetailsDto?> GetVenueDetailsByNameAsync(
    Guid domainId,
    string venueName)
{
    return await _context.LegalVenueList
        .AsNoTracking()
        .Where(v =>
            v.DomainId == domainId &&
           v.VenueName == venueName)
        .Select(v => new VenueDetailsDto
        {
           
          
            VenueName = v.VenueName,
            CompanyCode = v.CompanyCode,
            CompanyName = v.CompanyName,
            CostCenter = v.CostCenter,
            ShipToPlant = v.ShipToPlant,
            PurchaseOrg = v.PurchaseOrg,
            PurchaseGroup = v.PurchaseGroup,
            BillTo = v.BillTo,
            AribaUserId = v.AribaUserId,
            UOM = Common.LEGAL_DEPARTMENT_UOM
           

        })
        .FirstOrDefaultAsync();
}
    }
}
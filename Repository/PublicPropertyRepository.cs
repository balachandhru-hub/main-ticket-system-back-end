using Contracts.IRepository;
using Entities.DbContext;
using Entities.Models;
using Dto;
using Microsoft.EntityFrameworkCore;
using Entities.Common;

namespace Repository
{
    public class PublicPropertyRepository : RepositoryBase<PublicProperty>, IPublicPropertyRepository
    {
        private readonly RepositoryContext _context;

        public PublicPropertyRepository(RepositoryContext context) : base(context)
        {
            _context = context;
        }
          public async Task<VenueDetailsDto?> GetPublicVenueDetailsByNameAsync(
    Guid domainId,
    string venueName)
{
    return await _context.PublicProperty
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
            GLAccount = v.GLAccount,
            ShipToPlant = v.ShipToPlant,
            PurchaseOrg = v.PurchaseOrg,
            PurchaseGroup = v.PurchaseGroup,
            BillTo = v.BillTo,
            MaterialCode = Common.PUBLIC_RELATION_MATERIAL_CODE
        })
        .FirstOrDefaultAsync();
}

      

    }
    
}
using Contracts.IRepository;
using Entities.DbContext;
using Entities.Models;
using Dto;
using Microsoft.EntityFrameworkCore;
using Entities.Common;

namespace Repository
{
    public class AribaVenueListMappingRepository : RepositoryBase<AribaVenueListMapping>, IAribaVenueListMappingRepository
    {
        private readonly RepositoryContext _context;

        public AribaVenueListMappingRepository(RepositoryContext context) : base(context)
        {
            _context = context;
        }
        public async Task<VenueDetailsDto?> GetVenueDetailsByNameAsync(
    Guid domainId,
    string venueName)
{
    return await _context.AribaVenueListMapping
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
            AribaUserId = v.AribaUserId,
            MaterialCode = Common.PR_ENTERTAINMENT_MATERIAL_CODE

        })
        .FirstOrDefaultAsync();
}


   public async Task<VenueDetailsDto?> GetConciergeVenueDetailsByNameAsync(
    Guid domainId,
    string venueName)
{
    return await _context.ConciergeVenue
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
            BillTo = v.BillTo
        })
        .FirstOrDefaultAsync();
}

    }
    
}
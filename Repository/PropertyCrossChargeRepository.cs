using Contracts.IRepository;
using Entities.DbContext;
using Entities.Models;
using Dto;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    public class PropertyCrossChargeRepository : RepositoryBase<PropertyCrossCharge>, IPropertyCrossChargeRepository
    {
        private readonly RepositoryContext _context;

        public PropertyCrossChargeRepository(RepositoryContext context) : base(context)
        {
            _context = context;
        }
        public async Task<PropertyCrossChargeDetailsDto?> GetDetailsByNameAsync(
    Guid domainId,
    string propertyCrossChargeName,string? companyCode)
{
    return await _context.PropertyCrossCharge
        .AsNoTracking()
        .Where(v =>
            v.DomainId == domainId &&
           v.PropertyCrossChargeName == propertyCrossChargeName&& v.CompanyCode == companyCode)
        .Select(v => new PropertyCrossChargeDetailsDto
        {
             CostCenter = v.CostCenter,
            GLAccount = v.GLAccount,
          
        })
        .FirstOrDefaultAsync();
}

    }
    
}
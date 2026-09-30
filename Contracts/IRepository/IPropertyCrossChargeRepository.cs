using Entities.Models;
using Dto;

namespace Contracts.IRepository
{
    public interface IPropertyCrossChargeRepository : IRepositoryBase<PropertyCrossCharge>
    {
        Task<PropertyCrossChargeDetailsDto?> GetDetailsByNameAsync(
    Guid domainId,
    string propertyCrossChargeName,string? companyCode);
    }
}
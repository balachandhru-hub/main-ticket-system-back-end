using Entities.Models;
using Dto;

namespace Contracts.IRepository
{
    public interface ILegalDepartmentMaterialRepository : IRepositoryBase<LegalVenueList>
    {
        Task<VenueDetailsDto?> GetVenueDetailsByNameAsync(
    Guid domainId,
    string venueName);
    
    }
}

using Entities.Models;
using Dto;

namespace Contracts.IRepository
{
    public interface IAribaVenueListMappingRepository : IRepositoryBase<AribaVenueListMapping>
    {
        Task<VenueDetailsDto?> GetVenueDetailsByNameAsync(
    Guid domainId,
    string venueName);
    }
}
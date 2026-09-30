using Entities.Models;
using Dto;

namespace Contracts.IRepository
{
    public interface IConciergeVenueRepository : IRepositoryBase<ConciergeVenue>
    {
        Task<VenueDetailsDto?> GetConciergeVenueDetailsByNameAsync(
    Guid domainId,
    string venueName);
    }
}
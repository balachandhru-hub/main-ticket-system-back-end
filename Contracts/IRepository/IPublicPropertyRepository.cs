using Entities.Models;
using Dto;

namespace Contracts.IRepository
{
    public interface IPublicPropertyRepository : IRepositoryBase<PublicProperty>
    {
       Task<VenueDetailsDto?> GetPublicVenueDetailsByNameAsync(
    Guid domainId,
    string venueName);
    }
}
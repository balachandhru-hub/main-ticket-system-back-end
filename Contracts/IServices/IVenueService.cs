using Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Contracts.IServices
{
    public interface IVenueService
    {
        /// <summary>
        /// Create a new venue
        /// </summary>
        Task CreateVenueSupplierAsync(
    Guid domainId,
    CreateVenueSupplierDto dto);

        /// <summary>
        /// Get all venues for a domain
        /// </summary>
        Task<object> GetVenueSupplierAsync(
    Guid domainId,
    GetVenueSupplierRequestDto dto);



        /// <summary>
        /// Update a venue by name within a domain
        /// </summary>
        Task<bool> UpdateVenueSupplierAsync(
    Guid domainId,
    UpdateVenueSupplierDto dto);

        /// <summary>
        /// Delete a venue by name within a domain
        /// </summary>
        Task<bool> DeleteVenueSupplierAsync(
    Guid domainId,
    string type,
    string identifier);
    }
}

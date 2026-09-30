using Dto;
using Entities.Models;
using System;
using System.Collections.Generic;

namespace Contracts.IRepository
{
    /// <summary>
    /// Repository interface for Domain entity.
    /// </summary>
    public interface IDomainRepository : IRepositoryBase<Domain>
    {
        /// <summary>
        /// Retrieves all active domains.
        /// </summary>
        /// <returns>List of active domain DTOs</returns>
        List<DomainPreviewDto> GetAllDomain(int index, int limit, string? aribaNetworkId, string? searchTerm);
        /// <summary>
        /// Retrieves all expired domains.
        /// </summary>
        /// <returns>List of expired domain DTOs</returns>
        List<DomainPreviewDto> GetAllExpiredDomain(int index, int limit, string? searchTerm);
    }
}
using Dto;
using System;
using System.Collections.Generic;
using Entities.Models; // <-- Add this line

namespace Contracts.IServices
{
    /// <summary>
    /// Service interface for domain-related operations.
    /// </summary>
    public interface IDomainService
    {
        /// <summary>
        /// Creates a new domain based on the provided DTO.
        /// </summary>
        /// <param name="domainDto">Domain data to be created</param>
        /// <returns>Created domain DTO</returns>
        DomainDto CreateDomain(DomainDto domainDto);
        /// <summary>
        /// Fetches the Domain Details for the Given domain id
        /// </summary>
        /// <param name="domainId"></param>
        /// <returns></returns>
        DomainDetailsDto GetDomainDetails(Guid domainId);

        /// <summary>
        /// Retrieves a domain by its ID.
        /// </summary>
        /// <param name="domainId">Unique identifier of the domain</param>
        /// <returns>Domain DTO</returns>
        DomainDto GetDomainById(Guid domainId);

        /// <summary>
        /// Creates the Plan for the Given Domain.
        /// </summary>
        Guid CreateDomainPlan(DomainPlanDto dto);
        /// <summary>
        /// Retrieves all active domains.
        /// </summary>
        /// <returns>List of active domain DTOs</returns>
        List<DomainPreviewDto> GetAllDomains(int index, int limit, string? aribaNetworkId, string? searchTerm);
        /// <summary>
        /// Updates the details of an existing domain.  
        /// </summary>
        /// <param name="updateDto">The updated domain data.</param>
        /// 
        ///                                     
        void UpdateDomain(DomainUpdateDto updateDto);
        /// <summary>
        /// Get Domain Logo for the Given User
        /// </summary>
        /// <param name="userId">User ID</param>
        AssetDownloadDto GetDomainLogo(Guid userId);

        void UpdateDomainStatus(Guid domainId, string activeStatus);
        /// <summary> 
        ///   
        ///   
        /// Updates the payment status of a domain.
        /// </summary>
        /// <param name="domainId">The ID of the domain.</param>;
        public List<DomainPreviewDto> GetAllExpiredDomain(int index, int limit, string? searchTerm);

    public void CreateDomainSettings(DomainSettingPurchaseOrderDto dto, Guid domainId, string settingType);
   
    }
}
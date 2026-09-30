using Contracts.IRepository;
using Entities.Models;
using Entities.DbContext;
using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Dto;
using Entities.Common;

namespace Repository
{
    /// <summary>
    /// Repository implementation for Domain entity.
    /// </summary>
    public class DomainRepository : RepositoryBase<Domain>, IDomainRepository
    {
        private ILoggerManager _logger;
        public DomainRepository(RepositoryContext repositoryContext, ILoggerManager logger)
            : base(repositoryContext)
        {
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all active domains.
        /// </summary>
        /// <returns>List of active domain DTOs</returns>
        public List<DomainPreviewDto> GetAllDomain(int index, int limit, string? aribaNetworkId, string? searchTerm)
        {
            _logger.LogInfo($"Fetching the Active Domains");

            List<DomainPreviewDto> result = (
                from _domain in RepositoryContext.Domain
                where _domain.IsActive && _domain.Id != Common.DEFAULT_DOMAIN_ID
                    && (string.IsNullOrEmpty(aribaNetworkId) || _domain.AribaNetworkId == aribaNetworkId)
                join _domainPlan in RepositoryContext.DomainPlan
                    on _domain.Id equals _domainPlan.DomainId into domainPlanGroup
                from _domainPlan in domainPlanGroup.DefaultIfEmpty()
                where _domainPlan == null || _domainPlan.IsRunning
                orderby _domain.DateCreated descending
                select new DomainPreviewDto
                {
                    Id = _domain.Id,
                    DomainName = _domain.Name,
                    CurrentPlan = _domainPlan != null ? _domainPlan.PlanName : null,
                    ActiveSince = _domain.DateCreated.ToString("MMM yyyy"),
                    GST_VAT = _domain.GST ?? _domain.VAT,
                    LUTNumber= _domain.LUTNumber,
                    AribaNetworkId = _domain.AribaNetworkId,
                    IsActive = _domainPlan != null ? _domainPlan.IsActive : (bool?)null,
                }
            ).ToList();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                result = result.Where(d =>
                    (d.DomainName != null && d.DomainName.ToLower().Contains(searchTerm)) ||
                    (d.AribaNetworkId != null && d.AribaNetworkId.ToLower().Contains(searchTerm))
                ).ToList();
            }

            return result
                .Skip(index)
                .Take(limit)
                .ToList();
        }



        public List<DomainPreviewDto> GetAllExpiredDomain(int index, int limit, string? searchTerm)
        {
            _logger.LogInfo("Fetching the Inactive (Expired) Domains");

            var query = from _domain in RepositoryContext.Domain
                        where _domain.IsActive
                              && _domain.Id != Common.DEFAULT_DOMAIN_ID
                              && (string.IsNullOrEmpty(searchTerm) || _domain.Name.Contains(searchTerm) || _domain.AribaNetworkId.Contains(searchTerm))
                        let latestPlan = RepositoryContext.DomainPlan
                                .Where(p => p.DomainId == _domain.Id)
                                .OrderByDescending(p => p.DateCreated)
                                .FirstOrDefault()
                        where latestPlan != null && !latestPlan.IsRunning
                        orderby _domain.DateCreated descending
                        select new DomainPreviewDto
                        {
                            Id = _domain.Id,
                            DomainName = _domain.Name,
                            CurrentPlan = latestPlan != null ? latestPlan.PlanName : null,
                            ActiveSince = _domain.DateCreated.ToString("MMM yyyy"),
                            GST_VAT = _domain.GST ?? _domain.VAT,
                            AribaNetworkId = _domain.AribaNetworkId,
                            IsActive = latestPlan.IsActive
                        };

            return query.Skip(index).Take(limit).ToList();
        }



    }
}

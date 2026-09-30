using Contracts;
using Contracts.IServices;
using Dto;
using Entities.Models;
using ExceptionHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.IRepository;
using Entities.Common;

namespace Services
{
    public class VenueService : IVenueService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerManager _logger;

        public VenueService(IRepositoryWrapper repositoryWrapper, ILoggerManager logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
        }

        public async Task CreateVenueSupplierAsync(
     Guid domainId,
     CreateVenueSupplierDto dto)
        {
            _logger.LogInfo($"Creating data for domain {domainId} with type {dto.Type}");

            if (string.IsNullOrWhiteSpace(dto.Type)){
                _logger.LogError("Type cannot be empty");
                throw new PreConditionFailedCustomException("Type required", "Type cannot be empty");
            }

         
            if (dto.Type == Common.MASTER_VENUE_TYPE && (dto.Venues == null || dto.Suppliers != null)){
                _logger.LogError("Invalid request For VENUE type, only venue list must be provided");
                throw new PreConditionFailedCustomException(
                    "Invalid request",
                    "For VENUE type, only venue list must be provided");
            }

            if (dto.Type == Common.MASTER_SUPPLIER_TYPE && (dto.Suppliers == null || dto.Venues != null)){
                _logger.LogError("Invalid request For Supplier type, only venue list must be provided");
                throw new PreConditionFailedCustomException(
                    "Invalid request",
                    "For SUPPLIER type, only supplier list must be provided");
            }


            // =======================
            // CREATE VENUES
            // =======================
            if (dto.Type == Common.MASTER_VENUE_TYPE)
            {
                foreach (var venueDto in dto.Venues!)
                {
                    if (string.IsNullOrWhiteSpace(venueDto.VenueName))
                        continue;

                    var exists = _repositoryWrapper.AribaVenueListMapping
                        .FindByCondition(v =>
                            v.DomainId == domainId &&
                            v.VenueName.ToLower() == venueDto.VenueName.ToLower() && 
                            v.IsActive)
                        .Any();

                    if (exists)
                        continue;

                    var venue = new AribaVenueListMapping
                    {
                        Id = Guid.NewGuid(),
                        DomainId = domainId,
                        VenueName = venueDto.VenueName,
                        CompanyCode = venueDto.CompanyCode,
                        CompanyName = venueDto.CompanyName,
                        CostCenter = venueDto.CostCenter,
                        GLAccount = venueDto.GLAccount,
                        ShipToPlant = venueDto.ShipToPlant,
                        PurchaseOrg = venueDto.PurchaseOrg,
                        PurchaseGroup = venueDto.PurchaseGroup,
                        BillTo = venueDto.BillTo
                    };

                    _repositoryWrapper.AribaVenueListMapping.Create(venue);
                }
            }

            // =======================
            // CREATE SUPPLIERS
            // =======================
            if (dto.Type == Common.MASTER_SUPPLIER_TYPE)
            {
                foreach (var supplierDto in dto.Suppliers!)
                {
                    if (string.IsNullOrWhiteSpace(supplierDto.SupplierName))
                        continue;

                    var exists = _repositoryWrapper.AribaSupplierName
                        .FindByCondition(s =>
                            s.DomainId == domainId &&
                            s.SupplierName.ToLower() == supplierDto.SupplierName.ToLower() &&
                            s.IsActive)
                        .Any();

                    if (exists)
                        continue;

                    var supplier = new AribaSupplierName
                    {
                        Id = Guid.NewGuid(),
                        DomainId = domainId,
                        SupplierName = supplierDto.SupplierName,
                        SupplierId = supplierDto.SupplierId
                    };

                    _repositoryWrapper.AribaSupplierName.Create(supplier);
                }
            }

            _repositoryWrapper.Save();
            _logger.LogInfo("Venue or Supplier created successfully");
        }

        public async Task<object> GetVenueSupplierAsync(
        Guid domainId,
        GetVenueSupplierRequestDto dto)
        {
            _logger.LogInfo($"Fetching {dto.Type} for domain {domainId}");

            if (string.IsNullOrWhiteSpace(dto.Type))
                throw new PreConditionFailedCustomException("Type required", "Type cannot be empty");

            if (dto.Type == Common.MASTER_VENUE_TYPE)
            {
                var query = _repositoryWrapper.AribaVenueListMapping
                    .FindByCondition(v =>
                        v.DomainId == domainId &&
                        v.IsActive &&
                        (string.IsNullOrEmpty(dto.SearchTerm) ||
                         v.VenueName.Contains(dto.SearchTerm) ||
                         v.CompanyCode.Contains(dto.SearchTerm)))
                    .OrderByDescending(v => v.DateUpdated);

                return query
                    .Skip(dto.Index)
                    .Take(dto.Limit)
                    .Select(v => new VenueDto
                    {
                        Id = v.Id,
                        DomainId = v.DomainId,
                        VenueName = v.VenueName,
                        CompanyCode = v.CompanyCode,
                        CompanyName = v.CompanyName,
                        CostCenter = v.CostCenter,
                        GLAccount = v.GLAccount,
                        ShipToPlant = v.ShipToPlant,
                        PurchaseOrg = v.PurchaseOrg,
                        PurchaseGroup = v.PurchaseGroup,
                        BillTo = v.BillTo
                    })
                    .ToList();
            }

            if (dto.Type == Common.MASTER_SUPPLIER_TYPE)
            {
                var query = _repositoryWrapper.AribaSupplierName
                    .FindByCondition(s =>
                        s.DomainId == domainId &&
                        s.IsActive &&
                        (string.IsNullOrEmpty(dto.SearchTerm) ||
                         s.SupplierName.Contains(dto.SearchTerm) ||
                         s.SupplierId.Contains(dto.SearchTerm)))
                    .OrderByDescending(s => s.DateUpdated);

                return query
                    .Skip(dto.Index)
                    .Take(dto.Limit)
                    .Select(s => new MasterSupplierDto
                    {
                        Id = s.Id,
                        DomainId = s.DomainId,
                        SupplierName = s.SupplierName,
                        SupplierId = s.SupplierId
                    })
                    .ToList();
            }

            throw new PreConditionFailedCustomException("Invalid Type", "Type must be VENUE or SUPPLIER");
        }

        public async Task<bool> UpdateVenueSupplierAsync(
     Guid domainId,
     UpdateVenueSupplierDto dto)
        {
            _logger.LogInfo($"Updating {dto.Type} with identifier {dto.Identifier}");

            if (dto.Type == Common.MASTER_VENUE_TYPE)
            {
                var venue = _repositoryWrapper.AribaVenueListMapping
                    .FindByCondition(v =>
                        v.DomainId == domainId &&
                        v.VenueName.ToLower() == dto.Identifier.ToLower() &&
                        v.IsActive)
                    .FirstOrDefault();

                if (venue == null)
                    throw new NotFoundCustomException("Venue not found", dto.Identifier);

                var v = dto.Venue!;
                venue.VenueName = v.VenueName ?? venue.VenueName;
                venue.CompanyCode = v.CompanyCode ?? venue.CompanyCode;
                venue.CompanyName = v.CompanyName ?? venue.CompanyName;
                venue.CostCenter = v.CostCenter ?? venue.CostCenter;
                venue.GLAccount = v.GLAccount ?? venue.GLAccount;
                venue.ShipToPlant = v.ShipToPlant ?? venue.ShipToPlant;
                venue.PurchaseOrg = v.PurchaseOrg ?? venue.PurchaseOrg;
                venue.PurchaseGroup = v.PurchaseGroup ?? venue.PurchaseGroup;
                venue.BillTo = v.BillTo ?? venue.BillTo;

                _repositoryWrapper.AribaVenueListMapping.Update(venue);
                _repositoryWrapper.Save();
                return true;
            }

            if (dto.Type == Common.MASTER_SUPPLIER_TYPE)
            {
                var supplier = _repositoryWrapper.AribaSupplierName
                    .FindByCondition(s =>
                        s.DomainId == domainId &&
                        s.SupplierId == dto.Identifier &&
                        s.IsActive)
                    .FirstOrDefault();

                if (supplier == null)
                    throw new NotFoundCustomException("Supplier not found", dto.Identifier);

                supplier.SupplierName = dto.Supplier!.SupplierName ?? supplier.SupplierName;
                supplier.SupplierId = dto.Supplier.SupplierId ?? supplier.SupplierId;

                _repositoryWrapper.AribaSupplierName.Update(supplier);
                _repositoryWrapper.Save();
                return true;
            }

            throw new PreConditionFailedCustomException("Invalid Type", "Type must be VENUE or SUPPLIER");
        }

        public async Task<bool> DeleteVenueSupplierAsync(
      Guid domainId,
      string type,
      string identifier)
        {
            _logger.LogInfo($"Deleting {type} with identifier {identifier}");

            if (type == Common.MASTER_VENUE_TYPE)
            {
                var venue = _repositoryWrapper.AribaVenueListMapping
                    .FindByCondition(v =>
                        v.DomainId == domainId &&
                        v.VenueName.ToLower() == identifier.ToLower() &&
                        v.IsActive)
                    .FirstOrDefault();

                if (venue == null)
                    throw new NotFoundCustomException("Venue not found", identifier);

                venue.IsActive = false;
                _repositoryWrapper.AribaVenueListMapping.Update(venue);
                _repositoryWrapper.Save();
                return true;
            }

            if (type == Common.MASTER_SUPPLIER_TYPE)
            {
                var supplier = _repositoryWrapper.AribaSupplierName
                    .FindByCondition(s =>
                        s.DomainId == domainId &&
                        s.SupplierId == identifier &&
                        s.IsActive)
                    .FirstOrDefault();

                if (supplier == null)
                    throw new NotFoundCustomException("Supplier not found", identifier);

                supplier.IsActive = false;
                _repositoryWrapper.AribaSupplierName.Update(supplier);
                _repositoryWrapper.Save();
                return true;
            }

            throw new PreConditionFailedCustomException("Invalid Type", "Type must be VENUE or SUPPLIER");
        }


    }
}

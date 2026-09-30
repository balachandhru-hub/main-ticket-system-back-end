using Contracts;
using Contracts.IRepository;
using Contracts.IServices;
using Dto;
using Entities.Models;
using System;
using System.Threading.Tasks;
using Entities.Common;
using System.Linq;
using System.Collections.Generic;
using ExceptionHandler;


namespace Services
{
    public class AdditionalQueryService : IAdditionalQueryService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerManager _logger;

        public AdditionalQueryService(IRepositoryWrapper repositoryWrapper, ILoggerManager logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
        }

        public async Task<Guid> CreateAdditionalQueryAsync(AdditionalQueryDto dto, Guid userId)

        {
            _logger.LogDebug("Start: CreateAdditionalQueryAsync");
            _logger.LogDebug($"Invoked by user: {userId}");
            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.Name) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Phone) ||
                string.IsNullOrWhiteSpace(dto.Organization) ||
                string.IsNullOrWhiteSpace(dto.Message))
            {
                _logger.LogError("CreateAdditionalQueryAsync validation failed: required fields missing.");
                throw new PreConditionFailedCustomException(
                    "Required fields are missing.",
                    "Name, Email, Phone, Organization, and Message are all required.");
            }

            AdditionalQuery additionalQuery = new AdditionalQuery
            {
                Id = Guid.NewGuid(),
                Name = dto.Name.Trim(),
                Email = dto.Email.Trim(),
                Phone = dto.Phone.Trim(),
                Organization = dto.Organization.Trim(),
                Message = dto.Message.Trim(),
                Status = Common.INITIAL_STATUS_FOR_ADDITIONAL_QUERY
            };
            _logger.LogDebug($"Creating AdditionalQuery record: {additionalQuery.Id}");
            await _repositoryWrapper.AdditionalQuery.CreateAsync(additionalQuery);
            await _repositoryWrapper.SaveAsync();

            _logger.LogInfo($"Additional query created successfully with ID: {additionalQuery.Id}");
            _logger.LogDebug("End: CreateAdditionalQueryAsync");

            return additionalQuery.Id;
        }

        public async Task UpdateStatusAsync(Guid id, string status, Guid userId)
        {
            _logger.LogDebug($"Start: UpdateStatusAsync for id={id}");

            if (id == Guid.Empty)
            {
                _logger.LogError("Invalid query ID provided for status update.");
                throw new BadRequestCustomException("Invalid query ID.", "The provided query ID is empty or invalid.");
            }
            if (string.IsNullOrWhiteSpace(status))
            {
                _logger.LogError("Status is required for updating additional query.");
                throw new PreConditionFailedCustomException("Status is required.", "Status value must be provided.");
            }

            string normalizedStatus = status.Trim().ToUpperInvariant();

            if (normalizedStatus != Common.INITIAL_STATUS_FOR_ADDITIONAL_QUERY &&
                normalizedStatus != Common.STATUS_IN_PROGRESS &&
                normalizedStatus != Common.STATUS_COMPLETED)
            {
                _logger.LogError($"Invalid status value '{status}' provided for additional query status update.");
                throw new BadRequestCustomException("Invalid status.", $"Invalid status: {status}. Allowed: OPEN, IN_PROGRESS, RESOLVED.");
            }

            AdditionalQuery query = await _repositoryWrapper.AdditionalQuery.FindFirstByConditionAsync(
                q => q.Id == id && q.IsActive);

            if (query == null)
            {
                _logger.LogError($"AdditionalQuery with ID {id} not found for status update.");
                throw new NotFoundCustomException("AdditionalQuery not found.", $"No AdditionalQuery found with ID {id}.");
            }

            query.Status = normalizedStatus;
            query.UpdatedBy = userId;
            query.DateUpdated = DateTime.UtcNow;

            _repositoryWrapper.AdditionalQuery.Update(query);
            await _repositoryWrapper.SaveAsync();

            _logger.LogInfo($"Additional query status updated to {normalizedStatus} for ID: {id}");
            
        }

        public async Task<List<AdditionalQueryListDto>> GetAdditionalQueriesAsync(AdditionalQueryFilterDto filter)
        {
            if (filter == null)
                filter = new AdditionalQueryFilterDto();
            _logger.LogDebug("Start: GetAdditionalQueriesAsync");
            _logger.LogInfo("Fetching additional queries with filters.");
             IQueryable<AdditionalQuery> query = _repositoryWrapper.AdditionalQuery.FindByConditionAsync(q => q.IsActive);    
            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                string normalizedStatus = filter.Status.Trim().ToUpperInvariant();
                query = query.Where(q => q.Status == normalizedStatus);
            }

            if (filter.FromDate.HasValue)
            {
                query = query.Where(q => q.DateCreated >= filter.FromDate.Value);
            }

            if (filter.ToDate.HasValue)
            {
                DateTime toDateEnd = filter.ToDate.Value.AddDays(1);
                query = query.Where(q => q.DateCreated < toDateEnd);
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
            {
                string searchTerm = filter.SearchQuery.Trim().ToLower();
                query = query.Where(q => q.Organization.ToLower().Contains(searchTerm));
            }

            if (filter.Index.HasValue && filter.Index.Value > 0)
            {
                query = query.Skip(filter.Index.Value);
            }

            if (filter.Limit.HasValue && filter.Limit.Value > 0)
            {
                query = query.Take(filter.Limit.Value);
            }
  
            List<AdditionalQueryListDto> result = query.Select(q => new AdditionalQueryListDto
            {
                Id = q.Id,
                Name = q.Name,
                Email = q.Email,
                Phone = q.Phone,
                Organization = q.Organization,
                Message = q.Message,
                Status = q.Status,
                DateCreated = q.DateCreated
            }).ToList();

            _logger.LogInfo($"Fetched {result.Count} additional queries.");
            return result;
        }

    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using Contracts.IServices;
using Contracts.IRepository;
using Entities.Models;
using Contracts;
using Microsoft.EntityFrameworkCore;
using Dto;
using ExceptionHandler;

namespace Services
{
    public class LinkService : ILinkService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerManager _logger;

        public LinkService(IRepositoryWrapper repositoryWrapper, ILoggerManager logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
        }

        public async Task<LinkDto> GetLinkByTypeAsync(string type)
        {
            _logger.LogInfo($"Start: Fetching link by type: {type}");

            if (string.IsNullOrWhiteSpace(type))
            {
                _logger.LogError("Type parameter is empty or null");
                throw new PreConditionFailedCustomException("Link type is required.", "Link type must be provided.");
            }

            _logger.LogDebug($"Querying database for link with type: {type}");
            Link link = await _repositoryWrapper.Link
                .FindByCondition(l => l.IsActive && l.Type.ToLower() == type.ToLower() && !string.IsNullOrEmpty(l.LinkUrl))
                .FirstOrDefaultAsync();

            if (link == null || string.IsNullOrWhiteSpace(link.LinkUrl))
            {
                _logger.LogError($"Link not found or empty URL for type: {type}");
                return null;
            }

             link.LinkUrl = link.LinkUrl!.Trim();
            _logger.LogDebug($"URL trimmed for link id: {link.Id}");

            _logger.LogInfo($"End: Link fetched successfully for type: {type}, Link ID: {link.Id}");

            return new LinkDto
            {
                Id = link.Id,
                LinkUrl = link.LinkUrl,
                Type = link.Type
            };
        }

    }
}

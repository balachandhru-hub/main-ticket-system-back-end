using Dto;
using Entities.Models;
using Microsoft.Extensions.Caching.Memory;
using Contracts;
using Contracts.IRepository;
using Contracts.IServices;
using ExceptionHandler;
using AutoMapper;
using Entities.Common;


namespace Services
{
    /// <summary>
    /// Represents the class for MetaData Service
    /// </summary>
    public class MetadataService : IMetadataService
    {
        private readonly IRepositoryWrapper _repoWrapper;
        private readonly IMemoryCache _cache;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;

        /// <summary>
        /// Constructor for injecting the mapper,logger, repoWrapper and cache
        /// </summary>
        /// <param name="repoWrapper">The RepoWrapper</param>
        /// <param name="cache">Cache</param>
        /// <param name="logger">Logger</param>
        public MetadataService(
            IRepositoryWrapper repoWrapper,
            IMemoryCache cache,
            ILoggerManager logger,
            IMapper mapper
        )
        {
            _repoWrapper = repoWrapper;
            _cache = cache;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves the reference term ID corresponding to the specified reference term key.
        /// </summary>
        /// <param name="refTermKey">The key of the reference term.</param>
        /// <returns>The ID of the reference term.</returns>
        public Guid GetRefTermIdByRefTermKey(string refTermKey)
        {
            string cacheKey = $"RefTermKey:{refTermKey}";
            _logger.LogInfo($"Fetching the refTermId for the key {refTermKey}");
            Guid cachedRefTermId = _cache.Get<Guid>(cacheKey);

            if (!cachedRefTermId.Equals(Guid.Empty))
            {
                _logger.LogInfo($"RefTermId is fetched from the Memory cache for the key {refTermKey}");
                return cachedRefTermId;
            }

            _logger.LogInfo($"RefTermId is not found in the Memory cache for the key {refTermKey}");
            Metadata refTerm = _repoWrapper.Metadata.FindFirstByCondition(rt => rt.IsActive && rt.Key.ToLower().Equals(refTermKey.ToLower()));

            if (refTerm == null)
            {
                _logger.LogError($"RefTerm is not found in the database for the key {refTermKey}");
                throw new NotFoundCustomException($"RefTerm is not found in the database for the key {refTermKey}", $"RefTerm is not found in the database for the key {refTermKey}");
            }
            _logger.LogInfo($"RefTermId is fetched from the database for the key {refTermKey}");
            _cache.Set(cacheKey, refTerm.Id);
            return refTerm.Id;
        }

        /// <summary>
        /// Retrieves a list of reference term by the reference type.
        /// </summary>
        /// <param name="type">The list of reference type key.</param>
        /// <returns>A list of reference term </returns>
        public List<RefTermDto> GetRefTermListByRefSetKey(List<string> type)
        {
            _logger.LogInfo($"Fetching the list of reference term based on the given type : {type}");
            List<RefTermDto> result = _mapper.Map<List<RefTermDto>>(_repoWrapper.Metadata.FindByCondition(x => x.IsActive && type.Contains(x.Type)).ToList());
            if (!result.Any())
            {
                _logger.LogError($"No ref terms for ref set key found: {type}");
                throw new NotFoundCustomException($"No ref terms for ref set type key found: {type}", $"No ref terms for ref set type key found: {type}");
            }
            _logger.LogInfo($"Fetched the list of reference term based on the given type: {type}");
            return result;
        }

        /// <summary>
        /// Retrieves a reference term key by the reference term id.
        /// </summary>
        /// <param name="id">The reference term id.</param>
        /// <returns> Returns the reference term key</returns>
        public string GetRefTermKeyById(Guid id)
        {
            _logger.LogInfo($"Fetching the reference term key based on the given id : {id}");
            Metadata? result = _repoWrapper.Metadata.FindFirstByCondition(x => x.IsActive && x.Id == id);
            if (result == null)
            {
                _logger.LogError($"No ref terms key for ref term id found: {id}");
                throw new NotFoundCustomException($"No ref terms key for ref term id found: {id}", $"No ref terms key for ref term id found: {id}");
            }
            _logger.LogInfo($"Fetching the reference term key based on the given id : {id}");
            return result.Key;
        }

        /// <summary>
        /// Retrieves a List of Available Roles for Admin.
        /// </summary>
        /// <returns> Returns the List of Roles</returns>
        public List<RoleDetailDto> GetRoleList()
        {
            _logger.LogInfo("Fetching the List of Roles");
            List<RoleDetailDto> result = _mapper.Map<List<RoleDetailDto>>(_repoWrapper.Role.FindByCondition(x => x.IsActive).ToList());
            _logger.LogInfo("Fetched the List of Roles");
            return result;
        }
        /// <summary>
        /// Creates a new RefTerm (metadata entry) for a module.
        /// </summary>
        /// <param name="dto">RefTerm data (description)</param>
        /// <returns>The ID of the created RefTerm.</returns>
        private string FormatDescriptionToKey(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new PreConditionFailedCustomException("Description cannot be empty or null.", "Description is required.");
            return description.Trim().Replace(" ", "_").ToUpper();
        }

        public Guid CreateRefTerm(CreateRefTermDto dto)
        {
            _logger.LogInfo($"Creating RefTerm for module: {dto.Description}");

            string key = FormatDescriptionToKey(dto.Description);
            string type = dto.Type.Trim().ToUpper();

            Metadata newRefTerm = new Metadata
            {
                Id = Guid.NewGuid(),
                Key = key,
                Type = type,
                Description = dto.Description,
                IsActive = true
            };

            _repoWrapper.Metadata.Create(newRefTerm);
            _repoWrapper.Save();

            _logger.LogInfo($"Created RefTerm with Id: {newRefTerm.Id}");
            return newRefTerm.Id;
        }
        /// <summary>
        /// Retrieves a list of all modules (active and inactive).
        /// </summary>
        /// <returns>List of ModulePreviewDto</returns>
        public List<ModulePreviewDto> GetAllModules()
        {
            _logger.LogInfo("Fetching all ARIBA_MODEL modules (active and inactive)");

            List<Metadata> moduleEntities = _repoWrapper.Metadata
                .FindByCondition(x => x.Type == Common.ARIBA_MODEL)
                .ToList();

            if (moduleEntities.Count == 0)
            {
                _logger.LogError("No ARIBA_MODEL modules found");
                throw new NotFoundCustomException("No ARIBA_MODEL modules found", "No ARIBA_MODEL modules found in the database.");
            }

            List<ModulePreviewDto> result = new List<ModulePreviewDto>();
            foreach (Metadata module in moduleEntities)
            {
                ModulePreviewDto dto = new ModulePreviewDto
                {
                    Id = module.Id,
                    Key = module.Key,
                    Type = module.Type,
                    Description = module.Description,
                    IsActive = module.IsActive
                };
                result.Add(dto);
            }

            _logger.LogInfo("Fetched all ARIBA_MODEL modules");
            return result;
        }
        /// <summary>
        /// Updates the status of a Ariba Model.
        /// /// </summary>
        public void UpdateModelStatus(UpdateModelStatusDto dto)
        {
            _logger.LogInfo($"Updating IsActive status for module Id: {dto.Id}");

            Metadata module = _repoWrapper.Metadata.FindFirstByCondition(x => x.Id == dto.Id && x.Type == Common.ARIBA_MODEL);

            if (module == null)
            {
                _logger.LogError($"Ariba Model not found for Id: {dto.Id}");
                throw new NotFoundCustomException("Ariba Model not found", $"No ARIBA_MODEL module found for Id: {dto.Id}");
            }

            module.IsActive = dto.IsActive;
            _repoWrapper.Metadata.Update(module);
            _repoWrapper.Save();

            _logger.LogInfo($"Updated IsActive status for model Id: {dto.Id} to {dto.IsActive}");
        }

    }
}

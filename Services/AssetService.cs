using Contracts.IRepository;
using Contracts.IServices;
using Dto;
using Entities.Models;
using Contracts;
using ExceptionHandler;
using Entities.Common;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Collections.Generic;

namespace Services
{
    /// <summary>
    /// Service for managing asset.
    /// </summary>
    public class AssetService : IAssetService
    {
        private readonly IRepositoryWrapper _repoWrapper;

        private readonly ILoggerManager _logger;

        private readonly IConfiguration _configuration;
        private readonly IMetadataService _metadataService;

        /// <summary>
        /// Contructor used for injecting dependencies.
        /// </summary>
        /// <param name="repoWrapper">The repository wrapper for database operations.</param>
        /// <param name="logger">The logger for logging messages.</param>
        /// <param name="metaData">Metadata.</param>
        /// <param name="configuration">The configuration operations.</param>
        public AssetService(
            IRepositoryWrapper repoWrapper,
            ILoggerManager logger,
            IConfiguration configuration,
            IMetadataService metadataService
        )
        {
            _repoWrapper = repoWrapper;
            _logger = logger;
            _configuration = configuration;
            _metadataService = metadataService;
        }


        /// <summary>
        /// Uploads a document and creates an entry in the database for the uploaded asset.
        /// </summary>
        /// <param name="assetUploadDto">The DTO containing information about the document to be uploaded.</param>
        /// <returns>The unique identifier (GUID) of the uploaded asset.</returns>
        public Guid UploadDocument(AssetUploadDto assetUploadDto)
        {
            _logger.LogInfo("Uploading document...");
            Guid assetId = Guid.NewGuid();
            if (assetUploadDto.IsSingletonAsset)
            {
                Asset oldAsset = _repoWrapper.Asset.FindFirstByCondition(x => x.IsActive && x.EntityId == assetUploadDto.EntityId && x.AssetType == _metadataService.GetRefTermIdByRefTermKey(assetUploadDto.AssetType));

                if (oldAsset != null)
                {
                    oldAsset.IsActive = false;
                    _repoWrapper.Asset.Update(oldAsset);
                    _repoWrapper.Save();

                    string oldFilePath = Path.Combine($"{_configuration[Common.BASE_FOLDER_PATH]}", oldAsset.Id.ToString());
                    if (File.Exists(oldFilePath))
                    {
                        File.Delete(oldFilePath);
                        _logger.LogInfo($"Deleted The existing Asset with Id : {oldAsset.Id}");
                    }
                }
            }

            string filePath = Path.Combine($"{_configuration[Common.BASE_FOLDER_PATH]}", assetId.ToString());

            string fileExtension = Path.GetExtension(assetUploadDto.FileName!).TrimStart('.');
            string fullFilePath = Path.Combine(filePath, assetUploadDto.FileName!);

            // Log the directory and file path
            _logger.LogInfo($"Directory: {filePath}");
            _logger.LogInfo($"Full file path: {fullFilePath}");

            // Check if the directory exists, and create it if it doesn't
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
                _logger.LogInfo($"Directory created: {filePath}");
            }
            else
            {
                _logger.LogInfo($"Directory already exists: {filePath}");
            }

            // Write the file and log the action
            File.WriteAllBytes(fullFilePath, assetUploadDto.FileBytes!);
            _logger.LogInfo($"File created: {fullFilePath}");


            _logger.LogInfo($"Adding Assets for the {assetUploadDto.FileName}");
            Asset asset = new Asset
            {
                Id = assetId,
                AssetType = assetUploadDto.AssetType == null ? null : _metadataService.GetRefTermIdByRefTermKey(assetUploadDto.AssetType),
                AssetName = assetUploadDto.FileName,
                EntityType = assetUploadDto.EntityType == null ? null : _metadataService.GetRefTermIdByRefTermKey(assetUploadDto.EntityType),
                EntityId = assetUploadDto.EntityId,
                FileName = assetUploadDto.FileName!,
                FileType = _metadataService.GetRefTermIdByRefTermKey(fileExtension)

            };
            _repoWrapper.Asset.Create(asset);
            _repoWrapper.Save();
            _logger.LogInfo($"Asset with ID {assetId} uploaded successfully.");
            return assetId;
        }

        /// <summary>
        /// Retrieves the document associated with the specified asset ID.
        /// </summary>
        /// <param name="assetId">The ID of the asset whose document is to be retrieved.</param>
        /// <returns>An AssetDownloadDto object containing the document information.</returns>
        public AssetDownloadDto GetDocument(Guid assetId)
        {
            _logger.LogInfo($"Retrieving document for asset Id: {assetId}");

            Asset asset = _repoWrapper.Asset.FindFirstByCondition(x => x.Id == assetId);

            if (asset == null)
            {
                _logger.LogError($"Asset with ID {assetId} not found.");
                throw new NotFoundCustomException("Asset not found", $"The asset with ID {assetId} was not found.");
            }
            string fileName = asset.FileName;
            string filePath = Path.Combine($"{_configuration[Common.BASE_FOLDER_PATH]}", assetId.ToString(), fileName);

            if (!File.Exists(filePath))
            {
                _logger.LogError($"File for asset with ID {assetId} not found at path: {filePath}");
                throw new NotFoundCustomException("File not found", $"The file for asset with ID {assetId} was not found.");
            }
            byte[] encryptedFileBytes = File.ReadAllBytes(filePath);

            AssetDownloadDto assetDownloadDto = new AssetDownloadDto
            {
                AssetId = assetId,
                FileName = asset.FileName,
                ContentType = _metadataService.GetRefTermKeyById(asset.FileType),
                FileBytes = encryptedFileBytes
            };
            _logger.LogInfo($"Document retrieved successfully for asset Id: {assetId}");
            return assetDownloadDto;
        }

        /// <summary>
        /// Retrieves documents for multiple asset IDs.
        /// </summary>
        /// <param name="assetIds">The list of asset IDs to retrieve.</param>
        /// <returns>List of AssetDownloadListDto for found assets.</returns>
        public List<AssetDownloadListDto> GetDocumentsList(List<Guid> assetIds)
        {
            _logger.LogInfo($"Retrieving documents for asset Ids: {string.Join(',', assetIds ?? new List<Guid>())}");

            if (assetIds == null || assetIds.Count == 0)
            {
                _logger.LogError($"Asset with ID {assetIds} not found.");
                throw new PreConditionFailedCustomException("Invalid asset ids", "No asset IDs provided.");
            }

            List<Asset> assets = _repoWrapper.Asset.FindByCondition(a => assetIds.Contains(a.Id)).ToList();

            List<AssetDownloadListDto> results = new List<AssetDownloadListDto>();

            foreach (Asset asset in assets)
            {
                string fileName = asset.FileName;
                string filePath = Path.Combine($"{_configuration[Common.BASE_FOLDER_PATH]}", asset.Id.ToString(), fileName);

                if (!File.Exists(filePath))
                {
                    _logger.LogError($"File for asset with ID {asset.Id} not found at path: {filePath}");
                    continue;
                }

                byte[] fileBytes = File.ReadAllBytes(filePath);

                AssetDownloadListDto dto = new AssetDownloadListDto
                {
                    AssetId = asset.Id,
                    FileName = asset.FileName,
                    ContentType = _metadataService.GetRefTermKeyById(asset.FileType),
                    FileBytes = fileBytes
                };

                results.Add(dto);
            }

            _logger.LogInfo($"Retrieved documents for requested asset ids. Count: {results.Count}");
            return results;
        }

        /// <summary>
        /// Retrieves the details of assets associated with the specified entityId ID.
        /// </summary>
        /// <param name="personId">The ID of the person whose asset details are to be retrieved.</param>
        /// <returns>A list of AssetDto objects containing the details of the person's assets.</returns>
        public List<AssetDto> GetAssetDetail(Guid entityType, Guid entityId, List<Guid>? assetType)
        {
            _logger.LogInfo($"Fetching the asset details of the entity with Id : {entityId}");
            List<AssetDto> assets = _repoWrapper.Asset.FindByCondition(x => x.IsActive && x.EntityType == entityType && x.EntityId == entityId)
                                    .Select(a => new AssetDto
                                    {
                                        Id = a.Id,
                                        AssetName = a.AssetName,
                                        AssetType = (a.AssetType != null || a.AssetType != Guid.Empty) ? _metadataService.GetRefTermKeyById((Guid)a.AssetType) : null,
                                        FileName = a.FileName,
                                        FileType = (a.FileType != null || a.FileType != Guid.Empty) ? _metadataService.GetRefTermKeyById(a.FileType) : null
                                    }).ToList();
            _logger.LogInfo($"Retrieved the asset details of the entity with Id: {entityId}");
            return assets;
        }
    }
}
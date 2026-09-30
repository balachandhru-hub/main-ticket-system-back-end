
using Contracts.IServices;
using Contracts.IRepository;
using Dto;
using ExceptionHandler;
using System;
using Entities.Models;
using Contracts;
using System.Transactions;
using Entities.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;   // ← THIS LINE
namespace Services
{
    /// <summary>
    /// Person Service
    /// </summary>
    public class UploadAribaInvoiceService : IUploadAribaInvoiceService
    {
        private readonly IRepositoryWrapper _repoWrapper;
        private readonly ILoggerManager _logger;
        private readonly IUserService _userService;
        private readonly IUserRoleMappingService _userRoleMappingService;
        private readonly IMetadataService _metadataService;
        private readonly IAssetService _assetService;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public UploadAribaInvoiceService(
            IRepositoryWrapper repositoryWrapper,
            ILoggerManager logger,
            IUserService userService,
            IUserRoleMappingService userRoleMappingService,
            IMetadataService metadataService,
            IAssetService assetService, IConfiguration configuration, IEmailService emailService)
        {
            _repoWrapper = repositoryWrapper;
            _logger = logger;
            _userService = userService;
            _userRoleMappingService = userRoleMappingService;
            _metadataService = metadataService;
            _assetService = assetService;
            _configuration = configuration;
            _emailService = emailService;
        }



   public Guid UploadInvoiceAsset(string approver, string number, AssetUploadDto assetUploadDto, string type)
    {
        try
        {
            _logger.LogInfo("Starting UploadInvoiceAsset method...");

            var httpContext = new HttpContextAccessor().HttpContext;
            var file = httpContext?.Request.Form.Files["FileBytes"];

                if (file == null || file.Length == 0)
                {
                    _logger.LogError("File is missing or empty.");
                    throw new PreConditionFailedCustomException("File is missing or empty. Please upload a valid file.", "File bytes are required in the request.");
                }

            // Read file bytes
            using (var stream = file.OpenReadStream())
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                assetUploadDto.FileBytes = ms.ToArray();
            }

            assetUploadDto.FileName ??= file.FileName;
            assetUploadDto.ContentType ??= file.ContentType;

        
            if (string.IsNullOrWhiteSpace(type))
            {
                 throw new PreConditionFailedCustomException("Upload type is required (INVOICE or PR).", "Request parameter 'type' is missing or empty.");
            }

            string normalizedType = type.Trim().ToUpper();

            if (normalizedType != Common.TYPE_INVOICE_UPLOAD &&
                normalizedType != Common.TYPE_PR_UPLOAD && normalizedType != Common.TYPE_CONTRACT_UPLOAD)
            {
                _logger.LogError($"Invalid type received: {type}");
                throw new PreConditionFailedCustomException($"Invalid type '{type}'. Allowed: {Common.TYPE_INVOICE_UPLOAD}, {Common.TYPE_PR_UPLOAD},{Common.TYPE_CONTRACT_UPLOAD}.", "Unsupported upload type provided.");
            }

            assetUploadDto.AssetType = normalizedType;
            assetUploadDto.EntityType = normalizedType;

            if (normalizedType == Common.TYPE_INVOICE_UPLOAD)
            {
                _logger.LogInfo($"Processing Invoice Upload: Approver={approver}, InvoiceNumber={number}");

                var existingRecord = _repoWrapper.AribaInvoiceUpload
                    .FindFirstByCondition(x => x.Approver == approver && x.InvoiceNumber == number);

                if (existingRecord != null)
                {
                    
                    if (normalizedType != Common.TYPE_INVOICE_UPLOAD)
                    {
                        throw new PreConditionFailedCustomException($"Type mismatch: Number '{number}' is not an Invoice.", $"Expected invoice type but received '{type}'.");
                    }

                    _logger.LogInfo($"Existing invoice record found for Approver={approver}, InvoiceNumber={number}");

                    assetUploadDto.EntityId = existingRecord.Id;
                    Guid newAssetId = _assetService.UploadDocument(assetUploadDto);

                    existingRecord.AssetId = newAssetId;
                    _repoWrapper.AribaInvoiceUpload.Update(existingRecord);
                    _repoWrapper.Save();

                    return newAssetId;
                }

                // Create new invoice entity + asset
                Guid newEntityId = Guid.NewGuid();
                assetUploadDto.EntityId = newEntityId;

                Guid assetId = _assetService.UploadDocument(assetUploadDto);

                var newRecord = new AribaInvoiceUpload
                {
                    Id = newEntityId,
                    Approver = approver,
                    InvoiceNumber = number,
                    AssetId = assetId
                };

                _repoWrapper.AribaInvoiceUpload.Create(newRecord);
                _repoWrapper.Save();

                return assetId;
            }
        
            else if (normalizedType == Common.TYPE_PR_UPLOAD)
            {
                _logger.LogInfo($"Processing PR Upload: Approver={approver}, PRNumber={number}");

                var existingRecord = _repoWrapper.AribaPRUpload
                    .FindFirstByCondition(x => x.Approver == approver && x.PRNumber == number);

                if (existingRecord != null)
                {
                    
                    if (normalizedType != Common.TYPE_PR_UPLOAD)
                    {
                        throw new PreConditionFailedCustomException($"Type mismatch: Number '{number}' is not a PR.", $"Expected PR type but received '{type}'.");
                    }

                    _logger.LogInfo($"Existing PR record found for Approver={approver}, PRNumber={number}");

                    assetUploadDto.EntityId = existingRecord.Id;
                    Guid newAssetId = _assetService.UploadDocument(assetUploadDto);

                    existingRecord.AssetId = newAssetId;
                    _repoWrapper.AribaPRUpload.Update(existingRecord);
                    _repoWrapper.Save();

                    return newAssetId;
                }

                // Create new PR entity + asset
                Guid newEntityId = Guid.NewGuid();
                assetUploadDto.EntityId = newEntityId;

                Guid assetId = _assetService.UploadDocument(assetUploadDto);

                var newRecord = new AribaPRUpload
                {
                    Id = newEntityId,
                    Approver = approver,
                    PRNumber = number,
                    AssetId = assetId
                };

                _repoWrapper.AribaPRUpload.Create(newRecord);
                _repoWrapper.Save();

                return assetId;
            }
               else if (normalizedType == Common.TYPE_CONTRACT_UPLOAD)
            {
                _logger.LogInfo($"Processing Contract Upload: Approver={approver}, ContractTaskId={number}");

                var existingRecord = _repoWrapper.AribaContractUpload
                    .FindFirstByCondition(x => x.Approver == approver && x.ContractTaskId == number);

                if (existingRecord != null)
                {
                    
                    if (normalizedType != Common.TYPE_CONTRACT_UPLOAD)
                    {
                        throw new PreConditionFailedCustomException($"Type mismatch: Number '{number}' is not a CONTRACt.", $"Expected Contract type but received '{type}'.");
                    }

                    _logger.LogInfo($"Existing Contract record found for Approver={approver}, ContractTaskId={number}");

                    assetUploadDto.EntityId = existingRecord.Id;
                    Guid newAssetId = _assetService.UploadDocument(assetUploadDto);

                    existingRecord.AssetId = newAssetId;
                    _repoWrapper.AribaContractUpload.Update(existingRecord);
                    _repoWrapper.Save();

                    return newAssetId;
                }

                // Create new PR entity + asset
                Guid newEntityId = Guid.NewGuid();
                assetUploadDto.EntityId = newEntityId;

                Guid assetId = _assetService.UploadDocument(assetUploadDto);

                var newRecord = new AribaContractUpload
                {
                    Id = newEntityId,
                    Approver = approver,
                    ContractTaskId = number,
                    AssetId = assetId
                };

                _repoWrapper.AribaContractUpload.Create(newRecord);
                _repoWrapper.Save();

                return assetId;
            }
    
            else
            {
                _logger.LogError($"Invalid upload type provided: {type}");
                throw new PreConditionFailedCustomException("Invalid upload type.", $"Unsupported upload type: '{type}'.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error uploading asset: {ex.Message}");
            throw;
        }
    }




        public AssetDownloadDto GetUploadedAsset(string type, string approver, string number)
        {
            _logger.LogInfo("Starting GetUploadedAsset method...");

            string normalizedType = type.Trim().ToUpper();
            Guid? assetId = null;

            if (normalizedType == Common.TYPE_INVOICE_UPLOAD)
            {
                var record = _repoWrapper.AribaInvoiceUpload
                    .FindFirstByCondition(x => x.IsActive
                        && x.Approver == approver
                        && x.InvoiceNumber == number);

                if (record == null)
                    throw new NotFoundCustomException(
                        $"Invoice not found for approver '{approver}' and number '{number}'",
                        $"No active invoice record found with Approver = '{approver}' and InvoiceNumber = '{number}'");

                assetId = record.AssetId;
            }
            else if (normalizedType == Common.TYPE_PR_UPLOAD)
            {
                var prRecord = _repoWrapper.AribaPRUpload
                    .FindFirstByCondition(x => x.IsActive
                        && x.Approver == approver
                        && x.PRNumber == number);
                     if (prRecord == null)
                    throw new NotFoundCustomException(
                        $"PR not found for approver '{approver}' and PR number '{number}'",
                        $"No active PR record found with Approver = '{approver}' and PRNumber = '{number}'");

                assetId = prRecord.AssetId;
            }
              else if (normalizedType == Common.TYPE_CONTRACT_UPLOAD)
            {
                var prRecord = _repoWrapper.AribaContractUpload
                    .FindFirstByCondition(x => x.IsActive
                        && x.Approver == approver
                        && x.ContractTaskId == number);
                     if (prRecord == null)
                    throw new NotFoundCustomException(
                        $"PR not found for approver '{approver}' and Contract number '{number}'",
                        $"No active Contract record found with Approver = '{approver}' and ContractTaskId = '{number}'");

                assetId = prRecord.AssetId;
            }
            else
            {
                throw new BadRequestCustomException(
                    "Invalid type",
                    $"Supported types are: {Common.TYPE_INVOICE_UPLOAD}, {Common.TYPE_PR_UPLOAD},{Common.TYPE_CONTRACT_UPLOAD}. Received: '{type}'");
            }

            if (!assetId.HasValue || assetId == Guid.Empty)
                throw new NotFoundCustomException(
                    "No file attached to this record",
                    $"Record found but AssetId is null or empty (Approver: {approver}, Number: {number})");

            var fileDto = _assetService.GetDocument(assetId.Value);

            if (fileDto == null || fileDto.FileBytes == null || fileDto.FileBytes.Length == 0)
                throw new NotFoundCustomException(
                    "File not found in storage",
                    $"AssetId {assetId} exists in DB but file content is missing or corrupted");

            return fileDto;
        }
    }


    }

using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.Annotations;
using Contracts.IServices;
using Dto;
using Contracts;
using TicketSystemAPI.Attributes;
using TicketSystemAPI.Controllers;

namespace TicketSystemAPI.Controllers
{
    /// <summary>
    /// Controller for managing assets.
    /// </summary>
    [ApiController]
    public class AssetController : BaseController
    {
        private readonly ILoggerManager _logger;
        private readonly IAssetService _assetService;


        /// <summary>
        /// Constructor for Asset Controller.
        /// </summary>
        /// <param name="assetService">The asset service.</param>
        /// <param name="logger">The logger manager.</param>
        public AssetController(ILoggerManager logger, IAssetService assetService)
        {
            _logger = logger;
            _assetService = assetService;
        }

        /// <summary>
        /// Uploads a document.
        /// </summary>
        /// <param name="asset">The details of the document to be uploaded.</param>
        /// <response code="200">File Uploaded successfully</response>
        /// <response code="400">Invalid file.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpPost]
        [Route("api/v1/asset")]
        [ApiAuthorization(Name = "ASSET_CREATE")]
        [SwaggerOperation("UploadDocuments")]
        [SwaggerResponse(statusCode: 201, "File Uploaded successfully", typeof(SuccessResponseDto))]
        [SwaggerResponse(statusCode: 400, "Invalid file.", typeof(ErrorResponseDto))]
        [SwaggerResponse(statusCode: 401, "Unauthorized", typeof(ErrorResponseDto))]
        public IActionResult UploadDocument(AssetUploadDto asset)
        {
            Guid personId = getLoggedUser();

            _logger.LogDebug($"Uploading documents for person with Id : {personId}");
            Guid result = _assetService.UploadDocument(asset);

            _logger.LogDebug($"Document uploaded successfully for the file with Id: {result}");
            return StatusCode(201, new SuccessResponseDto { StatusCode = 201, Message = "Document Uploaded successfully.", Id = result.ToString() });
        }

        /// <summary>
        /// Downloads a document.
        /// </summary>
        /// <param name="assetId">The ID of the document to be downloaded.</param>
        /// <response code="200">File downloaded successfully</response>
        /// <response code="404">File not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet]
        [Route("api/v1/asset/{assetId}")]
        [ApiAuthorization(Name = "ASSET_DOWNLOAD")]
        [SwaggerOperation("GetDocument")]
        [SwaggerResponse(statusCode: 200, "Fetched the File Details", typeof(AssetDownloadDto))]
        [SwaggerResponse(statusCode: 404, "Not Found", typeof(ErrorResponseDto))]
        [SwaggerResponse(statusCode: 401, "Unauthorized", typeof(ErrorResponseDto))]
        public IActionResult GetDocument([FromRoute] Guid assetId)
        {
            _logger.LogDebug($"Downloading document for asset with Id: {assetId}");

            AssetDownloadDto assetDownloadDto = _assetService.GetDocument(assetId);
          
            _logger.LogDebug($"Document downloaded successfully for asset with Id: {assetId}");

            return Ok(assetDownloadDto);
        }

        /// <summary>
        /// Downloads multiple documents.
        /// </summary>
        /// <param name="assetIds">List of asset IDs to download.</param>
        /// <response code="200">Files downloaded successfully</response>
        /// <response code="404">Files not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost]
        [Route("api/v1/asset/list")]
        [ApiAuthorization(Name = "ASSET_DOWNLOAD")]
        [SwaggerOperation("GetDocuments")]
        [SwaggerResponse(statusCode: 200, "Fetched the File Details", typeof(List<AssetDownloadListDto>))]
        [SwaggerResponse(statusCode: 404, "Not Found", typeof(ErrorResponseDto))]
        [SwaggerResponse(statusCode: 401, "Unauthorized", typeof(ErrorResponseDto))]
        public IActionResult GetDocuments([FromBody] List<Guid> assetIds)
        {
            _logger.LogDebug($"Downloading documents for assets with Ids: {string.Join(',', assetIds ?? new List<Guid>())}");

            List<AssetDownloadListDto> assets = _assetService.GetDocumentsList(assetIds);
            _logger.LogDebug($"Documents downloaded successfully for assets count: {assets?.Count}");

            return Ok(assets);
        }
    }
}
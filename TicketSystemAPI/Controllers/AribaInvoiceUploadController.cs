using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Contracts;
using Contracts.IServices;
using Dto;
using Entities.Common;
using ExceptionHandler;
using TicketSystemAPI.Attributes;
using Entities.Models;

namespace TicketSystemAPI.Controllers
{
    /// <summary>
    /// Controller for managing Ariba Invoice Uploads
    /// </summary>
    public class AribaInvoiceUploadController : BaseController
    {
        private readonly IUploadAribaInvoiceService _uploadAribaInvoiceService;
        private readonly IAssetService _assetService;
        private readonly ILoggerManager _logger;


        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public AribaInvoiceUploadController(
            IUploadAribaInvoiceService uploadAribaInvoiceService,
            IAssetService assetService,
            ILoggerManager logger)
        {
            _uploadAribaInvoiceService = uploadAribaInvoiceService;
            _assetService = assetService;
            _logger = logger;
        }

        /// <summary>
        /// Upload invoice asset for an approver and invoice number
        /// </summary>
        [HttpPost]
        [Route("api/v1/ariba/invoice/upload/{approver}/{number}/{type}")]
        [ApiAuthorization(Name = "UPLOAD_INVOICE")]
        [SwaggerOperation("UploadInvoiceAsset")]
        [SwaggerResponse(statusCode: 200, type: typeof(SuccessResponseDto), description: "File Upload successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorResponseDto), description: "File Not Found")]
        public IActionResult UploadInvoiceAsset(
          [FromRoute] string approver,
          [FromRoute] string number,
           [FromForm] AssetUploadDto assetUploadDto, [FromRoute] string type)
        {
           _logger.LogDebug($"[UploadInvoiceAsset] START - Approver={approver}, Number={number}, Type={type}");

                var assetId = _uploadAribaInvoiceService.UploadInvoiceAsset(approver, number, assetUploadDto, type);

                _logger.LogDebug($"[UploadInvoiceAsset] SUCCESS - AssetId={assetId}");

                return Ok(assetId);
      
        }

        [HttpGet]
        [Route("api/v1/ariba/download/{type}/{approver}/{number}")]
        [ApiAuthorization(Name = "DOWNLOAD_INVOICE")]
        [SwaggerOperation("DownloadAsset")]
        [SwaggerResponse(statusCode: 200, type: typeof(SuccessResponseDto), description: "File downloaded successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorResponseDto), description: " File Not Found")]
        public IActionResult DownloadAsset(
            [FromRoute] string type,
            [FromRoute] string approver,
            [FromRoute] string number)
        {
                 _logger.LogDebug($"[GetUploadedAsset] START - Type={type}, Approver={approver}, Number={number}");

           
                var fileDto = _uploadAribaInvoiceService.GetUploadedAsset(type, approver, number);
                _logger.LogDebug($"[GetUploadedAsset] SUCCESS - FileName={fileDto.FileName}, Size={fileDto.FileBytes?.Length} bytes");

                return File(
                    fileDto.FileBytes,
                    "application/pdf",
                    fileDto.FileName?.Trim() ?? "download.pdf"
                );
            
        
        }
    }
}
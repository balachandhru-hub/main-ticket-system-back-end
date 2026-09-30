using System;
using System.Collections.Generic;
using Dto;
using Entities.Models;
using Microsoft.AspNetCore.Http;

namespace Contracts.IServices
{
    public interface IAssetService
    {

        /// <summary>
        /// The UploadDocument function in C# returns a unique identifier (Guid) after uploading a
        /// document using the provided AssetUploadDto.
        /// </summary>
        /// <param name="AssetUploadDto">The `AssetUploadDto` parameter likely represents a Data
        /// Transfer Object (DTO) that contains information related to uploading an asset. This DTO may
        /// include details such as the asset file, metadata, permissions, and any other relevant
        /// information needed for the upload process.</param>
        Guid UploadDocument(AssetUploadDto assetUploadDto);

        /// <summary>
        /// Retrieves document download details for the specified asset ID.
        /// </summary>
        /// <param name="assetId">The ID of the asset for which to retrieve download details.</param>
        /// <returns>The document download details.</returns>
        AssetDownloadDto GetDocument(Guid assetId);

        /// <summary>
        /// Retrieves document download details for the specified list of asset IDs.
        /// </summary>
        /// <param name="assetIds">The list of asset IDs to retrieve.</param>
        /// <returns>A list of AssetDownloadListDto objects containing the document information for found assets.</returns>
        List<AssetDownloadListDto> GetDocumentsList(List<Guid> assetIds);
        /// <summary>
        /// Retrieves the details of assets associated with the specified entityId ID.
        /// </summary>
        /// <param name="personId">The ID of the person whose asset details are to be retrieved.</param>
        /// <returns>A list of AssetDto objects containing the details of the person's assets.</returns>
        List<AssetDto> GetAssetDetail(Guid entityType, Guid entityId, List<Guid>? assetType);

    }
}
using Dto;
using Entities.Models;
using System;

namespace Contracts.IServices
{
    /// <summary>
    /// Interface for Upload Ariba Invoice Service
    /// </summary>
    public interface IUploadAribaInvoiceService
    {
        /// <summary>
        /// Upload invoice asset for an approver and invoice number
        /// If record exists, updates the asset ID
        /// If record doesn't exist, creates a new one
        /// </summary>
        /// <param name="approver">The approver name</param>
        /// <param name="invoiceNumber">The invoice number</param>
        /// <param name="assetUploadDto">Asset upload details</param>
        /// <returns>The AribaInvoiceUpload record</returns>
Guid UploadInvoiceAsset(string approver, string number, AssetUploadDto assetUploadDto, string type);
         AssetDownloadDto GetUploadedAsset(string type, string approver, string number);
    }
}

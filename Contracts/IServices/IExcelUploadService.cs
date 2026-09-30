using Dto;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.IServices
{
    public interface IExcelUploadService
    {
        /// <summary>
        /// Uplaod the PO Excel File
        /// </summary>
        /// <param name="excelFile"></param>
        Task<ExcelProcessingResultDto> UploadExcelFileAsync(IFormFile excelFile, Guid domainId);

        /// <summary>
        /// Uplaod the PO MasterExcel File
        /// </summary>
        /// <param name="excelFile"></param>
        Task<ExcelProcessingResultDto> UploadMasterExcelFileAsync(IFormFile excelFile, Guid domainId);

        /// <summary>
        /// Uplaod the Contact Excel File
        /// </summary>
        /// <param name="excelFile"></param>
        Task<ExcelProcessingResultDto> UploadContractExcelFileAsync(IFormFile excelFile, Guid domainId);
        /// <summary>
        /// Uplaod the Contact Excel File
        /// </summary>
        /// <param name="excelFile"></param>
        Task<ExcelProcessingResultDto> UploadCompanyCodeExcelFileAsync(IFormFile excelFile, Guid domainId);
        /// <summary>
        /// Uplaod the Material PR Excel File
        /// </summary>
        /// <param name="excelFile"></param>
        /// <returns></returns>
        Task<ExcelProcessingResultDto> UploadMaterialPRExcelFileAsync(IFormFile excelFile, Guid domainId, Guid userId);
         Task<ExcelProcessingResultDto> UploadBlockUnblockExcelFileAsync(IFormFile excelFile, Guid domainId);
         Task<ExcelProcessingResultDto> UploadServicePRExcelFileAsync(IFormFile excelFile, Guid domainId, Guid userId);
         Task<ExcelProcessingResultDto> UploadSupplierNameExcelFileAsync(IFormFile excelFile, Guid domainId);
         Task<ExcelProcessingResultDto> UploadVenueExcelFileAsync(IFormFile excelFile, Guid domainId);
         /// <summary>
         /// Upload the PO Bot Process Excel File
         /// </summary>
         /// <param name="excelFile"></param>
         Task<ExcelProcessingResultDto> UploadBotProcessExcelFileAsync(IFormFile excelFile, Guid domainId);
         Task<ExcelProcessingResultDto> UploadDraftMaterialPRFileAsync(IFormFile excelFile, Guid domainId,Guid userId);
         Task<ExcelProcessingResultDto> UploadPRBlockUnblockExcelFileAsync(IFormFile excelFile, Guid domainId);
         Task<ExcelProcessingResultDto> UploadCrossChargeExcelFileAsync(IFormFile excelFile, Guid domainId);
         Task<ExcelProcessingResultDto> UploadPublicVenueExcelFileAsync(IFormFile excelFile, Guid domainId);
         Task<ExcelProcessingResultDto> UploadPublicCategoryExcelFileAsync(IFormFile excelFile, Guid domainId);
         /// <summary>
         /// Upload the Duplicate Check Excel File
         /// </summary>
         /// <param name="excelFile"></param>
         Task<byte[]> ProcessAndGenerateExcel(IFormFile file);
         Task<ExcelProcessingResultDto> UploadConciergeVenueExcelFileAsync(IFormFile excelFile, Guid domainId);
          Task<ExcelProcessingResultDto> UploadDraftServicePRFileAsync(IFormFile excelFile, Guid domainId, Guid userId);   
          Task<ExcelProcessingResultDto> UploadLegalVenueExcelFileAsync(IFormFile excelFile, Guid domainId);  
          Task<ExcelProcessingResultDto> UploadContractSupplierNameExcelFileAsync(IFormFile excelFile, Guid domainId);
          Task<ExcelProcessingResultDto> UploadContractDepartmentNameExcelFileAsync(IFormFile excelFile, Guid domainId);
          Task<ExcelProcessingResultDto> UploadContractRegionNameExcelFileAsync(IFormFile excelFile, Guid domainId);
          Task<ExcelProcessingResultDto> UploadContractComoditiesNameExcelFileAsync(IFormFile excelFile, Guid domainId);
          Task<ExcelProcessingResultDto> UploadLegalMaterialCodeExcelFileAsync(IFormFile excelFile, Guid domainId);   
    }
}

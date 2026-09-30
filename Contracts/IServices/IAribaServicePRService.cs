using System.Collections.Generic;
using Dto;
using System;
using Contracts.IServices;
using Entities.Models;

namespace Contracts.IServices
{
    /// <summary>
    /// Interface IMaterialPRService
    /// </summary>
    public interface IAribaServicePRService
    {

        Task<List<MaterialPRResultDto>> PostMaterialPRToAriba(
      List<AribaServicePRDto> prRequests,
      Guid domainId,
      MyProfileDto dto,
      Guid userId,
      List<AssetUploadDto>? assetUploadDto,
      bool isUpdate);
      Task<List<ServicePRResponseDto>> GetServicePRsGroupedAsync(
        Guid userId,
        Guid domainId,
        ServicePRFilterRequestDto filter);
      
Task<BatchProcessingResultDto> CreateDraftServicePRBatchAsync(
            List<DraftServicePRExcelUploadDto> dtos,
            Guid userId, Guid domainId);

    }
}
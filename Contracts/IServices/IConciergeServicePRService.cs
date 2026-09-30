using System.Collections.Generic;
using Dto;
using System;
using Contracts.IServices;

namespace Contracts.IServices
{
    /// <summary>
    /// Interface IConciergePRCreationService
    /// </summary>
    public interface IConciergeServicePRService
    {
  Task<List<MaterialPRResultDto>> PostConciergeServicePRToAriba(
      List<AribaServicePRDto> prRequests,
      Guid domainId,
      MyProfileDto dto,
      Guid userId,
      List<AssetUploadDto>? assetUploadDto,
      bool isUpdate);

    }
}
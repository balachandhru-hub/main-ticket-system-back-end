using System.Collections.Generic;
using Dto;
using System;
using Contracts.IServices;
using Entities.Models;

namespace Contracts.IServices
{
    /// <summary>
    /// Interface IPublicRelationService
    /// </summary>
    public interface IPublicRelationServicePRService
    {
       Task<List<MaterialPRResultDto>> PostPublicQuickPRToAriba(
      List<AribaServicePRDto> prRequests,
      Guid domainId,
      MyProfileDto dto,
      Guid userId,
      List<AssetUploadDto>? assetUploadDto,
      bool isUpdate);



    }
}
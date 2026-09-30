using System.Collections.Generic;
using Dto;
using System;
using System.Threading.Tasks;
using Entities.Models;
using System;
using Contracts.IServices;

namespace Contracts.IServices
{
    /// <summary>
    /// Interface IConciergePRCreationService
    /// </summary>
    public interface IContractExcelService
    {
         Task<BatchProcessingResultDto> CreateSupplierNameBatchAsync(List<ContractSupplierNameExcelDto> dtos);
         Task<BatchProcessingResultDto> CreateContractDepartmentNameBatchAsync(List<ContractDepartmentExcelDto> dtos);
         Task<BatchProcessingResultDto> CreateContractRegionNameBatchAsync(List<ContractRegionExcelDto> dtos);
         Task<BatchProcessingResultDto> CreateContractComoditiesNameBatchAsync(List<ContractComoditiesExcelDto> dtos);
         Task<object> GetAllContractDetailsAsync(
        Guid domainId,
        GetVenueSupplierRequestDto dto);
        Task<List<ContractSupplierDto>> GetContractSupplierNamesAsync(Guid domainId, string searchTerm, int index, int limit);
      
        Task<object> GetCustomTypeNamesAsync(
        Guid domainId,
        GetVenueSupplierRequestDto dto);
        Task<string> GetAribaToken(string authToken, string tokenUrl);
       Task<EasyContractResponseDto> CreateEasyContract(Guid domainId, ContractWorkspaceRequestDto requestDto, MyProfileDto aribauser);
  Task<List<EasyContractDto>> GetEasyContractsAsync(
    Guid userId,
    Guid domainId,
    int index,
    int limit,
    string? searchTerm);
    Task<List<ContractListDto>> GetCOntractId(Guid domainId, string searchTerm, int index, int limit);
    }
}
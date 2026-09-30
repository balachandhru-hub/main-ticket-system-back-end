using Contracts;
using Contracts.IRepository;
using Contracts.IServices;
using Dto;
using Newtonsoft.Json.Linq;
using Entities.Models;
using Newtonsoft.Json;
using Entities.Util;
using Microsoft.Extensions.Hosting;
using NPOI.SS.Formula.Functions;
using Quartz.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using Entities.Common;
using System.Net.Mail;
using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;
using ExceptionHandler;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace Services
{
    public class ContractExcelService : IContractExcelService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerManager _logger;

        private readonly IBulkInsertHelper _bulkInsertHelper;

        public ContractExcelService(
            IRepositoryWrapper repositoryWrapper,
            ILoggerManager logger,

            IBulkInsertHelper bulkInsertHelper)
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;

            _bulkInsertHelper = bulkInsertHelper;
        }

        public async Task<BatchProcessingResultDto> CreateSupplierNameBatchAsync(List<ContractSupplierNameExcelDto> dtos)
        {
            _logger.LogInfo($"Batch creating {dtos.Count} Contract Data records");

            BatchProcessingResultDto result = new BatchProcessingResultDto();
            List<ContractSupplier> newCompanyDataList = new List<ContractSupplier>();
            List<ContractSupplier> updateCompanyDataList = new List<ContractSupplier>();

            // Track processed contracts to avoid duplicates
            HashSet<string> processedCompanyKeys = new HashSet<string>();

            try
            {
                List<string> supplierId = dtos.Select(k => k.SmVendorId).Distinct().ToList();

                Dictionary<string, ContractSupplier> existingCompanys = await _repositoryWrapper.ContractSupplier.FindByCondition(x => x.IsActive && supplierId.Contains(x.SmVendorId!))
                        .ToDictionaryAsync(x => x.SmVendorId!);

                foreach (var dto in dtos)
                {
                    try
                    {
                        string companyKey = dto.SmVendorId;

                        if (!processedCompanyKeys.Contains(companyKey))
                        {
                            processedCompanyKeys.Add(companyKey);

                            if (existingCompanys.TryGetValue(companyKey, out ContractSupplier existingCompany) && existingCompany.DomainId == dto.DomainId)
                            {
                                existingCompany.Name = dto.Name;
                                updateCompanyDataList.Add(existingCompany);
                            }
                            else
                            {
                                ContractSupplier newCompany = new ContractSupplier
                                {
                                    Id = Guid.NewGuid(),
                                    DomainId = dto.DomainId,
                                    Name = dto.Name,
                                    SmVendorId = dto.SmVendorId
                                };

                                newCompanyDataList.Add(newCompany);
                            }
                        }
                        result.SuccessfulCount++;
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount++;
                        result.Errors.Add(new ErrorResponseDto
                        {
                            StatusCode = 500,
                            Message = "Error processing row",
                            Description = $"Error processing row {dto.RowNumber}: {ex.Message}"
                        });
                        _logger.LogError($"Error processing row {dto.RowNumber}: {ex.Message}");
                    }
                }

                // Step 4: Bulk insert all new orders in one transaction
                if (newCompanyDataList.Any())
                {
                    _logger.LogInfo($"Bulk inserting {newCompanyDataList.Count} new contracts");
                    await _repositoryWrapper.ContractSupplier.CreateRangeAsync(newCompanyDataList);
                }

                if (updateCompanyDataList.Any())
                {
                    _logger.LogInfo($"Bulk updating {updateCompanyDataList.Count} contracts");
                    _repositoryWrapper.ContractSupplier.UpdateRange(updateCompanyDataList);
                }

                await _repositoryWrapper.SaveAsync();

                _logger.LogInfo($"Batch processing completed. Success: {result.SuccessfulCount}, Failed: {result.FailedCount}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Critical error in batch processing: {ex.Message}");
                throw;
            }
        }

        public async Task<BatchProcessingResultDto> CreateContractDepartmentNameBatchAsync(List<ContractDepartmentExcelDto> dtos)
        {
            _logger.LogInfo($"Batch creating {dtos.Count} Contract Department records");

            BatchProcessingResultDto result = new BatchProcessingResultDto();
            List<ContractDepartment> newCompanyDataList = new List<ContractDepartment>();
            List<ContractDepartment> updateCompanyDataList = new List<ContractDepartment>();

            // Track processed contracts to avoid duplicates
            HashSet<string> processedCompanyKeys = new HashSet<string>();

            try
            {
                List<string> supplierId = dtos.Select(k => k.UniqueName).Distinct().ToList();

                Dictionary<string, ContractDepartment> existingCompanys = await _repositoryWrapper.ContractDepartment.FindByCondition(x => x.IsActive && supplierId.Contains(x.UniqueName!))
                        .ToDictionaryAsync(x => x.UniqueName!);

                foreach (var dto in dtos)
                {
                    try
                    {
                        string companyKey = dto.UniqueName;

                        if (!processedCompanyKeys.Contains(companyKey))
                        {
                            processedCompanyKeys.Add(companyKey);

                            if (existingCompanys.TryGetValue(companyKey, out ContractDepartment existingCompany) && existingCompany.DomainId == dto.DomainId)
                            {
                                existingCompany.Name = dto.Name;
                                updateCompanyDataList.Add(existingCompany);
                            }
                            else
                            {
                                ContractDepartment newCompany = new ContractDepartment
                                {
                                    Id = Guid.NewGuid(),
                                    DomainId = dto.DomainId,
                                    Name = dto.Name,
                                    UniqueName = dto.UniqueName
                                };

                                newCompanyDataList.Add(newCompany);
                            }
                        }
                        result.SuccessfulCount++;
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount++;
                        result.Errors.Add(new ErrorResponseDto
                        {
                            StatusCode = 500,
                            Message = "Error processing row",
                            Description = $"Error processing row {dto.RowNumber}: {ex.Message}"
                        });
                        _logger.LogError($"Error processing row {dto.RowNumber}: {ex.Message}");
                    }
                }

                // Step 4: Bulk insert all new orders in one transaction
                if (newCompanyDataList.Any())
                {
                    _logger.LogInfo($"Bulk inserting {newCompanyDataList.Count} new contracts");
                    await _repositoryWrapper.ContractDepartment.CreateRangeAsync(newCompanyDataList);
                }

                if (updateCompanyDataList.Any())
                {
                    _logger.LogInfo($"Bulk updating {updateCompanyDataList.Count} contracts");
                    _repositoryWrapper.ContractDepartment.UpdateRange(updateCompanyDataList);
                }

                await _repositoryWrapper.SaveAsync();

                _logger.LogInfo($"Batch processing completed. Success: {result.SuccessfulCount}, Failed: {result.FailedCount}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Critical error in batch processing: {ex.Message}");
                throw;
            }
        }

        public async Task<BatchProcessingResultDto> CreateContractRegionNameBatchAsync(List<ContractRegionExcelDto> dtos)
        {
            _logger.LogInfo($"Batch creating {dtos.Count} Contract Region records");

            BatchProcessingResultDto result = new BatchProcessingResultDto();
            List<ContractRegion> newRegionDataList = new List<ContractRegion>();
            List<ContractRegion> updateRegionDataList = new List<ContractRegion>();

            // Track processed contracts to avoid duplicates
            HashSet<string> processedRegionKeys = new HashSet<string>();

            try
            {
                List<string> supplierId = dtos.Select(k => k.UniqueName).Distinct().ToList();

                Dictionary<string, ContractRegion> existingRegions = await _repositoryWrapper.ContractRegion.FindByCondition(x => x.IsActive && supplierId.Contains(x.UniqueName!))
                        .ToDictionaryAsync(x => x.UniqueName!);

                foreach (var dto in dtos)
                {
                    try
                    {
                        string regionKey = dto.UniqueName;

                        if (!processedRegionKeys.Contains(regionKey))
                        {
                            processedRegionKeys.Add(regionKey);

                            if (existingRegions.TryGetValue(regionKey, out ContractRegion existingRegion) && existingRegion.DomainId == dto.DomainId)
                            {
                                existingRegion.Name = dto.Name;
                                updateRegionDataList.Add(existingRegion);
                            }
                            else
                            {
                                ContractRegion newRegion = new ContractRegion
                                {
                                    Id = Guid.NewGuid(),
                                    DomainId = dto.DomainId,
                                    Name = dto.Name,
                                    UniqueName = dto.UniqueName
                                };

                                newRegionDataList.Add(newRegion);
                            }
                        }
                        result.SuccessfulCount++;
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount++;
                        result.Errors.Add(new ErrorResponseDto
                        {
                            StatusCode = 500,
                            Message = "Error processing row",
                            Description = $"Error processing row {dto.RowNumber}: {ex.Message}"
                        });
                        _logger.LogError($"Error processing row {dto.RowNumber}: {ex.Message}");
                    }
                }

                // Step 4: Bulk insert all new orders in one transaction
                if (newRegionDataList.Any())
                {
                    _logger.LogInfo($"Bulk inserting {newRegionDataList.Count} new contracts");
                    await _repositoryWrapper.ContractRegion.CreateRangeAsync(newRegionDataList);
                }

                if (updateRegionDataList.Any())
                {
                    _logger.LogInfo($"Bulk updating {updateRegionDataList.Count} contracts");
                    _repositoryWrapper.ContractRegion.UpdateRange(updateRegionDataList);
                }

                await _repositoryWrapper.SaveAsync();

                _logger.LogInfo($"Batch processing completed. Success: {result.SuccessfulCount}, Failed: {result.FailedCount}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Critical error in batch processing: {ex.Message}");
                throw;
            }
        }

        public async Task<BatchProcessingResultDto> CreateContractComoditiesNameBatchAsync(List<ContractComoditiesExcelDto> dtos)
        {
            _logger.LogInfo($"Batch creating {dtos.Count} Contract Region records");

            BatchProcessingResultDto result = new BatchProcessingResultDto();
            List<ContractComodities> newRegionDataList = new List<ContractComodities>();
            List<ContractComodities> updateRegionDataList = new List<ContractComodities>();

            // Track processed contracts to avoid duplicates
            HashSet<string> processedRegionKeys = new HashSet<string>();

            try
            {
                List<string> supplierId = dtos.Select(k => k.UniqueName).Distinct().ToList();

                Dictionary<string, ContractComodities> existingRegions = await _repositoryWrapper.ContractComodities.FindByCondition(x => x.IsActive && supplierId.Contains(x.UniqueName!))
                        .ToDictionaryAsync(x => x.UniqueName!);

                foreach (var dto in dtos)
                {
                    try
                    {
                        string regionKey = dto.UniqueName;

                        if (!processedRegionKeys.Contains(regionKey))
                        {
                            processedRegionKeys.Add(regionKey);

                            if (existingRegions.TryGetValue(regionKey, out ContractComodities existingRegion) && existingRegion.DomainId == dto.DomainId)
                            {
                                existingRegion.Name = dto.Name;
                                updateRegionDataList.Add(existingRegion);
                            }
                            else
                            {
                                ContractComodities newRegion = new ContractComodities
                                {
                                    Id = Guid.NewGuid(),
                                    DomainId = dto.DomainId,
                                    Name = dto.Name,
                                    UniqueName = dto.UniqueName,
                                    ContractDomain = dto.ContractDomain
                                };

                                newRegionDataList.Add(newRegion);
                            }
                        }
                        result.SuccessfulCount++;
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount++;
                        result.Errors.Add(new ErrorResponseDto
                        {
                            StatusCode = 500,
                            Message = "Error processing row",
                            Description = $"Error processing row {dto.RowNumber}: {ex.Message}"
                        });
                        _logger.LogError($"Error processing row {dto.RowNumber}: {ex.Message}");
                    }
                }

                // Step 4: Bulk insert all new orders in one transaction
                if (newRegionDataList.Any())
                {
                    _logger.LogInfo($"Bulk inserting {newRegionDataList.Count} new contracts");
                    await _repositoryWrapper.ContractComodities.CreateRangeAsync(newRegionDataList);
                }

                if (updateRegionDataList.Any())
                {
                    _logger.LogInfo($"Bulk updating {updateRegionDataList.Count} contracts");
                    _repositoryWrapper.ContractComodities.UpdateRange(updateRegionDataList);
                }

                await _repositoryWrapper.SaveAsync();

                _logger.LogInfo($"Batch processing completed. Success: {result.SuccessfulCount}, Failed: {result.FailedCount}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Critical error in batch processing: {ex.Message}");
                throw;
            }
        }

        public async Task<object> GetAllContractDetailsAsync(
 Guid domainId,
 GetVenueSupplierRequestDto dto)
        {
            _logger.LogInfo($"Fetching {dto.Type} for domain {domainId}");

            if (string.IsNullOrWhiteSpace(dto.Type))
                throw new PreConditionFailedCustomException("Type required", "Type cannot be empty");

            if (dto.Type == Common.MASTER_CONTRACT_DEPARTMENT_TYPE)
            {
                var query = _repositoryWrapper.ContractDepartment
                    .FindByCondition(v =>
                        v.DomainId == domainId &&
                        v.IsActive &&
                        (string.IsNullOrEmpty(dto.SearchTerm) ||
                         v.UniqueName.Contains(dto.SearchTerm) ||
                         v.Name.Contains(dto.SearchTerm)))
                    .OrderByDescending(v => v.DateUpdated);

                return query
                    .Skip(dto.Index)
                    .Take(dto.Limit)
                    .Select(v => new MasterContractDto
                    {
                        Id = v.Id,
                        DomainId = v.DomainId,
                        UniqueName = v.UniqueName,
                        Name = v.Name,

                    })
                    .ToList();
            }

            if (dto.Type == Common.MASTER_CONTRACT_REGION_TYPE)
            {
                var query = _repositoryWrapper.ContractRegion
                    .FindByCondition(s =>
                        s.DomainId == domainId &&
                        s.IsActive &&
                        (string.IsNullOrEmpty(dto.SearchTerm) ||
                         s.UniqueName.Contains(dto.SearchTerm) ||
                         s.Name.Contains(dto.SearchTerm)))
                    .OrderByDescending(s => s.DateUpdated);

                return query
                    .Skip(dto.Index)
                    .Take(dto.Limit)
                    .Select(s => new MasterContractDto
                    {
                        Id = s.Id,
                        DomainId = s.DomainId,
                        UniqueName = s.UniqueName,
                        Name = s.Name,

                    })
                    .ToList();
            }
            if (dto.Type == Common.MASTER_CONTRACT_COMODITY_TYPE)
            {
                var query = _repositoryWrapper.ContractComodities
                    .FindByCondition(s =>
                        s.DomainId == domainId &&
                        s.IsActive &&
                        (string.IsNullOrEmpty(dto.SearchTerm) ||
                         s.UniqueName.Contains(dto.SearchTerm) ||
                         s.Name.Contains(dto.SearchTerm)))
                    .OrderByDescending(s => s.DateUpdated);

                return query
                    .Skip(dto.Index)
                    .Take(dto.Limit)
                    .Select(s => new MasterContractDto
                    {
                        Id = s.Id,
                        DomainId = s.DomainId,
                        UniqueName = s.UniqueName,
                        Name = s.Name,
                        ContractDomain = s.ContractDomain

                    })
                    .ToList();
            }


            throw new PreConditionFailedCustomException("Invalid Type", "Type must be VENUE or SUPPLIER");
        }

        public async Task<List<ContractSupplierDto>> GetContractSupplierNamesAsync(Guid domainId, string searchTerm, int index, int limit)
        {
            var query = _repositoryWrapper.ContractSupplier
                .FindByCondition(s => s.IsActive)
                .Where(s =>
                    s.DomainId == domainId &&
                    (string.IsNullOrEmpty(searchTerm) ||
                     s.Name.Contains(searchTerm)));

            int total = await query.CountAsync();

            var suppliers = await query
                .OrderBy(s => s.Name)
                .Skip(index)
                .Take(limit)

                .Select(s => new ContractSupplierDto
                {
                    Name = s.Name,
                    SmVendorId = s.SmVendorId
                })
                .ToListAsync();

            return suppliers;
        }


        public async Task<object> GetCustomTypeNamesAsync(
      Guid domainId,
      GetVenueSupplierRequestDto dto)
        {
            _logger.LogInfo($"Fetching {dto.Type} for domain {domainId}");

            if (string.IsNullOrWhiteSpace(dto.Type))
                throw new PreConditionFailedCustomException("Type required", "Type cannot be empty");

            if (dto.Type == Common.MASTER_CONTRACT_TYPE)
            {
                var query = _repositoryWrapper.ContractCustomType
                    .FindByCondition(v =>
                        v.CustomType == dto.Type &&
                        v.IsActive &&
                        (string.IsNullOrEmpty(dto.SearchTerm) ||
                         v.Value.Contains(dto.SearchTerm)))
                    .OrderByDescending(v => v.DateUpdated);

                return query
                    .Skip(dto.Index)
                    .Take(dto.Limit)
                    .Select(v => new ContractCustomeTypeDto
                    {
                        Value = v.Value,
                        CustomType = v.CustomType

                    })
                    .ToList();
            }

            if (dto.Type == Common.MASTER_CONTRACT_HIERARCHICAL_TYPE)
            {
                var query = _repositoryWrapper.ContractCustomType
                    .FindByCondition(s =>
                        s.CustomType == dto.Type &&
                        s.IsActive &&
                        (string.IsNullOrEmpty(dto.SearchTerm) ||
                         s.Value.Contains(dto.SearchTerm)))
                    .OrderByDescending(s => s.DateUpdated);

                return query
                    .Skip(dto.Index)
                    .Take(dto.Limit)
                    .Select(s => new ContractCustomeTypeDto
                    {
                        Value = s.Value,
                        CustomType = s.CustomType

                    })
                    .ToList();
            }
            if (dto.Type == Common.MASTER_CONTRACT_TERM_TYPE)
            {
                var query = _repositoryWrapper.ContractCustomType
                    .FindByCondition(s =>
                        s.CustomType == dto.Type &&
                        s.IsActive &&
                        (string.IsNullOrEmpty(dto.SearchTerm) ||
                         s.Value.Contains(dto.SearchTerm)))
                    .OrderByDescending(s => s.DateUpdated);

                return query
                    .Skip(dto.Index)
                    .Take(dto.Limit)
                    .Select(s => new ContractCustomeTypeDto
                    {
                        Value = s.Value,
                        CustomType = s.CustomType
                    })
                    .ToList();
            }


            throw new PreConditionFailedCustomException("Invalid Type", "Type must be VENUE or SUPPLIER");
        }

        public async Task<string> GetAribaToken(string authToken, string tokenUrl)
        {
            using var client = new HttpClient();

            // Request body
            var body = new Dictionary<string, string> { { "grant_type", "openapi_2lo" } };

            using var content = new FormUrlEncodedContent(body);

            // Add headers
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            client.DefaultRequestHeaders.Connection.Add("keep-alive");
            client.DefaultRequestHeaders.Add("cache-control", "no-cache");

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

            // Send request
            var response = await client.PostAsync(tokenUrl, content);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to fetch token: {response.StatusCode} - {responseContent}");
            }

            var responseDTO = JsonConvert.DeserializeObject<AribaToken>(responseContent);

            if (string.IsNullOrEmpty(responseDTO?.access_token))
                throw new Exception("Failed to retrieve Ariba access token.");

            _logger.LogInfo("Ariba token retrieved successfully.");
            return responseDTO.access_token;
        }
        public async Task<EasyContractResponseDto> CreateEasyContract(Guid domainId, ContractWorkspaceRequestDto requestDto, MyProfileDto aribauser)
        {
            try
            {
                var contractSetting = _repositoryWrapper.DomainSetting
                    .FindFirstByCondition(x =>
                        x.DomainId == domainId &&
                        x.SettingsKey == Common.DOMAIN_SETTINGS_EASY_CONTRACT_CONFIG &&
                        x.IsActive);

                if (contractSetting == null)
                {
                    _logger.LogError($"[Domain: {domainId}] No contract URL settings found.");
                    return new EasyContractResponseDto
                    {
                        IsSuccess = false,
                        Message = "No contract URL settings found."
                    };
                }

                var contractConfig = JsonConvert.DeserializeObject<AribaPurchaseOrderSettings>(contractSetting.SettingsValue);

                var tokenSetting = _repositoryWrapper.DomainSetting
                    .FindFirstByCondition(x =>
                        x.DomainId == domainId &&
                        x.SettingsKey == Common.DOMAIN_SETTINGS_EASY_CONTRACT_CONFIG &&
                        x.IsActive);

                if (tokenSetting == null)
                {
                    _logger.LogError($"[Domain: {domainId}] No contract token settings found.");
                    return new EasyContractResponseDto
                    {
                        IsSuccess = false,
                        Message = "No contract token settings found."
                    };
                }

                var tokenConfig = JsonConvert.DeserializeObject<AribaPurchaseOrderSettings>(tokenSetting.SettingsValue);

                string token = await GetAribaToken(
                    tokenConfig.ContractAuthToken,
                    tokenConfig.ContractTokenUrl);

                if (string.IsNullOrWhiteSpace(token))
                {
                    _logger.LogError($"[Domain: {domainId}] Failed to retrieve token.");
                    return new EasyContractResponseDto
                    {
                        IsSuccess = false,
                        Message = "Failed to retrieve Ariba token."
                    };
                }

                string url = contractConfig.EasyContractUrl
                    .Replace("{user}", aribauser.AribaUserName)
                    .Replace("{passwordAdapter}", aribauser.AribaPassword)
                    .Replace("{realm}", contractConfig.Realm);

                _logger.LogInfo($"[ARIBA] Modify Contract URL: {url}");

                using var client = new HttpClient
                {
                    Timeout = TimeSpan.FromMinutes(5)
                };

                var request = new HttpRequestMessage(HttpMethod.Post, url);

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.Add("apiKey", contractConfig.ContractApiKey);

                var jsonContent = JsonConvert.SerializeObject(requestDto);
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                       return new EasyContractResponseDto
                        {
                            IsSuccess = false,
                            Message = $"Ariba error: {response.StatusCode} - {responseContent}"
                        };
                }
                var aribaResponse = JsonConvert.DeserializeObject<AribaContractResponse>(responseContent);

                var contractData = aribaResponse?.Value?.FirstOrDefault();

                string contractId = contractData?.ContractId;
                string contractStatus = contractData?.ContractStatus;


                var entity = new EasyContract
                {

                    DomainId = domainId,

                    Name = requestDto.Title,
                    HierarchicalType = requestDto.HierarchicalType,
                    TermType = requestDto.ExpirationTermType,
                    Supplier = requestDto.Supplier?.Name,
                    TemplateName = requestDto.TemplateId,

                    Email = requestDto.Owner?.EmailAddress,
                    ContactPerson = requestDto.Owner?.Name,

                    ContractValue = (decimal)(requestDto.ContractAmount?.Amount),
                    Currency = requestDto.ContractAmount?.Currency,

                    StartDate = requestDto.EffectiveDate,
                    EndDate = requestDto.ExpirationDate,

                    TestProject = requestDto.IsTestProject,
                    Description= requestDto.Description,
                    ContractId = contractId,
                    COntractStatus = contractStatus,
                    MaximumNumberOfRenewals = requestDto.MaximumNumberOfRenewals,
                    AutoRenewalInterval = requestDto.AutoRenewalInterval,
                    ParentAgreement = requestDto.ParentAgreement

                };

                _repositoryWrapper.EasyContract.Create(entity);

                if (requestDto.Regions != null && requestDto.Regions.Any())
                {
                    foreach (var region in requestDto.Regions)
                    {
                        var regionEntity = new EasyContractRegion
                        {
                            Id = Guid.NewGuid(),
                            DomainId = domainId,
                            ContractId = contractId,
                            UniqueName = region.UniqueName,
                            Name = region.Name
                        };

                        _repositoryWrapper.EasyContractRegion.Create(regionEntity);
                    }
                }


                if (requestDto.Departments != null && requestDto.Departments.Any())
                {
                    foreach (var department in requestDto.Departments)
                    {
                        var departmentEntity = new EasyContractDepartment
                        {
                            Id = Guid.NewGuid(),
                            DomainId = domainId,
                            ContractId = contractId,
                            UniqueName = department.UniqueName,
                            Name = department.Name
                        };

                        _repositoryWrapper.EasyContractDepartment.Create(departmentEntity);
                    }
                }


                if (requestDto.Commodities != null && requestDto.Commodities.Any())
                {
                    foreach (var commodity in requestDto.Commodities)
                    {
                        var commodityEntity = new EasyContractComoditity
                        {
                            Id = Guid.NewGuid(),
                            DomainId = domainId,
                            ContractId = contractId,
                            UniqueName = commodity.UniqueName,
                            Name = commodity.Name,
                            ContractDomain = commodity.Domain
                        };

                        _repositoryWrapper.EasyContractComodities.Create(commodityEntity);
                    }
                }
                await _repositoryWrapper.SaveAsync();


                        return new EasyContractResponseDto
        {
            IsSuccess = true,
            Message = "Contract created successfully",
            ContractId = contractId,
            ContractStatus = contractStatus
        };
            }
            catch (Exception ex)
            {
                return new EasyContractResponseDto
        {
            IsSuccess = false,
            Message = ex.Message
        };
            }
        }
        public async Task<List<EasyContractDto>> GetEasyContractsAsync(
      Guid userId,
      Guid domainId,
      int index,
      int limit,
      string? searchTerm)
        {
            _logger.LogInfo($"Fetching EasyContracts for UserId: {userId}, DomainId: {domainId}");

            var profile = _repositoryWrapper.Person.GetMyProfileByUserId(userId);

            if (profile == null)
                throw new NotFoundCustomException("User profile not found", $"UserId: {userId}");

            string? role = profile.Role;
            string? aribaUsername = profile.Email;

            IQueryable<EasyContract> query =
                _repositoryWrapper.EasyContract.FindByCondition(x => x.DomainId == domainId);

            // Role filter
            if (role != Common.BUSINESS_ADMIN_USER_KEY)
            {
                if (string.IsNullOrEmpty(aribaUsername))
                    throw new NotFoundCustomException("Ariba username missing", $"UserId: {userId}");

                query = query.Where(x => x.Email == aribaUsername);
            }

            // Search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string s = searchTerm.ToLower();

                query = query.Where(x =>
                    (x.Name != null && x.Name.ToLower().Contains(s)) ||
                    (x.Supplier != null && x.Supplier.ToLower().Contains(s)) ||
                    (x.ContractId != null && x.ContractId.ToLower().Contains(s))
                );
            }

            // ---------------- FLAT JOIN ----------------
            var flatData = await (
                from ec in query

                join r in _repositoryWrapper.EasyContractRegion
                    .FindByCondition(x => x.IsActive)
                    on ec.ContractId equals r.ContractId into rJoin
                from r in rJoin.DefaultIfEmpty()

                join d in _repositoryWrapper.EasyContractDepartment
                    .FindByCondition(x => x.IsActive)
                    on ec.ContractId equals d.ContractId into dJoin
                from d in dJoin.DefaultIfEmpty()

                join c in _repositoryWrapper.EasyContractComodities
                    .FindByCondition(x => x.IsActive)
                    on ec.ContractId equals c.ContractId into cJoin
                from c in cJoin.DefaultIfEmpty()

                orderby ec.DateUpdated descending

                select new
                {
                    Contract = ec,
                    Region = r,
                    Department = d,
                    Commodity = c
                }
            ).ToListAsync();

            // ---------------- GROUPING ----------------
            var result = flatData
                .GroupBy(x => x.Contract.ContractId)
                .Select(g => new EasyContractDto
                {
                    Id = g.First().Contract.Id,
                    Name = g.First().Contract.Name,
                    HierarchicalType = g.First().Contract.HierarchicalType,
                    TermType = g.First().Contract.TermType,
                    Supplier = g.First().Contract.Supplier,
                    ContractId = g.Key,
                    COntractStatus = g.First().Contract.COntractStatus,
                    ContractValue = g.First().Contract.ContractValue,
                    StartDate = g.First().Contract.StartDate,
                    EndDate = g.First().Contract.EndDate,
                    Currency = g.First().Contract.Currency,
                    TestProject = g.First().Contract.TestProject,
                    Description = g.First().Contract.Description,

                    // Regions
                    Regions = g
                        .Where(x => x.Region != null)
                        .GroupBy(x => x.Region.Id)
                        .Select(x => new LookupDto
                        {
                            UniqueName = x.First().Region.UniqueName,
                            Name = x.First().Region.Name
                        })
                        .ToList(),

                    // Departments
                    Departments = g
                        .Where(x => x.Department != null)
                        .GroupBy(x => x.Department.Id)
                        .Select(x => new LookupDto
                        {
                            UniqueName = x.First().Department.UniqueName,
                            Name = x.First().Department.Name
                        })
                        .ToList(),

                    // Commodities
                    Commodities = g
                        .Where(x => x.Commodity != null)
                        .GroupBy(x => x.Commodity.Id)
                        .Select(x => new EasyContractCommodityDto
                        {
                            UniqueName = x.First().Commodity.UniqueName,
                            Name = x.First().Commodity.Name,
                            ContractDomain = x.First().Commodity.ContractDomain
                        })
                        .ToList()
                })
                .Skip(index)
                .Take(limit)
                .ToList();

            return result;
        }
         public async Task<List<ContractListDto>> GetCOntractId(Guid domainId, string searchTerm, int index, int limit)
        {
            var query = _repositoryWrapper.EasyContract
                .FindByCondition(s => s.IsActive)
                .Where(s =>
                    s.DomainId == domainId &&
                    s.HierarchicalType!=Common.EASY_CONTRACT_HIERARCHICAL_TYPE &&
                    (string.IsNullOrEmpty(searchTerm) ||
                     s.ContractId.Contains(searchTerm)));

            int total = await query.CountAsync();

            var suppliers = await query
                .OrderBy(s => s.ContractId)
                .Skip(index)
                .Take(limit)

                .Select(s => new ContractListDto
                {
                    ContractId = s.ContractId
                })
                .ToListAsync();

            return suppliers;
        }

    }
}





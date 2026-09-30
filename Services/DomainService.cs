using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using AutoMapper;
using Contracts;
using Contracts.IRepository;
using Contracts.IServices;
using Dto;
using Entities.Common;
using Entities.Models;
using ExceptionHandler;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;


namespace Services
{
    /// <summary>
    /// Domain Service
    /// </summary>
    public class DomainService : IDomainService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        private readonly IAssetService _assetService;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public DomainService(IRepositoryWrapper repositoryWrapper, ILoggerManager logger, IMapper mapper, IAssetService assetService, IConfiguration configuration, IEmailService emailService)
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
            _mapper = mapper;
            _assetService = assetService;
            _emailService = emailService;
            _configuration = configuration;
        }

        /// <summary>
        /// Creates a new domain.
        /// </summary>
        /// <param name="domainDto">DomainDto containing domain details.</param>
        public DomainDto CreateDomain(DomainDto domainDto)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                _logger.LogInfo($"Creating a new domain with name: {domainDto.Name}");
                Domain existingDomain = _repositoryWrapper.Domain.FindFirstByCondition(x => x.IsActive && x.AribaNetworkId == domainDto.AribaNetworkId);

                if (existingDomain != null)
                {
                    _logger.LogError($"Domain Already Exists for the Given Network Id {domainDto.AribaNetworkId}");
                    throw new PreConditionFailedCustomException("Domain Already Exists", "Domain with Ariba Network Id Already Exists");
                }

                Domain domain = new Domain
                {
                    Id = Guid.NewGuid(),
                    Name = domainDto.Name,
                    WebsiteUrl = domainDto.WebsiteUrl,
                    RegistrationNumber = domainDto.RegistrationNumber,
                    HeadquartersLocation = domainDto.HeadquartersLocation,
                    CountriesOfOperation = domainDto.CountriesOfOperation,
                    BusinessType = domainDto.BusinessType,
                    IndustryType = domainDto.IndustryType,
                    BillingAddress = domainDto.BillingAddress,
                    GST = domainDto.GST,
                    VAT = domainDto.VAT,
                    TAN = domainDto.TAN,
                    PAN = domainDto.PAN,
                    TAXRegistration = domainDto.TAXRegistration,
                    OtherLocalTaxId = domainDto.OtherLocalTaxId,
                    NDA = domainDto.NDA,
                    TaxApplicable = domainDto.TaxApplicable,
                    NumberOfUser = domainDto.NumberOfUser,
                    SupportAdminName = domainDto.SupportAdminName,
                    SupportAdminEmail = domainDto.SupportAdminEmail,
                    SupportAdminTimeZone = domainDto.SupportAdminTimeZone,
                    SupportAdminPhoneNo = domainDto.SupportAdminPhoneNo,
                    ConnectERPSystem = domainDto.ConnectERPSystem,
                    AribaNetworkId = domainDto.AribaNetworkId,
                    Designation = domainDto.Designation,
                    LUTNumber=domainDto.LUTNumber
                };

                _repositoryWrapper.Domain.Create(domain);

                List<DomainRealm> domainRealms = new List<DomainRealm>();
                List<DomainModel> domainModels = new List<DomainModel>();
                if (domainDto.DomainRealms != null && domainDto.DomainRealms.Any())
                {
                    foreach (DomainRealmDto realmDto in domainDto.DomainRealms)
                    {
                        DomainRealm domainRealm = new DomainRealm
                        {
                            Id = Guid.NewGuid(),
                            DomainId = domain.Id,
                            RealmId = realmDto.RealmId,
                            AribaWebsiteUrl = realmDto.AribaWebsiteUrl,
                        };
                        domainRealms.Add(domainRealm);
                    }
                }

                if (domainDto.DomainModels != null && domainDto.DomainModels.Any())
                {
                    foreach (DomainModelDto modelDto in domainDto.DomainModels)
                    {
                        DomainModel domainModel = new DomainModel
                        {
                            Id = Guid.NewGuid(),
                            DomainId = domain.Id,
                            ModelName = modelDto.ModelName

                        };
                        domainModels.Add(domainModel);
                    }
                }

                if (domainRealms.Any())
                {
                    _repositoryWrapper.DomainRealm.CreateRange(domainRealms);
                }
                if (domainModels.Any())
                {
                    _repositoryWrapper.DomainModel.CreateRange(domainModels);
                }
                _repositoryWrapper.Save();

                try
                {
                    CreateDomainEmailNotification(domain, Common.DOMAIN_CREATION_EMAIL_KEY, Common.ENTITY_TYPE_DOMAIN);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to send ticket creation email notification: {ex.Message}");
                }

                _logger.LogInfo($"Domain successfully created with ID: {domain.Id}");
                transaction.Complete();
                return _mapper.Map<DomainDto>(domain);
            }
        }

        public DomainDto GetDomainById(Guid domainId)
        {
            _logger.LogInfo($"Retrieving domain with ID: {domainId}");

            Domain domain = _repositoryWrapper.Domain
                .FindFirstByCondition(x => x.IsActive && x.Id == domainId);

            if (domain == null)
            {
                _logger.LogError($"Domain not found with ID: {domainId}");
                throw new NotFoundCustomException("Domain not found", $"No domain exists with ID: {domainId}");
            }

            DomainDto domainDto = _mapper.Map<DomainDto>(domain);
            if (domainDto == null)
            {
                domainDto = new DomainDto();
            }

            List<DomainModel> domainModels = _repositoryWrapper.DomainModel
                 .FindByCondition(m => m.DomainId == domainId && m.IsActive)
                 .ToList();


            domainDto.DomainModels = domainModels
                .Select(model => new DomainModelDto
                {
                    DomainId = model.DomainId,
                    ModelName = model.ModelName,
                }).ToList();


            List<DomainRealm> domainRealms = _repositoryWrapper.DomainRealm
                .FindByCondition(r => r.DomainId == domainId && r.IsActive)
                .ToList();

            domainDto.DomainRealms = domainRealms
                .Select(realm => new DomainRealmDto
                {
                    RealmId = realm.RealmId,
                    AribaWebsiteUrl = realm.AribaWebsiteUrl,
                }).ToList();

            return domainDto;
        }


        public List<DomainPreviewDto> GetAllDomains(int index, int limit, string? aribaNetworkId, string? searchTerm)
        {
            _logger.LogInfo("Retrieving all active domains.");
            List<DomainPreviewDto> result = _repositoryWrapper.Domain.GetAllDomain(index, limit, aribaNetworkId, searchTerm);

            if (result == null || !result.Any())
            {
                _logger.LogError("No active domains found.");
                throw new NotFoundCustomException("No active domains", "No domains available in the system.");
            }
            return result;
        }

        public List<DomainPreviewDto> GetAllExpiredDomain(int index, int limit, string? searchTerm)
        {
            _logger.LogInfo("Retrieving all inactive (expired) domains.");
            List<DomainPreviewDto> result = _repositoryWrapper.Domain.GetAllExpiredDomain(index, limit, searchTerm);

            if (result == null || !result.Any())
            {
                _logger.LogError("No expired domains found.");
                throw new NotFoundCustomException("No expired domains", "No expired domains available in the system.");
            }
            return result;
        }

        public void CreateDomainSettings(DomainSettingPurchaseOrderDto dto, Guid domainId, string settingType)
        {
            _logger.LogInfo("Creating Domain setting");

            if (settingType == Common.DOMAIN_SETTINGS_ARIBA_PURCHASE_ORDER_CONFIG)
            {
                if (dto.aribaUrl == null || dto.Partition == null || dto.Password == null || dto.UserName == null || dto.Variant == null || dto.Server == null || dto.ApiKey == null || dto.Realm == null || dto.PurchaseOrderNotificationEmail == null)
                {
                    _logger.LogError("Invalid Ariba Purchase Order Settings: All fields are required");
                    throw new BadRequestCustomException("Invalid Ariba Purchase Order Settings",
                        "All fields are required for Ariba Purchase Order Settings");
                }

                var existingSettings = _repositoryWrapper.DomainSetting.FindByCondition(t =>
                    t.SettingsKey == Common.DOMAIN_SETTINGS_ARIBA_PURCHASE_ORDER_CONFIG &&
                    t.DomainId == domainId);

                var newAribaSetttingsDto = new AribaPurchaseOrderSettings()
                {
                    aribaUrl = dto.aribaUrl,
                    Partition = dto.Partition,
                    Password = dto.Password,
                    UserName = dto.UserName,
                    Variant = dto.Variant,
                    Server = dto.Server,
                    AribaAuthToken = dto.AribaAuthToken,
                    ApiKey = dto.ApiKey,
                    Realm = dto.Realm,
                    AribaTokenUrl = dto.AribaTokenUrl,
                    PurchaseOrderNotificationEmail = dto.PurchaseOrderNotificationEmail
                };

                var aribaConfig = JsonConvert.SerializeObject(newAribaSetttingsDto);

                DomainSetting req = new DomainSetting()
                {
                    DomainId = domainId,
                    Id = Guid.NewGuid(),
                    SettingsKey = Common.DOMAIN_SETTINGS_ARIBA_PURCHASE_ORDER_CONFIG,
                    SettingsValue = aribaConfig
                };

                if (existingSettings.Any())
                {
                    req.Id = existingSettings.First().Id;
                    req.IsActive = existingSettings.First().IsActive;
                    _repositoryWrapper.DomainSetting.Update(req);
                }
                else
                {
                    _repositoryWrapper.DomainSetting.Create(req);
                }
            }
            else if (settingType == Common.DOMAIN_SETTINGS_ARIBA_TOKEN)
            {
                if (dto.AribaAuthToken == null || dto.AribaHost == null || dto.AribaTokenUrl == null)
                {
                    _logger.LogError("Invalid Ariba Token Settings: All fields are required");
                    throw new BadRequestCustomException("Invalid Ariba Token Settings",
                        "All fields are required for Ariba Token Settings");
                }

                var existingToken = _repositoryWrapper.DomainSetting.FindByCondition(t =>
                    t.SettingsKey == Common.DOMAIN_SETTINGS_ARIBA_TOKEN &&
                    t.DomainId == domainId);

                var aribaTokenDto = new AribaPurchaseOrderSettings
                {
                    AribaAuthToken = dto.AribaAuthToken,
                    AribaTokenUrl = dto.AribaTokenUrl,
                    AribaHost = dto.AribaHost,
                    ApiKey = dto.ApiKey
                };

                var tokenConfig = JsonConvert.SerializeObject(aribaTokenDto);

                DomainSetting tokenReq = new DomainSetting()
                {
                    DomainId = domainId,
                    Id = Guid.NewGuid(),
                    SettingsKey = Common.DOMAIN_SETTINGS_ARIBA_TOKEN,
                    SettingsValue = tokenConfig
                };

                if (existingToken.Any())
                {
                    tokenReq.Id = existingToken.First().Id;
                    tokenReq.IsActive = existingToken.First().IsActive;
                    _repositoryWrapper.DomainSetting.Update(tokenReq);
                }
                else
                {
                    _repositoryWrapper.DomainSetting.Create(tokenReq);
                }
            }
            else if (settingType == Common.DOMAIN_SETTINGS_SUPPLIER_FETCH_TOKEN_AND_URL)
            {
                if (dto.AribaAuthToken == null || dto.AribaHost == null || dto.AribaTokenUrl == null || dto.ApiKey == null || dto.Realm == null || dto.SupplierFetchUrl == null)
                {
                    _logger.LogError("Invalid Supplier Fetch token/url settings: All fields are required");
                    throw new BadRequestCustomException("Invalid SupplierFetchTokenAndUrl settings",
                        "All fields are required for SupplierFetchTokenAndUrl settings");
                }

                var existingSupplierFetch = _repositoryWrapper.DomainSetting.FindByCondition(t =>
                    t.SettingsKey == Common.DOMAIN_SETTINGS_SUPPLIER_FETCH_TOKEN_AND_URL &&
                    t.DomainId == domainId);

                var supplierFetchDto = new AribaPurchaseOrderSettings
                {
                    AribaAuthToken = dto.AribaAuthToken,
                    AribaTokenUrl = dto.AribaTokenUrl,
                    SupplierFetchUrl = dto.SupplierFetchUrl,
                    AribaHost = dto.AribaHost,
                    ApiKey = dto.ApiKey,
                    Realm = dto.Realm,
                    SupplierInitialStartDate = dto.SupplierInitialStartDate
                };

                var supplierFetchConfig = JsonConvert.SerializeObject(supplierFetchDto);

                DomainSetting supplierFetchReq = new DomainSetting()
                {
                    DomainId = domainId,
                    Id = Guid.NewGuid(),
                    SettingsKey = Common.DOMAIN_SETTINGS_SUPPLIER_FETCH_TOKEN_AND_URL,
                    SettingsValue = supplierFetchConfig
                };

                if (existingSupplierFetch.Any())
                {
                    supplierFetchReq.Id = existingSupplierFetch.First().Id;
                    supplierFetchReq.IsActive = existingSupplierFetch.First().IsActive;
                    _repositoryWrapper.DomainSetting.Update(supplierFetchReq);
                }
                else
                {
                    _repositoryWrapper.DomainSetting.Create(supplierFetchReq);
                }
            }
            else if (settingType == Common.DOMAIN_SETTINGS_ARIBA_INVOICE)
            {
                if (dto.PendingApprovablesUrl == null || dto.RequisitionAttachmentUrl == null ||dto.RequisitionPRAttachmentUrl==null||
                    dto.AcceptRejectUrl == null || dto.User == null || dto.PasswordAdapter == null || dto.StartDateForInvoiceJob == null || dto.Realm == null
                    || dto.ApiKey == null)
                {
                    _logger.LogError("Invalid Ariba Invoice Settings: All fields are required");
                    throw new BadRequestCustomException("Invalid Ariba Invoice Settings",
                        "All fields are required for Ariba Invoice Settings");
                }

                var existingInvoice = _repositoryWrapper.DomainSetting.FindByCondition(t =>
                    t.SettingsKey == Common.DOMAIN_SETTINGS_ARIBA_INVOICE &&
                    t.DomainId == domainId);

                var invoiceDto = new AribaPurchaseOrderSettings
                {
                    PendingApprovablesUrl = dto.PendingApprovablesUrl,
                    RequisitionAttachmentUrl = dto.RequisitionAttachmentUrl,
                    RequisitionDetailsUrl = dto.RequisitionDetailsUrl,
                    AcceptRejectUrl = dto.AcceptRejectUrl,
                    RequisitionTaskUrl = dto.RequisitionTaskUrl,
                    AcceptRejectPRUrl = dto.AcceptRejectPRUrl,
                    User = dto.User,
                    PasswordAdapter = dto.PasswordAdapter,
                    Token = dto.Token,
                    Realm = dto.Realm,
                    RequisitionPRAttachmentUrl=dto.RequisitionPRAttachmentUrl,
                    StartDateForInvoiceJob = dto.StartDateForInvoiceJob,
                    ApiKey = dto.ApiKey
                };

                var invoiceConfig = JsonConvert.SerializeObject(invoiceDto);

                DomainSetting invoiceReq = new DomainSetting()
                {
                    DomainId = domainId,
                    Id = Guid.NewGuid(),
                    SettingsKey = Common.DOMAIN_SETTINGS_ARIBA_INVOICE,
                    SettingsValue = invoiceConfig
                };

                if (existingInvoice.Any())
                {
                    invoiceReq.Id = existingInvoice.First().Id;
                    invoiceReq.IsActive = existingInvoice.First().IsActive;
                    _repositoryWrapper.DomainSetting.Update(invoiceReq);
                }
                else
                {
                    _repositoryWrapper.DomainSetting.Create(invoiceReq);
                }
            }
            else if (settingType == Common.DOMAIN_SETTINGS_JOB_SCHEDULAR_CONFIG)
            {
                if (dto.JobSchedularUrl == null || dto.JobSchedularResultUrl == null || dto.PRAribaDataJobStartDate == null || dto.FactRecieptTemplate == null || dto.PurchaseOrderTemplate == null || dto.FactRecieptJobStartDate == null || dto.PurchaseOrderJobStartDate == null || dto.ApiKey == null || dto.Realm == null)
                {
                    _logger.LogError("Invalid Job Scheduler Settings: All fields are required");
                    throw new BadRequestCustomException("Invalid Job Scheduler Settings", "All fields are required for Job Scheduler Settings");
                }

                var existingJobSchedularConfig = _repositoryWrapper.DomainSetting.FindByCondition(t =>
                    t.SettingsKey == Common.DOMAIN_SETTINGS_JOB_SCHEDULAR_CONFIG &&
                    t.DomainId == domainId);

                var jobSchedularDto = new AribaPurchaseOrderSettings
                {
                    JobSchedularUrl = dto.JobSchedularUrl,
                    JobSchedularResultUrl = dto.JobSchedularResultUrl,
                    JobSchedularResultDownloadUrl = dto.JobSchedularResultDownloadUrl,
                    FactRecieptTemplate = dto.FactRecieptTemplate,
                    PurchaseOrderTemplate = dto.PurchaseOrderTemplate,
                    FactRecieptJobStartDate = dto.FactRecieptJobStartDate,
                    PurchaseOrderJobStartDate = dto.PurchaseOrderJobStartDate,
                    ApiKey = dto.ApiKey,
                    Realm = dto.Realm
                };

                var jobSchedularConfig = JsonConvert.SerializeObject(jobSchedularDto);

                DomainSetting jobSchedularReq = new DomainSetting()
                {
                    Id = new Guid(),
                    DomainId = domainId,
                    SettingsKey = Common.DOMAIN_SETTINGS_JOB_SCHEDULAR_CONFIG,
                    SettingsValue = jobSchedularConfig
                };

                if (existingJobSchedularConfig.Any())
                {
                    jobSchedularReq.Id = existingJobSchedularConfig.FirstOrDefault().Id;
                    jobSchedularReq.IsActive = existingJobSchedularConfig.FirstOrDefault().IsActive;
                    _repositoryWrapper.DomainSetting.Update(jobSchedularReq);
                }
                else
                    _repositoryWrapper.DomainSetting.Create(jobSchedularReq);
            }

            else if (settingType == Common.DOMAIN_SETTINGS_CONTRACT_CONFIG)
            {
                if (dto.ContractAuthToken == null || dto.ContractHost == null || dto.ContractTokenUrl == null || dto.AttachmentAuthToken == null || dto.AttachmentHost == null || dto.AttachmentTokenUrl == null || dto.Realm == null || dto.ContractWorkspaceAuthToken == null
                    )
                {
                    _logger.LogError("Invalid Ariba Token Settings: All fields are required");
                    throw new BadRequestCustomException("Invalid Ariba Token Settings",
                        "All fields are required for Ariba Token Settings");
                }

                var existingToken = _repositoryWrapper.DomainSetting.FindByCondition(t =>
                    t.SettingsKey == Common.DOMAIN_SETTINGS_CONTRACT_CONFIG &&
                    t.DomainId == domainId);

                var contractTokenDto = new AribaPurchaseOrderSettings
                {
                    ContractAuthToken = dto.ContractAuthToken,
                    ContractTokenUrl = dto.ContractTokenUrl,
                    ContractHost = dto.ContractHost,
                    ContractWorkspaceAuthToken = dto.ContractWorkspaceAuthToken,
                    AttachmentAuthToken = dto.AttachmentAuthToken,
                    AttachmentTokenUrl = dto.AttachmentTokenUrl,
                    AttachmentHost = dto.AttachmentHost,
                    Realm = dto.Realm,


                };

                var tokenConfig = JsonConvert.SerializeObject(contractTokenDto);

                DomainSetting tokenReq = new DomainSetting()
                {
                    DomainId = domainId,
                    Id = Guid.NewGuid(),
                    SettingsKey = Common.DOMAIN_SETTINGS_CONTRACT_CONFIG,
                    SettingsValue = tokenConfig
                };

                if (existingToken.Any())
                {
                    tokenReq.Id = existingToken.First().Id;
                    tokenReq.IsActive = existingToken.First().IsActive;
                    _repositoryWrapper.DomainSetting.Update(tokenReq);
                }
                else
                {
                    _repositoryWrapper.DomainSetting.Create(tokenReq);
                }
            }

              else if (settingType == Common.DOMAIN_SETTINGS_EASY_CONTRACT_CONFIG)
            {
                if (dto.ContractAuthToken == null || dto.ContractTokenUrl == null || dto.EasyContractUrl == null||dto.Realm == null ||dto.ContractApiKey==null
                    )
                {
                    _logger.LogError("Invalid Ariba Token Settings: All fields are required");
                    throw new BadRequestCustomException("Invalid Ariba Token Settings",
                        "All fields are required for Ariba Token Settings");
                }

                var existingToken = _repositoryWrapper.DomainSetting.FindByCondition(t =>
                    t.SettingsKey == Common.DOMAIN_SETTINGS_EASY_CONTRACT_CONFIG &&
                    t.DomainId == domainId);

                var contractTokenDto = new AribaPurchaseOrderSettings
                {
                    ContractAuthToken = dto.ContractAuthToken,
                    ContractTokenUrl = dto.ContractTokenUrl,
                    EasyContractUrl = dto.EasyContractUrl,
                    ContractApiKey = dto.ContractApiKey,
                   
                    Realm = dto.Realm,


                };

                var tokenConfig = JsonConvert.SerializeObject(contractTokenDto);

                DomainSetting tokenReq = new DomainSetting()
                {
                    DomainId = domainId,
                    Id = Guid.NewGuid(),
                    SettingsKey = Common.DOMAIN_SETTINGS_EASY_CONTRACT_CONFIG,
                    SettingsValue = tokenConfig
                };

                if (existingToken.Any())
                {
                    tokenReq.Id = existingToken.First().Id;
                    tokenReq.IsActive = existingToken.First().IsActive;
                    _repositoryWrapper.DomainSetting.Update(tokenReq);
                }
                else
                {
                    _repositoryWrapper.DomainSetting.Create(tokenReq);
                }
            }
            else if (settingType == Common.DOMAIN_SETTINGS_ARIBA_CONTRACT_URL)
            {
                if (dto.ContractUrl == null || dto.ContractAttachmentUrl == null ||
                    dto.ContractDocumentUrl == null || dto.ContractAmountUrl == null || dto.Realm == null
                    || dto.ApiKey == null || dto.AttachmentApiKey == null || dto.ContractApiKey == null || dto.ContractWorkspaceUrl == null
                    || dto.AribaContractDefaultUser == null || dto.AribaContractDefaultPassword == null || dto.ContractMaxAmountUrl == null
                    || dto.ContractMaxAmountAuthToken == null || dto.ContractMaxAmountApiKey == null || dto.ContractTokenUrl == null || dto.ContractMaxAmountRealm == null|| dto.ContractAttachmentWorkspaceUrl==null)
                {
                    _logger.LogError("Invalid Ariba Invoice Settings: All fields are required");
                    throw new BadRequestCustomException("Invalid Ariba Invoice Settings",
                        "All fields are required for Ariba Invoice Settings");
                }

                var existingContract = _repositoryWrapper.DomainSetting.FindByCondition(t =>
                    t.SettingsKey == Common.DOMAIN_SETTINGS_ARIBA_CONTRACT_URL &&
                    t.DomainId == domainId);

                var contractDto = new AribaPurchaseOrderSettings
                {
                    ContractUrl = dto.ContractUrl,
                    ContractAttachmentUrl = dto.ContractAttachmentUrl,
                    ContractDocumentUrl = dto.ContractDocumentUrl,
                    ContractAmountUrl = dto.ContractAmountUrl,
                    Realm = dto.Realm,
                    ContractWorkspaceUrl = dto.ContractWorkspaceUrl,
                    ContractRealm = dto.ContractRealm,
                    ApiKey = dto.ApiKey,
                    AttachmentApiKey = dto.AttachmentApiKey,
                    ContractApiKey = dto.ContractApiKey,
ContractAttachmentWorkspaceUrl = dto.ContractAttachmentWorkspaceUrl,
                    AribaContractDefaultUser = dto.AribaContractDefaultUser,
                    AribaContractDefaultPassword = dto.AribaContractDefaultPassword,
                    ContractMaxAmountUrl = dto.ContractMaxAmountUrl,
                    ContractMaxAmountAuthToken = dto.ContractMaxAmountAuthToken,
                    ContractMaxAmountApiKey = dto.ContractMaxAmountApiKey,
                    ContractTokenUrl = dto.ContractTokenUrl,
                    ContractMaxAmountRealm = dto.ContractMaxAmountRealm
                };

                var contractConfig = JsonConvert.SerializeObject(contractDto);

                DomainSetting contractReq = new DomainSetting()
                {
                    DomainId = domainId,
                    Id = Guid.NewGuid(),
                    SettingsKey = Common.DOMAIN_SETTINGS_ARIBA_CONTRACT_URL,
                    SettingsValue = contractConfig
                };

                if (existingContract.Any())
                {
                    contractReq.Id = existingContract.First().Id;
                    contractReq.IsActive = existingContract.First().IsActive;
                    _repositoryWrapper.DomainSetting.Update(contractReq);
                }
                else
                {
                    _repositoryWrapper.DomainSetting.Create(contractReq);
                }
            }
            else if (settingType == Common.DOMAIN_SETTINGS_CONTRACT_URL)
            {
                if (dto.ContractPendingApprovablesUrl == null || dto.ContractWorkspaceUrl == null || dto.Realm == null ||  dto.StartDateForContractJob == null || dto.ContractAcceptRejectUrl==null||dto.User==null|| dto.PasswordAdapter==null||dto.ContractWorkspaceDetailsUrl==null||  dto.WorkspaceDetailsApiKey==null

                    || dto.ApiKey == null)
                {
                    _logger.LogError("Invalid Ariba Invoice Settings: All fields are required");
                    throw new BadRequestCustomException("Invalid Ariba Invoice Settings",
                        "All fields are required for Ariba Invoice Settings");
                }

                var existingContract = _repositoryWrapper.DomainSetting.FindByCondition(t =>
                    t.SettingsKey == Common.DOMAIN_SETTINGS_CONTRACT_URL &&
                    t.DomainId == domainId);

              AribaPurchaseOrderSettings contractDto = new AribaPurchaseOrderSettings
              {
                  ContractPendingApprovablesUrl = dto.ContractPendingApprovablesUrl,
                  ContractWorkspaceUrl = dto.ContractWorkspaceUrl,
                  ContractWorkspaceDetailsUrl = dto.ContractWorkspaceDetailsUrl,
    WorkspaceDetailsApiKey = dto.WorkspaceDetailsApiKey,
                  ContractAcceptRejectUrl = dto.ContractAcceptRejectUrl,
                  ApiKey = dto.ApiKey,
                  Realm = dto.Realm,
                  User = dto.User,
                  PasswordAdapter = dto.PasswordAdapter,

                  StartDateForContractJob = dto.StartDateForContractJob

              };

                var contractConfig = JsonConvert.SerializeObject(contractDto);

                DomainSetting contractReq = new DomainSetting()
                {
                    DomainId = domainId,
                    Id = Guid.NewGuid(),
                    SettingsKey = Common.DOMAIN_SETTINGS_CONTRACT_URL,
                    SettingsValue = contractConfig
                };

                if (existingContract.Any())
                {
                    contractReq.Id = existingContract.First().Id;
                    contractReq.IsActive = existingContract.First().IsActive;
                    _repositoryWrapper.DomainSetting.Update(contractReq);
                }
                else
                {
                    _repositoryWrapper.DomainSetting.Create(contractReq);
                }
            }
 else if (settingType == Common.DOMAIN_SETTINGS_ARIBA_CONTRACT_TOKEN)
            {
                if (dto.AribaTokenUrl==null|| dto.AribaAuthToken==null||dto.WorkspaceDetailsAuthToken==null|| dto.WorkspaceDetailsTokenUrl==null|| dto.WorkspaceDetailsApiKey==null
                  
                    || dto.ApiKey == null )
                {
                    _logger.LogError("Invalid Ariba Invoice Settings: All fields are required");
                    throw new BadRequestCustomException("Invalid Ariba Invoice Settings",
                        "All fields are required for Ariba Invoice Settings");
                }

                var existingContract = _repositoryWrapper.DomainSetting.FindByCondition(t =>
                    t.SettingsKey == Common.DOMAIN_SETTINGS_ARIBA_CONTRACT_TOKEN &&
                    t.DomainId == domainId);

                AribaPurchaseOrderSettings contractDto = new AribaPurchaseOrderSettings
                {

                    AribaTokenUrl = dto.AribaTokenUrl,
                    WorkspaceDetailsApiKey = dto.WorkspaceDetailsApiKey,
                    WorkspaceDetailsTokenUrl = dto.WorkspaceDetailsTokenUrl,
                    WorkspaceDetailsAuthToken = dto.WorkspaceDetailsAuthToken,
                    AribaAuthToken = dto.AribaAuthToken,
                    ApiKey = dto.ApiKey,


                };

                var contractConfig = JsonConvert.SerializeObject(contractDto);

                DomainSetting contractReq = new DomainSetting()
                {
                    DomainId = domainId,
                    Id = Guid.NewGuid(),
                    SettingsKey = Common.DOMAIN_SETTINGS_ARIBA_CONTRACT_TOKEN,
                    SettingsValue = contractConfig
                };

                if (existingContract.Any())
                {
                    contractReq.Id = existingContract.First().Id;
                    contractReq.IsActive = existingContract.First().IsActive;
                    _repositoryWrapper.DomainSetting.Update(contractReq);
                }
                else
                {
                    _repositoryWrapper.DomainSetting.Create(contractReq);
                }
            }
            else if (settingType == Common.PR_ARIBA_SCHEDULAR_KEY)
            {
                if (dto.JobSchedularUrl == null || dto.JobSchedularResultUrl == null || dto.PRAribaDataJobStartDate == null || dto.PRAribaDataTemplate == null || dto.AribaTokenUrl == null || dto.AribaAuthToken == null || dto.ApiKey == null || dto.Realm == null || dto.AribaHost == null)
                {
                    _logger.LogError("Invalid PR Job Scheduler Settings: All fields are required");
                    throw new BadRequestCustomException("Invalid PR Job Scheduler Settings", "All fields are required for PR Job Scheduler Settings");
                }

                var existingJobPRSchedularConfig = _repositoryWrapper.DomainSetting.FindByCondition(t =>
                    t.SettingsKey == Common.PR_ARIBA_SCHEDULAR_KEY &&
                    t.DomainId == domainId);

                var jobPRSchedularDto = new AribaPurchaseOrderSettings
                {
                    JobSchedularUrl = dto.JobSchedularUrl,
                    JobSchedularResultUrl = dto.JobSchedularResultUrl,
                    PRAribaDataJobStartDate = dto.PRAribaDataJobStartDate,
                    PRAribaDataTemplate = dto.PRAribaDataTemplate,
                    AribaTokenUrl = dto.AribaTokenUrl,
                    AribaAuthToken = dto.AribaAuthToken,
                    AribaHost = dto.AribaHost,
                    ApiKey = dto.ApiKey,
                    Realm = dto.Realm
                };

                var jobSchedularConfig = JsonConvert.SerializeObject(jobPRSchedularDto);

                DomainSetting jobPRSchedularReq = new DomainSetting()
                {
                    Id = new Guid(),
                    DomainId = domainId,
                    SettingsKey = Common.PR_ARIBA_SCHEDULAR_KEY,
                    SettingsValue = jobSchedularConfig
                };

                if (existingJobPRSchedularConfig.Any())
                {
                    jobPRSchedularReq.Id = existingJobPRSchedularConfig.FirstOrDefault().Id;
                    jobPRSchedularReq.IsActive = existingJobPRSchedularConfig.FirstOrDefault().IsActive;
                    _repositoryWrapper.DomainSetting.Update(jobPRSchedularReq);
                }
                else
                    _repositoryWrapper.DomainSetting.Create(jobPRSchedularReq);
            }
            else if (settingType == Common.DOMAIN_SETTINGS_ARIBA_TOKEN_INVOICE)
            {
                if (dto.AribaAuthToken == null || dto.AribaHost == null || dto.AribaTokenUrl == null)
                {
                    _logger.LogError("Invalid Ariba Token Settings: All fields are required");
                    throw new BadRequestCustomException("Invalid Ariba Token Settings",
                        "All fields are required for Ariba Token Settings");
                }

                var existingToken = _repositoryWrapper.DomainSetting.FindByCondition(t =>
                    t.SettingsKey == Common.DOMAIN_SETTINGS_ARIBA_TOKEN_INVOICE &&
                    t.DomainId == domainId);

                var aribaTokenDto = new AribaPurchaseOrderSettings
                {
                    AribaAuthToken = dto.AribaAuthToken,
                    AribaTokenUrl = dto.AribaTokenUrl,
                    AribaHost = dto.AribaHost,
                    ApiKey = dto.ApiKey
                };

                var tokenConfig = JsonConvert.SerializeObject(aribaTokenDto);

                DomainSetting tokenReq = new DomainSetting()
                {
                    DomainId = domainId,
                    Id = Guid.NewGuid(),
                    SettingsKey = Common.DOMAIN_SETTINGS_ARIBA_TOKEN_INVOICE,
                    SettingsValue = tokenConfig
                };

                if (existingToken.Any())
                {
                    tokenReq.Id = existingToken.First().Id;
                    tokenReq.IsActive = existingToken.First().IsActive;
                    _repositoryWrapper.DomainSetting.Update(tokenReq);
                }
                else
                {
                    _repositoryWrapper.DomainSetting.Create(tokenReq);
                }
            }

            else if (settingType == Common.ARIBA_CONTRACT_JOB_KEY)
            {
                if (dto.JobSchedularUrl == null || dto.JobSchedularResultUrl == null || dto.AribaContractDataJobStartDate == null || dto.AribaContractDataTemplate == null || dto.AribaTokenUrl == null || dto.AribaAuthToken == null || dto.ApiKey == null || dto.Realm == null || dto.AribaHost == null)
                {
                    _logger.LogError("Invalid PR Job Scheduler Settings: All fields are required");
                    throw new BadRequestCustomException("Invalid Contract Job Scheduler Settings", "All fields are required for PR Job Scheduler Settings");
                }

                var existingJobContractSchedularConfig = _repositoryWrapper.DomainSetting.FindByCondition(t =>
                    t.SettingsKey == Common.ARIBA_CONTRACT_JOB_KEY &&
                    t.DomainId == domainId);

                var jobContractSchedularDto = new AribaPurchaseOrderSettings
                {
                    JobSchedularUrl = dto.JobSchedularUrl,
                    JobSchedularResultUrl = dto.JobSchedularResultUrl,
                    AribaContractDataJobStartDate = dto.AribaContractDataJobStartDate,
                    JobSchedularResultDownloadUrl = dto.JobSchedularResultDownloadUrl,
                    AribaContractDataTemplate = dto.AribaContractDataTemplate,
                    AribaTokenUrl = dto.AribaTokenUrl,
                    AribaAuthToken = dto.AribaAuthToken,
                    AribaHost = dto.AribaHost,
                    ApiKey = dto.ApiKey,
                    Realm = dto.Realm
                };

                var jobSchedularConfig = JsonConvert.SerializeObject(jobContractSchedularDto);

                DomainSetting jobContractSchedularReq = new DomainSetting()
                {
                    Id = new Guid(),
                    DomainId = domainId,
                    SettingsKey = Common.ARIBA_CONTRACT_JOB_KEY,
                    SettingsValue = jobSchedularConfig
                };

                if (existingJobContractSchedularConfig.Any())
                {
                    jobContractSchedularReq.Id = existingJobContractSchedularConfig.FirstOrDefault().Id;
                    jobContractSchedularReq.IsActive = existingJobContractSchedularConfig.FirstOrDefault().IsActive;
                    _repositoryWrapper.DomainSetting.Update(jobContractSchedularReq);
                }
                else
                    _repositoryWrapper.DomainSetting.Create(jobContractSchedularReq);
            }

             else if (settingType == Common.DOMAIN_SETTINGS_MATERIAL_PR_POSTING)
            {
                if (dto.aribaUrl == null || dto.Partition == null || dto.Password == null || dto.UserName == null || dto.Variant == null||dto.Server==null||dto.ServiceVariant==null||dto.ServicePartition==null )
                {
                    _logger.LogError("Invalid Ariba aterial PR Settings: All fields are required");
                    throw new BadRequestCustomException("Invalid Ariba Purchase Order Settings",
                        "All fields are required for Ariba Purchase Order Settings");
                }

                var existingSettings = _repositoryWrapper.DomainSetting.FindByCondition(t =>
                    t.SettingsKey == Common.DOMAIN_SETTINGS_MATERIAL_PR_POSTING &&
                    t.DomainId == domainId);

                var newAribaSetttingsDto = new AribaPurchaseOrderSettings()
                {
                    aribaUrl = dto.aribaUrl,
                    Partition = dto.Partition,
                    Password = dto.Password,
                    UserName = dto.UserName,
                    Variant = dto.Variant,
                    Server=dto.Server,
                    ServiceVariant=dto.ServiceVariant,
                    ServicePartition=dto.ServicePartition
                  

                };

                var aribaConfig = JsonConvert.SerializeObject(newAribaSetttingsDto);

                DomainSetting req = new DomainSetting()
                {
                    DomainId = domainId,
                    Id = Guid.NewGuid(),
                    SettingsKey = Common.DOMAIN_SETTINGS_MATERIAL_PR_POSTING,
                    SettingsValue = aribaConfig
                };

                if (existingSettings.Any())
                {
                    req.Id = existingSettings.First().Id;
                    req.IsActive = existingSettings.First().IsActive;
                    _repositoryWrapper.DomainSetting.Update(req);
                }
                else
                {
                    _repositoryWrapper.DomainSetting.Create(req);
                }
            }

            else if (settingType == Common.DOMAIN_SETTINGS_AUTO_INVOICE_POSTING)
            {
                if (dto.AutoInvoiceURL == null|| dto.ApiKey==null)
                {
                    _logger.LogError("Invalid Ariba Material PR Settings: All fields are required");
                    throw new BadRequestCustomException("Invalid Ariba Purchase Order Settings",
                        "All fields are required for Ariba Purchase Order Settings");
                }

                var existingSettings = _repositoryWrapper.DomainSetting.FindByCondition(t =>
                    t.SettingsKey == Common.DOMAIN_SETTINGS_AUTO_INVOICE_POSTING&&
                    t.DomainId == domainId);

                var newAribaSetttingsDto = new AribaPurchaseOrderSettings()
                {
                    AutoInvoiceURL = dto.AutoInvoiceURL,
                 ApiKey=dto.ApiKey
               
                  

                };

                var aribaConfig = JsonConvert.SerializeObject(newAribaSetttingsDto);

                DomainSetting req = new DomainSetting()
                {
                    DomainId = domainId,
                    Id = Guid.NewGuid(),
                    SettingsKey = Common.DOMAIN_SETTINGS_AUTO_INVOICE_POSTING,
                    SettingsValue = aribaConfig
                };

                if (existingSettings.Any())
                {
                    req.Id = existingSettings.First().Id;
                    req.IsActive = existingSettings.First().IsActive;
                    _repositoryWrapper.DomainSetting.Update(req);
                }
                else
                {
                    _repositoryWrapper.DomainSetting.Create(req);
                }
            }

              else if (settingType == Common.DOMAIN_SETTINGS_AUTO_SES_POSTING)
            {
                if ( dto.ApiKey==null||dto.AutoSESURL == null||dto.AutoInvoiceURL==null)
                {
                    _logger.LogError("Invalid Ariba Material PR Settings: All fields are required");
                    throw new BadRequestCustomException("Invalid Ariba Purchase Order Settings",
                        "All fields are required for Ariba Purchase Order Settings");
                }

                var existingSettings = _repositoryWrapper.DomainSetting.FindByCondition(t =>
                    t.SettingsKey == Common.DOMAIN_SETTINGS_AUTO_SES_POSTING &&
                    t.DomainId == domainId);

                var newAribaSetttingsDto = new AribaPurchaseOrderSettings()
                {
                  
                 ApiKey=dto.ApiKey,
                    AutoSESURL = dto.AutoSESURL,
                    AutoInvoiceURL=dto.AutoInvoiceURL
                  

                };

                var aribaConfig = JsonConvert.SerializeObject(newAribaSetttingsDto);

                DomainSetting req = new DomainSetting()
                {
                    DomainId = domainId,
                    Id = Guid.NewGuid(),
                    SettingsKey = Common.DOMAIN_SETTINGS_AUTO_SES_POSTING,
                    SettingsValue = aribaConfig
                };

                if (existingSettings.Any())
                {
                    req.Id = existingSettings.First().Id;
                    req.IsActive = existingSettings.First().IsActive;
                    _repositoryWrapper.DomainSetting.Update(req);
                }
                else
                {
                    _repositoryWrapper.DomainSetting.Create(req);
                }
            }
            else if (settingType == Common.SYNC_DATA_CONFIG)
            {
                if (dto.ProcurementAuthToken == null || dto.ProcurementTokenUrl == null || dto.ProcurementApiKey == null || dto.ProcurementFactRecieptUrl == null || dto.ProcurementPurchaseOrderUrl == null || dto.ProcurementFactRecieptStartDate == null || dto.ProcurementPurchaseOrderStartDate == null || dto.ProcurementRealm == null || dto.ProcurementHost == null)
                {
                    _logger.LogError("Invalid SYNC Job Scheduler Settings: All fields are required");
                    throw new BadRequestCustomException("Invalid SYNC Job Scheduler Settings: All fields are required", "Invalid SYNC Job Scheduler Settings: All fields are required");
                }

                var existingSyncJobSchedularConfig = _repositoryWrapper.DomainSetting.FindByCondition(t =>
                    t.SettingsKey == Common.SYNC_DATA_CONFIG &&
                    t.DomainId == domainId);

                var syncJobSchedularDto = new AribaPurchaseOrderSettings
                {
                    ProcurementAuthToken = dto.ProcurementAuthToken,
                    ProcurementTokenUrl = dto.ProcurementTokenUrl,
                    ProcurementApiKey = dto.ProcurementApiKey,
                    ProcurementFactRecieptUrl = dto.ProcurementFactRecieptUrl,
                    ProcurementPurchaseOrderUrl = dto.ProcurementPurchaseOrderUrl,
                    ProcurementFactRecieptStartDate = dto.ProcurementFactRecieptStartDate,
                    ProcurementPurchaseOrderStartDate = dto.ProcurementPurchaseOrderStartDate,
                    ProcurementRealm = dto.ProcurementRealm,
                    ProcurementHost = dto.ProcurementHost
                };

                var jobSchedularConfig = JsonConvert.SerializeObject(syncJobSchedularDto);

                DomainSetting syncJobSchedularReq = new DomainSetting()
                {
                    Id = new Guid(),
                    DomainId = domainId,
                    SettingsKey = Common.SYNC_DATA_CONFIG,
                    SettingsValue = jobSchedularConfig
                };

                if (existingSyncJobSchedularConfig.Any())
                {
                    syncJobSchedularReq.Id = existingSyncJobSchedularConfig.FirstOrDefault().Id;
                    syncJobSchedularReq.IsActive = existingSyncJobSchedularConfig.FirstOrDefault().IsActive;
                    _repositoryWrapper.DomainSetting.Update(syncJobSchedularReq);
                }
                else
                    _repositoryWrapper.DomainSetting.Create(syncJobSchedularReq);
            }
            else
            {
                throw new ArgumentException("Invalid DomainSettingPurchaseOrderDto: Could not determine setting type.");
            }

            _repositoryWrapper.Save();
        }


        /// <summary>
        /// Creates a new domain plan for a given domain.
        /// </summary>
        /// <param name="dto">DomainPlanDto containing plan details</param>
        /// <returns>Domain Plan Id</returns>
        public Guid CreateDomainPlan(DomainPlanDto dto)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                _logger.LogInfo($"Creating domain plan for domainId: {dto.DomainId}");

                // Check if domain exists
                Domain? domain = _repositoryWrapper.Domain.FindByCondition(x => x.IsActive && x.Id == dto.DomainId).FirstOrDefault();
                if (domain == null)
                {
                    _logger.LogError($"Domain not found with ID: {dto.DomainId}");
                    throw new NotFoundCustomException("Domain not found", $"No domain exists with ID: {dto.DomainId}");
                }

                DomainPlan existingPlan = _repositoryWrapper.DomainPlan.FindFirstByCondition(x => x.IsActive && x.IsRunning && x.DomainId == dto.DomainId && x.ContractEndDate > DateTime.UtcNow);

                if (existingPlan != null)
                {
                    _logger.LogInfo($"Already a Plan Exists for the Given Domain : {dto.DomainId}");
                    throw new PreConditionFailedCustomException("Plan Aready Exists", $"Domain Plan Already Exist for the Given Domain");
                }


                DomainPlan plan = new DomainPlan
                {
                    Id = Guid.NewGuid(),
                    DomainId = dto.DomainId,
                    PlanName = dto.PlanName,
                    EngagementType = dto.EngagementType,
                    StartDate = dto.StartDate,
                    EndDate = dto.PaymentType == Common.MONTHLY ? dto.StartDate.AddMonths(1) : dto.PaymentType == Common.YEARLY ? dto.StartDate.AddYears(1) : DateTime.UtcNow,
                    DiscountCouponId = dto.DiscountCouponId,
                    PaymentType = dto.PaymentType,
                    ContractStartDate = dto.ContractStartDate,
                    ContractEndDate = dto.ContractEndDate,
                    IsRunning = true,
                    NumberOfDues = dto.ExpiryTime,


                };

                _repositoryWrapper.DomainPlan.Create(plan);

                PlanDetails planDetails = new PlanDetails
                {
                    Id = Guid.NewGuid(),
                    DomainPlanId = plan.Id,
                    CRHoursBeyondLimit = dto.CRHoursBeyondLimit,
                    NumberOfConsult = dto.NumberOfConsult,
                    NumberOfManager = dto.NumberOfManager,
                    TicketCharge = dto.TicketCharge,
                    MonthlyHours = dto.MonthlyHours,
                    AdditionalCRHoursCharge = dto.AdditionalCRHoursCharge,
                    NumberOfResources = dto.NumberOfResources,
                    CRCost = dto.CRCost,
                    ResourceCost = dto.ResourceCost,
                    ManagerCost = dto.ManagerCost,
                    TotalChargeOfConsult = dto.TotalChargeOfConsult,
                    TotalChargeOfManager = dto.TotalChargeOfManager,
                    TotalCost = dto.TotalCost
                };
                _repositoryWrapper.PlanDetails.Create(planDetails);
                if (dto.PlanName == Common.SUBSCRIPTION_PLAN_KEY)
                {
                    int totalMonths = ((plan.EndDate.Year - plan.StartDate.Year) * 12) + plan.EndDate.Month - plan.StartDate.Month;
                    DomainPlanCredit domainPlanCredit = new DomainPlanCredit
                    {
                        Id = Guid.NewGuid(),
                        DomainPlanId = plan.Id,
                        ActualMonthlyHourCredit = dto.MonthlyHours,
                        TotalHourCredit = totalMonths * dto.MonthlyHours,
                        MonthlyHourCredit = dto.MonthlyHours
                    };
                    _repositoryWrapper.DomainPlanCredit.Create(domainPlanCredit);
                }
                else if (dto.PlanName == Common.SUPPORT_TEAM_PLAN_KEY)
                {
                    int totalMonths = ((plan.EndDate.Year - plan.StartDate.Year) * 12) + plan.EndDate.Month - plan.StartDate.Month;
                    DomainPlanCredit domainPlanCredit = new DomainPlanCredit
                    {
                        Id = Guid.NewGuid(),
                        DomainPlanId = plan.Id,
                        ActualMonthlyHourCredit = dto.MonthlyHours,
                        TotalHourCredit = totalMonths * dto.MonthlyHours,
                        MonthlyHourCredit = dto.MonthlyHours
                    };
                    _repositoryWrapper.DomainPlanCredit.Create(domainPlanCredit);
                }
                // --- Asset Handling: Add this block ---
                if (dto.Asset != null)
                {
                    dto.Asset.EntityId = plan.Id;
                    dto.Asset.EntityType = Common.ENTITY_TYPE_DOMAIN_PLAN;
                    dto.Asset.AssetType = Common.ASSET_TYPE;
                    dto.Asset.IsSingletonAsset = true;

                    _assetService.UploadDocument(dto.Asset);
                }
                _repositoryWrapper.Save();

                try
                {
                    CreateDomainPlanEmailNotification(domain, plan, Common.DOMAIN_PLAN_CREATION_EMAIL_KEY, Common.ENTITY_TYPE_DOMAIN_PLAN);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to send ticket creation email notification: {ex.Message}");
                }

                transaction.Complete();
                _logger.LogInfo($"Domain plan and plan details successfully created with ID: {plan.Id}");
                return plan.Id;
            }
        }


        /// <summary>
        /// Get domain details (company, plan, tax) by domain Id
        /// </summary>
        public DomainDetailsDto GetDomainDetails(Guid domainId)
        {
            _logger.LogInfo($"Fetching the Domain Details for the Given Domain : {domainId}");
            Domain? domain = _repositoryWrapper.Domain.FindByCondition(d => d.Id == domainId && d.IsActive).FirstOrDefault();
            if (domain == null)
            {
                _logger.LogError($"Unble to Find the Domain : {domainId}");
                throw new NotFoundCustomException("Unable to Find Domain, Please Check Domain Exists", $"Unable to Find the Domain Detials for the Domain : {domainId}");
            }

            DomainPlan? domainPlan = _repositoryWrapper.DomainPlan
                .FindByCondition(p => p.IsRunning && p.DomainId == domainId)
                .OrderByDescending(p => p.StartDate)
                .FirstOrDefault();

            PlanDetails? planDetails = domainPlan != null ? _repositoryWrapper.PlanDetails.FindByCondition(pd => pd.DomainPlanId == domainPlan.Id).FirstOrDefault() : null;

            DomainRealm domainRealm = _repositoryWrapper.DomainRealm.FindFirstByCondition(dr => dr.IsActive && dr.DomainId == domain.Id);

            CompanyDetailsDto companyDetails = new CompanyDetailsDto
            {
                CompanyName = domain.Name,
                AribaNetworkId = domain.AribaNetworkId,
                AribaRealmId = (domainRealm != null) ? domainRealm.RealmId : null,
                CompanyRegistrationNo = domain.RegistrationNumber,
                Location = domain.HeadquartersLocation,
                BillingAddress = domain.BillingAddress
            };

            PlanDetailsDto? planDetailsDto = domainPlan != null ? new PlanDetailsDto
            {
                PlanType = domainPlan.PlanName,
                EngagementType = domainPlan.EngagementType,
                NoOfConsultants = planDetails?.NumberOfConsult ?? 0,
                MonthlyHours = planDetails?.MonthlyHours ?? 0,
                CrChargesForAdditionalHours = planDetails?.AdditionalCRHoursCharge != null ? planDetails.AdditionalCRHoursCharge : 0,
                PlanStartDate = domainPlan.StartDate,
                PlanEndDateOrRenewalDate = domainPlan.EndDate,
                PaymentAmount = planDetails?.TotalCost != null ? planDetails.TotalCost : 0,
                PaymentType = domainPlan.PaymentType,
                PaymentStatus = domainPlan.IsActive ? Common.ACTIVE_STATUS : Common.INACTIVE_STATUS,
                PlanStatus = domainPlan.IsActive ? Common.ACTIVE_STATUS : Common.INACTIVE_STATUS,
                ContractStartDate = domainPlan.ContractStartDate,
                ContractEndDate = domainPlan.ContractEndDate,
                CRHoursBeyondLimit = planDetails.CRHoursBeyondLimit,
                TicketCharge = planDetails.TicketCharge

            } : null;

            // TAX DETAILS
            TaxDetailsDto taxDetails = new TaxDetailsDto
            {
                GST = domain.GST,
                PAN = domain.PAN,
                TAN = domain.TAN,
                TAXRegistration = domain.TAXRegistration,
                VAT = domain.VAT,
                OtherLocalTaxId = domain.OtherLocalTaxId,
                CINNumber = domain.CINNumber,
                LUTNumber = domain.LUTNumber
            };
            AssetDto? assetDto = null;
            if (domainPlan != null)
            {
                Guid? contractPlanAssetTypeId = _repositoryWrapper.Metadata
                    .FindFirstByCondition(m => m.Key == Common.ASSET_TYPE && m.Type == "ASSET_TYPE")
                    ?.Id;

                Asset? assetEntity = _repositoryWrapper.Asset
                    .FindByCondition(a => a.EntityId == domainPlan.Id
                        && a.IsActive
                        && a.AssetType == contractPlanAssetTypeId)
                    .FirstOrDefault();
                if (assetEntity != null)
                {
                    assetDto = new AssetDto
                    {
                        Id = assetEntity.Id,
                        AssetType = assetEntity.AssetType != null ? assetEntity.AssetType.ToString() : null,
                        AssetName = assetEntity.FileName,
                        FileType = assetEntity.FileType != Guid.Empty ? assetEntity.FileType.ToString() : null,
                        FileName = assetEntity.FileName
                    };
                }
            }
            _logger.LogInfo($"Fetched the Domain Details for the Given Domain : {domainId}");
            List<AppNameDto> appNames =_repositoryWrapper.AppModelMapping.GetAppNamesByDomainId(domainId);
            return new DomainDetailsDto
            {
                CompanyDetails = companyDetails,
                PlanDetails = planDetailsDto,
                TaxDetails = taxDetails,
                Asset = assetDto,
                AppNames = appNames
            };
        }
        /// <summary>
        /// Updates the details of an existing domain.
        /// </summary>
        public void UpdateDomain(DomainUpdateDto domainUpdateDto)
        {
            Domain domain = _repositoryWrapper.Domain
                .FindFirstByCondition(x => x.Id == domainUpdateDto.Id && x.IsActive);

            if (domain == null)
            {
                throw new NotFoundCustomException("Domain not found", $"No domain exists with ID: {domainUpdateDto.Id}");
            }

            domain.NumberOfUser = domainUpdateDto.NumberOfUser ?? domain.NumberOfUser;
            domain.SupportAdminName = domainUpdateDto.SupportAdminName;
            domain.SupportAdminEmail = domainUpdateDto.SupportAdminEmail;
            domain.SupportAdminPhoneNo = domainUpdateDto.SupportAdminPhoneNo;
            domain.SupportAdminTimeZone = domainUpdateDto.SupportAdminTimeZone;
            domain.Designation = domainUpdateDto.Designation;
            domain.ConnectERPSystem = domainUpdateDto.ConnectERPSystem;

            if (domainUpdateDto.DomainModel != null)
            {
                Dictionary<string, DomainModel> existingModels = _repositoryWrapper.DomainModel
                    .FindByCondition(m => m.DomainId == domain.Id)
                    .ToDictionary(m => m.ModelName, m => m, StringComparer.OrdinalIgnoreCase);

                IEnumerable<DomainModel> updatedModels = domainUpdateDto.DomainModel
                    .Where(dto => existingModels.ContainsKey(dto.ModelName) && dto.IsActive.HasValue)
                    .Select(dto =>
                    {
                        DomainModel model = existingModels[dto.ModelName];
                        model.IsActive = dto.IsActive.Value;
                        return model;
                    });

                foreach (DomainModel model in updatedModels)
                {
                    _repositoryWrapper.DomainModel.Update(model);
                }

                IEnumerable<DomainModel> newModels = domainUpdateDto.DomainModel
                    .Where(dto => !existingModels.ContainsKey(dto.ModelName) && dto.IsActive == true)
                    .Select(dto => new DomainModel
                    {
                        DomainId = domain.Id,
                        ModelName = dto.ModelName,
                        IsActive = true
                    });

                foreach (DomainModel model in newModels)
                {
                    _repositoryWrapper.DomainModel.Create(model);
                }
            }

            if (domainUpdateDto.DomainRealm != null)
            {
                Dictionary<string, DomainRealm> existingRealms = _repositoryWrapper.DomainRealm
                    .FindByCondition(r => r.DomainId == domain.Id)
                    .ToDictionary(r => r.RealmId, r => r, StringComparer.OrdinalIgnoreCase);

                IEnumerable<DomainRealm> updatedRealms = domainUpdateDto.DomainRealm
                    .Where(dto => existingRealms.ContainsKey(dto.RealmId) && dto.IsActive.HasValue)
                    .Select(dto =>
                    {
                        DomainRealm realm = existingRealms[dto.RealmId];
                        realm.IsActive = dto.IsActive.Value;
                        realm.AribaWebsiteUrl = dto.AribaWebsiteUrl;
                        return realm;
                    });

                foreach (DomainRealm realm in updatedRealms)
                {
                    _repositoryWrapper.DomainRealm.Update(realm);
                }

                IEnumerable<DomainRealm> newRealms = domainUpdateDto.DomainRealm
                    .Where(dto => !existingRealms.ContainsKey(dto.RealmId) && dto.IsActive == true)
                    .Select(dto => new DomainRealm
                    {
                        DomainId = domain.Id,
                        RealmId = dto.RealmId,
                        AribaWebsiteUrl = dto.AribaWebsiteUrl,
                        IsActive = true
                    });

                foreach (DomainRealm realm in newRealms)
                {
                    _repositoryWrapper.DomainRealm.Create(realm);
                }
            }

            _repositoryWrapper.Domain.Update(domain);
            _repositoryWrapper.Save();
        }

        /// <summary>
        /// Get Domain Logo for the Given User
        /// </summary>
        /// <param name="userId">User ID</param>
        public AssetDownloadDto GetDomainLogo(Guid userId)
        {
            _logger.LogInfo($"Fetching the Domain Logo for the User : {userId}");

            User user = _repositoryWrapper.User.FindFirstByCondition(u => u.Id == userId && u.IsActive);
            if (user == null)
            {
                _logger.LogError($"User with ID {userId} not found.");
                throw new NotFoundCustomException("User Not Found", $"User with ID {userId} not found.");
            }

            Person person = _repositoryWrapper.Person.FindFirstByCondition(p => p.Id == user.PersonId && p.IsActive);
            if (person == null)
            {
                _logger.LogError($"Person with ID {user.PersonId} not found.");
                throw new NotFoundCustomException("Person Not Found", $"Person with ID {user.PersonId} not found.");
            }

            Asset? asset = _repositoryWrapper.Asset.FindFirstByCondition(a => a.IsActive && a.EntityType == Common.DOMAIN_ENTITY_TYPE_ID && a.EntityId == person.DomainId && a.AssetType == Common.DOMAIN_LOGO_ASSET_TYPE_ID);
            if (asset == null)
            {
                _logger.LogError($"Domain logo not found for user with ID {userId}.");
                throw new NotFoundCustomException("Domain Logo Not Found", $"No domain logo found for user with ID {userId}.");
            }

            AssetDownloadDto assetDownloadDto = _assetService.GetDocument(asset.Id);

            if (assetDownloadDto == null)
            {
                _logger.LogError($"Failed to retrieve domain logo for user with ID {userId}.");
                throw new NotFoundCustomException("Domain Logo Retrieval Failed", $"Failed to retrieve domain logo for user with ID {userId}.");
            }
            return assetDownloadDto;
        }
        /// <summary>
        /// Updates the status of a domain.
        /// </summary>
        /// <param name="domainId">The ID of the domain to update.</param>
        /// <param name="activeStatus">The new active status for the domain.</param>
        public void UpdateDomainStatus(Guid domainId, string activeStatus)
        {
            _logger.LogInfo($"Updating domain Status for Domain: {domainId} to {activeStatus}");


            if (string.IsNullOrWhiteSpace(activeStatus) ||
                (activeStatus != Common.ACTIVE_STATUS &&
                activeStatus != Common.INACTIVE_STATUS))
            {
                throw new ArgumentException(
                    $"Invalid domain status. Allowed values: {Common.ACTIVE_STATUS}, {Common.INACTIVE_STATUS}"
                );
            }


            DomainPlan? domainPlan = _repositoryWrapper.DomainPlan
                .FindByCondition(p => p.DomainId == domainId)
                .OrderByDescending(p => p.StartDate)
                .FirstOrDefault();

            if (domainPlan == null)
            {
                _logger.LogError($"No DomainPlan found for DomainId: {domainId}");
                throw new NotFoundCustomException("Domain Plan not found", $"No plan exists for domain {domainId}");
            }
            domainPlan.IsActive = activeStatus.ToUpper() == Common.ACTIVE_STATUS;



            _repositoryWrapper.DomainPlan.Update(domainPlan);
            _repositoryWrapper.Save();

            _logger.LogInfo($"Successfully updated Payment Status for DomainId: {domainId} to {activeStatus}");
        }

        private void CreateDomainEmailNotification(Domain domain, string emailKey, string entityType)
        {
            _logger.LogInfo($"Creating email notification for Domain with Id : {domain.Id}");
            EmailContent emailContent = _repositoryWrapper.EmailContent.FindFirstByCondition(x => x.IsActive && x.Key == emailKey);

            if (emailContent != null)
            {
                EmailPlaceholderDto emailPlaceholder = new EmailPlaceholderDto
                {
                    EntityId = domain.Id,
                    EmailType = emailKey,
                    ToEmail = domain.SupportAdminEmail,
                    PersonName = domain.SupportAdminName,
                    EmailBody = emailContent.Body,
                    Subject = emailContent.Subject,
                    CompanyName = _configuration[Common.COMPANY_NAME],
                    SupportEmail = _configuration[Common.SUPPORT_EMAIL],
                    Date = DateOnly.FromDateTime(domain.DateCreated),
                    PortalLink = _configuration[Common.PORTAL_LINK],
                    CompanyLogo = _configuration[Common.COMPANY_LOGO_URL],
                    WebsiteUrl = domain.WebsiteUrl,
                    CinNo = domain.RegistrationNumber,
                    BusinessType = domain.BusinessType,
                    Country = domain.IndustryType,
                    CompanyLocation = domain.HeadquartersLocation,
                    IndustryType = domain.IndustryType,
                    DomainCompanyName = domain.Name,
                    EntityType = entityType,
                    BillingAddress = domain.BillingAddress

                };
                CreateEmailNotification(emailPlaceholder, emailContent, emailKey, entityType);
            }
        }

        private void CreateDomainPlanEmailNotification(Domain domain, DomainPlan domainPlan, string emailKey, string entityType)
        {
            _logger.LogInfo($"Creating email notification for Domain with Id : {domainPlan.Id}");
            EmailContent emailContent = _repositoryWrapper.EmailContent.FindFirstByCondition(x => x.IsActive && x.Key == emailKey);

            if (emailContent != null)
            {
                EmailPlaceholderDto emailPlaceholder = new EmailPlaceholderDto
                {
                    EntityId = domainPlan.Id,
                    EmailType = emailKey,
                    ToEmail = domain.SupportAdminEmail,
                    PersonName = domain.SupportAdminName,
                    EmailBody = emailContent.Body,
                    Subject = emailContent.Subject,
                    CompanyName = _configuration[Common.COMPANY_NAME],
                    SupportEmail = _configuration[Common.SUPPORT_EMAIL],
                    Date = DateOnly.FromDateTime(domainPlan.DateCreated),
                    PortalLink = _configuration[Common.PORTAL_LINK],
                    CompanyLogo = _configuration[Common.COMPANY_LOGO_URL],
                    DomainCompanyName = domain.Name,
                    EntityType = entityType,
                    DomainPlanName = domainPlan.PlanName,
                    DomainPlanEngagementType = domainPlan.EngagementType,
                    DomainPlanStartDate = domainPlan.StartDate,
                    DomainPlanEndDate = domainPlan.EndDate,
                    DomainPlanPaymentType = domainPlan.PaymentType,
                    DomainPlanDiscountCouponId = domainPlan.DiscountCouponId,
                    DomainPlanContractStartDate = domainPlan.ContractStartDate,
                    DomainPlanContractEndDate = domainPlan.ContractEndDate,
                    DomainPlanNumberOfDues = domainPlan.NumberOfDues
                };

                CreateEmailNotification(emailPlaceholder, emailContent, emailKey, entityType);
            }

        }

        private void CreateEmailNotification(EmailPlaceholderDto emailPlaceholder, EmailContent emailContent, string emailKey, string entityType)
        {
            emailPlaceholder.Subject = _emailService.ReplacePlaceholders(emailContent.Subject!, emailPlaceholder);
            emailPlaceholder.EmailBody = _emailService.ReplacePlaceholders(emailContent.Body!, emailPlaceholder);

            List<string>? ccEmails = _repositoryWrapper.EmailCCList.FindByCondition(x => x.IsActive && x.EntityType == entityType).Select(x => x.Email).ToList();

            if (_emailService.SendEmail(emailPlaceholder.ToEmail, emailPlaceholder.Subject, emailPlaceholder.EmailBody, emailPlaceholder.EmailType, emailPlaceholder.EntityType!, emailPlaceholder.EntityId, ccEmails))
            {
                _emailService.CreateEmailSentDetails(emailPlaceholder);
                _logger.LogInfo($"Email notification successfully created and sent for the {emailPlaceholder.EntityType} with Id: {emailPlaceholder.EntityId}");
            }
        }



    }
}



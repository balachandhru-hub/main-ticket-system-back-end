using Contracts.IRepository;
using Contracts.IServices;
using Dto;
using Entities.DbContext;
using Microsoft.Extensions.Configuration;
using Contracts;
using Entities.Models;

namespace Repository
{
    public class RepositoryWrapper : IRepositoryWrapper
    {
        private IUserRepository _user;
        private IUserRoleMappingRepository _userRoleMapping;
        private IDraftServicePRRepository _draftServicePR;
        private IDraftServicePrAttachmentRepository _draftServicePRAttachment;

        private RepositoryContext _context;
        private IUserIdentityService _userIdentityService;
        private IAccessTokenRepository _accessToken;
        private IRoleFeatureMappingRepository _roleFeatureMapping;
        private IFeatureRepository _feature;
        private readonly string _dbConnectionString;
        private readonly IConfiguration _configuration;
        private readonly ILoggerManager _logger;
        private IAssetRepository _asset;
        private IMetadataRepository _metadata;
        private IPersonRepository? _person;
        private ITicketRepository _ticket;
        private IRoleRepository _role;
        private IApiKeyRepository _apiKey;
        private IConfigRepository _config;
        private IUserTicketCountRepository _userTicketCount;
        private ITicketMessageRepository _ticketMessage;
        private IDomainRepository _domain;
        private IApiConfigRepository _apiConfig;
        private IEmailFailedDetailRepository _emailFailedDetail;
        private IEmailSentDetailRepository _emailSentDetail;
        private IDomainPlanCreditRepository _domainPlanCredit;
        private ITicketPRRepository _ticketPR;
        private IPlanConfigRepository _planConfig;
        private IEmailCCListRepository _emailCCList;
        private IPurchaseOrderRepository _purchaseOrder;

        private IDomainSetting _domainSetting;
        private IDomainPlanRepository _domainPlan;
        private IPlanDetailsRepository _planDetails;
        private IDiscountCouponRepository _discountCoupon;
        private IDomainRealmRepository _domainRealm;
        private IDomainModelRepository _domainModel;
        private IEmailContentRepository _emailContent;
        private IPurchaseOrderMasterRepository _purchaseOrderMaster;
        private IPurchaseOrderLogRepository _purchaseOrderLog;
        private ISchedularHistoryRepository _schedularHistory;
        private IAribaTaskRepository _aribatask;
        private IAribaAttachmentRepository _aribaattachment;
        private IAribaInvoiceApprovalHistoryRepository _aribainvoiceapprovalhistory;
        private IAribaContractRepository _aribaContract;
        private IAribaContractDetailsRepository _aribaContractDetails;
        private IFinalAttachmentRepository _finalAttachment;
        private IFinalPRAttachmentRepository _finalPRAttachment;
        private IAribaInvoiceRepository _aribaInvoice;
        private IAribaCompanyMappingRepository _aribaCompanyMapping;
        private IAribaExtensionRepository _aribaExtension;
        private IAribaPRRepository _aribaPR;
        private IAribaPRAttachmentRepository _aribaPRAttachment;
        private IAribaPRTaskRepository _aribaPRTask;
        private IAribaPRHistoryRepository _aribaPRHistory;
        private IAribaContractHistoryRepository _aribaContractHistory;
        private IContractRepository _contractDetails;
        private IContractPendingRepository _contractPending;
        private IContractAttachmentRepository _contractAttachment;
        private IUserModelMappingRepository _userModelMapping;
        private IUserModelNameRepository _userModelName;
        private IAppUserMappingRepository _appUserMapping;
        private IAribaInvoiceUploadRepository _aribaInvoiceUpload;
        private IAribaPRUploadRepository _aribaPRUpload;
        private IAdditionalQueryRepository _additionalQuery;
        private ILinkRepository _link;
        private ILoginRecordRepository _loginRecord;
        private IMaterialPRRepository _materialPR;
        private IMaterialInvoiceRepository _materialInvoice;
        private IAribaPaymentRepository _aribaPayment;
        private IServicePRRepository _servicePR;
        private IFinalContractAttachmentRepository _finalContractAttachment;
        private IAribaContractUploadRepository _aribaContractUpload;
        private IMaterialPRAttachmentRepository _materialPRRepository;
        private IDraftMaterialPRRepository _draftMaterialPR;
        private IDraftMaterialPRAttachmentRepository _draftMaterialPRAttachment;
        private IAribaSupplierMappingRepository _aribaSupplierName;
        private IAribaVenueListMappingRepository _aribaVenueListMapping;
        private IFinalPurchaseOrderRepository _finalPurchaseOrder;
        private IPurchaseOrderBotProcessRepository _purchaseOrderBotProcess;
        private IPurchaseOrderBotReprocessingLogRepository _purchaseOrderBotReprocessingLog;
        private IPropertyCrossChargeRepository _propertyCrossCharge;
        private IPublicPropertyRepository _publicProperty;
        private IPublicRelationCategoryRepository _publicRelationCategory;
        private IServicePRInvoiceRepository _servicePRInvoice;
        private IServicePRAttachmentRepository _servicePRAttachment;
        private IConciergeVenueRepository _conciergeVenue;
        private IRequesterNameRepository _requesterName;
        private IAribaCurrencyRepository _aribaCurrency;
        private ILegalDepartmentMaterialRepository _legalDepartmentMaterial;
        private IContractSupplierRepository _contractSupplier;
        private IContractRegionRepository _contractRegion;
        private IContractDepartmentRepository _contractDepartment;
        private IContractComoditiesRepository _contractComodities;
        private IContractCustomTypeRepository _contractCustomType;
        private IEasyContractRepository _easyContract;
        private IEasyContractDepartmentRepository _easyContractDepartment;
        private IEasyContractComoditiesRepository _easyContractComodities;
        private IEasyContractRegionRepository _easyContractRegion;
        private ILegalMaterialCodeRepository _legalMaterialCode;
        private ICountryListRepository _countryListRepository;
        private IUserAppMappingRepository _userAppMapping;
        private IAppModelMappingRepository _appModelMapping;
        


        public RepositoryWrapper(RepositoryContext repositoryContext, IUserIdentityService userIdentityService, IConfiguration configuration, ILoggerManager logger)
        {
            _context = repositoryContext;
            _userIdentityService = userIdentityService;
            _logger = logger;
            _configuration = configuration;
            _dbConnectionString = configuration.GetConnectionString("DefaultConnection")!;
        }
        public IUserRepository User
        {
            get
            {
                if (_user == null)
                {
                    _user = new UserRepository(_context, _logger);
                }
                return _user;
            }
        }
        public IDraftServicePRRepository DraftServicePR
        {
            get
            {
                if (_draftServicePR == null)
                {
                    _draftServicePR = new DraftServicePRRepository(_context, _logger, _configuration);
                }
                return _draftServicePR;
            }
        }
        public ILegalDepartmentMaterialRepository LegalDepartmentMaterial
        {
            get
            {
                if (_legalDepartmentMaterial == null)
                {
                    _legalDepartmentMaterial = new LegalDepartmentMaterialRepository(_context);
                }
                return _legalDepartmentMaterial;
            }
        }

        public IDraftServicePrAttachmentRepository DraftServicePRAttachment
        {
            get
            {
                if (_draftServicePRAttachment == null)
                {
                    _draftServicePRAttachment = new DraftServicePrAttachmentRepository(_context, _logger, _configuration);
                }
                return _draftServicePRAttachment;
            }
        }

        public IUserRoleMappingRepository UserRoleMapping
        {
            get
            {
                if (_userRoleMapping == null)
                {
                    _userRoleMapping = new UserRoleMappingRepository(_context);
                }
                return _userRoleMapping;
            }
        }
        public IAribaSupplierMappingRepository AribaSupplierName
        {
            get
            {
                if (_aribaSupplierName == null)
                {
                    _aribaSupplierName = new AribaSupplierMappingRepository(_context);
                }
                return _aribaSupplierName;
            }
        }
          public IContractSupplierRepository ContractSupplier
        {
            get
            {
                if (_contractSupplier == null)
                {
                    _contractSupplier = new ContractSupplierRepository(_context);
                }
                return _contractSupplier;
            }
        }
        public ILegalMaterialCodeRepository LegalMaterialCode
        {
            get
            {
                if (_legalMaterialCode == null)
                {
                    _legalMaterialCode = new LegalMaterialCodeRepository(_context);
                }
                return _legalMaterialCode;
            }
        }
          public IContractCustomTypeRepository ContractCustomType
        {
            get
            {
                if (_contractCustomType == null)
                {
                    _contractCustomType = new ContractCustomTypeRepository(_context);
                }
                return _contractCustomType;
            }
        }
 public IEasyContractRepository EasyContract
        {
            get
            {
                if (_easyContract == null)
                {
                    _easyContract = new EasyContractRepository(_context);
                }
                return _easyContract;
            }
        }

            public IContractRegionRepository ContractRegion
            {
                get
                {
                    if (_contractRegion == null)
                    {
                        _contractRegion = new ContractRegionRepository(_context);
                    }
                    return _contractRegion;
                }
            }
             public IContractDepartmentRepository ContractDepartment
        {
            get
            {
                if (_contractDepartment == null)
                {
                    _contractDepartment = new ContractDepartmentRepository(_context);
                }
                return _contractDepartment;
            }
        }
         public IContractComoditiesRepository ContractComodities
        {
            get
            {
                if (_contractComodities == null)
                {
                    _contractComodities = new ContractComoditiesRepository(_context);
                }
                return _contractComodities;
            }
        }
        public IEasyContractDepartmentRepository  EasyContractDepartment
        {
            get
            {
                if (_easyContractDepartment == null)
                {
                    _easyContractDepartment = new EasyContractDepartmentRepository(_context);
                }
                return _easyContractDepartment;
            }
        }   
        public ICountryListRepository CountryList
        {
            get
            {
                if (_countryListRepository == null)
                {
                    _countryListRepository = new CountryListRepository(_context);
                }
                return _countryListRepository;
            }
        }
        public IUserAppMappingRepository UserAppMapping
        {
            get
            {
                if (_userAppMapping == null)
                {
                    _userAppMapping = new UserAppMappingRepository(_context);
                }
                return _userAppMapping;
            }
        }
        public IEasyContractRegionRepository  EasyContractRegion
        {
            get
            {
                if (_easyContractRegion == null)
                {
                    _easyContractRegion = new EasyContractRegionRepository(_context);
                }
                return _easyContractRegion;
            }
        }
        public IEasyContractComoditiesRepository EasyContractComodities
        {
            get
            {
                if (_easyContractComodities == null)
                {
                    _easyContractComodities = new EasyContractComoditiesRepository(_context);
                }
                return _easyContractComodities;
            }
        }
        public IRequesterNameRepository RequesterName
        {
            get
            {
                if (_requesterName == null)
                {
                    _requesterName = new RequesterNameRepository(_context);
                }
                return _requesterName;
            }
        }
        public IAccessTokenRepository AccessToken
        {
            get
            {
                if (_accessToken == null)
                {
                    _accessToken = new AccessTokenRepository(_context);
                }
                return _accessToken;
            }
        }
        public IAribaVenueListMappingRepository AribaVenueListMapping
        {
            get
            {
                if (_aribaVenueListMapping == null)
                {
                    _aribaVenueListMapping = new AribaVenueListMappingRepository(_context);
                }
                return _aribaVenueListMapping;
            }
        }
        public IMaterialPRAttachmentRepository MaterialPRAttachment
        {
            get
            {
                if (_materialPRRepository == null)
                {
                    _materialPRRepository = new MaterialPRAttachmentRepository(_context, _logger, _configuration);
                }
                return _materialPRRepository;
            }
        }
        public IMaterialPRRepository MaterialPR
        {
            get
            {
                if (_materialPR == null)
                {
                    _materialPR = new MaterialPRRepository(_context, _logger, _configuration);
                }
                return _materialPR;
            }
        }
        public IConciergeVenueRepository ConciergeVenue
        {
            get
            {
                if (_conciergeVenue == null)
                {
                    _conciergeVenue = new ConciergeVenueRepository(_context);
                }
                return _conciergeVenue;
            }
        }
        public IServicePRInvoiceRepository ServicePRInvoice
        {
            get
            {
                if (_servicePRInvoice == null)
                {
                    _servicePRInvoice = new ServicePRInvoiceRepository(_context, _logger, _configuration);
                }
                return _servicePRInvoice;
            }
        }

        public IServicePRAttachmentRepository ServicePRAttachment
        {
            get
            {
                if (_servicePRAttachment == null)
                {
                    _servicePRAttachment = new ServicePRAttachmentRepository(_context, _logger, _configuration);
                }
                return _servicePRAttachment;
            }
        }
        public IMaterialInvoiceRepository MaterialInvoice
        {
            get
            {
                if (_materialInvoice == null)
                {
                    _materialInvoice = new MaterialInvoiceRepository(_context, _logger, _configuration);
                }
                return _materialInvoice;
            }
        }
        public IAribaCurrencyRepository AribaCurrency
        {
            get
            {
                if (_aribaCurrency == null)
                {
                    _aribaCurrency = new AribaCurrencyRepository(_context);
                }
                return _aribaCurrency;
            }
        }
        public IServicePRRepository ServicePR
        {
            get
            {
                if (_servicePR == null)
                {
                    _servicePR = new ServicePRRepository(_context, _logger, _configuration);
                }
                return _servicePR;
            }
        }
        public IPublicRelationCategoryRepository PublicRelationCategory
        {
            get
            {
                if (_publicRelationCategory == null)
                {
                    _publicRelationCategory = new PublicRelationCategoryRepository(_context);
                }
                return _publicRelationCategory;
            }
        }
        public IDraftMaterialPRRepository DraftMaterialPR
        {
            get
            {
                if (_draftMaterialPR == null)
                {
                    _draftMaterialPR = new DraftMaterialPRRepository(_context, _logger, _configuration);
                }
                return _draftMaterialPR;
            }
        }
        public IPublicPropertyRepository PublicProperty
        {
            get
            {
                if (_publicProperty == null)
                {
                    _publicProperty = new PublicPropertyRepository(_context);
                }
                return _publicProperty;
            }
        }

        public IDraftMaterialPRAttachmentRepository DraftMaterialPRAttachment
        {
            get
            {
                if (_draftMaterialPRAttachment == null)
                {
                    _draftMaterialPRAttachment = new DraftMaterialPRAttachmentRepository(_context, _logger, _configuration);
                }
                return _draftMaterialPRAttachment;
            }
        }

        public IRoleFeatureMappingRepository RoleFeatureMapping
        {
            get
            {
                if (_roleFeatureMapping == null) _roleFeatureMapping = new RoleFeatureMappingRepository(_context, _logger, _configuration, _dbConnectionString);
                return _roleFeatureMapping;
            }
        }
        public IAribaPRUploadRepository AribaPRUpload
        {
            get
            {
                if (_aribaPRUpload == null)
                {
                    _aribaPRUpload = new AribaPRUploadRepository(_context);
                }
                return _aribaPRUpload;
            }
        }

        public IAribaContractUploadRepository AribaContractUpload
        {
            get
            {
                if (_aribaContractUpload == null)
                {
                    _aribaContractUpload = new AribaContractUploadRepository(_context);
                }
                return _aribaContractUpload;
            }
        }
        public IFinalContractAttachmentRepository FinalContractAttachment
        {
            get
            {
                if (_finalContractAttachment == null)
                {
                    _finalContractAttachment = new FinalContractAttachmentRepository(_context);
                }
                return _finalContractAttachment;
            }
        }

        public IFeatureRepository Feature
        {
            get
            {
                if (_feature == null) _feature = new FeatureRepository(_context);
                return _feature;
            }
        }
        public IAribaInvoiceUploadRepository AribaInvoiceUpload
        {
            get
            {
                if (_aribaInvoiceUpload == null)
                {
                    _aribaInvoiceUpload = new AribaInvoiceUploadRepository(_context);
                }
                return _aribaInvoiceUpload;
            }
        }

        public ILoginRecordRepository LoginRecord
        {
            get
            {
                if (_loginRecord == null)
                {
                    _loginRecord = new LoginRecordRepository(_context);
                }
                return _loginRecord;
            }
        }

        public IAssetRepository Asset
        {
            get
            {
                if (_asset == null) _asset = new AssetRepository(_context, _configuration, _logger, _dbConnectionString);
                return _asset;
            }
        }

        public IMetadataRepository Metadata
        {
            get
            {
                if (_metadata == null) _metadata = new MetadataRepository(_context);
                return _metadata;
            }
        }

        public IPersonRepository Person
        {
            get
            {
                if (_person == null)
                {
                    _person = new PersonRepository(_context, _logger, _configuration);
                    // _person = new PersonRepository(_context, _logger);
                }
                return _person;
            }
        }

        public ITicketRepository Ticket
        {
            get
            {
                if (_ticket == null)
                {
                    _ticket = new TicketRepository(_context, _logger);

                }
                return _ticket;
            }
        }
        public IContractRepository ContractDetails
        {
            get
            {
                if (_contractDetails == null)
                {
                    _contractDetails = new ContractRepository(_context);
                }
                return _contractDetails;
            }
        }
        public IContractPendingRepository ContractPending
        {
            get
            {
                if (_contractPending == null)
                {
                    _contractPending = new ContractPendingRepository(_context);
                }
                return _contractPending;
            }
        }
        public IContractAttachmentRepository ContractAttachment
        {
            get
            {
                if (_contractAttachment == null)
                {
                    _contractAttachment = new ContractAttachmentRepository(_context);
                }
                return _contractAttachment;
            }
        }
        public IAribaContractHistoryRepository AribaContractHistory
        {
            get
            {
                if (_aribaContractHistory == null)
                {
                    _aribaContractHistory = new AribaContractHistoryRepository(_context);
                }
                return _aribaContractHistory;
            }
        }
        public ISchedularHistoryRepository SchedularHistory
        {
            get
            {
                if (_schedularHistory == null)
                {
                    _schedularHistory = new SchedularHistoryRepository(_context, _logger);
                }
                return _schedularHistory;
            }
        }


        public IRoleRepository Role
        {
            get
            {
                if (_role == null)
                {
                    _role = new RoleRepository(_context);
                }
                return _role;
            }
        }
        public IUserModelMappingRepository UserModelMapping
        {
            get
            {
                if (_userModelMapping == null)
                {
                    _userModelMapping = new UserModelMappingRepository(_context);
                }
                return _userModelMapping;
            }
        }
        public IUserModelNameRepository UserModelName
        {
            get
            {
                if (_userModelName == null)
                {
                    _userModelName = new UserModelNameRepository(_context);
                }
                return _userModelName;
            }
        }
        public IAppUserMappingRepository AppUserMapping
        {
            get
            {
                if (_appUserMapping == null)
                {
                    _appUserMapping = new AppUserMappingRepository(_context);
                }
                return _appUserMapping;
            }
        }

        public IAdditionalQueryRepository AdditionalQuery
        {
            get
            {
                if (_additionalQuery == null)
                {
                    _additionalQuery = new AdditionalQueryRepository(_context);
                }
                return _additionalQuery;
            }
        }

        public IApiKeyRepository ApiKey
        {
            get
            {
                if (_apiKey == null)
                {
                    _apiKey = new ApiKeyRepository(_context);
                }
                return _apiKey;
            }
        }

        public IConfigRepository Config
        {
            get
            {
                if (_config == null)
                {
                    _config = new ConfigRepository(_context);
                }
                return _config;
            }
        }

        public IUserTicketCountRepository UserTicketCount
        {
            get
            {
                if (_userTicketCount == null)
                {
                    _userTicketCount = new UserTicketCountRepository(_context);
                }
                return _userTicketCount;
            }
        }

        public ITicketMessageRepository TicketMessage
        {
            get
            {
                if (_ticketMessage == null)
                {
                    _ticketMessage = new TicketMessageRepository(_context, _logger);
                }
                return _ticketMessage;
            }
        }
        public IDomainRepository Domain
        {
            get
            {
                if (_domain == null)
                {
                    _domain = new DomainRepository(_context, _logger);
                }
                return _domain;
            }
        }
        public IDomainPlanCreditRepository DomainPlanCredit
        {
            get
            {
                if (_domainPlanCredit == null)
                {
                    _domainPlanCredit = new DomainPlanCreditRepository(_context, _logger);
                }
                return _domainPlanCredit;
            }
        }


        // Add this property for DomainPlan
        public IDomainPlanRepository DomainPlan
        {
            get
            {
                if (_domainPlan == null)
                {
                    _domainPlan = new DomainPlanRepository(_context, _logger);
                }
                return _domainPlan;
            }
        }
        public IPlanDetailsRepository PlanDetails
        {
            get
            {
                if (_planDetails == null)
                {
                    _planDetails = new PlanDetailsRepository(_context, _logger);
                }
                return _planDetails;
            }
        }

        public ITicketPRRepository TicketPR
        {
            get
            {
                if (_ticketPR == null)
                {
                    _ticketPR = new TicketPRRepository(_context);
                }
                return _ticketPR;
            }
        }

        public IApiConfigRepository ApiConfig
        {
            get
            {
                if (_apiConfig == null)
                {
                    _apiConfig = new ApiConfigRepository(_context);
                }
                return _apiConfig;
            }
        }

        public IEmailFailedDetailRepository EmailFailedDetail
        {
            get
            {
                if (_emailFailedDetail == null)
                {
                    _emailFailedDetail = new EmailFailedDetailRepository(_context);
                }
                return _emailFailedDetail;
            }
        }


        public IEmailSentDetailRepository EmailSentDetail
        {
            get
            {
                if (_emailSentDetail == null)
                {
                    _emailSentDetail = new EmailSentDetailRepository(_context);
                }
                return _emailSentDetail;
            }
        }

        public IDiscountCouponRepository DiscountCoupon
        {
            get
            {
                if (_discountCoupon == null)
                {
                    _discountCoupon = new DiscountCoupenRepository(_context);
                }
                return _discountCoupon;
            }
        }
        public IPlanConfigRepository PlanConfig
        {
            get
            {
                if (_planConfig == null)
                {
                    _planConfig = new PlanConfigRepository(_context);
                }
                return _planConfig;
            }
        }
        public IDomainRealmRepository DomainRealm
        {
            get
            {
                if (_domainRealm == null)
                {
                    _domainRealm = new DomainRealmRepository(_context);
                }
                return _domainRealm;
            }
        }
        public IDomainModelRepository DomainModel
        {
            get
            {
                if (_domainModel == null)
                {
                    _domainModel = new DomainModelRepository(_context);
                }
                return _domainModel;
            }
        }

        public IEmailContentRepository EmailContent
        {
            get
            {
                if (_emailContent == null)
                {
                    _emailContent = new EmailContentRepository(_context);
                }
                return _emailContent;
            }
        }
        public IEmailCCListRepository EmailCCList
        {
            get
            {
                if (_emailCCList == null)
                {
                    _emailCCList = new EmailCCListRepository(_context);
                }
                return _emailCCList;
            }
        }

        public IPurchaseOrderRepository PurchaseOrder
        {
            get
            {
                if (_purchaseOrder == null)
                {
                    _purchaseOrder = new PurchaseOrderRepository(_context, _logger);
                }
                return _purchaseOrder;
            }
        }

        public IDomainSetting DomainSetting
        {
            get
            {
                if (_domainSetting == null)
                {
                    _domainSetting = new DomainSettingRepository(_context);
                }
                return _domainSetting;
            }
        }
        public IFinalAttachmentRepository FinalAttachment
        {
            get
            {
                if (_finalAttachment == null)
                {
                    _finalAttachment = new FinalAttachmentRepository(_context);
                }
                return _finalAttachment;
            }
        }
        public IFinalPRAttachmentRepository FinalPRAttachment
        {
            get
            {
                if (_finalPRAttachment == null)
                {
                    _finalPRAttachment = new FinalPRAttachmentRepository(_context);
                }
                return _finalPRAttachment;
            }
        }

        public IPurchaseOrderMasterRepository PurchaseOrderMaster
        {
            get
            {
                if (_purchaseOrderMaster == null)
                {
                    _purchaseOrderMaster = new PurchaseOrderRepositoryMaster(_context);
                }
                return _purchaseOrderMaster;
            }
        }
        public IPurchaseOrderLogRepository PurchaseOrderLog
        {
            get
            {
                if (_purchaseOrderLog == null)
                {
                    _purchaseOrderLog = new PurchaseOrderLogRepository(_context);
                }
                return _purchaseOrderLog;
            }
        }

        public IPurchaseOrderBotProcessRepository PurchaseOrderBotProcess
        {
            get
            {
                if (_purchaseOrderBotProcess == null)
                {
                    _purchaseOrderBotProcess = new PurchaseOrderBotProcessRepository(_context, _logger);
                }
                return _purchaseOrderBotProcess;
            }
        }
        public IPurchaseOrderBotReprocessingLogRepository PurchaseOrderBotReprocessingLog
        {
            get
            {
                if (_purchaseOrderBotReprocessingLog == null)
                {
                    _purchaseOrderBotReprocessingLog = new PurchaseOrderBotReprocessingLogRepository(_context, _logger);
                }
                return _purchaseOrderBotReprocessingLog;
            }
        }
        public IAribaInvoiceRepository AribaInvoice
        {
            get
            {
                if (_aribaInvoice == null)
                {
                    _aribaInvoice = new AribaInvoiceRepository(_context);
                }
                return _aribaInvoice;
            }
        }


        public IAribaTaskRepository AribaTask
        {
            get
            {
                if (_aribatask == null)
                {
                    _aribatask = new AribaTaskRepository(_context);
                }
                return _aribatask;
            }
        }
        public IAribaAttachmentRepository AribaAttachment
        {
            get
            {
                if (_aribaattachment == null)
                {
                    _aribaattachment = new AribaAttachmentRepository(_context);
                }
                return _aribaattachment;
            }
        }
        public IAribaInvoiceApprovalHistoryRepository AribaInvoiceApprovalHistory
        {
            get
            {
                if (_aribainvoiceapprovalhistory == null)
                {
                    _aribainvoiceapprovalhistory = new AribaInvoiceApprovalHistoryRepository(_context);
                }
                return _aribainvoiceapprovalhistory;
            }
        }

        public IAribaContractRepository AribaContract
        {
            get
            {
                if (_aribaContract == null)
                {
                    _aribaContract = new AribaContractRepository(_context);
                }
                return _aribaContract;
            }
        }
        public IAribaContractDetailsRepository AribaContractDetails
        {
            get
            {
                if (_aribaContractDetails == null)
                {
                    _aribaContractDetails = new AribaContractDetailsRepository(_context);
                }
                return _aribaContractDetails;
            }
        }
        public IAribaPRRepository AribaPR
        {
            get
            {
                if (_aribaPR == null)
                {
                    _aribaPR = new AribaPRRepository(_context, _logger, _configuration);
                }
                return _aribaPR;
            }
        }
        public IAribaPRAttachmentRepository AribaPRAttachment
        {
            get
            {
                if (_aribaPRAttachment == null)
                {
                    _aribaPRAttachment = new AribaPRAttachmentRepository(_context);
                }
                return _aribaPRAttachment;
            }
        }
        public IAribaPRTaskRepository AribaPRTask
        {
            get
            {
                if (_aribaPRTask == null)
                {
                    _aribaPRTask = new AribaPRTaskRepository(_context);
                }
                return _aribaPRTask;
            }
        }
        public IAribaPRHistoryRepository AribaPRHistory
        {
            get
            {
                if (_aribaPRHistory == null)
                {
                    _aribaPRHistory = new AribaPRHistoryRepository(_context);
                }
                return _aribaPRHistory;
            }
        }




        public IAribaCompanyMappingRepository AribaCompanyMapping
        {
            get
            {
                if (_aribaCompanyMapping == null)
                {
                    _aribaCompanyMapping = new AribaCompanyMappingRepository(_context);
                }
                return _aribaCompanyMapping;
            }
        }

        public IPropertyCrossChargeRepository PropertyCrossCharge
        {
            get
            {
                if (_propertyCrossCharge == null)
                {
                    _propertyCrossCharge = new PropertyCrossChargeRepository(_context);
                }
                return _propertyCrossCharge;
            }
        }
        public IAribaExtensionRepository AribaExtension
        {
            get
            {
                if (_aribaExtension == null)
                {
                    _aribaExtension = new AribaExtensionRepository(_context);
                }
                return _aribaExtension;
            }
        }
        public ILinkRepository Link
        {
            get
            {
                if (_link == null)
                {
                    _link = new LinkRepository(_context);
                }
                return _link;
            }
        }
        public IAribaPaymentRepository AribaPayment
        {
            get
            {
                if (_aribaPayment == null)
                {
                    _aribaPayment = new AribaPaymentRepository(_context, _logger, _configuration);
                }
                return _aribaPayment;
            }
        }
        public IFinalPurchaseOrderRepository FinalPurchaseOrder
        {
            get
            {
                if (_finalPurchaseOrder == null)
                {
                    _finalPurchaseOrder = new FinalPurchaseOrderRepository(_context, _logger);
                }
                return _finalPurchaseOrder;
            }
        }
         public IAppModelMappingRepository AppModelMapping
        {
            get
            {
                if (_appModelMapping == null)
                {
                    _appModelMapping = new AppModelMappingRepository(_context);
                }
                return _appModelMapping;
            }
        }
        public bool Save()
        {
            _context.OnBeforeSaving(_userIdentityService.GetCurrentUser());
            _context.SaveChanges();
            return true;
        }

        public async Task<bool> SaveAsync()
        {
            _context.OnBeforeSaving(_userIdentityService.GetCurrentUser());
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
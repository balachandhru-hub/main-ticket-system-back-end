namespace Contracts.IRepository
{
    /// <summary>
    /// Repository Wrapper class holding every instance of repository.
    /// </summary>
    public interface IRepositoryWrapper
    {

        IRoleFeatureMappingRepository RoleFeatureMapping { get; }
        IAccessTokenRepository AccessToken { get; }
        IDraftServicePrAttachmentRepository DraftServicePRAttachment { get; }
        IDraftServicePRRepository DraftServicePR { get; }
        
        IFeatureRepository Feature { get; }
        IAssetRepository Asset { get; }
        IMetadataRepository Metadata { get; }
        IPersonRepository Person { get; }
        ITicketRepository Ticket { get; }
        IUserRepository User { get; }
        IUserRoleMappingRepository UserRoleMapping { get; }       
        IRoleRepository Role { get; }
        IApiKeyRepository ApiKey { get; }
        IConfigRepository Config { get; }
        IUserTicketCountRepository UserTicketCount { get; }
        ITicketMessageRepository TicketMessage { get; }
        IDomainRepository Domain { get; }
        IApiConfigRepository ApiConfig { get; }
        IEmailFailedDetailRepository EmailFailedDetail { get; }
        IEmailSentDetailRepository EmailSentDetail { get; }
        IDomainPlanCreditRepository DomainPlanCredit { get; }
        IDomainPlanRepository DomainPlan { get; }
        IPlanDetailsRepository PlanDetails { get; }
        IDiscountCouponRepository DiscountCoupon { get; }
        ITicketPRRepository TicketPR { get; }
        IPlanConfigRepository PlanConfig { get; }
        IDomainRealmRepository DomainRealm { get; }
        IDomainModelRepository DomainModel { get; }
        IEmailContentRepository EmailContent { get; }
        IEmailCCListRepository EmailCCList { get; }
        IPurchaseOrderRepository PurchaseOrder { get; }
        IDomainSetting DomainSetting { get; }
        IPurchaseOrderMasterRepository PurchaseOrderMaster { get; }
        IPurchaseOrderLogRepository PurchaseOrderLog { get; }
        IPurchaseOrderBotProcessRepository PurchaseOrderBotProcess { get; }
        IPurchaseOrderBotReprocessingLogRepository PurchaseOrderBotReprocessingLog { get; }
         IAribaInvoiceRepository AribaInvoice { get; }
         IAribaTaskRepository AribaTask { get; }
        IAribaAttachmentRepository AribaAttachment { get; }
        IAribaInvoiceApprovalHistoryRepository AribaInvoiceApprovalHistory { get; }
        IAribaContractRepository AribaContract { get; }
        IAribaContractDetailsRepository AribaContractDetails { get; }
        IFinalAttachmentRepository FinalAttachment { get; }
        IFinalPRAttachmentRepository FinalPRAttachment { get; }
        IAribaCompanyMappingRepository AribaCompanyMapping { get; }
        IAribaExtensionRepository AribaExtension { get; }
        IAribaPRRepository AribaPR{ get; }
        IAribaPRTaskRepository AribaPRTask { get; }
        IAribaPRAttachmentRepository AribaPRAttachment { get; }
        IAribaPRHistoryRepository AribaPRHistory { get; }
        IAribaContractHistoryRepository AribaContractHistory { get; }
        IContractRepository ContractDetails { get; }
        IContractPendingRepository ContractPending { get; }
        IContractAttachmentRepository ContractAttachment { get; }
        IUserModelMappingRepository UserModelMapping { get; }
        IUserModelNameRepository UserModelName { get; }
        IAppUserMappingRepository AppUserMapping { get; }
        IAdditionalQueryRepository AdditionalQuery { get; }
        IAribaInvoiceUploadRepository AribaInvoiceUpload { get; }
        IAribaPRUploadRepository AribaPRUpload { get; }
        ILinkRepository Link { get; }
        ILoginRecordRepository LoginRecord { get; }
        IMaterialPRRepository MaterialPR { get; }
        IMaterialInvoiceRepository MaterialInvoice { get; }
        IFinalContractAttachmentRepository FinalContractAttachment { get; }
        
         
        ISchedularHistoryRepository SchedularHistory { get; }
        IAribaPaymentRepository AribaPayment { get; }
        IServicePRRepository ServicePR { get; }
        IDraftMaterialPRRepository DraftMaterialPR { get; }
        IDraftMaterialPRAttachmentRepository DraftMaterialPRAttachment { get; }
        IAribaContractUploadRepository AribaContractUpload {get;}
        IMaterialPRAttachmentRepository MaterialPRAttachment {get;}
        IAribaSupplierMappingRepository AribaSupplierName { get; }
        IAribaVenueListMappingRepository AribaVenueListMapping { get; }
        IFinalPurchaseOrderRepository FinalPurchaseOrder { get; }
        IPropertyCrossChargeRepository PropertyCrossCharge { get; }
        IPublicPropertyRepository PublicProperty { get; }
        IPublicRelationCategoryRepository PublicRelationCategory { get; }
        IConciergeVenueRepository ConciergeVenue { get; }
        IServicePRAttachmentRepository ServicePRAttachment { get; }
        IServicePRInvoiceRepository ServicePRInvoice { get; }
        IRequesterNameRepository RequesterName { get; }
        IAribaCurrencyRepository AribaCurrency { get; }
        ILegalDepartmentMaterialRepository LegalDepartmentMaterial { get; }
        IContractSupplierRepository ContractSupplier { get; }
        IContractDepartmentRepository ContractDepartment {get; }
        IContractComoditiesRepository ContractComodities {get; }
        IContractRegionRepository ContractRegion {get;}
        IContractCustomTypeRepository ContractCustomType {get;}
        ILegalMaterialCodeRepository LegalMaterialCode {get;}
        IEasyContractRepository EasyContract {get;}
        IEasyContractDepartmentRepository EasyContractDepartment {get;}
        IEasyContractComoditiesRepository EasyContractComodities {get;}
        IEasyContractRegionRepository EasyContractRegion {get;}
        ICountryListRepository CountryList { get; }
        IUserAppMappingRepository UserAppMapping { get; }
        IAppModelMappingRepository AppModelMapping{get;}
        bool Save();
        Task<bool> SaveAsync();
    }
}
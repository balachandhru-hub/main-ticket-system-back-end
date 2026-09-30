using Microsoft.EntityFrameworkCore;
using Entities.Models;
using Entities.Util;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using System.Reflection.Emit;
// <-- Add this line at the top
// <-- Add this line at the top

namespace Entities.DbContext
{
    /// <summary>
    /// Class <c>RepositoryContext</c> acts as a middleware component
    /// for the communication with the database and the transactions.
    /// </summary>
    public class RepositoryContext : Microsoft.EntityFrameworkCore.DbContext
    {
        private readonly IConfiguration _configuration;
        public RepositoryContext(DbContextOptions<RepositoryContext> options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
        }

        public DbSet<Person> Person { get; set; }
        public DbSet<AccessToken> AccessToken { get; set; }
        public DbSet<Asset> Asset { get; set; }
        public DbSet<Feature> Feature { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<RoleFeatureMapping> RoleFeatureMapping { get; set; }
        public DbSet<Ticket> Ticket { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<UserRoleMapping> UserRoleMapping { get; set; }
        public DbSet<Metadata> Metadata { get; set; }
        public DbSet<ApiKey> ApiKey { get; set; }
        public DbSet<Config> Config { get; set; }
        public DbSet<UserTicketCount> UserTicketCount { get; set; }
        public DbSet<TicketMessage> TicketMessage { get; set; }
        public DbSet<TicketPR> TicketPR { get; set; }
        public DbSet<Domain> Domain { get; set; }
        public DbSet<DomainPlan> DomainPlan { get; set; }
        public DbSet<DiscountCoupon> DiscountCoupon { get; set; }
        public DbSet<PlanDetails> PlanDetails { get; set; }

        public DbSet<AribaUsageProfile> AribaUsageProfile { get; set; }
        public DbSet<EmailContent> EmailContent { get; set; }
        public DbSet<EmailSentDetail> EmailSentDetail { get; set; }
        public DbSet<EmailFailedDetail> EmailFailedDetail { get; set; }
        public DbSet<ApiConfig> ApiConfig { get; set; }
        public DbSet<DomainRealm> DomainRealm { get; set; }
        public DbSet<DomainPlanCredit> DomainPlanCredit { get; set; }
        public DbSet<PlanConfig> PlanConfig { get; set; }
        public DbSet<DomainModel> DomainModel { get; set; }
        public DbSet<EmailCCList> EmailCCList { get; set; }
        public DbSet<DomainSetting> DomainSetting { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrder { get; set; }
        public DbSet<SchedularHistory> SchedularHistory { get; set; }
        public DbSet<PurchaseOrderMaster> PurchaseOrderMaster { get; set; }
        public DbSet<PurchaseOrderLog> PurchaseOrderLog { get; set; }
        public DbSet<AribaInvoice> AribaInvoice { get; set; }
        public DbSet<AribaExtension> AribaExtension { get; set; }
        public DbSet<AribaTasks> AribaTasks { get; set; }
        public DbSet<AribaAttachment> AribaAttachment { get; set; }
        public DbSet<AribaInvoiceApprovalHistory> AribaInvoiceApprovalHistory { get; set; }
        public DbSet<AribaJobSchedular> AribaJobSchedular { get; set; }
        public DbSet<PRAribaData> PRAribaData { get; set; }
        public DbSet<AribaContract> AribaContract { get; set; }
        public DbSet<AribaContractDetails> AribaContractDetails { get; set; }
        public DbSet<FinalAttachment> FinalAttachment { get; set; }
        public DbSet<FinalPRAttachment> FinalPRAttachment { get; set; }
        public DbSet<AribaCompanyMapping> AribaCompanyMapping { get; set; }
        public DbSet<AribaPR> AribaPR { get; set; }
        public DbSet<ContractAttachment> ContractAttachment { get; set; }
        public DbSet<ContractPending> ContractPending { get; set; }
        public DbSet<ContractDetails> ContractDetails { get; set; }
        public DbSet<AdditionalQuery> AdditionalQuery { get; set; }
        public DbSet<AribaContractHistory> AribaContractHistory { get; set; }
        public DbSet<UserModelName> UserModelName { get; set; }
        public DbSet<UserModelMapping> UserModelMapping { get; set; }
        public DbSet<Link> Link { get; set; }
        public DbSet<AribaInvoiceUpload> AribaInvoiceUpload { get; set; }
        public DbSet<AribaPRUpload> AribaPRUpload { get; set; }
        public DbSet<LoginRecord> LoginRecord { get; set; }
        public DbSet<MaterialPR> MaterialPR { get; set; }
        public DbSet<MaterialPRLog> MaterialPRLog { get; set; }
        public DbSet<AribaPayment> AribaPayment { get; set; }
        public DbSet<ServicePR> ServicePR { get; set; }
        public DbSet<ServicePRLog> ServicePRLog { get; set; }
        public DbSet<AribaContractUpload> AribaContractUpload { get; set; }
        public DbSet<MaterialPRAttachment> MaterialPRAttachment { get; set; }
        public DbSet<FinalContractAttachment> FinalContractAttachment { get; set; }
        public DbSet<PRLog> PRLog { get; set; }
        public DbSet<AribaSupplierName> AribaSupplierName { get; set; }
        public DbSet<AribaVenueListMapping> AribaVenueListMapping { get; set; }
        public DbSet<FinalPurchaseOrder> FinalPurchaseOrder { get; set; }
        public DbSet<DraftMaterialPR> DraftMaterialPR { get; set; }
        public DbSet<DraftMaterialPRAttachment> DraftMaterialPRAttachment { get; set; }
        public DbSet<PurchaseOrderBotProcess> PurchaseOrderBotProcess { get; set; }
        public DbSet<PurchaseOrderBotReprocessingLog> PurchaseOrderBotReprocessingLog { get; set; }
        public DbSet<MaterialInvoice> MaterialInvoice { get; set; }
        public DbSet<PropertyCrossCharge> PropertyCrossCharge { get; set; }
        public DbSet<PublicProperty> PublicProperty { get; set; }
        public DbSet<PublicRelationCategory> PublicRelationCategory { get; set; }
        public DbSet<ConciergeVenue> ConciergeVenue { get; set; }
        public DbSet<ServicePRAttachment> ServicePRAttachment { get; set; }
        public DbSet<ServicePRInvoice> ServicePRInvoice { get; set; }
        public DbSet<DraftServicePR> DraftServicePR { get; set; }
        public DbSet<DraftServicePrAttachment> DraftServicePrAttachment { get; set; }
        public DbSet<RequesterName> RequesterName { get; set; }
        public DbSet<AribaCurrency> AribaCurrency { get; set; }
        public DbSet<LegalVenueList> LegalVenueList { get; set; }
        public DbSet<ContractRegion> ContractRegion { get; set; }
        public DbSet<ContractDepartment> ContractDepartment { get; set; }
        public DbSet<ContractSupplier> ContractSupplier { get; set; }
        public DbSet<ContractCustomType> ContractCustomType { get; set; }
        public DbSet<ContractComodities> ContractComodities { get; set; }
        public DbSet<EasyContract> EasyContract { get; set; }
        public DbSet<EasyContractDepartment> EasyContractDepartment { get; set; }
        public DbSet<EasyContractComoditity> EasyContractComoditity { get; set; }
        public DbSet<EasyContractRegion> EasyContractRegion { get; set; }
        public DbSet<LegalMaterialCode> LegalMaterialCode { get; set; }
        public DbSet<CountryList> CountryList { get; set; }
        public DbSet<AribaPRHistory> AribaPRHistory { get; set; }
        public DbSet<UserAppMapping> UserAppMapping { get; set;}
        public DbSet<AppUserMapping> AppUserMapping{get;set;}
        public DbSet<AppModelMapping> AppModelMapping { get; set; }

        protected override void OnModelCreating(Microsoft.EntityFrameworkCore.ModelBuilder modelBuilder)
        {
            _ = modelBuilder.HasDefaultSchema(_configuration[Common.Common.APPLICATION_SCHEMA]);
            _ = modelBuilder.Entity<Person>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<AccessToken>().HasIndex(a => new { a.IsActive, a.UserId }).IncludeProperties(a => new { a.AccessTokenValue, a.ExpiredTime, a.DateCreated, a.DateUpdated, a.CreatedBy, a.UpdatedBy, a.RefreshToken });
            _ = modelBuilder.Entity<Asset>().HasIndex(a => new { a.IsActive, a.EntityType, a.EntityId });
            _ = modelBuilder.Entity<Feature>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<Role>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<RoleFeatureMapping>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<Ticket>().HasIndex(a => new { a.IsActive, a.DomainId }).IncludeProperties(a => new { a.Status });
            _ = modelBuilder.Entity<User>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<UserRoleMapping>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<Metadata>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<ApiKey>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<Config>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<UserTicketCount>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<TicketMessage>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<TicketPR>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<Domain>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<DomainPlan>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<DomainRealm>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<DiscountCoupon>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<PlanDetails>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<AribaUsageProfile>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<EmailContent>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<EmailSentDetail>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<EmailFailedDetail>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<ApiConfig>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<DomainPlanCredit>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<PlanDetails>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<PlanConfig>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<DomainModel>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<EmailCCList>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<DomainSetting>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<PurchaseOrder>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<PurchaseOrder>().HasIndex(a => a.PurchaseOrderId);
            _ = modelBuilder.Entity<SchedularHistory>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<PurchaseOrderMaster>().HasIndex(a => new { a.IsActive, a.DomainId, a.EPID });
            _ = modelBuilder.Entity<PurchaseOrderLog>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<AribaJobSchedular>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<AribaInvoice>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<AribaInvoice>().HasIndex(a => a.AribaTaskId);
            _ = modelBuilder.Entity<AribaExtension>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<AribaTasks>().HasIndex(a => new { a.IsActive, a.Approver, a.AssignedDate });
            _ = modelBuilder.Entity<AribaAttachment>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<AribaInvoiceApprovalHistory>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<AribaInvoiceApprovalHistory>().HasIndex(a => new { a.AribaTaskId, a.InvoiceID });
            _ = modelBuilder.Entity<PRAribaData>().HasIndex(a => new { a.PRNumber, a.IsActive });
            _ = modelBuilder.Entity<FinalAttachment>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<AribaCompanyMapping>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<AribaPR>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<AdditionalQuery>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<AribaPRHistory>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<AribaPRHistory>().HasIndex(a => new { a.AribaTaskId, a.RequisitionsId });
            _ = modelBuilder.Entity<AribaContract>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<AribaPRAttachment>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<AribaPRTasks>().HasIndex(a => new { a.IsActive, a.Approver, a.AssignedDate }).IncludeProperties(a => new { a.ApprovalId, a.DomainId, a.Description, a.Email, a.Status, a.ApprovableType, a.ApprovableUniqueName, a.DateCreated, a.DateUpdated, a.CreatedBy, a.UpdatedBy, a.FullUrl, a.PaymentStatus });
            _ = modelBuilder.Entity<FinalPRAttachment>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<ContractAttachment>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<ContractPending>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<ContractDetails>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<AribaContractHistory>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<UserModelMapping>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<UserModelName>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<AribaInvoiceUpload>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<AribaPRUpload>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<LoginRecord>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<Link>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<AribaPRTasks>().HasIndex(a => new { a.DomainId, a.Approver, a.DateUpdated });
            _ = modelBuilder.Entity<AribaPRTasks>().HasIndex(a => new { a.DomainId, a.Status, a.DateUpdated });
            _ = modelBuilder.Entity<AribaPR>().HasIndex(a => new { a.IsActive, a.AribaTaskId });
            _ = modelBuilder.Entity<AribaPR>().HasIndex(a => new { a.IsActive, a.RequisitionsId });
            _ = modelBuilder.Entity<AribaPR>().HasIndex(a => new { a.IsActive, a.RequisitionsDate });
            _ = modelBuilder.Entity<PurchaseOrderMaster>().HasIndex(a => new { a.IsActive, a.GLAccount, a.RequisitionId });
            _ = modelBuilder.Entity<PurchaseOrderMaster>().HasIndex(a => new { a.IsActive, a.GLAccount, a.EPID });
            _ = modelBuilder.Entity<AribaTasks>().HasIndex(a => new { a.DomainId, a.Approver, a.DateUpdated });
            _ = modelBuilder.Entity<AribaTasks>().HasIndex(a => new { a.DomainId, a.Status, a.DateUpdated });
            _ = modelBuilder.Entity<AribaInvoice>().HasIndex(a => new { a.IsActive, a.AribaTaskId });
            _ = modelBuilder.Entity<AribaInvoice>().HasIndex(a => new { a.IsActive, a.PONumber });
            _ = modelBuilder.Entity<AribaInvoice>().HasIndex(a => new { a.IsActive, a.InvoiceDate });
            _ = modelBuilder.Entity<AribaInvoice>().HasIndex(a => new { a.IsActive, a.SupplierName });
            _ = modelBuilder.Entity<AribaInvoice>().HasIndex(a => new { a.IsActive, a.InvoiceNumber });
            _ = modelBuilder.Entity<PurchaseOrderMaster>().HasIndex(a => new { a.IsActive, a.GLAccount, a.PurchaseOrderId });
            _ = modelBuilder.Entity<MaterialPR>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<MaterialPRLog>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<AribaPayment>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<FinalAttachment>().HasIndex(a => new { a.AribaId, a.IsActive });
            _ = modelBuilder.Entity<FinalPRAttachment>().HasIndex(a => new { a.AribaId, a.IsActive });
            _ = modelBuilder.Entity<ServicePR>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<ServicePRLog>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<FinalContractAttachment>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<AribaContractUpload>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<MaterialPRAttachment>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<PRLog>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<AribaSupplierName>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<AribaVenueListMapping>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<FinalPurchaseOrder>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<FinalPurchaseOrder>().HasIndex(a => new { a.IsActive, a.Supplier });
            _ = modelBuilder.Entity<FinalPurchaseOrder>().HasIndex(a => new { a.IsActive, a.Department });
            _ = modelBuilder.Entity<FinalPurchaseOrder>().HasIndex(a => new { a.IsActive, a.Property });
            _ = modelBuilder.Entity<FinalPurchaseOrder>().HasIndex(a => new { a.IsActive, a.GlAccount });
            _ = modelBuilder.Entity<FinalPurchaseOrder>().HasIndex(a => new { a.IsActive, a.CostCenter });
            _ = modelBuilder.Entity<FinalPurchaseOrder>().HasIndex(a => new { a.IsActive, a.Requester });
            _ = modelBuilder.Entity<FinalPurchaseOrder>().HasIndex(a => new { a.IsActive, a.PoNumber });
            _ = modelBuilder.Entity<FinalPurchaseOrder>().HasIndex(a => new { a.IsActive, a.Supplier, a.Department, a.Property, a.GlAccount, a.CostCenter, a.Requester, a.PoNumber });
            _ = modelBuilder.Entity<DraftMaterialPR>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<DraftMaterialPRAttachment>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<PurchaseOrderBotProcess>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<PurchaseOrderBotProcess>().HasIndex(a => a.PurchaseOrderId);
            _ = modelBuilder.Entity<PurchaseOrderBotReprocessingLog>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<MaterialInvoice>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<PropertyCrossCharge>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<PublicProperty>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<PublicRelationCategory>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<ConciergeVenue>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<ServicePRAttachment>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<ServicePRInvoice>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<DraftMaterialPR>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<DraftMaterialPRAttachment>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<RequesterName>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<AribaCurrency>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<DraftServicePR>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<DraftServicePrAttachment>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<LegalVenueList>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<ContractRegion>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<ContractDepartment>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<ContractSupplier>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<ContractCustomType>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<ContractComodities>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<EasyContract>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<EasyContractDepartment>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<EasyContractComoditity>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<EasyContractRegion>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<LegalMaterialCode>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<CountryList>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<UserAppMapping>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<AppUserMapping>().HasIndex(a => a.IsActive);
            _ = modelBuilder.Entity<AppModelMapping>().HasIndex(a => a.IsActive);

            base.OnModelCreating(modelBuilder);

            foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entity in modelBuilder.Model.GetEntityTypes())
            {

                entity.SetTableName(entity.GetTableName()!.ConvertToSnakeCase());
                var storeObjectIdentifier = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());
                foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableProperty property in entity.GetProperties())
                {

                    property.SetColumnName(property.GetColumnName(storeObjectIdentifier)!.ConvertToSnakeCase());
                }

                foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableKey key in entity.GetKeys())
                {
                    key.SetName(key.GetName()!.ConvertToSnakeCase());
                }

                foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableForeignKey key in entity.GetForeignKeys())
                {
                    key.SetConstraintName(key.GetConstraintName()!.ConvertToSnakeCase());
                }

                foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableIndex index in entity.GetIndexes())
                {
                    index.SetDatabaseName(index.GetDatabaseName()!.ConvertToSnakeCase());
                }
            }

            var schema = _configuration[Common.Common.APPLICATION_SCHEMA];

            // Create a SQL sequence for generating incremental numbers
            modelBuilder.HasSequence<int>("TicketCodeSequence", schema: schema)
                .StartsAt(1)
                .IncrementsBy(1);

            // Use default value SQL to generate formatted code automatically
            modelBuilder.Entity<Ticket>()
                .Property(t => t.TicketCode)
                .HasDefaultValueSql(
                    $"'TC' + RIGHT('000' + CAST(NEXT VALUE FOR [{schema}].[TicketCodeSequence] AS VARCHAR(3)), 3)"
                );
        }

        public void OnBeforeSaving(Guid UserId)
        {
            System.Collections.Generic.IEnumerable<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry> entries = ChangeTracker.Entries();
            foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry in entries)
            {
                if (entry.Entity is BaseModel trackable)
                {
                    DateTime now = DateTime.UtcNow;
                    Guid user = UserId;
                    switch (entry.State)
                    {
                        case EntityState.Modified:
                            trackable.DateUpdated = now;
                            trackable.UpdatedBy = user;
                            break;
                        case EntityState.Added:
                            trackable.DateCreated = now;
                            trackable.CreatedBy = user;
                            trackable.DateUpdated = now;
                            trackable.UpdatedBy = user;
                            trackable.IsActive = true;
                            break;
                        case EntityState.Detached:
                            break;
                        case EntityState.Unchanged:
                            break;
                        case EntityState.Deleted:
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
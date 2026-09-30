using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dto
{
    public class DomainSettingPurchaseOrderDto
    {

        public string? aribaUrl { get; set; }

        public string? UserName { get; set; }

        public string? Password { get; set; }
         public string?AribaUserName { get; set; }

        public string? Partition { get; set; }

        public string? Variant { get; set; }

        public string? Server { get; set; }
        public string? AribaHost { get; set; }
        public string? AribaAuthToken { get; set; }
        public string? AribaTokenUrl { get; set; }
        public string? ServicePartition { get; set; }
        public string? ServiceVariant { get; set; }
        public string? Token { get; set; }
        public string? ApiKey { get; set; }
        public string? PendingApprovablesUrl { get; set; }
        public string? RequisitionAttachmentUrl { get; set; }
public string? RequisitionPRAttachmentUrl { get; set; }
        public string? RequisitionDetailsUrl { get; set; }
        public string? Realm { get; set; }
        public string? StartDateForContractJob { get; set; }
        public string? ContractPendingApprovablesUrl { get; set; }
public string? ContractAcceptRejectUrl { get; set; }
 public string? ContractWorkspaceDetailsUrl { get; set; }
        public string? WorkspaceDetailsAuthToken { get; set; }
        public string? WorkspaceDetailsApiKey { get; set; }
        public string?WorkspaceDetailsTokenUrl { get; set; }
        public string? User { get; set; }
        public string? PasswordAdapter { get; set; }
        public string? AcceptRejectUrl { get; set; }
        public string? StartDateForInvoiceJob { get; set; }
        public List<string>? PurchaseOrderNotificationEmail { get; set; }
        public string? JobSchedularUrl { get; set; }
        public string? JobSchedularResultUrl { get; set; }
        public string? JobSchedularResultDownloadUrl { get; set; }
        public string? FactRecieptTemplate { get; set; }
        public string? PurchaseOrderTemplate { get; set; }
        public string? FactRecieptJobStartDate { get; set; }
        public string? PurchaseOrderJobStartDate { get; set; }
        public string? ContractHost { get; set; }
        public string? ContractTokenUrl { get; set; }
        public string? ContractAuthToken { get; set; }
        public string? ContractUrl { get; set; }
        public string? ContractDocumentUrl { get; set; }
        public string? ContractAmountUrl { get; set; }
        public string? ContractAttachmentUrl { get; set; }
        public string? AttachmentApiKey { get; set; }
        public string? AttachmentRealm { get; set; }
        public string? ContractApiKey { get; set; }
        public string? ContractWorkspaceUrl { get; set; }
        public string? ContractWorkspaceAuthToken { get; set; }
        public string? ContractRealm { get; set; }
        public string? RequisitionTaskUrl { get; set; }
        public string? AttachmentTokenUrl { get; set; }
        public string? AttachmentAuthToken { get; set; }
        public string? AttachmentHost { get; set; }
        public string? AribaContractDataJobStartDate { get; set; }
        public string? AribaContractDataTemplate { get; set; }
        public string? AcceptRejectPRUrl { get; set; }

public string? ContractAttachmentWorkspaceUrl{ get; set; }

        public string? PRAribaDataJobStartDate { get; set; }
        public string? PRAribaDataTemplate { get; set; }
        public string? AribaContractDefaultUser { get; set; }
        public string? AribaContractDefaultPassword { get; set; }
        public string? ContractMaxAmountUrl { get; set; }
        public string? ContractMaxAmountAuthToken { get; set; }
        public string? ContractMaxAmountApiKey { get; set; }
        public string? ContractMaxAmountRealm { get; set; }
        public string? ProcurementAuthToken { get; set; }
        public string? ProcurementTokenUrl { get; set; }
        public string? ProcurementApiKey { get; set; }
        public string? ProcurementFactRecieptUrl { get; set; }
        public string? ProcurementPurchaseOrderUrl { get; set; }
        public string? ProcurementFactRecieptStartDate { get; set; }
        public string? ProcurementPurchaseOrderStartDate { get; set; }
        public string? ProcurementRealm { get; set; }
        public string? ProcurementHost { get; set; }
        public string? AutoInvoiceURL { get; set; }
        public string? AutoSESURL { get; set; }
        public string? SupplierInitialStartDate { get; set; }
        public string? SupplierFetchUrl { get; set; }
        public string? EasyContractUrl { get; set; }

    }
}

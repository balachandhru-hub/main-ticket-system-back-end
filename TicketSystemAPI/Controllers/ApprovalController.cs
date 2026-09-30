using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using Dto;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Entities.Common;
using Contracts.IRepository;
using Contracts.IServices;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Contracts;
using System.IO;
using TicketSystemAPI.Attributes;
using System.Net.Mail;
using Services;
using Entities.Models;
using NPOI.POIFS.FileSystem;
using ExceptionHandler;
using NPOI.SS.Formula.Functions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;


namespace TicketSystemAPI.Controllers
{

    public class ApprovalController : BaseController
    {
        private readonly IAribaService _aribaService;
        private readonly IInvoicePendingApprovableService _invoicePendingApprovableService;
        private readonly IEmailService _emailService;
        private readonly ILoggerManager _logger;
        private IRepositoryWrapper _repositoryWrapper;

        public ApprovalController(IAribaService aribaService, IInvoicePendingApprovableService invoicePendingApprovableService, ILoggerManager logger, IRepositoryWrapper repositoryWrapper, IEmailService emailService)

        {
            _aribaService = aribaService;
            _logger = logger;
            _repositoryWrapper = repositoryWrapper;
            _invoicePendingApprovableService = invoicePendingApprovableService;
            _emailService = emailService;
        }



        /// <summary>
        /// Get pending approvals for a domain
        /// </summary>
        [HttpGet]
        [Route("api/v1/ariba/pending-approvals")]
        [ApiAuthorization(Name = "GET_PENDING_APPROVALS")]
        [SwaggerOperation("GetPendingApprovals")]
        [SwaggerResponse(200, type: typeof(List<AribaInvoiceDto>), description: "Pending approvals fetched successfully")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Domain Not Found")]
        public async Task<IActionResult> GetPendingApprovals([FromQuery][Required] int index = 0, [FromQuery][Required] int limit = 10)
        {
            Guid domainId = getUserDomainId();
            Guid userId = getLoggedUser();
            MyProfileDto? profileDto = _repositoryWrapper.Person.GetMyProfileByUserId(userId);
            string aribausername = null;
            string role = null;
            if (profileDto != null)
            {
                aribausername = profileDto.AribaUserName;
                role = profileDto.Role;
            }
            IQueryable<Entities.Models.AribaTasks> pendingTasks;

            if (role == Common.BUSINESS_ADMIN_USER_KEY)
            {
                pendingTasks = (IQueryable<Entities.Models.AribaTasks>)_repositoryWrapper.AribaTask.FindByConditionAsync(x => x.DomainId == domainId && x.Status == Common.STATUS_PENDING).OrderByDescending(x => x.DateUpdated).Skip(index).Take(limit);
            }
            else if (aribausername != null)
            {
                pendingTasks = (IQueryable<Entities.Models.AribaTasks>)_repositoryWrapper.AribaTask.FindByConditionAsync(x => x.Approver == aribausername && x.Status == Common.STATUS_PENDING).OrderByDescending(x => x.DateUpdated).Skip(index).Take(limit);
            }
            else
            {
                pendingTasks = null;
                _logger.LogError($"Unable to Find Any Records for User with Id : {userId}");
                throw new NotFoundCustomException("Unable to Find Any Records", "Unable to Find Any Records");
            }

            var pendingApprovals = pendingTasks.Select(x => x.ApprovalId).ToList();
            var invoices = _repositoryWrapper.AribaInvoice
                .FindByConditionAsync(x => pendingApprovals.Contains(x.AribaTaskId));

            List<AribaInvoiceDto> aribaInvoices = new List<AribaInvoiceDto>();
            foreach (var invoice in invoices)
            {
                var dto = new AribaInvoiceDto
                {
                    InvoiceId = invoice.InvoiceId,
                    AribaTaskId = invoice.AribaTaskId,
                    InvoiceNumber = invoice.InvoiceNumber,
                    Currency = invoice.Currency,
                    Amount = invoice.Amount,
                    SupplierName = invoice.SupplierName,
                    Description = invoice.Description,
                    DueDate = invoice.DueDate,
                    InvoiceDate = invoice.InvoiceDate,
                    CompanyUniqueName = invoice.CompanyUniqueName,
                    PONumber = invoice.PONumber,
                    ContractId = invoice.ContractId,
                    Department = invoice.Department,

                    InvoiceNetAmount = invoice.InvoiceNetAmount,
                    InvoiceTaxAmount = invoice.InvoiceTaxAmount,
                    HasAttachment = _repositoryWrapper.FinalAttachment.FindFirstByCondition(x => x.IsActive && x.AribaId == invoice.InvoiceId) != null ? true : false,
                };
                var attachments = _repositoryWrapper.AribaAttachment
                .FindByConditionAsync(x => x.AribaTaskId == dto.AribaTaskId && x.InvoiceID == dto.InvoiceId).Select(x => x.AttachmentId).ToList();
                dto.Attachments = attachments;
                var aribainvoiceapprovalhistory = _repositoryWrapper.AribaInvoiceApprovalHistory
                .FindByConditionAsync(x => x.IsActive && x.AribaTaskId == dto.AribaTaskId && x.InvoiceID == dto.InvoiceId).OrderBy(x => x.DateCreated).ToList();
                dto.ApprovalHistory = aribainvoiceapprovalhistory;
                AribaContract aribacontract = await _repositoryWrapper.AribaContract.FindFirstByConditionAsync(x => x.ContractId == dto.ContractId);
                AribaContractDetails aribacontractdetails = await _repositoryWrapper.AribaContractDetails.FindFirstByConditionAsync(x => x.ContractId == dto.ContractId && x.SupplierName == dto.SupplierName);
                if (aribacontract != null && aribacontractdetails != null)
                {
                    var contract = new InvoiceContractDto
                    {
                        ContractName = aribacontract.ContractName,
                        BeginDate = aribacontract.BeginDate,
                        EndDate = aribacontract.EndDate,
                        TotalAmount = aribacontract.TotalAmount,
                        ContractId = aribacontract.ContractId,
                        AmountSpend = aribacontractdetails.AmountSpend,
                        BalanceAmount = aribacontractdetails.BalanceAmount,
                        Description = aribacontractdetails?.Description,
                        AttachmentID = aribacontractdetails?.AttachmentID,
                        WorkspaceID = aribacontractdetails?.WorkspaceID,
                    };
                }
                aribaInvoices.Add(dto);
            }

            return Ok(aribaInvoices);
        }
        /// <summary>
        /// Get pending approvals for a domain
        /// </summary>
        [HttpGet]
        [Route("api/v1/ariba/pending-approvals-details")]
        [ApiAuthorization(Name = "GET_PENDING_APPROVALS_DETAILS")]
        [SwaggerOperation("GetPendingApprovalsDetails")]
        [SwaggerResponse(200, type: typeof(List<AribaInvoiceDetailDto>), description: "Pending approvals details fetched successfully")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Domain Not Found")]
        public async Task<IActionResult> GetPendingApprovalsDetails([FromQuery] int index = 0, [FromQuery] int limit = 8, [FromQuery] string? searchTerm = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null, [FromQuery] string? status = null, [FromQuery] List<string>? glAccount = null)
        {
            Guid domainId = getUserDomainId();
            Guid userId = getLoggedUser();
            var aribaInvoices = await _aribaService.GetInvoicePendingApprovals(userId, domainId, index, limit, searchTerm, fromDate, toDate, status, glAccount);
            return Ok(aribaInvoices);
        }

        /// <summary>
        /// Get requisition attachment for a domain
        /// </summary>


        [HttpGet]
        [Route("api/v1/ariba/attachments")]
        [ApiAuthorization(Name = "GET_REQUISITION_ATTACHMENT")]
        [SwaggerOperation("GetRequisitionAttachment")]
        [SwaggerResponse(StatusCodes.Status200OK, type: typeof(FileContentResult), description: "Requisition attachment fetched successfully")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, type: typeof(ErrorResponseDto), description: "Attachment Not Found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorResponseDto), description: "Unexpected error occurred")]
        public async Task<IActionResult> GetRequisitionAttachment(
            [FromQuery][Required] string requisitionId,
            [FromQuery(Name = "attachments")][Required] string attachmentId)
        {

            Guid domainId = getUserDomainId();
            var result = await _aribaService.GetRequisitionAttachments(domainId, requisitionId, attachmentId);

            if (result == null || result.Count == 0)
                return NotFound(new ErrorResponseDto { Message = "No attachments found." });

            var attachment = result.First();


            return File(attachment.FileBytes, attachment.ContentType ?? "application/octet-stream", attachment.FileName);
        }



        // /// <summary>
        // /// Patch (approve/reject) a requisition
        // /// </summary>
        [HttpPatch]
        [Route("api/v1/ariba/requisition/patch")]
        [ApiAuthorization(Name = "PATCH_REQUISITION")]
        [SwaggerOperation("PatchRequisition")]
        [SwaggerResponse(200, type: typeof(SuccessResponseDto), description: "Requisition patched successfully")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Domain Not Found")]
        public async Task<IActionResult> PatchRequisition(
            [FromQuery][Required] string requisitionId,
            [FromBody] AribaPatchRequestDto patchRequest)
        {
            Guid domainId = getUserDomainId();
            Guid userId = getLoggedUser();
            MyProfileDto aribauser = _repositoryWrapper.Person.GetMyProfileByUserId(userId)!;
            string result = await _aribaService.PatchRequisitionAsync(domainId, requisitionId, patchRequest, aribauser, userId);
            return Ok(new SuccessResponseDto { Message = "Successfully Updated In Ariba", Description = result, StatusCode = 200 });
        }

        [HttpGet]
        [Route("api/v1/ariba/ContractAttachments/{documentId}")]
        [ApiAuthorization(Name = "GET_CONTRACT_ATTACHMENT")]
        [SwaggerOperation("GetContractAttachmentWithAttachmentID")]
        [SwaggerResponse(200, type: typeof(SuccessResponseDto), description: "Contract attachment fetched successfully")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Domain Not Found")]
        public async Task<IActionResult> GetContractAttachmentWithAttachmentID
           ([FromRoute][Required] string documentId)
        {

            Guid domainId = getUserDomainId();
            Guid userId = getLoggedUser();
            MyProfileDto aribauser = _repositoryWrapper.Person.GetMyProfileByUserId(userId)!;
            var result = await _aribaService.GetContractDocumentById(domainId, documentId, aribauser);
            if (result == null || result.Length == 0)
                return NotFound(new ErrorResponseDto { Message = "No attachments found." });

            return File(result, "application/pdf", "ContractDocument.pdf");
        }

        // /// <summary>
        // /// Patch (approve/reject) a requisition
        // /// </summary>
        [HttpPatch]
        [Route("api/v1/ariba/details/edit")]
        [ApiAuthorization(Name = "EDIT_DETAILS")]
        [SwaggerOperation("EditDetails")]
        [SwaggerResponse(200, type: typeof(SuccessResponseDto), description: "Requisition edited successfully")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Domain Not Found")]
        public async Task<IActionResult> EditDetails(
            [FromQuery][Required] string invoiceId,
            [FromBody] PatchDetailsDto patchRequest)
        {
            Guid domainId = getUserDomainId();
            Guid userId = getLoggedUser();
            MyProfileDto aribauser = _repositoryWrapper.Person.GetMyProfileByUserId(userId)!;


            if (aribauser.Role != Common.BUSINESS_ADMIN_USER_KEY)
            {
                throw new PreConditionFailedCustomException($"User not allowed to do the operation", $"Invalid Operation");
            }

            AribaInvoice invoice = _repositoryWrapper.AribaInvoice.FindFirstByCondition(x => x.InvoiceId == invoiceId);
            if (invoice == null)
            {
                return NotFound(new ErrorResponseDto
                {
                    StatusCode = 404,
                    Message = "Invoice not found",
                    Description = $"No invoice found with InvoiceId={invoiceId}"
                });
            }


            invoice.Description = patchRequest.Description;
            _repositoryWrapper.AribaInvoice.Update(invoice);



            if (string.IsNullOrEmpty(invoice.ContractId))
            {
                bool isAmountFieldSet = (patchRequest.ContractValue.HasValue && patchRequest.ContractValue != 0) ||
                                        (patchRequest.ValueAlreadyPaid.HasValue && patchRequest.ValueAlreadyPaid != 0) ||
                                        (patchRequest.RemainingBalance.HasValue && patchRequest.RemainingBalance != 0);

                if (isAmountFieldSet)
                {
                    await _repositoryWrapper.SaveAsync();

                    return Ok(new SuccessResponseDto
                    {
                        StatusCode = 200,
                        Message = "Description updated successfully",
                        Description = "Invoice description has been updated. Contract-related fields were ignored because ContractId is missing."
                    });
                }
            }

            if (!string.IsNullOrEmpty(invoice.ContractId))
            {
                AribaContract contract = _repositoryWrapper.AribaContract.FindFirstByCondition(x => x.ContractId == invoice.ContractId);
                AribaContractDetails contractdetails = _repositoryWrapper.AribaContractDetails.FindFirstByCondition(x => x.ContractId == invoice.ContractId);

                if (contract != null && contractdetails != null)
                {
                    contract.TotalAmount = patchRequest.ContractValue.ToString();
                    contractdetails.BalanceAmount = patchRequest.RemainingBalance.ToString();
                    contractdetails.AmountSpend = patchRequest.ValueAlreadyPaid.ToString();
                    _repositoryWrapper.AribaContract.Update(contract);
                    _repositoryWrapper.AribaContractDetails.Update(contractdetails);
                }
            }

            await _repositoryWrapper.SaveAsync();

            // Send notification if task exists
            var aribaTask = _repositoryWrapper.AribaTask.FindFirstByCondition(x => x.ApprovalId == invoice.AribaTaskId);
            if (aribaTask != null)
            {
                _emailService.CreateAribaApprovalEmailNotification(aribaTask, Common.ARIBA_APPROVAL_EMAIL_KEY);
            }

            return Ok(new SuccessResponseDto { Message = "Successfully Updated Data", Description = "Success", StatusCode = 200 });
        }
        /// <summary>
        /// Trigger Ariba approval email notification for an entity (invoice/contract/PR).
        /// </summary>
        [HttpPost]
        [Route("api/v1/ariba/notify")]
        [ApiAuthorization(Name = "TRIGGER_ARIBA_NOTIFICATION")]
        [SwaggerOperation("TriggerAribaNotification")]
        [SwaggerResponse(200, type: typeof(SuccessResponseDto), description: "Notification triggered")]
        [SwaggerResponse(400, type: typeof(ErrorResponseDto), description: "Bad request - missing parameters")]
        public IActionResult TriggerAribaNotification(
            [FromQuery][Required] string entityType,
            [FromQuery][Required] string aribaId,
            [FromQuery][Required] string approver)
        {
            _logger.LogInfo($"Triggering Ariba notification for {entityType} with id {aribaId}");
            _emailService.AribaApprovalEmailNotification(aribaId, entityType, approver);
            return Ok(new SuccessResponseDto { Message = "Notification triggered.", StatusCode = 200, Description = "Email notification has been sent." });

        }
        [HttpGet]
        [Route("api/v1/ariba/contract/report")]
        [ApiAuthorization(Name = "GET_CONTRACT_REPORT")]
        [SwaggerOperation("GetContractReport")]
        [SwaggerResponse(200, type: typeof(AribaInvoiceDetailDto), description: "Contract report fetched successfully")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Domain Not Found")]
        public async Task<IActionResult> GetContractReport([FromQuery][Required] DateTime updatedDateFrom, [FromQuery][Required] DateTime updatedDateTo)
        {
            Guid domainId = getUserDomainId();
            Guid userId = getLoggedUser();
            MyProfileDto aribauser = _repositoryWrapper.Person.GetMyProfileByUserId(userId)!;
            var result = await _aribaService.GetContractDetailsFromReportAsync(domainId, aribauser, updatedDateFrom, updatedDateTo);
            if (result == null)
                return NotFound(new ErrorResponseDto { Message = "No data found." });

            return Ok(result);
        }
        [HttpGet]
        [Route("api/v1/ariba/contract/{contractId}")]
        [ApiAuthorization(Name = "GET_CONTRACT_BY_ID")]
        [SwaggerOperation("GetContractById")]
        [SwaggerResponse(200, type: typeof(InvoiceContractDto), description: "Contract fetched successfully")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Domain Not Found")]
        public async Task<IActionResult> GetContractWorkspaceById([FromRoute][Required] string contractId)
        {
            Guid domainId = getUserDomainId();
            var result = await _aribaService.GetContractWorkspaceId(domainId, contractId);
            if (result == null)
                return NotFound(new ErrorResponseDto { Message = "No contract found." });

            return Ok(result);
        }
        /// <summary>
        /// Get contract attachment by contract id
        /// </summary>
        /// <param name="contractId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/v1/ariba/contract-attachment-by-id")]
        [ApiAuthorization(Name = "GET_ATTACHMENT_BY_CONTRACT_ID")]
        [SwaggerOperation("GetAttachmentByContractId")]
        [SwaggerResponse(StatusCodes.Status200OK, type: typeof(FileContentResult), description: "Attachment bytes fetched successfully")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, type: typeof(ErrorResponseDto), description: "Attachment Not Found")]
        public async Task<IActionResult> GetAttachmentbyContractID([FromQuery][Required] string contractId)
        {
            Guid domainId = getUserDomainId();
            Guid userId = getLoggedUser();
            var aribaUser = _repositoryWrapper.Person.GetMyProfileByUserId(userId);

            var attachmentBytes = await _aribaService.GetAttachmentbyContractID(domainId, contractId, aribaUser);

            if (attachmentBytes == null || attachmentBytes.Length == 0)
                return NotFound(new ErrorResponseDto { Message = "Attachment could not be retrieved." });

            return File(attachmentBytes, "application/pdf", $"ContractAttachment_{contractId}.pdf");
        }
        [HttpGet]
        [Route("api/v1/ariba/final-attachment/{invoiceId}")]
        [ApiAuthorization(Name = "GET_FINAL_ATTACHMENT")]
        [SwaggerOperation("GetFinalAttachment")]
        [SwaggerResponse(200, type: typeof(FileContentResult), description: "Final attachment fetched successfully")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Final Attachment Not Found")]
        public IActionResult GetFinalAttachment([FromRoute][Required] string invoiceId)
        {
            _logger.LogDebug($"Fetching the Attachments for the Invoice with Id : {invoiceId}");
            invoiceId = Uri.UnescapeDataString(invoiceId);
            SplitAttachmentResponseDto result = _aribaService.GetFinalAttachmentBase64(invoiceId);
            _logger.LogDebug($"Fetched the Attachments for the Invoice with Id : {invoiceId}");
            return Ok(result);
        }
        [HttpDelete]
        [Route("api/v1/ariba/task")]
        [ApiAuthorization(Name = "DELETE_ARIBA_TASK")]
        [SwaggerOperation("DeleteAribaTaskByInvoiceAndApprover")]
        [SwaggerResponse(200, type: typeof(SuccessResponseDto), description: "Task deleted successfully")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Task or invoice not found")]
        public async Task<IActionResult> DeleteAribaTaskByInvoiceAndApprover(
            [FromQuery][Required] string invoiceId,
            [FromQuery][Required] string approver)
        {
            _logger.LogInfo($"Start: Deleting AribaTask for InvoiceId={invoiceId}, Approver={approver}");
            await _aribaService.DeleteAribaTaskByInvoiceAndApproverAsync(invoiceId, approver);
            _logger.LogInfo($"End: Deleted AribaTask for InvoiceId={invoiceId}, Approver={approver}");
            return Ok(new SuccessResponseDto
            {
                Message = "Task deleted successfully",
                StatusCode = 200,
                Description = $"AribaTask with InvoiceId={invoiceId} and Approver={approver} deleted successfully."
            });
        }

        /// <summary>
        /// Get leadership approval dashboard summary
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/v1/leadership/approval-dashboard-summary")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GET_LEADERSHIP_APPROVAL_DASHBOARD_SUMMARY")]
        [SwaggerOperation("GetLeadershipApprovalDashboardSummary")]
        [SwaggerResponse(200, type: typeof(AribaApprovalDashboardSummaryDto), description: "Leadership approval dashboard summary")]
        [SwaggerResponse(401, "Unauthorized")]
        public IActionResult GetLeadershipApprovalDashboardSummary([FromQuery] string type = Common.TYPE_INVOICE)
        {
            Guid userId = getLoggedUser();
            _logger.LogInfo($"Start: Fetching leadership approval dashboard summary for type: {type}");
            Guid domainId = getUserDomainId();
            AribaApprovalDashboardSummaryDto summary = _aribaService.GetLeadershipApprovalDashboardSummary(userId, domainId, type);
            _logger.LogInfo($"End: Fetched leadership approval dashboard summary for type: {type}");
            return Ok(summary);
        }

        /// <summary>
        /// Get latest pending approvals from ariba with user creadentials
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/v1/leadership/latest-pending-aprovals-usersync")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GET_PENDING_APPROVALS_BY_USER_INVC")]
        [SwaggerOperation("GetLatestPendingProvalsByUserSync")]
        [SwaggerResponse(200, type: typeof(List<AribaInvoiceDto>), description: "Get Latest Pending Provals By User Sync")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Domain Not Found")]

        public IActionResult GetLatestPendingProvalsByUserSync()
        {
            Guid userId = getLoggedUser();
            var userData = _repositoryWrapper.User.GetUserDetailsById(userId);
            Guid domainId = getUserDomainId();
            _logger.LogInfo($"Start: Fetching Latest Pending Provals By User Sync for user: {userId}");

            if (userData != null)
            {
                var data = _invoicePendingApprovableService.ProcessInvoiceAsyncByUserId(userData?.AribaUserName, userData?.AribaPassword, domainId.ToString(), "");
            }
            _logger.LogInfo($"End: Fetching Latest Pending Provals By User Sync for user: {userId}");
            return Ok(new SuccessResponseDto { Message = "Successfully Updated Data for Invoice Approval", Description = "Success", StatusCode = 200 });
        }
        /// <summary>
        /// Upload an extension and persist it into the InvoiceExtension table (domain-scoped).
        /// If an extension already exists for the same domain+invoiceId it will be updated.
        /// </summary>
        [HttpPost]
        [Route("api/v1/ariba/extension")]
        [ApiAuthorization(Name = "UPLOAD_EXTENSION")]
        [SwaggerOperation("UploadAribaExtension")]
        [SwaggerResponse(200, type: typeof(SuccessResponseDto), description: "Extension uploaded successfully")]
        [SwaggerResponse(400, "Invalid file")]
        [SwaggerResponse(500, "Server error")]
        public async Task<IActionResult> UploadAribaExtension([FromBody] ExtensionUploadDto dto, [FromQuery][Required] Guid domainId)
        {
            _logger.LogInfo("Start: Uploading Ariba extension");

            Guid userId = getLoggedUser();
            await _aribaService.SaveOrUpdateExtensionAsync(domainId, dto, userId);

            _logger.LogInfo("End: Uploaded Ariba extension");
            return Ok(new SuccessResponseDto
            {
                Message = "Extension uploaded successfully.",
                StatusCode = 200,
                Description = dto?.FileName
            });
        }

        /// <summary>
        /// Download the latest extension for a domain.
        /// </summary>
        [HttpGet]
        [Route("api/v1/ariba/extension/latest")]
        [ApiAuthorization(Name = "FETCH_EXTENSION")]
        [SwaggerOperation("DownloadLatestExtension")]
        [SwaggerResponse(200, type: typeof(SuccessResponseDto), description: "Extension downloaded successfully")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Extension not found")]
        [SwaggerResponse(500, type: typeof(ErrorResponseDto), description: "Server error")]
        public async Task<IActionResult> DownloadLatestExtension()
        {
            _logger.LogInfo("Start: Downloading latest extension");
            Guid domainId = getUserDomainId();
            ExtensionDownloadDto extension = await _aribaService.GetLatestInvoiceExtensionAsync(domainId);
            _logger.LogInfo("End: Downloaded latest extension");
            return Ok(extension);
        }

        /// <summary>
        /// Get extension by domain ID passed as query parameter.
        /// </summary>
        [HttpGet]
        [Route("api/v1/ariba/fetchextension")]
        [ApiAuthorization(Name = "FETCH_BDEXTENSION")]
        [SwaggerOperation("GetExtensionByDomainId")]
        [SwaggerResponse(200, type: typeof(ExtensionDownloadDto), description: "Extension retrieved successfully")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Extension not found")]
        [SwaggerResponse(500, type: typeof(ErrorResponseDto), description: "Server error")]
        public async Task<IActionResult> GetExtensionByDomainId([FromQuery][Required] Guid domainId)
        {
            _logger.LogDebug($"Start: Getting extension for domain {domainId}");
            ExtensionDownloadDto extension = await _aribaService.GetExtensionByDomainIdAsync(domainId);
            _logger.LogDebug($"End: Retrieved extension for domain {domainId}");
            return Ok(extension);
        }

        [HttpGet]
        [Route("api/v1/ariba/pending-approvals-details-excel")]
        [ApiAuthorization(Name = "GET_PENDING_APPROVALS_DETAILS_EXCEL")]
        [SwaggerOperation("ExportPendingApprovalsDetailsExcel")]
        [SwaggerResponse(200, type: typeof(List<AribaInvoiceDetailDto>), description: "Pending approvals details downloaded  successfully")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Domain Not Found")]

        public async Task<IActionResult> ExportPendingApprovalsDetailsExcel(
     [FromQuery] int limit = 8,
     [FromQuery] string? searchTerm = null,
     [FromQuery] DateTime? fromDate = null,
     [FromQuery] DateTime? toDate = null,
     [FromQuery] string? status = null,
     [FromQuery] List<string>? glAccount = null)
        {
            Guid userId = getLoggedUser();
            Guid domainId = getUserDomainId();

            var result = await _aribaService.ExportInvoicePendingApprovalsExcelAsync(
            userId, domainId, limit, searchTerm, fromDate, toDate, status, glAccount);

            return File(result.FileContent,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                result.FileName);

        }

        [HttpPut]
        [Route("api/v1/ariba/PaymentStatus")]
        [ApiAuthorization(Name = "UPDATE_PAYMENT_STATUS")]
        [SwaggerOperation("UpdateInvoicePaymentStatus")]
        [SwaggerResponse(200, type: typeof(SuccessResponseDto), description: "Updated the invoice payment statu successfully")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Domain Not Found")]
        public async Task<IActionResult> UpdatePaymentStatus(
   
        [FromBody] UpdatePaymentStatusBodyDto dto)
        {



            await _aribaService.UpdatePaymentStatusAsync(dto.invoiceId, dto.PaymentStatus);
            return Ok(new { message = "Payment status updated successfully", dto.invoiceId, dto.PaymentStatus });
        }

        [HttpGet]
        [Route("api/v1/ariba/GetPaymentStatuses")]
        [ApiAuthorization(Name = "GET_PAYMENT_STATUSES")]
        [SwaggerOperation("GetPaymentStatuses")]
        public async Task<IActionResult> GetPaymentStatuses()
        {
            var statuses = await _aribaService.GetPaymentStatusesAsync();
            return Ok(statuses);
        }

    }

}


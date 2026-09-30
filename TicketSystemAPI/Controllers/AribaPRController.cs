using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using Dto;
using Newtonsoft.Json;
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
using TicketSystemAPI.Attributes;
using System.Net.Mail;
using Services;
using Entities.Models;
using NPOI.POIFS.FileSystem;
using ExceptionHandler;
using Contracts.IServices;
using NPOI.SS.Formula.Functions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;


namespace TicketSystemAPI.Controllers
{

    public class AribaPRController : BaseController
    {
        private readonly IAribaService _aribaService;
        private readonly IPRPendingApprovableService _pRPendingApprovableService;
        private readonly IAribaPRService _aribaPRService;
        private readonly IEmailService _emailService;

        private readonly ILoggerManager _logger;
        private IRepositoryWrapper _repositoryWrapper;

        public AribaPRController(IAribaService aribaService, IPRPendingApprovableService pRPendingApprovableService, ILoggerManager logger, IRepositoryWrapper repositoryWrapper, IAribaPRService aribaPRService, IEmailService emailService)

        {
            _aribaService = aribaService;
            _logger = logger;
            _repositoryWrapper = repositoryWrapper;
            _pRPendingApprovableService = pRPendingApprovableService;
            _aribaPRService = aribaPRService;
            _emailService = emailService;
        }

        /// <summary>
        /// Get pending approvals for a domain
        /// </summary>
        [HttpGet]
        [Route("api/v1/ariba/pending-approvals-PR")]
        [ApiAuthorization(Name = "GET_PENDING_APPROVALS_PR")]
        [SwaggerOperation("GetPendingApprovals")]
        [SwaggerResponse(200, type: typeof(List<AribaPRDto>), description: "Pending approvals fetched successfully")]
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
            IQueryable<Entities.Models.AribaPRTasks> pendingTasks;

            if (role == Common.BUSINESS_ADMIN_USER_KEY)
            {
                pendingTasks = (IQueryable<Entities.Models.AribaPRTasks>)_repositoryWrapper.AribaPRTask.FindByConditionAsync(x => x.DomainId == domainId && x.Status == Common.STATUS_PENDING).OrderByDescending(x => x.DateUpdated).Skip(index).Take(limit);
            }
            else if (aribausername != null)
            {
                pendingTasks = (IQueryable<Entities.Models.AribaPRTasks>)_repositoryWrapper.AribaPRTask.FindByConditionAsync(x => x.Approver == aribausername && x.Status == Common.STATUS_PENDING).OrderByDescending(x => x.DateUpdated).Skip(index).Take(limit);
            }
            else
            {
                pendingTasks = null;
                _logger.LogError($"Unable to Find Any Records for User with Id : {userId}");
                throw new NotFoundCustomException("Unable to Find Any Records", "Unable to Find Any Records");
            }

            var pendingApprovals = pendingTasks.Select(x => x.ApprovalId).ToList();
            var requisitions = _repositoryWrapper.AribaPR
                .FindByConditionAsync(x => pendingApprovals.Contains(x.AribaTaskId));

            List<AribaPRDto> aribaInvoices = new List<AribaPRDto>();
            foreach (var requisition in requisitions)
            {
                var dto = new AribaPRDto
                {
                    RequisitionsId = requisition.RequisitionsId,
                    AribaTaskId = requisition.AribaTaskId,
                    Currency = requisition.Currency,
                    Amount = requisition.Amount,
                    SupplierName = requisition.SupplierName,
                    Description = requisition.Description,
                    DueDate = requisition.DueDate,
                    RequisitionsDate = requisition.RequisitionsDate,

                    CompanyUniqueName = _repositoryWrapper.AribaPR.GetCompanyTaxDetailByRequisition(requisition.RequisitionsId).CompanyCode,
                    PRTaxAmount = _repositoryWrapper.AribaPR.GetCompanyTaxDetailByRequisition(requisition.RequisitionsId).TaxAmount,
                    RequesterName = requisition.RequesterName,
                    PRNetAmount = requisition.PRNetAmount,

                    ContractId = requisition.ContractId,
                    Department = requisition.Department,
                    NeedBYDate = requisition.NeedBYDate,
                    HasAttachment = _repositoryWrapper.FinalPRAttachment.FindFirstByCondition(x => x.IsActive && x.AribaId == requisition.RequisitionsId) != null ? true : false,
                };
                var attachments = _repositoryWrapper.AribaPRAttachment
                .FindByConditionAsync(x => x.AribaTaskId == dto.AribaTaskId && x.RequisitionsId == dto.RequisitionsId).Select(x => x.AttachmentId).ToList();
                dto.Attachments = attachments;
                var aribaPRapprovalhistory = _repositoryWrapper.AribaPRHistory
                .FindByConditionAsync(x => x.IsActive && x.AribaTaskId == dto.AribaTaskId && x.RequisitionsId == dto.RequisitionsId).OrderBy(x => x.DateCreated).ToList();
                dto.ApprovalHistory = aribaPRapprovalhistory;
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
        [Route("api/v1/ariba/pending-approvals-details-PR")]
        [ApiAuthorization(Name = "GET_PENDING_APPROVALS_DETAILS_PR")]
        [SwaggerOperation("GetPendingApprovalsDetails")]
        [SwaggerResponse(200, type: typeof(List<AribaInvoiceDetailDto>), description: "Pending approvals details fetched successfully")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Domain Not Found")]
        public async Task<IActionResult> GetPendingApprovalsDetails([FromQuery] int index = 0, [FromQuery] int limit = 8, [FromQuery] string? searchTerm = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null, [FromQuery] string? status = null, [FromQuery] List<string>? glAccount = null)
        {
            Guid domainId = getUserDomainId();
            Guid userId = getLoggedUser();
            var aribarequisitions = await _aribaPRService.GetPendingApprovalsAsync(userId, domainId, index, limit, searchTerm, fromDate, toDate, status, glAccount);

            return Ok(aribarequisitions);
        }
        /// <summary>
        /// Patch requisition in Ariba
        /// </summary>
        ///


        [HttpPatch]
        [Route("api/v1/ariba/requisition-PR/patch")]
        [ApiAuthorization(Name = "PATCH_REQUISITION_PR")]
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
            string result = await _aribaPRService.PatchRequisition(domainId, requisitionId, patchRequest, aribauser, userId);
            return Ok(new SuccessResponseDto { Message = "Successfully Updated In Ariba", Description = result, StatusCode = 200 });
        }
        /// <summary>
        /// Get Final Attachment for PR
        /// </summary>

        [HttpGet]
        [Route("api/v1/ariba/final-PR-attachment/{requisitionId}")]
        [ApiAuthorization(Name = "GET_FINAL_ATTACHMENT_PR")]
        [SwaggerOperation("GetFinalAttachment")]
        [SwaggerResponse(200, type: typeof(FileContentResult), description: "Final attachment fetched successfully")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Final Attachment Not Found")]
        public IActionResult GetFinalAttachment([FromRoute][Required] string requisitionId)
        {
            _logger.LogDebug($"Fetching the Attachments for the Requisition with Id : {requisitionId}");
            requisitionId = Uri.UnescapeDataString(requisitionId);
            SplitAttachmentResponseDto result = _aribaPRService.GetFinalPRAttachmentBase64(requisitionId);
            _logger.LogDebug($"Fetched the Attachments for the Requisition with Id : {requisitionId}");
            return Ok(result);
        }

        /// <summary>
        /// Get Attachment by Contract ID for PR
        /// </summary>

        [HttpGet]
        [Route("api/v1/ariba/PR/contract-attachment-by-id")]
        [ApiAuthorization(Name = "GET_ATTACHMENT_BY_CONTRACT_ID_PR")]
        [SwaggerOperation("GetAttachmentByContractId")]
        [SwaggerResponse(StatusCodes.Status200OK, type: typeof(FileContentResult), description: "Attachment bytes fetched successfully")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, type: typeof(ErrorResponseDto), description: "Attachment Not Found")]
        public async Task<IActionResult> GetAttachmentbyContractID([FromQuery][Required] string contractId)
        {
            Guid domainId = getUserDomainId();
            Guid userId = getLoggedUser();
            var aribaUser = _repositoryWrapper.Person.GetMyProfileByUserId(userId);

            var attachmentBytes = await _aribaPRService.GetAttachmentbyContractID(domainId, contractId, aribaUser);

            if (attachmentBytes == null || attachmentBytes.Length == 0)
                return NotFound(new ErrorResponseDto { Message = "Attachment could not be retrieved." });

            return File(attachmentBytes, "application/pdf", $"ContractAttachment_{contractId}.pdf");
        }
        /// <summary>
        /// Get Requisition Attachment for PR
        /// </summary>


        [HttpGet]
        [Route("api/v1/ariba/attachments/PR")]
        [ApiAuthorization(Name = "GET_REQUISITION_PR_ATTACHMENT")]
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
            var result = await _aribaPRService.GetRequisitionAttachments(domainId, requisitionId, attachmentId);

            if (result == null || result.Count == 0)
                return NotFound(new ErrorResponseDto { Message = "No attachments found." });

            var attachment = result.First();


            return File(attachment.FileBytes, attachment.ContentType ?? "application/octet-stream", attachment.FileName);
        }

        [HttpPatch]
        [Route("api/v1/ariba/details-PR/edit")]
        [ApiAuthorization(Name = "EDIT_DETAILS_PR")]
        [SwaggerOperation("EditDetails")]
        [SwaggerResponse(200, type: typeof(SuccessResponseDto), description: "Requisition edited successfully")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Domain Not Found")]
        public async Task<IActionResult> EditDetails(
          [FromQuery][Required] string requisitionId,
          [FromBody] PatchDetailsDto patchRequest)
        {
            Guid domainId = getUserDomainId();
            Guid userId = getLoggedUser();
            MyProfileDto aribauser = _repositoryWrapper.Person.GetMyProfileByUserId(userId)!;
            AribaPR requisition = _repositoryWrapper.AribaPR.FindFirstByCondition(x => x.RequisitionsId == requisitionId);
            if (requisition == null)
            {
                return NotFound(new ErrorResponseDto
                {
                    StatusCode = 404,
                    Message = "Requisition not found",
                    Description = $"No requisition found with RequisitionsId ={requisitionId}"
                });
            }


            requisition.Description = patchRequest.Description;
            _repositoryWrapper.AribaPR.Update(requisition);


            if (string.IsNullOrEmpty(requisition.ContractId))
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
                        Description = "Requisition description has been updated. Contract-related fields were ignored because ContractId is missing."
                    });
                }
            }

            if (!string.IsNullOrEmpty(requisition.ContractId))
            {
                AribaContract contract = _repositoryWrapper.AribaContract.FindFirstByCondition(x => x.ContractId == requisition.ContractId);
                AribaContractDetails contractdetails = _repositoryWrapper.AribaContractDetails.FindFirstByCondition(x => x.ContractId == requisition.ContractId);

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

            // // Send notification if task exists
            // var aribaTask = _repositoryWrapper.AribaTask.FindFirstByCondition(x => x.ApprovalId == invoice.AribaTaskId);
            // if (aribaTask != null)
            // {
            //     _emailService.CreateAribaApprovalEmailNotification(aribaTask, Common.ARIBA_APPROVAL_EMAIL_KEY);
            // }

            return Ok(new SuccessResponseDto { Message = "Successfully Updated Data", Description = "Success", StatusCode = 200 });
        }

        /// <summary>
        /// Get pending approvals for a domain
        /// </summary>
        [HttpGet]
        [Route("api/v1/ariba/pending-approvals-PR-usersync")]
        [ApiAuthorization(Name = "GET_PENDING_APPROVALS_PR_USERSYNC")]
        [SwaggerOperation("GetPendingApprovalsByUserSync")]
        [SwaggerResponse(200, type: typeof(List<AribaPRDto>), description: "Pending approvals fetched successfully")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Domain Not Found")]
        public async Task<IActionResult> GetPendingApprovalsByUserSync()
        {
            Guid userId = getLoggedUser();
            var userData = _repositoryWrapper.User.GetUserDetailsById(userId);
            Guid domainId = getUserDomainId();
            _logger.LogInfo($"Start: Fetching Latest PR Pending Provals By User Sync for user: {userId}");
            if (userData != null)
            {
                var data = _pRPendingApprovableService.ProcessPRApprovalsByUserIDAsync(userData?.AribaUserName, userData?.AribaPassword, domainId.ToString(), "");
            }
            _logger.LogInfo($"End: Fetching Latest PR Pending Provals By User Sync for user: {userId}");
            return Ok(new SuccessResponseDto { Message = "Successfully Updated Data for PR Approval", Description = "Success", StatusCode = 200 });
        }

        [HttpGet]
        [Route("api/v1/ariba/pending-approvals-PR-details-excel")]
        [ApiAuthorization(Name = "GET_PENDING_APPROVALS_DETAILS_PR_EXCEL")]
        [SwaggerOperation("ExportPendingApprovalsPRDetailsExcel")]
        [SwaggerResponse(200, type: typeof(List<AribaInvoiceDetailDto>), description: "Pending approvals details downloaded  successfully")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Domain Not Found")]
        public async Task<IActionResult> ExportPendingApprovalsPRDetailsExcel([FromQuery] int limit = 8,
   [FromQuery] string? searchTerm = null,
   [FromQuery] DateTime? fromDate = null,
   [FromQuery] DateTime? toDate = null,
   [FromQuery] string? status = null,
    [FromQuery] List<string>? glAccount = null
 )
        {
            Guid userId = getLoggedUser();
            Guid domainId = getUserDomainId();

            var result = await _aribaPRService.ExportPendingApprovalsExcelAsync(
            userId, domainId, limit, searchTerm, fromDate, toDate, status, glAccount);

            return File(result.FileContent,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                result.FileName);

        }

        [HttpPut]
        [Route("api/v1/ariba/PRPaymentStatus")]
        [ApiAuthorization(Name = "PR_UPDATE_PAYMENT_STATUS")]
        [SwaggerOperation("UpdatePRPaymentStatus")]
        [SwaggerResponse(200, type: typeof(SuccessResponseDto), description: "Updated the PR payment statu successfully")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Domain Not Found")]
        public async Task<IActionResult> UpdatePaymentStatus(
     
      [FromBody] UpdatePaymentStatusBodyDto dto)
        {


            await _aribaPRService.UpdatePaymentStatusAsync(dto.PRNumber, dto.PaymentStatus);
            return Ok(new { message = "Payment status updated successfully",dto. PRNumber, dto.PaymentStatus });


        }
    }
}
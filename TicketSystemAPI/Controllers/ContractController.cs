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
using TicketSystemAPI.Attributes;
using System.Net.Mail;
using Services;
using Entities.Models;
using NPOI.POIFS.FileSystem;
using ExceptionHandler;
using NPOI.SS.Formula.Functions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Diagnostics.Contracts;


namespace TicketSystemAPI.Controllers
{

    public class ContractController : BaseController
    {
        private readonly IContractService _contractService;
        private readonly IContractPendingService _contractPendingService;

        private readonly IEmailService _emailService;
        private readonly ILoggerManager _logger;
        private IRepositoryWrapper _repositoryWrapper;

        public ContractController(IContractService contractService, IContractPendingService contractPendingService, ILoggerManager logger, IRepositoryWrapper repositoryWrapper, IEmailService emailService)

        {
            _contractService = contractService;
            _contractPendingService = contractPendingService;
            _logger = logger;
            _repositoryWrapper = repositoryWrapper;

            _emailService = emailService;
        }

        /// <summary>
        /// Get pending approvals for a domain
        /// </summary>
        [HttpGet]
        [Route("api/v1/ariba/pending-approvals-contracts")]
        [ApiAuthorization(Name = "GET_PENDING_APPROVALS_CONTRACTS")]
        [SwaggerOperation("GetPendingApprovals")]
        [SwaggerResponse(200, type: typeof(List<AribaContractDto>), description: "Pending approvals fetched successfully")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Domain Not Found")]

        public async Task<IActionResult> GetPendingApprovalsDetails([FromQuery] int index = 0, [FromQuery] int limit = 8, [FromQuery] string? searchTerm = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null, [FromQuery] string? status = null)
        {
           {
    Guid domainId = getUserDomainId();
    Guid userId = getLoggedUser();

    var result = await _contractService.GetContractPendingApprovalsAsync( userId,domainId,index,limit,searchTerm,fromDate,toDate,status);

    return Ok(result);
        }
        }

        [HttpPatch]
        [Route("api/v1/ariba/contract/action")]
        [ApiAuthorization(Name = "POST_CONTRACT_ACTION")]
        [SwaggerOperation("PostAribaContractAction")]
        [SwaggerResponse(200, type: typeof(SuccessResponseDto), description: "Action executed successfully in Ariba")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Domain Not Found")]
        public async Task<IActionResult> PostAribaContractAction(
    [FromBody] AribaActionRequestDto actionRequest)
        {
            Guid domainId = getUserDomainId();
            Guid userId = getLoggedUser();
            MyProfileDto aribaUser = _repositoryWrapper.Person.GetMyProfileByUserId(userId);

            string result = await _contractService.PostRequisitionActionAsync(domainId, actionRequest.UniqueName, actionRequest, aribaUser);

            return Ok(new SuccessResponseDto
            {
                Message = "Successfully executed action in Ariba",
                Description = result,
                StatusCode = 200
            });
        }

  
        [HttpGet]
        [Route("api/v1/ariba/ContractDocument/{documentId}")]
        [ApiAuthorization(Name = "GET_CONTRACT_ATTACHMENT_WITH_ATTACHMENT_ID")]
        [SwaggerOperation("GetContractAttachmentWithAttachmentID")]
        [SwaggerResponse(200, type: typeof(SuccessResponseDto), description: "Contract attachment fetched successfully")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Domain Not Found")]
        public async Task<IActionResult> GetContractAttachmentWithAttachmentID([FromRoute][Required] string documentId)
        {
            Guid domainId = getUserDomainId();
            Guid userId = getLoggedUser();
            MyProfileDto aribauser = _repositoryWrapper.Person.GetMyProfileByUserId(userId)!;


            var base64String  =
                   await _contractService.GetContractDocumentById(
                       domainId,
                       documentId,
                       aribauser
                   );

            return Ok(new
    {
        FileName = "ContractDocument.pdf",
        FileType = "application/pdf",
        Base64 = base64String
    });
        }

        [HttpPatch]
        [Route("api/v1/ariba/contract/edit")]
        [ApiAuthorization(Name = "EDIT_CONTRACT_DETAILS")]
        [SwaggerOperation("EditContractDetails")]
        [SwaggerResponse(200, type: typeof(SuccessResponseDto), description: "Contract details edited successfully")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Contract not found")]
        public async Task<IActionResult> EditContractDetails(
        [FromQuery][Required] string TaskId,
        [FromBody] PatchContractDetailsDto patchRequest)
        {
            Guid domainId = getUserDomainId();
            Guid userId = getLoggedUser();
            MyProfileDto aribaUser = _repositoryWrapper.Person.GetMyProfileByUserId(userId)!;
            ContractDetails contract = _repositoryWrapper.ContractDetails
                .FindFirstByCondition(x => x.ContractTaskId == TaskId);

            if (contract == null)
            {
                return NotFound(new ErrorResponseDto
                {
                    StatusCode = 404,
                    Message = "Contract not found",
                    Description = $"No contract found with ContractId={TaskId}"
                });
            }

            if (aribaUser.Role == Common.BUSINESS_ADMIN_USER_KEY)
            {


                contract.TotalAmount = patchRequest.ContractValue?.ToString() ?? contract.TotalAmount;
                contract.Description = patchRequest.Description ?? contract.Description;
            
                _repositoryWrapper.ContractDetails.Update(contract);

                await _repositoryWrapper.SaveAsync();

                // 🔹 6. Optional: trigger email or audit log
                // var contractPending = _repositoryWrapper.ContractPending
                //     .FindFirstByCondition(x => x.UniqueName == contract.ContractTaskId);

                // if (contractPending != null)
                // {
                //     _emailService.CreateAribaApprovalEmailNotification(, Common.ARIBA_APPROVAL_EMAIL_KEY);
                // }
              _emailService.CreateContractEmailNotification(contract, aribaUser.AribaUserName, Common.ARIBA_CONTRACT_EMAIL_NOTIFICATION_KEY);
                return Ok(new SuccessResponseDto
                {
                    StatusCode = 200,
                    Message = "Successfully updated contract details",
                    Description = "Contract update success"
                });
            }
            else
            {
                throw new PreConditionFailedCustomException(
                    $"User not allowed to edit contract",
                    "Invalid operation - insufficient role");
            }
        }

[HttpGet]
[Route("api/v1/ariba/contract/has-attachment/{contractTaskId}")]
[ApiAuthorization(Name = "CHECK_CONTRACT_HAS_ATTACHMENT")]
[SwaggerOperation("CheckContractHasAttachment")]
[SwaggerResponse(200, type: typeof(SuccessResponseDto), description: "Checked contract attachment successfully")]
[SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
[SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Contract not found")]
public async Task<IActionResult> CheckContractHasAttachment([FromRoute][Required] string contractTaskId)
{
           _logger.LogDebug($"Fetching the Attachments for the Contract with Id : {contractTaskId}");
             contractTaskId= Uri.UnescapeDataString(contractTaskId);
            SplitAttachmentResponseDto result = _contractService.GetFinalContractAttachmentBase64(contractTaskId);
            _logger.LogDebug($"Fetched the Attachments for the Contract with Id : {contractTaskId}");
            return Ok(result);
        }


        [HttpGet]
        [Route("api/v1/ariba/pending-approvals-contract-details-excel")]
        [ApiAuthorization(Name = "CONTRACT_PENDING_APPROVALS_EXPORT_EXCEL")]
        [SwaggerOperation("ExportPendingApprovalsContractDetailsExcel")]
        [SwaggerResponse(200, type: typeof(List<AribaContractDto>), description: "Pending contract approvals details downloaded successfully")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Domain Not Found")]
        public async Task<IActionResult> ExportPendingApprovalsContractDetailsExcel(
            [FromQuery] int limit = 8,
            [FromQuery] string? searchTerm = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string? status = null)
        {
            Guid userId = getLoggedUser();
            Guid domainId = getUserDomainId();

            var result = await _contractService.ExportContractPendingApprovalsExcelAsync(
                userId, domainId, limit, searchTerm, fromDate, toDate, status);

            return File(result.FileContent,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                result.FileName);
        }

           [HttpGet]
        [Route("api/v1/ariba/pending-approvals-Contract-usersync")]
        [ApiAuthorization(Name = "GET_PENDING_APPROVALS_CONTRACT_USERSYNC")]
        [SwaggerOperation("GetPendingApprovalsByUserSync")]
        [SwaggerResponse(200, type: typeof(List<AribaContractDto>), description: "Pending approvals fetched successfully")]
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
                var data = _contractPendingService.ProcessContractApprovalsByUserIDAsync(userData?.AribaUserName, userData?.AribaPassword, domainId, "");
            }
            _logger.LogInfo($"End: Fetching Latest Contract Pending Provals By User Sync for user: {userId}");
            return Ok(new SuccessResponseDto { Message = "Successfully Updated Data for PR Approval", Description = "Success", StatusCode = 200 });
        }





    }
}
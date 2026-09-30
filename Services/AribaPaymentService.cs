using Contracts;
using Contracts.IRepository;
using Contracts.IServices;
using Dto;
using Entities.Common;
using Entities.Models;
using ExceptionHandler;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class AribaPaymentService : IAribaPaymentService
    {
        private readonly ILoggerManager _logger;
        private readonly IRepositoryWrapper _repositoryWrapper;
        public AribaPaymentService(ILoggerManager logger, IRepositoryWrapper repositoryWrapper)
        {
            _logger = logger;
            _repositoryWrapper = repositoryWrapper;
        }

        /// <summary>
        /// Fetches payment information.
        /// </summary>
        public async Task<List<PaymentResponseDto>> GetAllPayment(int index, int limit, Guid userId, Guid domainId, string? searchTerm, DateTime? fromDate, DateTime? toDate, string? status)
        {
            _logger.LogInfo($"Fetching all payment information from the database for user : {userId}.");
            MyProfileDto? profileDto = _repositoryWrapper.Person.GetMyProfileByUserId(userId);

            string aribaUserName = profileDto?.AribaUserName;
            string role = profileDto?.Role;

            if (string.IsNullOrEmpty(aribaUserName) && role != Common.BUSINESS_ADMIN_USER_KEY)
            {
                _logger.LogError($"Unable to Find Any Ariba User Name Linked with Given Id : {userId}");
                throw new NotFoundCustomException("User not mapped with Ariba", "User not mapped with Ariba");
            }

            IQueryable<AribaPayment> paymentQuery = _repositoryWrapper.AribaPayment.FindByCondition(x => x.IsActive && x.DomainId == domainId);

            if (role == Common.BUSINESS_USER_KEY)
            {
                if (string.IsNullOrEmpty(aribaUserName))
                {
                    _logger.LogError($"Unable to Find Any Records for User with Id : {userId}");
                    throw new NotFoundCustomException("Unable to Find Any Records", "Unable to Find Any Records");
                }
                paymentQuery = paymentQuery.Where(p =>
                    _repositoryWrapper.AribaInvoice
                        .FindByCondition(i =>
                            i.IsActive &&
                            i.InvoiceNumber == p.InvoiceNumber &&
                            i.RequesterUser == aribaUserName).Any()
                );
            }

            else if (role != Common.BUSINESS_ADMIN_USER_KEY)
            {
                paymentQuery = paymentQuery.Where(p =>
                    _repositoryWrapper.AribaInvoice
                        .FindByCondition(i => i.IsActive && i.InvoiceNumber == p.InvoiceNumber)
                        .Any(i => _repositoryWrapper.AribaTask.FindByCondition(t =>
                                    t.IsActive &&
                                    t.ApprovalId == i.AribaTaskId &&
                                    t.Approver == aribaUserName)
                                .Any()
                        )
                );
            }

            if (!string.IsNullOrEmpty(status))
            {
                paymentQuery = paymentQuery.Where(p => p.PaymentStatus != null && p.PaymentStatus.ToLower() == status.ToLower());
            }
            if (!string.IsNullOrEmpty(searchTerm))
            {
                paymentQuery = paymentQuery.Where(p =>
                    (p.PRNumber != null && p.PRNumber.ToLower() == searchTerm.ToLower()) ||
                    (p.PONumber != null && p.PONumber.ToLower() == searchTerm.ToLower()) ||
                    (p.InvoiceNumber != null && p.InvoiceId.ToLower() == searchTerm.ToLower()) ||
                    (p.SupplierName != null && p.SupplierName.ToLower() == searchTerm.ToLower())
                );
            }

            if (fromDate.HasValue)
            {
                paymentQuery = paymentQuery.Where(x => x.InvoiceDate >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                paymentQuery = paymentQuery.Where(x => x.InvoiceDate <= toDate.Value);
            }

            var finalAttachmentQuery = _repositoryWrapper.FinalAttachment.FindByCondition(x => x.IsActive).GroupBy(x => x.AribaId).Select(g => g.Key);
            var prFinalAttachmentQuery = _repositoryWrapper.FinalPRAttachment.FindByCondition(x => x.IsActive).GroupBy(x => x.AribaId).Select(g => g.Key);

            var approvalHistoryLookup = (await _repositoryWrapper.AribaInvoiceApprovalHistory
                .FindByCondition(h => h.IsActive)
                .OrderByDescending(h => h.DateCreated)
                .ToListAsync())
                .ToLookup(h => h.InvoiceID);


            var result = await (
                from p in paymentQuery
                join cm in _repositoryWrapper.AribaCompanyMapping.FindByCondition(c => c.IsActive)
                    on p.CompanyUniqueName equals cm.CompanyCode into cmJoin
                from cm in cmJoin.DefaultIfEmpty()   // LEFT JOIN

                join fa in finalAttachmentQuery
                on p.InvoiceId equals fa into faJoin
                from fa in faJoin.DefaultIfEmpty()

                join pra in prFinalAttachmentQuery
                on p.PRNumber equals pra into praJoin
                from pra in praJoin.DefaultIfEmpty()

                orderby p.DateUpdated descending
                select new PaymentResponseDto
                {
                    Id = p.Id,
                    PRNumber = p.PRNumber,
                    PONumber = p.PONumber,
                    InvoiceNumber = p.InvoiceNumber,
                    InvoiceAmount = p.InvoiceAmount,
                    AribaPaymentId = p.AribaPaymentId,
                    ERPInvoiceNumber = p.ERPInvoiceNumber,
                    AdvancePaymentId = p.AdvancePaymentId,
                    AdvanceAmount = p.AdvanceAmount,
                    AdvancePaymentStatus = p.AdvancePaymentStatus,
                    PaymentStatus = p.PaymentStatus,
                    PaymentId = p.PaymentId,
                    PaidAmount = p.PaidAmount,
                    PaidDate = p.PaidDate,
                    BankClearance = p.BankClearance,
                    BankClearanceDate = p.BankClearanceDate,
                    SupplierName = p.SupplierName,
                    Description = p.Description,
                    ContractId = p.ContractId,
                    CompanyUniqueName = cm != null ? cm.CompanyName : null,
                    ContractValue = p.ContractValue,
                    ValueAlreadyPaid = p.ValueAlreadyPaid,
                    RemainingBalance = p.RemainingBalance,
                    HasAttachment = fa != null || pra != null,
                    InvoiceId = p.InvoiceId,
                    InvoiceDate = p.InvoiceDate,
                    InvoiceApprovalHistory = p.InvoiceId != null ? approvalHistoryLookup[p.InvoiceId].ToList() : new List<AribaInvoiceApprovalHistory>()
                }
            )
            .Skip(index)
            .Take(limit)
            .ToListAsync();


            if (!result.Any())
            {
                _logger.LogError($"No Payment Records Found for user : {userId}");
                throw new NotFoundCustomException("No Payment Records Found", "No Payment Records Found");
            }

            _logger.LogInfo($"Payment records fetched successfully for user : {userId}");
            return result;
        }
        /// <summary>
        /// Fetches final attachment in Base64 format for a given invoice ID.
        /// </summary>
        /// <param name="invoiceId"></param>
        /// <returns></returns>
        public SplitAttachmentResponseDto GetFinalAttachmentBase64(string invoiceId)
        {
            _logger.LogInfo($"Fetching the Attachments for the Invoice with Id : {invoiceId}");

            if (string.IsNullOrWhiteSpace(invoiceId))
            {
                _logger.LogError($"Invoice Id is Empty : {invoiceId}");
                throw new PreConditionFailedCustomException("Invoice ID cannot be null or empty", "Invoice ID cannot be null or empty");
            }

            AribaPayment aribaPayment = _repositoryWrapper.AribaPayment.FindFirstByCondition(p => p.InvoiceId.ToLower() == invoiceId.ToLower() && p.IsActive);

            if (aribaPayment == null)
            {
                _logger.LogError($"No payment record found for Invoice ID: {invoiceId}");
                throw new NotFoundCustomException($"No payment record found for Invoice ID: {invoiceId}", $"No payment record found for Invoice ID: {invoiceId}");
            }

            SplitAttachmentResponseDto result = _repositoryWrapper.AribaPayment.GetAllAttachmentsByInvoiceId(invoiceId);

            if (result == null)
            {
                _logger.LogError($"No Attachments Found for the Invoice with Id : {invoiceId}");
                throw new NotFoundCustomException($"No Attachments Found for the Invoice with Id : {invoiceId}", $"No Attachments Found for the Invoice with Id : {invoiceId}");
            }

            return result;
        }
    }
}

using Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.IServices
{
    public interface IAribaPaymentService
    {
        /// <summary>
        /// Fetches payment information.
        /// </summary>
        Task<List<PaymentResponseDto>> GetAllPayment(int index, int limit, Guid userId, Guid domainId, string? searchTerm, DateTime? fromDate, DateTime? toDate, string? status);
        /// <summary>
        /// Fetches final attachment in Base64 format for a given invoice ID.
        /// </summary>
        /// <param name="invoiceId"></param>
        /// <returns></returns>
        SplitAttachmentResponseDto GetFinalAttachmentBase64(string invoiceId);
    }
}

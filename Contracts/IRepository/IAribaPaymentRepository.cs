using Dto;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.IRepository
{
    /// <summary>
    /// AribaPaymentRepository
    /// </summary>
    public interface IAribaPaymentRepository : IRepositoryBase<AribaPayment>
    {
        /// <summary>
        /// GetAllAttachmentsByInvoiceId
        /// </summary>
        /// <param name="invoiceId"></param>
        /// <returns></returns>
        SplitAttachmentResponseDto GetAllAttachmentsByInvoiceId(string invoiceId);
    }
}

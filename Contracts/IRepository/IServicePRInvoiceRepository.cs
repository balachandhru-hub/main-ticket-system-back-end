using Entities.Models;
using System;
using System.Collections.Generic;

namespace Contracts.IRepository
{
    public interface IServicePRInvoiceRepository : IRepositoryBase<ServicePRInvoice>
    {
        Task<ExtractedInvoiceDto?> GetExtractedInvoiceByPRAsync(
    string prNumber,
    string invoiceId,
    int? lineItemNumber = null);
    }
}

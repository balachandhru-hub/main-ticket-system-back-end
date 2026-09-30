using Entities.Models;
using System;
using System.Collections.Generic;

namespace Contracts.IRepository
{
    public interface IMaterialInvoiceRepository : IRepositoryBase<MaterialInvoice>
    {
        Task<ExtractedInvoiceDto?> GetExtractedInvoiceByPRAsync(
    string prNumber,
    string invoiceId,
    int? lineItemNumber = null);
    Task<ExtractedSSIDDto?> GetExtractedSSIDByPRAsync(
    string ponumber,
    int? lineItemNumber) ;
    Task<ExtractedInvoiceDto?> GetExtractedInvoiceByServicePRAsync(
    string prNumber,
    string invoiceId,
    int? lineItemNumber = null);
    }
}

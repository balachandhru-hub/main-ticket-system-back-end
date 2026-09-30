using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.IServices
{
    public interface IInvoicePendingApprovableService
    {
        Task ProcessAsync(string TaskId);
        Task ProcessInvoiceAsyncByUserId(string aribaUserName, string aribaPassword,string domainId, string TaskId);
    }
}

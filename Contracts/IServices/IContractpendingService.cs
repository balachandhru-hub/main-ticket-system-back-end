using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.IServices
{
    public interface IContractPendingService
    {
        Task ProcessAsync(string TaskId);
        Task ProcessContractApprovalsByUserIDAsync(string aribaUserName, string aribaPassword, Guid DomainId, string TaskId = "");
    }
}

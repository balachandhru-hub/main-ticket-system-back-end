using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.IServices
{
    public interface IPRPendingApprovableService
    {
        Task ProcessAsync(string TaskId);
        Task ProcessPRApprovalsByUserIDAsync(string aribaUserName, string aribaPassword, string DomainId, string TaskId);
    }
}

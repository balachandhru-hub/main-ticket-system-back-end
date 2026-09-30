using Dto;
using System;
using System.Threading.Tasks;

namespace Contracts.IServices
{
    public interface ISchedulerHistoryService
    {
        Task<SchedulerHistoryResponseDto> GetLastSchedulerEndDate(string schedulerKey);
    }
}
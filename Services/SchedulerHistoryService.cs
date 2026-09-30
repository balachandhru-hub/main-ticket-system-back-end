using Contracts;
using Contracts.IRepository;
using Contracts.IServices;
using Dto;
using Entities.Common;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Services
{
    public class SchedulerHistoryService : ISchedulerHistoryService
    {
        private readonly IRepositoryWrapper _repoWrapper;
        private readonly ILoggerManager _logger;

        public SchedulerHistoryService(IRepositoryWrapper repoWrapper, ILoggerManager logger)
        {
            _repoWrapper = repoWrapper;
            _logger = logger;
        }

        public async Task<SchedulerHistoryResponseDto> GetLastSchedulerEndDate(string schedulerKey)
        {
            try
            {
                _logger.LogInfo($"[SchedulerHistory] Fetching last end date for scheduler key: {schedulerKey}");

                var lastRecord = await _repoWrapper.SchedularHistory
                    .FindByCondition(x => x.SchedularKey == schedulerKey)
                    .OrderByDescending(x => x.EndDate)
                    .FirstOrDefaultAsync();

                var response = new SchedulerHistoryResponseDto
                {
                    SchedulerKey = schedulerKey,
                    LastEndDate = lastRecord?.EndDate,
                    NextStartDate = lastRecord?.EndDate.HasValue == true 
                        ? schedulerKey == Common.SCHEDULAR_KEY_FETCH_ARIBA_CONTRACT 
                            ? lastRecord.EndDate.Value.AddHours(1) // 1 hour for contracts
                            : lastRecord.EndDate.Value.AddMinutes(10) // 10 minutes for others
                        : DateTime.UtcNow
                };

                _logger.LogInfo($"[SchedulerHistory] Last end date for {schedulerKey}: {response.LastEndDate}, Next start date: {response.NextStartDate}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[SchedulerHistory] Error fetching scheduler history for {schedulerKey}: {ex.Message}");
                throw;
            }
        }
    }
}
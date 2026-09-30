using Contracts.IRepository;
using Contracts;
using Entities.Models;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Threading.Tasks;

namespace Services
{
    public class DomainContractExpiryBackgroundService : IJob
    {
        private readonly IServiceProvider _serviceProvider;

        public DomainContractExpiryBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using (IServiceScope scope = _serviceProvider.CreateScope())
            {
                ILoggerManager logger = scope.ServiceProvider.GetRequiredService<ILoggerManager>();
                IRepositoryWrapper repositoryWrapper = scope.ServiceProvider.GetRequiredService<IRepositoryWrapper>();

                logger.LogInfo("Checking for expired domain plans to deactivate...");

                var expiredPlans = repositoryWrapper.DomainPlan
                    .FindByCondition(dp => dp.IsActive && dp.IsRunning&& dp.ContractEndDate.Date < DateTime.UtcNow.Date)
                    .ToList();

              
                foreach (DomainPlan plan in expiredPlans)
                {
                    plan.IsActive = false;
                    plan.IsRunning = false;
                    logger.LogInfo($"Deactivated DomainPlan: {plan.Id} for DomainId: {plan.DomainId} (expired on {plan.ContractEndDate:yyyy-MM-dd})");
                }

                repositoryWrapper.DomainPlan.UpdateRange(expiredPlans);
                repositoryWrapper.Save();

                logger.LogInfo($"DomainContractExpiryService: domain plans deactivated.");
            }
        }
    }
}
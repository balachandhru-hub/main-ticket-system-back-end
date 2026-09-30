using System.Diagnostics.CodeAnalysis;
using Contracts;
using Contracts.IRepository;
using Entities.Models;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Services
{
    [ExcludeFromCodeCoverage]
    public class CreditDefaultBackgroundService : IJob
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the CreditDefaultBackgroundService class.
        /// </summary>
        public CreditDefaultBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Executes the scheduled job.
        /// </summary>
        /// <param name="context">The job execution context.</param>
        public Task Execute(IJobExecutionContext context)
        {
            CreditDefault();
            return Task.CompletedTask;
        }

        private void CreditDefault()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILoggerManager>();
                var repositoryWrapper = scope.ServiceProvider.GetRequiredService<IRepositoryWrapper>();

                logger.LogInfo("Triggering Credit Default for the Plans");
                List<DomainPlanCredit> domainPlanCredit = (from dpc in repositoryWrapper.DomainPlanCredit.FindByCondition(x => x.IsActive)
                                                           join dp in repositoryWrapper.DomainPlan.FindByCondition(x => x.IsRunning)
                                                           on dpc.DomainPlanId equals dp.Id
                                                           select dpc).ToList();

                if (domainPlanCredit.Any())
                {
                    domainPlanCredit.ForEach(x => x.MonthlyHourCredit = x.ActualMonthlyHourCredit);
                    repositoryWrapper.DomainPlanCredit.UpdateRange(domainPlanCredit);
                    repositoryWrapper.Save();
                }

                logger.LogInfo("Triggered Credit Default for the Plans");
            }
        }
    }
}
using Contracts;
using Contracts.IServices;
using Entities.Common;
using Microsoft.Extensions.Configuration;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
//using Contracts.IServices;

namespace Services
{
    public class SchedulerService : ISchedulerService
    {
        private readonly IJobFactory _myJobFactory;
        private readonly ILoggerManager _logger;
        private readonly IConfiguration _configuration;

        public SchedulerService(IJobFactory myJobFactory, ILoggerManager logger, IConfiguration configuration)
        {
            _myJobFactory = myJobFactory;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task Start()
        {
            _logger.LogInfo("Starting the scheduler service");

            var schedulerFactory = new StdSchedulerFactory();
            var scheduler = await schedulerFactory.GetScheduler();
            scheduler.JobFactory = _myJobFactory;

            // Email Reminder Job
            var job = JobBuilder.Create<EmailRemainderService>()
                .WithIdentity("EmailRemainderService", "group1")
                .Build();

            var immediateTrigger = TriggerBuilder.Create()
                .WithIdentity("immediateTrigger", "group1")
                .StartNow()
                .Build();

            var dailyTrigger = TriggerBuilder.Create()
                .WithIdentity("dailyTrigger", "group1")
                .WithSchedule(DailyTimeIntervalScheduleBuilder.Create()
                    .OnEveryDay()
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(
                        int.Parse(_configuration[Common.SCHEDULER_HOUR]!),
                        int.Parse(_configuration[Common.SCHEDULER_MINUTE]!)))
                    .EndingDailyAfterCount(int.Parse(_configuration[Common.SCHEDULER_COUNT]!)))
                .Build();

            await scheduler.ScheduleJob(job, new ITrigger[] { immediateTrigger, dailyTrigger }, replace: true);

            await scheduler.Start();
            _logger.LogInfo("Scheduler service started successfully");
        }

        public async Task ScheduleFailedEmailJob()
        {
            _logger.LogInfo("Scheduling Failed emails as a background job");

            var schedulerFactory = new StdSchedulerFactory();
            var scheduler = await schedulerFactory.GetScheduler();

            IJobDetail job = JobBuilder.Create<EmailFailedBackgroundService>()
                .WithIdentity("EmailFailedJob", "group2")
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("EmailFailedJob", "group2")
                .StartNow()
                .WithCronSchedule("0 0 * ? * *") // Every hour
                .Build();

            await scheduler.ScheduleJob(job, trigger);
            await scheduler.Start();

            _logger.LogInfo("Failed emails background job scheduled successfully");
        }

        /// <summary>
        /// Schedules the DomainPlanRemainderService job.
        /// Private helper method used internally in Start().
        /// </summary>
        public async Task ScheduleDomainPlanRemainderJob()
        {
            _logger.LogInfo("Scheduling DomainPlanRemainderService job...");

            var schedulerFactory = new StdSchedulerFactory();
            var scheduler = await schedulerFactory.GetScheduler();
            IJobDetail job = JobBuilder.Create<DomainPlanBackgroundService>()
                .WithIdentity("DomainPlanRemainderJob", "group3")
                .Build();

            ITrigger initialTrigger = TriggerBuilder.Create()
                .WithIdentity("DomainPlanRemainderTrigger", "group3")
                .Build();

            ITrigger everyDayTrigger = TriggerBuilder.Create()
                .WithIdentity("DomainPlanRemainderDailyTrigger", "group3")
                .WithCronSchedule("0 0 12 * * ?")
                .Build();

            await scheduler.ScheduleJob(job, new ITrigger[] { initialTrigger, everyDayTrigger }, false);
            _logger.LogInfo("DomainPlanRemainderService job scheduled successfully.");
        }
        public async Task ScheduleDomainContractExpiryJob()
        {
            _logger.LogInfo("Scheduling DomainContractExpiryService job...");

            var schedulerFactory = new StdSchedulerFactory();
            var scheduler = await schedulerFactory.GetScheduler();
            scheduler.JobFactory = _myJobFactory;

            IJobDetail job = JobBuilder.Create<DomainContractExpiryBackgroundService>()
                .WithIdentity("DomainContractExpiryJob", "group4")
                .Build();


            ITrigger initialTrigger = TriggerBuilder.Create()
                .WithIdentity("DomainContractExpiryTrigger", "group4")
                .StartNow()
                .Build();

            ITrigger everyDayTrigger = TriggerBuilder.Create()
                .WithIdentity("DomainContractExpiryDailyTrigger", "group4")
                .WithCronSchedule("0 0 1 * * ?") // Runs every day at 1 AM
                .Build();

            await scheduler.ScheduleJob(job, new ITrigger[] { initialTrigger, everyDayTrigger }, false);
            _logger.LogInfo("DomainContractExpiryService job scheduled successfully.");
        }
        public async Task ScheduleAribaInvoiceJob()
        {
            _logger.LogInfo($"Scheduling AribaInvoiceJob...");
            var schedulerFactory = new StdSchedulerFactory();
            var scheduler = await schedulerFactory.GetScheduler();
            IJobDetail job = JobBuilder.Create<AribaInvoiceJob>()
                .WithIdentity("AribaInvoiceJob", "group5")
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("AribaInvoiceJob", "group5")
                .WithCronSchedule("0 0/10 * * * ?")//every 10 minutes daily
                .Build();


            await scheduler.ScheduleJob(job, new ITrigger[] { trigger }, replace: true);
            await scheduler.Start();
            _logger.LogInfo($"Scheduled AribaInvoiceJob...");
        }


        //   public async Task ScheduleContractJob()
        // {
        //     _logger.LogInfo($"Scheduling ContractJob...");
        //     var schedulerFactory = new StdSchedulerFactory();
        //     var scheduler = await schedulerFactory.GetScheduler();
        //     IJobDetail job = JobBuilder.Create<ContractJob>()
        //         .WithIdentity("ContractJob", "group6")
        //         .Build();

        //     ITrigger trigger = TriggerBuilder.Create()
        //         .WithIdentity("ContractJob", "group6")
        //         .StartNow()
        //         .Build();


        //     await scheduler.ScheduleJob(job, trigger);
        //     await scheduler.Start();
        //     _logger.LogInfo($"Scheduled ContractJob...");
        // }

        /// <summary>
        /// Schedules a job to Add the Default Credits
        /// </summary>
        /// <returns></returns>
        /// 
        public async Task ScheduleAribPRJob()
        {
            _logger.LogInfo($"Scheduling AribaPRJob...");
            var schedulerFactory = new StdSchedulerFactory();
            var scheduler = await schedulerFactory.GetScheduler();
            IJobDetail job = JobBuilder.Create<AribaPRJob>()
                .WithIdentity("AribaPRJob", "group6")
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("AribaPRJob", "group6")
                .WithCronSchedule("0 0/10 * * * ?")
                .Build();


            await scheduler.ScheduleJob(job, new ITrigger[] { trigger }, replace: true);
            await scheduler.Start();
            _logger.LogInfo($"Scheduled AribaPRJob...");
        }
        
          public async Task ScheduleAribContractJob()
        {
            _logger.LogInfo($"Scheduling AribaContractJob...");
            var schedulerFactory = new StdSchedulerFactory();
            var scheduler = await schedulerFactory.GetScheduler();
            IJobDetail job = JobBuilder.Create<AribaContractJob>()
                .WithIdentity("AribaContractJob", "group7")
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("AribaContractJob", "group7")
                .WithCronSchedule("0 0/10 * * * ?")
                .Build();


            await scheduler.ScheduleJob(job, new ITrigger[] { trigger }, replace: true);
            await scheduler.Start();
            _logger.LogInfo($"Scheduled AribaContractJob...");
        }

        public async Task SchedulePlanCreditDefault()
        {
            _logger.LogInfo($"Scheduling Plan Credit...");
            var schedulerFactory = new StdSchedulerFactory();
            var scheduler = await schedulerFactory.GetScheduler();

            IJobDetail job = JobBuilder.Create<CreditDefaultBackgroundService>()
                .WithIdentity("CreditDefaultBackgroundService", "group6")
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("CreditDefaultBackgroundService", "group6")
                .WithCronSchedule("0 0 0 1 * ?")
                .Build();

            await scheduler.ScheduleJob(job, trigger);
            await scheduler.Start();
            _logger.LogInfo($"Scheduled Plan Credit...");
        }


    }
}

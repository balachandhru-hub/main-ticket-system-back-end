using Contracts;
using Contracts.IRepository;
using Contracts.IServices;
using Dto;
using Entities.Common;
using Entities.Models;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System.Diagnostics.CodeAnalysis;

namespace Services
{
    /// <summary>
    /// Service for handling email reminders using Quartz.NET scheduler.
    /// </summary>
    /// <remarks>
    /// This method is excluded from code coverage analysis as it is used internally for remainder mail 
    /// it cannot be triggered manually and does not contribute to the public API surface.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public class EmailFailedBackgroundService : IJob
    {
        private readonly IServiceProvider _serviceProvider;



        /// <summary>
        /// Initializes a new instance of the EmailRemainderService class.
        /// </summary>
        /// <param name="repositoryWrapper">The repository wrapper.</param>
        /// <param name="emailService">The appointment service.</param>
        /// <param name="metaData">The metadata service.</param>
        /// <param name="logger">The logger.</param>
        public EmailFailedBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Executes the scheduled job.
        /// </summary>
        /// <param name="context">The job execution context.</param>
        public Task Execute(IJobExecutionContext context)
        {
            TriggerEmail();
            return Task.CompletedTask;
        }
        private void TriggerEmail()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILoggerManager>();
                var repositoryWrapper = scope.ServiceProvider.GetRequiredService<IRepositoryWrapper>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                logger.LogInfo("Triggering Email for records with failed email sending");


                List<EmailFailedDetail> emailFailedDetails = repositoryWrapper.EmailFailedDetail.FindByCondition(x => x.IsActive && x.TriggerCount <= 3).ToList();

                foreach (EmailFailedDetail emailFailedDetail in emailFailedDetails)
                {
                    EmailPlaceholderDto emailPlaceholderDto = new EmailPlaceholderDto()
                    {
                        ToEmail = emailFailedDetail.Email,
                        Subject = emailFailedDetail.Subject,
                        EmailBody = emailFailedDetail.Body,
                        EntityId = emailFailedDetail.EntityId,
                        EmailType = emailFailedDetail.EmailType,
                        EntityType = emailFailedDetail.EntityType,
                    };
                    emailService.SendEmail(emailPlaceholderDto.ToEmail!, emailPlaceholderDto.Subject!, emailPlaceholderDto.EmailBody!, emailPlaceholderDto.EmailType!, emailPlaceholderDto.EntityType!, emailPlaceholderDto.EntityId, null);
                    emailService.CreateEmailSentDetails(emailPlaceholderDto);
                }
                logger.LogInfo($"TriggerEmail: {emailFailedDetails.Count} emails triggered successfully.");
            }


        }
    }
}

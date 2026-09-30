using Contracts;
using Contracts.IRepository;
using Contracts.IServices;
using Dto;
using Entities.Models;
using Entities.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Quartz;
using System.Diagnostics.CodeAnalysis;

namespace Services
{
    [ExcludeFromCodeCoverage]
    public class DomainPlanBackgroundService : IJob
    {
        private readonly IServiceProvider _serviceProvider;

        public DomainPlanBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task Execute(IJobExecutionContext context)
        {
            TriggerDomainPlanReminderEmails();
            return Task.CompletedTask;
        }

        private void TriggerDomainPlanReminderEmails()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILoggerManager>();
                var repositoryWrapper = scope.ServiceProvider.GetRequiredService<IRepositoryWrapper>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                string supportEmail = configuration[Common.SUPPORT_EMAIL];
                string companyName = configuration[Common.COMPANY_NAME];
                string portalLink = configuration[Common.PORTAL_LINK];

                logger.LogInfo("Triggering reminder emails for expired and inactive domain plans.");

                List<DomainPlan> expiredPlans = repositoryWrapper.DomainPlan
                    .FindByCondition(dp => !dp.IsActive && dp.ContractEndDate < DateTime.UtcNow)
                    .ToList();

                List<Guid> expiredDomainIds = expiredPlans.Select(dp => dp.DomainId).Distinct().ToList();

                List<Domain> expiredDomains = repositoryWrapper.Domain
                    .FindByCondition(d => expiredDomainIds.Contains(d.Id) && d.IsActive)
                    .ToList();

                EmailContent expiredEmailContent = repositoryWrapper.EmailContent
                    .FindFirstByCondition(e => e.Key == Common.REMAINDER_EMAIL_KEY);

                string expiredSubjectTemplate = expiredEmailContent?.Subject;
                string expiredBodyTemplate = expiredEmailContent?.Body;

                int expiredEmailCount = 0;
                foreach (var domain in expiredDomains)
                {

                    bool hasActivePlan = repositoryWrapper.DomainPlan
                        .FindByCondition(dp => dp.DomainId == domain.Id && dp.IsActive && dp.ContractEndDate >= DateTime.UtcNow)
                        .Any();

                    if (hasActivePlan) continue;

                    if (!string.IsNullOrEmpty(domain.SupportAdminEmail))
                    {
                        EmailPlaceholderDto emailPlaceholder = new EmailPlaceholderDto
                        {
                            EntityId = domain.Id,
                            EmailType = Common.REMAINDER_EMAIL_KEY,
                            EntityType = Common.DOMAIN_ENTITY_TYPE,
                            ToEmail = domain.SupportAdminEmail,
                            CompanyName = companyName,
                            SupportEmail = supportEmail,
                            PortalLink = portalLink,
                            Title = domain.Name,
                            SupportAdminEmail = domain.SupportAdminEmail,
                            PersonName = domain.SupportAdminName
                        };

                        string subject = emailService.ReplacePlaceholders(expiredSubjectTemplate, emailPlaceholder);
                        string body = emailService.ReplacePlaceholders(expiredBodyTemplate, emailPlaceholder);

                        try
                        {
                            if (emailService.SendEmail(
                                emailPlaceholder.ToEmail,
                                subject,
                                body,
                                emailPlaceholder.EmailType,
                                emailPlaceholder.EntityType,
                                emailPlaceholder.EntityId, null))
                            {
                                emailService.CreateEmailSentDetails(emailPlaceholder);
                                expiredEmailCount++;
                                logger.LogInfo($"Reminder email sent to {domain.SupportAdminEmail} for domain {domain.Name}.");
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError($"Failed to send domain plan reminder email for domain {domain.Name}: {ex.Message}");
                        }
                    }
                }

                logger.LogInfo($"TriggerDomainPlanReminderEmails: {expiredEmailCount} reminder emails triggered for expired/inactive plans.");



                logger.LogInfo("Triggering daily reminder emails for active domain plans expiring within 15 days.");

                List<DomainPlan> expiringSoonPlans = repositoryWrapper.DomainPlan
                    .FindByCondition(dp => dp.IsActive
                        && dp.ContractEndDate > DateTime.UtcNow
                        && dp.ContractEndDate <= DateTime.UtcNow.AddDays(15))
                    .ToList();

                List<Guid> expiringDomainIds = expiringSoonPlans.Select(dp => dp.DomainId).Distinct().ToList();

                List<Domain> expiringDomains = repositoryWrapper.Domain
                    .FindByCondition(d => expiringDomainIds.Contains(d.Id) && d.IsActive)
                    .ToList();

                EmailContent expiringSoonEmailContent = repositoryWrapper.EmailContent
                    .FindFirstByCondition(e => e.Key == Common.REMAINDER_EMAIL_EXPIRING_KEY);

                string expiringSubjectTemplate = expiringSoonEmailContent?.Subject;
                string expiringBodyTemplate = expiringSoonEmailContent?.Body;

                int expiringEmailCount = 0;
                foreach (var plan in expiringSoonPlans)
                {
                    var domain = expiringDomains.FirstOrDefault(d => d.Id == plan.DomainId);
                    if (domain == null || string.IsNullOrEmpty(domain.SupportAdminEmail)) continue;

                    bool hasAnotherLongerPlan = repositoryWrapper.DomainPlan
                        .FindByCondition(dp => dp.DomainId == domain.Id && dp.IsActive && dp.ContractEndDate > DateTime.UtcNow.AddDays(15))
                        .Any();

                    if (hasAnotherLongerPlan) continue;

                    int daysLeft = (plan.ContractEndDate.Date - DateTime.UtcNow.Date).Days;

                    EmailPlaceholderDto emailPlaceholder = new EmailPlaceholderDto
                    {
                        EntityId = domain.Id,
                        EmailType = Common.REMAINDER_EMAIL_EXPIRING_KEY,
                        EntityType = Common.DOMAIN_ENTITY_TYPE,
                        ToEmail = domain.SupportAdminEmail,
                        CompanyName = companyName,
                        SupportEmail = supportEmail,
                        PortalLink = portalLink,
                        Title = domain.Name,
                        SupportAdminEmail = domain.SupportAdminEmail,
                        PersonName = domain.SupportAdminName,
                        ExpiryDate = plan.ContractEndDate.ToString("yyyy-MM-dd"),
                        DaysLeft = daysLeft.ToString()
                    };

                    string subject = emailService.ReplacePlaceholders(expiringSubjectTemplate, emailPlaceholder);
                    string body = emailService.ReplacePlaceholders(expiringBodyTemplate, emailPlaceholder);

                    try
                    {
                        if (emailService.SendEmail(
                            emailPlaceholder.ToEmail,
                            subject,
                            body,
                            emailPlaceholder.EmailType,
                            emailPlaceholder.EntityType,
                            emailPlaceholder.EntityId, null))
                        {
                            emailService.CreateEmailSentDetails(emailPlaceholder);
                            expiringEmailCount++;
                            logger.LogInfo($"Daily reminder email sent to {domain.SupportAdminEmail} for domain {domain.Name} (expires on {plan.ContractEndDate:yyyy-MM-dd}, {daysLeft} days left).");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Failed to send daily expiring domain plan reminder email for domain {domain.Name}: {ex.Message}");
                    }
                }

                logger.LogInfo($"TriggerDomainPlanReminderEmails: {expiringEmailCount} daily reminder emails triggered for active expiring plans.");
            }
        }
    }
}

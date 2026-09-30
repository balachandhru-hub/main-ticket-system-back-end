using Contracts;
using Contracts.IRepository;
using Contracts.IServices;
using Entities.Common;
using Entities.Models;
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
    public class EmailRemainderService : IJob
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerManager _logger;

        /// <summary>
        /// Initializes a new instance of the EmailRemainderService class.
        /// </summary>
        /// <param name="repositoryWrapper">The repository wrapper.</param>
        /// <param name="logger">The logger.</param>
        public EmailRemainderService(IRepositoryWrapper repositoryWrapper, ILoggerManager logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
        }

        /// <summary>
        /// Executes the scheduled job.
        /// </summary>
        /// <param name="context">The job execution context.</param>
        public Task Execute(IJobExecutionContext context)
        {
            DeleteOldEmailSentDetails();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Deletes old email sent details that are 15 days or older.
        /// </summary>
        private void DeleteOldEmailSentDetails()
        {
            _logger.LogInfo("Initiated the process of deleting the emailSentDetails older than 15 days");
            DateTime dateToDeleteBefore = DateTime.Today.AddDays(-15).ToUniversalTime();

            var recordsToDelete = _repositoryWrapper.EmailSentDetail
                .FindByCondition(x => x.DateCreated <= dateToDeleteBefore)
                .ToList();

            foreach (var record in recordsToDelete)
            {
                _repositoryWrapper.EmailSentDetail.Delete(record);
            }

            var recordsToDeleteEmailFailed = _repositoryWrapper.EmailFailedDetail
                .FindByCondition(x => x.DateCreated <= dateToDeleteBefore)
                .ToList();

            foreach (var record in recordsToDeleteEmailFailed)
            {
                _repositoryWrapper.EmailFailedDetail.Delete(record);
            }

            _repositoryWrapper.Save();
            _logger.LogInfo("Completed the process of deleting the emailSentDetails older than 15 days");
        }
    }
}

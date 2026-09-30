namespace Contracts.IServices
{
    public interface ISchedulerService
    {
        /// <summary>
        /// Starts the scheduler service asynchronously.
        /// </summary>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task Start();


        /// <summary>
        /// Schedules a job to handle failed email sending.
        /// </summary>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task ScheduleFailedEmailJob();

        /// <summary>
        /// Schedules a job to send reminder emails for expired or inactive domain plans.
        /// </summary>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task ScheduleDomainPlanRemainderJob();
        /// <summary>
        /// Schedules a job to send reminder emails for expired or inactive domain contracts.
        /// </summary>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task ScheduleDomainContractExpiryJob();

        Task ScheduleAribaInvoiceJob();
        Task ScheduleAribPRJob();
        Task ScheduleAribContractJob();
        /// <summary>
        /// Schedules a job to Add the Default Credits
        /// </summary>
        /// <returns></returns>

        Task SchedulePlanCreditDefault();
        /// <summary>
        /// Schedules a job to handle Contracts
        /// </summary>
        // Task ScheduleContractJob();
      
    }
}
using System.Diagnostics.CodeAnalysis;
using Quartz;
using Quartz.Spi;
using ExceptionHandler;

namespace Services
{
    /// <remarks>
    /// This method is excluded from code coverage analysis as it is used internally for Email schedular 
    /// does not contribute to the public API surface.
    /// </remarks>

    [ExcludeFromCodeCoverage]
    public class MyJobFactoryService : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the MyJobFactoryService class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used for job instantiation.</param>
        public MyJobFactoryService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Creates a new job instance.
        /// </summary>
        /// <param name="bundle">The TriggerFiredBundle containing job details.</param>
        /// <param name="scheduler">The scheduler instance.</param>
        /// <returns>A new job instance.</returns>

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            if (_serviceProvider.GetService(bundle.JobDetail.JobType) is IJob job)
            {
                return job;
            }

            throw new FailedDependencyCustomException($"Job of type {bundle.JobDetail.JobType} could not be created.", $"Job of type {bundle.JobDetail.JobType} could not be created.");
        }


        /// <summary>
        /// Returns the job instance.
        /// </summary>
        /// <param name="job">The job instance to be returned.</param>
        public void ReturnJob(IJob job)
        {
            var disposable = job as IDisposable;
            disposable?.Dispose();
        }
    }
}
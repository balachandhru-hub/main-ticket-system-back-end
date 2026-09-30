using Contracts.IRepository;
using Entities.Models;
using Entities.DbContext;
using Contracts;

namespace Repository
{
    public class DomainPlanRepository : RepositoryBase<DomainPlan>, IDomainPlanRepository
    {
        private readonly ILoggerManager _logger;
        public DomainPlanRepository(RepositoryContext repositoryContext, ILoggerManager loggerManager) : base(repositoryContext)
        {
            _logger = loggerManager;
        }
        public DomainPlan? GetDomainPlanByUserId(Guid userId)
        {
            _logger.LogInfo($"Fetching DomainPlan for User with Id: {userId}");

            Guid? domainId = (
                from user in RepositoryContext.User
                where user.IsActive && user.Id == userId
                join person in RepositoryContext.Person on user.PersonId equals person.Id where person.IsActive
                join domain in RepositoryContext.Domain on person.DomainId equals domain.Id where domain.IsActive
                select domain.Id
                ).FirstOrDefault();

            if (domainId == null)
                return null;

            return RepositoryContext.DomainPlan
                .Where(dp => dp.DomainId == domainId && dp.IsActive)
                .OrderByDescending(dp => dp.DateCreated)
                .FirstOrDefault(dp => dp.IsRunning);
        }







    }
}
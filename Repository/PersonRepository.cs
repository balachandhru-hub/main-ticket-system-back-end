using Contracts.IRepository;
using Dto;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Entities.Models;
using Entities.DbContext;
using Contracts;
using Entities.Common;
using Microsoft.Data.SqlClient;

namespace Repository
{

    /// <summary>
    /// Repository for Person entity, implementing IPersonRepository interface.
    /// </summary>
    public class PersonRepository : RepositoryBase<Person>, IPersonRepository
    {
        private ILoggerManager _logger;
        private IConfiguration _configuration;




        public PersonRepository(RepositoryContext repositoryContext, ILoggerManager logger, IConfiguration configuration)
            : base(repositoryContext)
        {
            _logger = logger;
            _configuration = configuration;

        }

        /// <summary>
        /// Fetches the List of Person Details for the Domain Id
        /// </summary>
        /// <param name="domainId"></param>
        /// <returns></returns>
        public List<PersonPreviewDto> GetPersonDetailByDomainId(int index, int limit, Guid domainId, string? searchTerm)
        {
            var query = from _person in RepositoryContext.Person
                        where _person.IsActive && _person.DomainId == domainId
                        join _user in RepositoryContext.User on _person.Id equals _user.PersonId
                        join _userRoleMapping in RepositoryContext.UserRoleMapping on _user.Id equals _userRoleMapping.UserId
                        join _role in RepositoryContext.Role on _userRoleMapping.RoleId equals _role.Id
                        orderby _person.DateCreated descending
                        select new PersonPreviewDto
                        {
                            Id = _user.Id,
                            Name = _person.Name,
                            Email = _person.Email,
                            AribaUserName = _person.AribaUserName,
                            Role = _role.UserRole
                        };

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchTerm) ||
                    p.Email.ToLower().Contains(searchTerm));
            }

            return query
                .Skip(index)
                .Take(limit)
                .ToList();
        }

        /// <summary>
        /// Gets the User, Person, and Domain details for a given UserId (for GetMyProfile join query)
        /// </summary>
        /// <param name="userId">The User's Id</param>

        public MyProfileDto? GetMyProfileByUserId(Guid userId)
        {
            var result = (from user in RepositoryContext.User
                          where user.Id == userId && user.IsActive
                          join person in RepositoryContext.Person on user.PersonId equals person.Id
                          join domain in RepositoryContext.Domain on person.DomainId equals domain.Id into domains
                          from domain in domains.DefaultIfEmpty()
                          let latestPlan = RepositoryContext.DomainPlan
                                .Where(dp => dp.DomainId == domain.Id && dp.IsActive && dp.IsRunning)
                                .OrderByDescending(dp => dp.DateCreated)
                                .FirstOrDefault()
                          join dpc in RepositoryContext.DomainPlanCredit on latestPlan.Id equals dpc.DomainPlanId into domainCredits
                          from domainCredit in domainCredits.DefaultIfEmpty()
                          join urm in RepositoryContext.UserRoleMapping on user.Id equals urm.UserId into userRoleMappings
                          from userRoleMapping in userRoleMappings.DefaultIfEmpty()
                          join role in RepositoryContext.Role on userRoleMapping.RoleId equals role.Id into roles
                          from role in roles.DefaultIfEmpty()
                          where (person == null || person.IsActive)
                                && (domain == null || domain.IsActive)
                          select new MyProfileDto
                          {
                              PersonName = person != null ? person.Name : null,
                              Email = person.Email,
                              Domain = domain != null ? domain.Name : null,
                              PlanType = latestPlan != null ? latestPlan.PlanName : null,
                              MonthlyCreditRemaining = domainCredit != null ? domainCredit.MonthlyHourCredit : null,
                              TotalCreditRemaining = domainCredit != null ? domainCredit.TotalHourCredit : null,
                              Address = person != null ? person.Address : null,
                              Role = role != null ? role.UserRole : null,
                              Country = person != null ? person.Country : null,
                              MobilePhone = person != null ? person.MobilePhone : null,
                              AribaUserName = person != null ? person.AribaUserName : null,
                              AribaPassword = person != null ? person.AribaPassword : null
                          }).FirstOrDefault();
            return result;
        }

        /// <summary>
        /// Fetches the List of Business Admin Detail for the Given Domain Id 
        /// </summary>
        /// <param name="domainId"></param>
        /// <returns></returns>
        public List<Person> GetBusinessAdminByDomainId(Guid domainId)
        {
            _logger.LogInfo($"Fetching the List of Person Detail of Business Admin for the Given Domain : {domainId}");
            return (from _person in RepositoryContext.Person
                    join _user in RepositoryContext.User on _person.Id equals _user.PersonId
                    join _userRole in RepositoryContext.UserRoleMapping on _user.Id equals _userRole.UserId
                    where _person.IsActive && _user.IsActive && _userRole.IsActive && _person.DomainId == domainId && _userRole.RoleId == Common.BUSINESS_ADMIN_ROLE_ID
                    select _person).ToList();
        }
        /// <summary>
        /// Fetches the List of Consultants with Ticket Count
        ///     </summary>
        ///    <param name="index"></param>
        ///   <param name="limit"></param>
        /// <param name="searchTerm"></param>
        /// <returns></returns>

        public List<ConsultantTicketCountDto> GetConsultantsWithTicketCount(int index, int limit, string searchTerm)
        {
            _logger.LogInfo($"Fetching the List of Consultants with Ticket Count, SearchTerm: {searchTerm}");

            var query =
                from ur in RepositoryContext.UserRoleMapping
                where ur.IsActive && ur.RoleId == Common.CONSULTANT_ROLE_ID

                join u in RepositoryContext.User
                on ur.UserId equals u.Id
                where u.IsActive

                join p in RepositoryContext.Person
                    on u.PersonId equals p.Id
                where p.IsActive

                select new ConsultantTicketCountDto
                {
                    UserId = u.Id,
                    Name = p.Name
                };

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(searchTerm));
            }
          

            query = query
                .OrderBy(x => x.Name)
                .Skip(index)
                .Take(limit);

            return query.ToList();
        }
        /// <summary>
        /// Fetches the Person Name by User Id
        ///   /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Person? GetPersonNameByUserId(Guid userId)
        {
            _logger.LogInfo($"Fetching the Person Name for the Given User Id : {userId}");
            User? user = RepositoryContext.User
                .FirstOrDefault(u => u.Id == userId && u.IsActive);

            if (user != null)
            {
                Person? person = RepositoryContext.Person
                    .FirstOrDefault(p => p.Id == user.PersonId && p.IsActive);
                return person;
            }
            return null;
        }

        //// <summary>
        /// Retrieves the department name for the specified Ariba user from the external PRDashboardDB database.
        /// </summary>
        /// <param name="aribaUser">The Ariba username to look up.</param>
        /// <returns>The department name associated with the given Ariba user, or an empty string if not found.</returns>
        public string GetDepartmentDetail(string aribaUser)
        {
            Person person = new Person();
            string _dbConnectionString = _configuration[Common.PYTHON_USER_DB]!;
               string databaseName = _configuration[Common.PYTHON_USER_DB_NAME]!;
            string department = string.Empty;
            _logger.LogDebug($"Fetching  department for Ariba user: {aribaUser}");
            using (SqlConnection connection = new SqlConnection(_dbConnectionString))
            {
                connection.Open();
                 string sql = $@"
                    SELECT [cus_Department]
                    FROM [{databaseName}].[dbo].[ConsolidatedGroupExport]
                    WHERE [Name] = @AribaUser";
                SqlCommand sqlCommand = new SqlCommand(sql, connection);
                sqlCommand.Parameters.AddWithValue("@AribaUser", aribaUser); 

                using (var reader = sqlCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(0))
                        {
                            department = reader.GetString(0);
                        }
                    }
                }
                connection.Close();
            }
            _logger.LogDebug($"Fetched department for Ariba user : {aribaUser}-Department: {department}");
            return department;
        }






    }
}
using Contracts.IRepository;
using Entities.Models;
using Entities.DbContext;
using Contracts;
using Dto;

namespace Repository
{
    /// <summary>
    /// Class <c>UserRepository</c> used to implement the methods related for User.
    /// </summary>
    public class UserRepository : RepositoryBase<User>, IUserRepository
    {
        private readonly ILoggerManager _logger;
        /// <summary>
        /// Constructor for injecting DbContext
        /// </summary>
        /// <param name="repositoryContext"></param>
        /// <returns></returns>
        public UserRepository(RepositoryContext repositoryContext, ILoggerManager logger) : base(repositoryContext)
        {
            _logger = logger;
        }
        /// <summary>
        /// Get the Role for given User Id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public RoleDetailDto GetUserRole(Guid userId)
        {
            _logger.LogInfo("Fetching the Role Detail for the User with Id : {userId}");
            return (from _role in RepositoryContext.Role.Where(r => r.IsActive)
                    join _userRoleMapping in RepositoryContext.UserRoleMapping on _role.Id equals _userRoleMapping.RoleId
                    where _userRoleMapping.IsActive && _userRoleMapping.UserId == userId
                    select new RoleDetailDto
                    {
                        Id = _role.Id,
                        UserRole = _role.UserRole
                    }).FirstOrDefault();
        }
        public Guid GetDomainIdByUserId(Guid userId)
        {
            return (from user in RepositoryContext.User
                    join person in RepositoryContext.Person
                        on user.PersonId equals person.Id
                    where user.IsActive && person.IsActive && user.Id == userId
                    select person.DomainId).FirstOrDefault();
        }


        /// <summary>
        /// Get the List of User for given Domain
        /// </summary>
        /// <param name="domain">domain name</param>
        public List<DomainUserDto> GetUserByDomainName(string domain)
        {
            _logger.LogInfo($"Fetching users for domain: {domain}");
            return (from _person in RepositoryContext.Person.Where(x => x.IsActive)
                    join _user in RepositoryContext.User on _person.Id equals _user.PersonId
                    join _userRoleMapping in RepositoryContext.UserRoleMapping on _user.Id equals _userRoleMapping.UserId
                    join _role in RepositoryContext.Role on _userRoleMapping.RoleId equals _role.Id
                    select new DomainUserDto
                    {
                        Name = _person.Name,
                        Email = _person.Email,
                        Role = _role.UserRole
                    }).ToList();
        }
        /// <summary>
        /// Get User by Id
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <returns>User details</returns>
        public GetUserDto GetUserDetailsById(Guid userId)
        {
            var result = (from user in RepositoryContext.User
                          join person in RepositoryContext.Person on user.PersonId equals person.Id
                          join userRoleMapping in RepositoryContext.UserRoleMapping on user.Id equals userRoleMapping.UserId
                          join role in RepositoryContext.Role on userRoleMapping.RoleId equals role.Id
                          where user.Id == userId && user.IsActive
                          select new GetUserDto
                          {
                              Name = person.Name,
                              UserName = user.UserName,
                              UserRole = role.UserRole,
                              Email = person.Email,
                              MobilePhone = person.MobilePhone,
                              Address = person.Address,
                              Designation = person.Designation,
                              EmployeeId = person.EmployeeId,
                              Department = person.Department,
                              Country = person.Country,
                              AribaUserName = person.AribaUserName,
                              AribaPassword = person.AribaPassword

                          }).FirstOrDefault();

            return result;
        }
    }
}
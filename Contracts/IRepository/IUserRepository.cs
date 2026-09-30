using Dto;
using Entities.Models;
using System.Linq;
namespace Contracts.IRepository
{
    /// <summary>
    /// IUserRepository
    /// </summary>
    public interface IUserRepository : IRepositoryBase<User>
    {
        /// <summary>
        /// Get the Role for given User Id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        RoleDetailDto GetUserRole(Guid userId);

        /// <summary>
        /// Get the List of User for given Domain
        /// </summary>
        /// <param name="domain">domain name</param>
        List<DomainUserDto> GetUserByDomainName(string domain);
        Guid GetDomainIdByUserId(Guid userId);
        /// <summary>
        /// Get User Details by User Id
        ///     
        GetUserDto GetUserDetailsById(Guid userId);

    }
}
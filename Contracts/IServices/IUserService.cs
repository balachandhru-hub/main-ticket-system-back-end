using Dto;
using System;
using System.Collections.Generic;

namespace Contracts.IServices
{
    public interface IUserService
    {
        /// <summary>
        /// Creates User for the Given Details
        /// </summary>
        /// <param name="userDto"></param>
        /// <returns></returns>
        Guid CreateUser(UserDto userDto);

        // /// <summary>
        // /// Fetches all the Users for the Given Domain
        // /// </summary>
        // /// <param name="domain"></param>
        // /// <returns></returns>
        public List<DomainUserDto> GetUserByDomain(string domain);
        /// <summary>
        /// Fetches the User Details for the Given User Id
        /// /// </summary>
        /// <param name="userId"></param>
        /// /// <returns></returns>
        GetUserDto GetUserById(Guid userId);
    }
}
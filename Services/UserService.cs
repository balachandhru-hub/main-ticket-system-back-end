using System;
using System.Linq;
using System.Collections.Generic;
using Contracts;
using Contracts.IServices;
using Entities.Models;
using Dto;
using Contracts.IRepository;
using ExceptionHandler;
using HashingSystem;
using Entities.DbContext;

using Entities.Common;

namespace Services
{
    public class UserService : IUserService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerManager _logger;
        private readonly IBcryptHashing _hashing;

        public UserService(IRepositoryWrapper repositoryWrapper, ILoggerManager logger, IBcryptHashing hashing)
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
            _hashing = hashing;

        }

        /// <summary>
        /// Creates User for the Given Details
        /// </summary>
        /// <param name="userDto"></param>
        /// <returns></returns>
        public Guid CreateUser(UserDto userDto)
        {
            _logger.LogInfo($"Creating user with PersonId: {userDto.PersonId}");
            userDto.UserSecret = _hashing.HashStringWithSalt(userDto.UserSecret);
            User? oldUser = _repositoryWrapper.User.FindFirstByCondition(x => x.IsActive && x.UserName == userDto.UserName);
            if (oldUser != null)
            {
                _logger.LogInfo($"User with user name ; {userDto.UserName} already exists");
                throw new PreConditionFailedCustomException($"User Name Already Exists", $"User Name :{userDto.UserName} Already Exists");
            }
            User user = new User
            {
                Id = Guid.NewGuid(),
                PersonId = userDto.PersonId,
                UserName = userDto.UserName,
                UserSecret = userDto.UserSecret,
                UserType = userDto.UserType,

            };

            _repositoryWrapper.User.Create(user);
            _repositoryWrapper.Save();

            _logger.LogInfo($"User created with ID: {user.Id}");
            return user.Id;
        }


        //     /// <summary>
        //     /// Fetches all the Users for the Given Domain
        //     /// </summary>
        //     /// <param name="domain"></param>
        //     /// <returns></returns>
        public List<DomainUserDto> GetUserByDomain(string domain)
        {
            _logger.LogDebug($"Fetching List of users for domain with name: {domain}");
            List<DomainUserDto> result = _repositoryWrapper.User.GetUserByDomainName(domain);

            if (!result.Any())
            {
                _logger.LogError($"Not Found Any User Linked to Domain : {domain}");
                throw new NotFoundCustomException("Unable to Find Any User for the Given Domain", $"Unable to Find Any User for the Given Domain : {domain}");
            }

            _logger.LogDebug($"Fetched List of users for domain with name: {domain}");
            return result;
        }

        public GetUserDto GetUserById(Guid userId)
        {
            _logger.LogDebug($"Attempting to fetch user with ID: {userId}");

            GetUserDto userDto = _repositoryWrapper.User.GetUserDetailsById(userId);

            if (userDto == null)
                throw new NotFoundCustomException("User not found", $"No active user found with ID: {userId}");
            var models = _repositoryWrapper.UserModelName.GetModelNamesByUserId(userId);
            var requesterNames = _repositoryWrapper.RequesterName.GetRequesterNamesByUserId(userId);
            var apps = _repositoryWrapper.UserAppMapping.GetAppNamesByUserId(userId);


            userDto.Models = models;
            userDto.RequesterNames = requesterNames;
            userDto.Apps = apps;
            _logger.LogInfo($"Fetched user details for user ID: {userId}");

            return userDto;
        }

    }

}

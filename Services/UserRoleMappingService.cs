using System;
using Contracts;
using Contracts.IRepository;
using Contracts.IServices;
using Entities.Models;
using Dto; // <-- Add this

namespace Services
{
    public class UserRoleMappingService : IUserRoleMappingService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerManager _logger;

        public UserRoleMappingService(IRepositoryWrapper repositoryWrapper, ILoggerManager logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
        }

        public void CreateUserRole(UserRoleMappingDto userRoleMappingDto)
        {
            _logger.LogInfo($"Creating user role mapping for user {userRoleMappingDto.UserId} and role {userRoleMappingDto.RoleId}");
            var userRoleMapping = new UserRoleMapping
            {
                Id = Guid.NewGuid(),
                UserId = userRoleMappingDto.UserId,
                RoleId = userRoleMappingDto.RoleId,
                DateCreated = DateTime.UtcNow
            };
            _repositoryWrapper.UserRoleMapping.Create(userRoleMapping);
            _repositoryWrapper.Save();
        }
    }
}
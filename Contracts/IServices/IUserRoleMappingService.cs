using Dto;

namespace Contracts.IServices
{
    public interface IUserRoleMappingService
    {
        void CreateUserRole(UserRoleMappingDto userRoleMappingDto);
        // Add other user-role-related methods as needed
    }
}
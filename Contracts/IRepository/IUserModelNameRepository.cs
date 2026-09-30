// Contracts/IRepository/ITicketPRRepository.cs
using Entities.Models;
using System;
using System.Collections.Generic;
using Dto;

namespace Contracts.IRepository
{
    public interface IUserModelNameRepository : IRepositoryBase<UserModelName>
    {
        /// <summary>
        /// Get all distinct model names
        /// </summary>
        /// <returns>List of model names</returns>
        List<UserModelNameDto> GetAllModelNames();

        /// <summary>
        /// Get all model names for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of model names for the user</returns>
         List<UserModelNameDto> GetModelNamesByUserId(Guid userId);
    }
}

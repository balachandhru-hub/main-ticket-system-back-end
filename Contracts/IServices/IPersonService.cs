using Dto;
using System;
using System.Collections.Generic;

namespace Contracts.IServices
{
    public interface IPersonService
    {
        /// <summary>
        /// Creates a new person in the system.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        Guid CreatePerson(PersonDto person);
        /// <summary>
        /// Updates the name, address, and mobile number of the currently logged-in person.
        /// </summary>
        /// <param name="userId">The ID of the logged-in user.</param>
        /// <param name="updateDto">The updated person details.</param>
        void UpdatePerson(Guid userId, UpdatePersonDto updateDto);
        /// <summary>
        /// Retrieves the profile of the logged-in person.
        /// </summary>
        /// <param name="userId">The ID of the logged-in user.</param>
        /// <returns>The profile details of the user.</returns>
        MyProfileDto GetMyProfile(Guid userId);
        /// <summary>
        /// Removes a person from the system by their User ID.  
        /// </summary>
        /// <param name="id">The User ID of the person to remove.</param>
        bool RemovePerson(Guid id);
        /// <summary>
        /// Fetches the list of Person Details for the Given Domain Id
        /// </summary>
        /// <param name="domainId">Domain Id</param>
        /// <returns></returns>
        List<PersonPreviewDto> GetPersonDetialByDomainId(int index, int limit, Guid domainId, string? searchTerm);

        List<ConsultantTicketCountDto> GetConsultantsWithTicketCount(int index, int limit, string searchTerm);

        /// <summary>
        /// Get all model names from UserModelName table
        /// </summary>
        /// <returns>List of distinct model names</returns>
        List<UserModelNameDto> GetAllModelNames();

        /// <summary>
        /// Get all model names created by a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of model names for the user</returns>
        List<UserModelNameDto> GetModelNamesByUserId(Guid userId);

        /// <summary>
        /// Deactivate a person by their User ID. This will prevent the person from logging in or accessing the system.
        /// </summary> 
        /// <param name="userId">The User ID of the person to deactivate.</param>
        void DeactivatePerson(Guid userId);
        List<CurrencyNameDto> GetAllCurrencyNames();
        List<AppNameDto> GetAllAppNames();
        List<AppNameDto> GetAppNamesByUserId(Guid userId);
       void UpdateAppModelMapping(Guid domainId, List<Guid> appIds);
       List<AppNameDto> GetAppNamesByDomainId(Guid domainId);
    }
}
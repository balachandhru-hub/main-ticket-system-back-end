using System.Collections.Generic;
using Dto;
using System;
using Contracts.IServices;

namespace Contracts.IServices
{
    /// <summary>
    /// Interface IMetaDataService
    /// </summary>
    public interface IMetadataService
    {
        /// <summary>
        /// Retrieves the reference term ID corresponding to the specified reference term key.
        /// </summary>
        /// <param name="refTermKey">The key of the reference term.</param>
        /// <returns>The ID of the reference term.</returns>
        Guid GetRefTermIdByRefTermKey(string refTermKey);

        /// <summary>
        /// Retrieves a list of reference term by the reference type.
        /// </summary>
        /// <param name="type">The list of reference type key.</param>
        /// <returns>A list of reference term </returns>
        List<RefTermDto> GetRefTermListByRefSetKey(List<string> type);

        /// <summary>
        /// Retrieves a reference term key by the reference term id.
        /// </summary>
        /// <param name="id">The reference term id.</param>
        /// <returns> Returns the reference term key</returns>
        string GetRefTermKeyById(Guid id);

        /// <summary>
        /// Retrieves a List of Available Roles for Admin.
        /// </summary>
        /// <returns> Returns the List of Roles</returns>
        List<RoleDetailDto> GetRoleList();
        /// <summary>
        /// Creates a new reference term.
        /// </summary>
        /// <param name="createRefTermDto">The DTO containing the details of the reference  term to create.</param>
        /// <returns>The created reference term.</returns>
        Guid CreateRefTerm(CreateRefTermDto dto);
        /// <summary>
        /// Retrieves all modules (active and inactive).        
        /// /// </summary>
        /// <returns>A list of all modules.</returns>
        /// List<ModulePreviewDto> GetAllModules();
        List<ModulePreviewDto> GetAllModules();
        /// <summary>
        /// Updates the status of a module.
        /// </summary>
        void UpdateModelStatus(UpdateModelStatusDto dto);
        
    }
}
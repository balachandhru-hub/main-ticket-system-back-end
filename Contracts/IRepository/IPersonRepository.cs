using Dto;
using Entities.Models;


namespace Contracts.IRepository
{
    /// <summary>
    /// Interface for Person repository, extending IRepositoryBase.
    /// </summary>
    public interface IPersonRepository : IRepositoryBase<Person>
    {
        /// <summary>
        /// Fetches the List of Person Details for the Domain Id
        /// </summary>
        /// <param name="domainId"></param>
        /// <returns></returns>
        List<PersonPreviewDto> GetPersonDetailByDomainId(int index, int limit, Guid domainId, string searchTerm);
        /// <summary>
        /// Gets the User, Person, and Domain details for a given UserId (for GetMyProfile join query)
        /// </summary>
        /// <param name="userId">The User's Id</param>

        MyProfileDto? GetMyProfileByUserId(Guid userId);

        /// <summary>
        /// Fetches the List of Business Admin Detail for the Given Domain Id 
        /// </summary>
        /// <param name="domainId"></param>
        /// <returns></returns>
        List<Person> GetBusinessAdminByDomainId(Guid domainId);
        /// <summary>
        /// Fetches the List of Consultants with Ticket Count
        /// </summary>
        ///    <param name="index"></param>
        ///     <param name="limit"></param>
        ///   <param name="searchTerm"></param> 
        /// <returns></returns>
        List<ConsultantTicketCountDto> GetConsultantsWithTicketCount(int index, int limit, string searchTerm);
        /// <summary>
        /// Fetches the Person Name by User Id
        ///   /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Person? GetPersonNameByUserId(Guid userId);
        /// <summary>
        /// Fetches the Department by Ariba User
        /// </summary>
        /// <param name="aribaUser"></param>
        /// <returns></returns>
        string GetDepartmentDetail(string aribaUser);
      

        
    }
}
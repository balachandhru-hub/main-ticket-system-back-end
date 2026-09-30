using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dto;
using Entities.Models;

namespace Contracts.IServices
{
    /// <summary>
    /// Interface for sending email messages.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// The SendEmail function in C# sends an email using the provided EmailPlaceholderDto.
        /// </summary>
        bool SendEmail(string? toEmail, string subject, string body, string? emailType, string entityType, Guid? entityId, List<string>? ccEmail);

        /// <summary>
        /// The function CreateEmailSentDetails takes an EmailPlaceholderDto parameter.
        /// </summary>
        /// <param name="EmailPlaceholderDto">The `EmailPlaceholderDto` is a data transfer object (DTO) that
        /// likely contains information related to an email message. It may include fields such as sender,
        /// recipient, subject, body, attachments, and any other relevant details needed to send an email. In
        /// the `CreateEmailSentDetails` method,</param>
        void CreateEmailSentDetails(EmailPlaceholderDto dto);

        /// <summary>
        /// This function updates the details of a failed email placeholder.
        /// </summary>
        /// <param name="EmailPlaceholderDto">The `EmailPlaceholderDto` is a data transfer object (DTO)
        /// that likely contains information related to an email placeholder. This object may include
        /// properties such as the placeholder name, value, and any other relevant details needed for
        /// updating email failed details.</param>
        void CreateEmailFailedDetails(string toEmail, string subject, string body, string emailType, string entityType, Guid entityId);

        /// <summary>
        /// The function UpdateEmailFailedDetals updates email failed details for a specific appointment and
        /// entity type.
        /// </summary>
        /// <param name="Guid">A Guid (Globally Unique Identifier) is a 128-bit integer value used to uniquely
        /// identify an entity or object in software development. It is often used to uniquely identify records
        /// in databases, files, or other data sources. In the context of the method signature you provided, the
        /// Guid parameter `</param>
        /// <param name="entityType">The `entityType` parameter is a string that represents the type of entity
        /// for which the email update failed.</param>
        void UpdateEmailFailedDetals(Guid entityId, string entityType);

        /// <summary>
        /// The function ReplacePlaceholders takes an input string and an EmailPlaceholderDto object, and
        /// replaces placeholders in the input string with values from the EmailPlaceholderDto.
        /// </summary>
        /// <param name="input">The `input` parameter is a string that contains text with placeholders that need
        /// to be replaced with specific values.</param>
        /// <param name="EmailPlaceholderDto">EmailPlaceholderDto is a data transfer object that contains
        /// placeholders for email content. It may include fields such as recipient name, sender name, subject,
        /// body, etc.</param>
        string ReplacePlaceholders(string input, EmailPlaceholderDto emailPlaceholder);
        /// <summary>
        /// Used to Create the CC Email
        /// </summary>
        /// <param name="dto">DTO</param>
        /// <returns>Id</returns>
        Guid CreateCCEmail(EmailCCDto dto);

        /// <summary>
        /// Used to Delete the CC Email
        /// </summary>
        /// <param name="id">Id</param>
        void DeleteCCEmail(Guid id);

        /// <summary>
        /// Fetches the List of Active Email CC Addresses
        /// </summary>
        /// <returns></returns>
        List<EmailCCDetailDto> GetCCEmail(string entityType);
        void CreateAribaApprovalEmailNotification(AribaTasks aribaTask, string emailKey);
        void CreateContractEmailNotification(ContractDetails contract, string approverAribaUsername, string emailKey);
        void AribaApprovalEmailNotification(string aribaId, string entityType, string approverAribaUsername);
        void SendGatekeeperApprovalNotification(Person person, string entityType, string approvalStatus, AribaInvoice? invoice = null, AribaPR? pr = null, ContractDetails? contract = null);
        void CreateMaterialPREmailNotification(MaterialPR materialPR, string emailKey, Guid loggedInUserId);
        void CreateServicePREmailNotification(ServicePR servicePR, string emailKey, Guid loggedInUserId);

    }
}

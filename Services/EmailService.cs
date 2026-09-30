using Dto;
using Microsoft.Extensions.Configuration;
using Entities.Common;
using Contracts.IServices;
using System.Net.Mail;
using System.Net;
using Contracts;
using Entities.Models;
using Contracts.IRepository;
using ExceptionHandler;

namespace Services
{
    public class EmailService : IEmailService
    {/// <summary>
     /// Service for sending emails.
     /// </summary>
        private readonly IConfiguration _configuration;
        private readonly ILoggerManager _logger;
        private readonly IRepositoryWrapper _repoWrapper;

        private readonly IAssetService _assetService;
        /// <summary>
        /// Constructor for initializing EmailService.
        /// </summary>
        /// <param name="configuration">Configuration settings.</param>
        /// <param name="logger">Logger instance.</param>
        public EmailService(IConfiguration configuration, ILoggerManager logger, IRepositoryWrapper repoWrapper, IAssetService assetService)
        {
            _configuration = configuration;
            _logger = logger;
            _repoWrapper = repoWrapper;
            _assetService = assetService;
        }



        /// <summary>
        /// The function `SendEmail` sends an email using SMTP client server and handles
        /// exceptions by logging errors and creating email failed details if necessary.
        /// </summary>
        /// <returns>
        /// The method `SendEmail` is returning a boolean value. It returns `true` if the email
        /// was sent successfully, and `false` if there was an exception during the process.
        /// </returns>
        public bool SendEmail(string toEmail, string subject, string body, string emailType, string entityType, Guid? entityId, List<string>? ccEmail)
        {
            try
            {
                _logger.LogInfo("Sending mail to the user using the SMTP client server");

                ApiConfig? apiConfig = _repoWrapper.ApiConfig.FindFirstByCondition(x => x.IsActive && x.Description == Common.EMAIL_API_CREDENTIAL_DESCRIPTION);
                string senderEmail = apiConfig.Username!;
                string senderPassword = apiConfig.Password!;

                // Construct the email message
                MailMessage message = new MailMessage();
                // Set recipient email address using Add method
                message.To.Add(new MailAddress(toEmail!));

                ccEmail?.Where(email => !string.IsNullOrWhiteSpace(email)).ToList().ForEach(email => message.CC.Add(new MailAddress(email)));

                message.From = new MailAddress(senderEmail, _configuration[Common.SENDER_DISPLAYNAME]);

                message.Subject = subject;
                message.IsBodyHtml = true;
                message.Body = body;
                // Guid entityTypeId = _metaDataService.GetRefTermIdByRefTermKey(entityType!.ToUpper());
                // if (emailType.Equals(Common.EMAIL_CONTENT_KEY_FOR_PERSON_ASSETS))
                // {
                //     List<Asset> assets = _repoWrapper.Asset.FindByCondition(x => x.IsActive && x.EntityId == entityId && x.EntityType == entityTypeId).ToList();
                //     foreach (var item in assets)
                //     {
                //         AssetDownloadDto asset = _assetService.GetDocument(item.Id);
                //         Attachment attachment = new Attachment(new MemoryStream(asset.FileBytes), asset.FileName);
                //         message.Attachments.Add(attachment);
                //     }
                // }


                // Configure SMTP client (consider using a dedicated SMTP service provider)
                var smtpClient = new SmtpClient(apiConfig.BaseUrl!, int.Parse(_configuration[Common.SENDER_PORT]!));
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);

                // Send the email
                smtpClient.SendMailAsync(message);
                EmailFailedDetail emailFailedDetail = _repoWrapper.EmailFailedDetail.FindFirstByCondition(x => x.IsActive && x.EntityId == entityId && x.EntityType == entityType && x.EmailType!.Equals(emailType));
                if (emailFailedDetail != null)
                {
                    emailFailedDetail.IsActive = false;
                    _repoWrapper.EmailFailedDetail.Update(emailFailedDetail);
                    _repoWrapper.Save();
                }
                _logger.LogInfo("Sended mail to the user using the SMTP client server");
                return true;

            }
            catch (Exception ex)
            {
                _logger.LogError($"Job execution failed: {ex.Message}");
                CreateEmailFailedDetails(toEmail, subject, body, emailType, entityType, (Guid)entityId!);
                return false;
            }
        }

        /// <summary>
        /// Replaces placeholders in the input string with actual values from EmailPlaceholderDto.
        /// </summary>
        /// <param name="input">Input string containing placeholders.</param>
        /// <param name="emailPlaceholder">EmailPlaceholderDto containing actual values.</param>
        /// <returns>String with placeholders replaced by actual values.</returns>
        public string ReplacePlaceholders(string input, EmailPlaceholderDto emailPlaceholder)
        {
            _logger.LogInfo("Replacing the mail placeholders for the Email body or its subject");
            // Replace placeholders with property values, handling null values
            string content = input.Replace(Common.TO_EMAIL_PLACEHOLDER, emailPlaceholder?.ToEmail ?? "")
                                    .Replace(Common.PERSON_NAME_EMAIL_PLACEHOLDER, emailPlaceholder?.PersonName ?? "")
                                    .Replace(Common.TICKET_ID_EMAIL_PLACEHOLDER, emailPlaceholder?.TicketId ?? "")
                                    .Replace(Common.TITLE_PLACEHOLDER, emailPlaceholder?.Title ?? "")
                                    .Replace(Common.CONTRACT_TASK_ID_PLACEHOLDER, emailPlaceholder?.ContractTaskId ?? "")
                                    .Replace(Common.CONTRACT_BEGIN_DATE_PLACEHOLDER, emailPlaceholder?.ContractBeginDate?.ToString() ?? "")
                                    .Replace(Common.CONTRACT_TOTAL_AMOUNT_PLACEHOLDER, emailPlaceholder?.ContractTotalAmount?.ToString() ?? "")
                                    .Replace(Common.CONTRACT_CURRENCY_PLACEHOLDER, emailPlaceholder?.ContractCurrency ?? "")
                                    .Replace(Common.CONTRACT_AMOUNT_SPEND_PLACEHOLDER, emailPlaceholder?.ContractAmountSpend?.ToString() ?? "")
                                    .Replace(Common.CONTRACT_BALANCE_AMOUNT_PLACEHOLDER, emailPlaceholder?.ContractBalanceAmount?.ToString() ?? "")
                                    .Replace(Common.DESCRIPTION_PLACEHOLDER, emailPlaceholder?.Description ?? "")
                                    .Replace(Common.DATE_PLACEHOLDER, emailPlaceholder?.Date.ToString() ?? "")
                                    .Replace(Common.COMPANY_PLACEHOLDER, emailPlaceholder?.CompanyName ?? "")
                                    .Replace(Common.SUPPORT_EMAIL_PLACEHOLDER, emailPlaceholder?.SupportEmail ?? "")
                                    .Replace(Common.PORTAL_LINK_PLACEHOLDER, emailPlaceholder?.PortalLink ?? "")
                                    .Replace(Common.COMPANY_LOGO_URL_PLACEHOLDER, emailPlaceholder?.CompanyLogo ?? "")
                                    .Replace(Common.DOMAIN_COMPANY_NAME_PLACEHOLDER, emailPlaceholder?.DomainCompanyName ?? "")
                                    .Replace(Common.DOMAIN_WEBSITE_URL_PLACEHOLDER, emailPlaceholder?.WebsiteUrl ?? "")
                                    .Replace(Common.DOMAIN_COMPANY_LOCATION_PLACEHOLDER, emailPlaceholder?.CompanyLocation ?? "")
                                    .Replace(Common.DOMAIN_CIN_NO_PLACEHOLDER, emailPlaceholder?.CinNo ?? "")
                                    .Replace(Common.DOMAIN_COUNTRY_PLACEHOLDER, emailPlaceholder?.Country ?? "")
                                    .Replace(Common.DOMAIN_BUSINESS_TYPE_PLACEHOLDER, emailPlaceholder?.BusinessType ?? "")
                                    .Replace(Common.DOMAIN_INDUSTRY_TYPE_PLACEHOLDER, emailPlaceholder?.IndustryType ?? "")
                                    .Replace(Common.DOMAIN_PLAN_NAME_PLACEHOLDER, emailPlaceholder?.DomainPlanName ?? "")
                                    .Replace(Common.DOMAIN_PLAN_ENGAGE_TYPE_PLACEHOLDER, emailPlaceholder?.DomainPlanEngagementType ?? "")
                                    .Replace(Common.DOMAIN_PLAN_START_DATE_PLACEHOLDER, emailPlaceholder?.DomainPlanStartDate.ToString() ?? "")
                                    .Replace(Common.DOMAIN_PLAN_END_DATE_PLACEHOLDER, emailPlaceholder?.DomainPlanEndDate.ToString() ?? "")
                                    .Replace(Common.DOMAIN_PLAN_PAYMENT_TYPE_PLACEHOLDER, emailPlaceholder?.DomainPlanPaymentType ?? "")
                                    .Replace(Common.DOMAIN_DIS_COUP_ID_PLACEHOLDER, emailPlaceholder?.DomainPlanDiscountCouponId.ToString() ?? "")
                                    .Replace(Common.DOMAIN_CONTRACT_START_DATE_PLACEHOLDER, emailPlaceholder?.DomainPlanContractStartDate.ToString() ?? "")
                                    .Replace(Common.DOMAIN_CONTRACT_END_DATE_PLACEHOLDER, emailPlaceholder?.DomainPlanContractEndDate.ToString() ?? "")
                                    .Replace(Common.DOMAIN_NUMBER_OF_DUES_PLACEHOLDER, emailPlaceholder?.DomainPlanNumberOfDues.ToString() ?? "")
                                    .Replace(Common.DOMAIN_BILLING_ADDR_PLACEHOLDER, emailPlaceholder?.BillingAddress ?? "")
                                    .Replace(Common.DOMAIN_PLAN_NAME_PLACEHOLDER, emailPlaceholder?.DomainPlanName ?? "")
                                    .Replace(Common.USERNAME_PLACEHOLDER, emailPlaceholder?.UserName ?? "")
                                    .Replace(Common.USERSECRET_PLACEHOLDER, emailPlaceholder?.Password ?? "")
                                    .Replace(Common.EXPIRY_DATE_PLACEHOLDER, emailPlaceholder?.ExpiryDate ?? "")
                                    .Replace(Common.DAYS_LEFT_PLACEHOLDER, emailPlaceholder?.DaysLeft ?? "")
                                    .Replace(Common.INVOICE_ID_PLACEHOLDER, emailPlaceholder?.InvoiceId ?? "")
                                    .Replace(Common.APPROVAL_LINK_PLACEHOLDER, emailPlaceholder?.PortalLink ?? "")
                                    .Replace(Common.SUPPLIER_NAME_PLACEHOLDER, emailPlaceholder?.SupplierName ?? "")
                                    .Replace(Common.REQUESTER_NAME_PLACEHOLDER, emailPlaceholder?.RequesterName ?? "")
                                    .Replace(Common.REQUISITION_ID_PLACEHOLDER, emailPlaceholder?.RequisitionId ?? "")
                                   .Replace(Common.AMOUNT_PLACEHOLDER, emailPlaceholder?.Amount?.ToString() ?? "")
                                   .Replace(Common.DUE_DATE_PLACEHOLDER, emailPlaceholder?.DueDate?.ToString("yyyy-MM-dd") ?? "")
                                   .Replace(Common.PR_NUMBER_PLACEHOLDER, emailPlaceholder?.PRNumber ?? "")
                                   .Replace(Common.STATUS_PLACEHOLDER, emailPlaceholder?.StatusPR ?? "");



            _logger.LogInfo("Replaced the mail placeholders for the Email body or its subject");
            return content;
        }

        /// <summary>
        /// Records details of an email sent.
        /// </summary>
        /// <param name="dto">The DTO containing email placeholder details.</param>
        public void CreateEmailSentDetails(EmailPlaceholderDto dto)
        {
            _logger.LogInfo($"Creating email sent details for the entity with {dto.EntityType} and with entity id {dto.EntityId}.");
            EmailSentDetail emailSent = new EmailSentDetail
            {
                Id = Guid.NewGuid(),
                EntityId = dto.EntityId,
                EntityType = dto.EntityType?.ToUpper(),
                Email = dto.ToEmail,
                EmailType = dto.EmailType,
            };
            _repoWrapper.EmailSentDetail.Create(emailSent);
            _repoWrapper.Save();
            _logger.LogInfo($"Created email sent details for the entity with {dto.EntityType} and with entity id {dto.EntityId}.");
        }


        /// <summary>
        /// Updates the details of failed email sending.
        /// </summary>
        /// <param name="emailPlaceholderDto">The DTO containing email placeholder details.</param>
        public void CreateEmailFailedDetails(string toEmail, string subject, string body, string emailType, string entityType, Guid entityId)
        {
            _logger.LogInfo($"Updating email failed details for the entity with {entityType} and with entity id {entityId}.");
            EmailFailedDetail emailFailedDetail = _repoWrapper.EmailFailedDetail.FindFirstByCondition(x => x.IsActive && x.EntityId == entityId && x.EntityType == entityType && x.EmailType!.Equals(emailType));
            if (emailFailedDetail != null)
            {
                emailFailedDetail.TriggerCount += 1;
                if (emailFailedDetail.TriggerCount > 3)
                {
                    emailFailedDetail.IsActive = false;
                }
                _repoWrapper.EmailFailedDetail.Update(emailFailedDetail);
                _repoWrapper.Save();
            }
            else
            {
                EmailFailedDetail failedDetail = new EmailFailedDetail()
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    EmailType = emailType,
                    Email = toEmail,
                    EntityType = entityType!.ToUpper(),
                    TriggerCount = 1,
                    Subject = subject,
                    Body = body
                };
                _repoWrapper.EmailFailedDetail.Create(failedDetail);
                _repoWrapper.Save();
            }
            _logger.LogInfo($"Updated email failed details for the entity with {entityType} and with entity id {entityId}.");
        }

        public void UpdateEmailFailedDetals(Guid entityId, string entityType)
        {
            EmailFailedDetail emailFailedDetail = _repoWrapper.EmailFailedDetail.FindFirstByCondition(x => x.IsActive && x.EntityId == entityId && x.EntityType == entityType);
            if (emailFailedDetail != null)
            {
                emailFailedDetail.IsActive = false;
                _repoWrapper.EmailFailedDetail.Update(emailFailedDetail);
                _repoWrapper.Save();
            }
        }

        /// <summary>
        /// Used to Create the CC Email
        /// </summary>
        /// <param name="dto">DTO</param>
        /// <returns>Id</returns>
        public Guid CreateCCEmail(EmailCCDto dto)
        {
            _logger.LogInfo($"Creating CC Email for Email Address : {dto.EmailAddress}");
            EmailCCList existingEmail = _repoWrapper.EmailCCList.FindFirstByCondition(x => x.IsActive && x.Email == dto.EmailAddress && x.EntityType == dto.EntityType);
            if (existingEmail != null)
            {
                _logger.LogInfo($"CC Email already exists for Email Address : {dto.EmailAddress}");
                throw new PreConditionFailedCustomException("Already Email Address Exists", "Already Email Address Exists for Email : {}");
            }
            EmailCCList emailCC = new EmailCCList
            {
                Id = Guid.NewGuid(),
                Email = dto.EmailAddress,
                EntityType = dto.EntityType
            };
            _repoWrapper.EmailCCList.Create(emailCC);
            _repoWrapper.Save();

            _logger.LogInfo($"Created CC Email for Email Address : {dto.EmailAddress}");
            return emailCC.Id;
        }

        /// <summary>
        /// Used to Delete the CC Email
        /// </summary>
        /// <param name="id">Id</param>
        public void DeleteCCEmail(Guid id)
        {
            _logger.LogInfo($"Deleting CC Email for Email Address with Id : {id}");
            EmailCCList email = _repoWrapper.EmailCCList.FindFirstByCondition(x => x.IsActive && x.Id == id);
            if (email == null)
            {
                _logger.LogInfo($"Unable to Find Email for Email Id: {id}");
                throw new NotFoundCustomException("Unable to Find Email Address", $"Unable to Find Email Address for Email Id : {id}");
            }
            _repoWrapper.EmailCCList.Delete(email);
            _repoWrapper.Save();

            _logger.LogInfo($"Deleted CC Email for Email Address with Id : {id}");
        }

        /// <summary>
        /// Fetches the List of Active Email CC Addresses
        /// </summary>
        /// <returns></returns>
        public List<EmailCCDetailDto> GetCCEmail(string entityType)
        {
            _logger.LogInfo($"Fetching CC Email Details");
            List<EmailCCDetailDto> emailCCDetails = _repoWrapper.EmailCCList.FindByCondition(x => x.IsActive && x.EntityType == entityType).ToList().Select(x => new EmailCCDetailDto
            {
                Id = x.Id,
                EntityType = x.EntityType,
                EmailAddress = x.Email
            }).ToList();
            if (!emailCCDetails.Any())
            {
                _logger.LogInfo($"No CC Email Details Found");
                throw new NotFoundCustomException("No CC Email Details Found", "No CC Email Details Found");
            }
            _logger.LogInfo($"Fetched CC Email Details");
            return emailCCDetails;
        }
        /// <summary>
        /// Creates the Email Notification for Ariba Approval (Invoice Pending Approval).
        /// </summary>
        public void CreateAribaApprovalEmailNotification(AribaTasks aribaTask, string emailKey)
        {
            _logger.LogInfo($"Creating Ariba approval email notification for AribaTask with Id : {aribaTask.Id}");

            Person? person = _repoWrapper.Person.FindFirstByCondition(x => x.AribaUserName == aribaTask.Approver);
            if (person == null) return;

            User? user = _repoWrapper.User.FindFirstByCondition(u => u.PersonId == person.Id);
            if (user == null) return;


            UserRoleMapping? userRoleMapping = _repoWrapper.UserRoleMapping.FindFirstByCondition(m => m.UserId == user.Id);
            if (userRoleMapping == null) return;


            Guid roleId = userRoleMapping.RoleId;


            bool isLeadership = roleId == Common.LEADERSHIP_USER_ROLE_ID;


            EmailContent? emailContent = _repoWrapper.EmailContent.FindFirstByCondition(x => x.IsActive && x.Key == emailKey);
            if (emailContent == null) return;

            var invoice = _repoWrapper.AribaInvoice.FindFirstByCondition(x => x.InvoiceId == aribaTask.ApprovableUniqueName);

            var emailPlaceholder = new EmailPlaceholderDto
            {
                EntityId = aribaTask.Id,
                EmailType = emailKey,
                ToEmail = person.Email,
                PersonName = person.Name,
                EmailBody = emailContent.Body,
                Subject = emailContent.Subject,
                CompanyName = _configuration[Common.COMPANY_NAME],
                SupportEmail = _configuration[Common.SUPPORT_EMAIL],
                PortalLink = isLeadership ? _configuration[Common.ARIBA_PORTAK_LINK] : "",
                CompanyLogo = _configuration[Common.COMPANY_LOGO_URL],
                EntityType = Common.INVOICE_ENTITY,
                InvoiceId = aribaTask.ApprovableUniqueName,
                SupplierName = invoice?.SupplierName ?? "",
                SupplierId = invoice?.InvoiceId ?? "",
                Amount = invoice?.Amount ?? 0,
                DueDate = invoice?.DueDate,
            };

            emailPlaceholder.Subject = ReplacePlaceholders(emailContent.Subject!, emailPlaceholder);
            emailPlaceholder.EmailBody = ReplacePlaceholders(emailContent.Body!, emailPlaceholder);

            if (SendEmail(emailPlaceholder.ToEmail, emailPlaceholder.Subject, emailPlaceholder.EmailBody, emailPlaceholder.EmailType, emailPlaceholder.EntityType!, emailPlaceholder.EntityId, null))
            {
                CreateEmailSentDetails(emailPlaceholder);
                _logger.LogInfo($"Ariba approval email notification successfully created and sent for AribaTask with Id: {aribaTask.Id}");
            }
        }

         public void CreateContractEmailNotification(ContractDetails contract, string approverAribaUsername, string emailKey)
        {
            _logger.LogInfo($"Creating contract email notification for ContractDetails with ContractTaskId: {contract?.ContractTaskId}");
 
            if (contract == null) return;
 
            Person? person = _repoWrapper.Person.FindFirstByCondition(x => x.IsActive && x.AribaUserName == approverAribaUsername);
            if (person == null)
            {
                _logger.LogInfo($"No active person found with AribaUserName {approverAribaUsername}");
                return;
            }
 
            if (string.IsNullOrWhiteSpace(person.Email))
            {
                _logger.LogInfo($"Person found but has no email address: {approverAribaUsername}");
                return;
            }
 
            User? user = _repoWrapper.User.FindFirstByCondition(u => u.PersonId == person.Id);
            if (user == null) return;
 
            UserRoleMapping? userRoleMapping = _repoWrapper.UserRoleMapping.FindFirstByCondition(m => m.UserId == user.Id);
            bool isLeadership = userRoleMapping != null && userRoleMapping.RoleId == Common.LEADERSHIP_USER_ROLE_ID;
 
            EmailContent? emailContent = _repoWrapper.EmailContent.FindFirstByCondition(x => x.IsActive && x.Key == emailKey);
            if (emailContent == null) return;
 
            var emailPlaceholder = new EmailPlaceholderDto
            {
                EntityId = Guid.TryParse(contract.ContractTaskId, out var contractTaskGuid) ? contractTaskGuid : (Guid?)null,
                EmailType = emailKey,
                ToEmail = person.Email,
                PersonName = person.Name,
                EmailBody = emailContent.Body,
                Subject = emailContent.Subject,
                CompanyName = _configuration[Common.COMPANY_NAME],
                SupportEmail = _configuration[Common.SUPPORT_EMAIL],
                PortalLink = isLeadership ? _configuration[Common.ARIBA_PORTAK_LINK] : "",
                CompanyLogo = _configuration[Common.COMPANY_LOGO_URL],
                EntityType = Common.TYPE_CONTRACT,
                ContractTaskId = contract.ContractTaskId,
                ContractBeginDate = contract.BeginDate,
                ContractEndDate = contract.EndDate,
                ContractTotalAmount = !string.IsNullOrWhiteSpace(contract.TotalAmount) && decimal.TryParse(contract.TotalAmount, out var totalAmt) ? totalAmt : (decimal?)null,
                ContractCurrency = contract.Currency,
                ContractAmountSpend = !string.IsNullOrWhiteSpace(contract.AmountSpend) && decimal.TryParse(contract.AmountSpend, out var spentAmt) ? spentAmt : (decimal?)null,
                ContractBalanceAmount = !string.IsNullOrWhiteSpace(contract.BalanceAmount) && decimal.TryParse(contract.BalanceAmount, out var balanceAmt) ? balanceAmt : (decimal?)null,
                UserName = person.Name
            };
 
            emailPlaceholder.Subject = ReplacePlaceholders(emailContent.Subject!, emailPlaceholder);
            emailPlaceholder.EmailBody = ReplacePlaceholders(emailContent.Body!, emailPlaceholder);
 
            if (SendEmail(emailPlaceholder.ToEmail!, emailPlaceholder.Subject!, emailPlaceholder.EmailBody!, emailPlaceholder.EmailType!, emailPlaceholder.EntityType!, emailPlaceholder.EntityId, null))
            {
                CreateEmailSentDetails(emailPlaceholder);
                _logger.LogInfo($"Contract email notification successfully created and sent for ContractDetails with ContractTaskId: {contract.ContractTaskId}");
            }
        }
   public void SendGatekeeperApprovalNotification(Person person, string entityType, string approvalStatus, AribaInvoice? invoice = null, AribaPR? pr = null, ContractDetails? contract = null)
        {
            _logger.LogInfo($"Creating gatekeeper approval notification for {entityType}, status: {approvalStatus}");

            if (person == null)
            {
                _logger.LogInfo($"Person is null");
                return;
            }

            if (string.IsNullOrWhiteSpace(person.Email))
            {
                _logger.LogInfo($"Person found but has no email address: {person.AribaUserName}");
                return;
            }

            // Fetch email template from database
            EmailContent? emailContent = _repoWrapper.EmailContent.FindFirstByCondition(x => x.IsActive && x.Key == Common.GATEKEEPER_APPROVAL_EMAIL_KEY);
            if (emailContent == null)
            {
                _logger.LogInfo($"Email template not found for key {Common.GATEKEEPER_APPROVAL_EMAIL_KEY}");
                return;
            }

            EmailPlaceholderDto emailPlaceholder = new EmailPlaceholderDto
            {
                EmailType = emailContent.Key,
                ToEmail = person.Email,
                PersonName = person.Name,
                EmailBody = emailContent.Body,
                Subject = emailContent.Subject,
                CompanyName = _configuration[Common.COMPANY_NAME],
                SupportEmail = _configuration[Common.SUPPORT_EMAIL],
                CompanyLogo = _configuration[Common.COMPANY_LOGO_URL],
                EntityType = entityType,
                PortalLink = _configuration[Common.ARIBA_PORTAK_LINK],
                UserName = person.Name
            };

            // Add entity-specific information based on what object was passed
            if (invoice != null)
            {
                emailPlaceholder.EntityId = invoice.Id;
                emailPlaceholder.InvoiceId = invoice.InvoiceId;
                emailPlaceholder.SupplierName = invoice.SupplierName;
                emailPlaceholder.Amount = invoice.Amount;
                emailPlaceholder.DueDate = invoice.DueDate;
            }
            else if (pr != null)
            {
                emailPlaceholder.EntityId = pr.Id;
                emailPlaceholder.RequisitionId = pr.RequisitionsId;
                emailPlaceholder.RequesterName = pr.RequesterName ?? "";
                emailPlaceholder.SupplierName = pr.SupplierName;
                emailPlaceholder.Amount = pr.Amount;
                emailPlaceholder.DueDate = pr.DueDate ?? pr.NeedBYDate;
            }
            else if (contract != null)
            {
                emailPlaceholder.EntityId = Guid.TryParse(contract.ContractTaskId, out var contractTaskGuid) ? contractTaskGuid : (Guid?)null;
                emailPlaceholder.ContractTaskId = contract.ContractTaskId;
                emailPlaceholder.ContractBeginDate = contract.BeginDate;
                emailPlaceholder.ContractEndDate = contract.EndDate;
                emailPlaceholder.ContractTotalAmount = !string.IsNullOrWhiteSpace(contract.TotalAmount) && decimal.TryParse(contract.TotalAmount, out var totalAmt) ? totalAmt : (decimal?)null;
                emailPlaceholder.ContractCurrency = contract.Currency;
                emailPlaceholder.ContractAmountSpend = !string.IsNullOrWhiteSpace(contract.AmountSpend) && decimal.TryParse(contract.AmountSpend, out var spentAmt) ? spentAmt : (decimal?)null;
                emailPlaceholder.ContractBalanceAmount = !string.IsNullOrWhiteSpace(contract.BalanceAmount) && decimal.TryParse(contract.BalanceAmount, out var balanceAmt) ? balanceAmt : (decimal?)null;
            }

            // Replace placeholders in email template
            emailPlaceholder.Subject = ReplacePlaceholders(emailContent.Subject!, emailPlaceholder);
            emailPlaceholder.EmailBody = ReplacePlaceholders(emailContent.Body!, emailPlaceholder);

            // Send the email (no CC emails for gatekeeper notifications)
            if (SendEmail(emailPlaceholder.ToEmail, emailPlaceholder.Subject, emailPlaceholder.EmailBody, emailPlaceholder.EmailType, emailPlaceholder.EntityType!, emailPlaceholder.EntityId, null))
            {
                CreateEmailSentDetails(emailPlaceholder);
                _logger.LogInfo($"Gatekeeper approval notification successfully sent to {person.Email} for {entityType}");
            }
        }

        /// <summary>
        /// Generic Ariba approval email notification. Supports invoices, contracts and PRs.
        /// aribaId: the Ariba identifier (invoice id, contract id or PR id)
        /// entityType: a string indicating type (e.g. Common.TYPE_INVOICE, Common.TYPE_CONTRACT, Common.TYPE_PR)
        /// emailKey: key to lookup the email template
        /// </summary>
        public void AribaApprovalEmailNotification(string aribaId, string entityType, string approverAribaUsername)
        {
            _logger.LogInfo($"Creating Ariba approval email notification for entity {entityType} with Id : {aribaId}");

            string emailKey;
            if (string.Equals(entityType, Common.TYPE_PR, StringComparison.OrdinalIgnoreCase))
            {
                emailKey = Common.ARIBA_PR_EMAIL_NOTIFICATION_KEY;
            }
            else if (string.Equals(entityType, Common.TYPE_CONTRACT, StringComparison.OrdinalIgnoreCase))
            {
                emailKey = Common.ARIBA_CONTRACT_EMAIL_NOTIFICATION_KEY;
            }
            else
            {
                emailKey = Common.ARIBA_APPROVAL_EMAIL_KEY;
            }

            var emailContent = _repoWrapper.EmailContent.FindFirstByCondition(x => x.IsActive && x.Key == emailKey);
            if (emailContent == null)
            {
                _logger.LogInfo($"Email template not found for key {emailKey}");
                return;
            }

            EmailPlaceholderDto emailPlaceholder = new EmailPlaceholderDto
            {
                EmailType = emailKey,
                EmailBody = emailContent.Body,
                Subject = emailContent.Subject,
                CompanyName = _configuration[Common.COMPANY_NAME],
                SupportEmail = _configuration[Common.SUPPORT_EMAIL],
                CompanyLogo = _configuration[Common.COMPANY_LOGO_URL],
                EntityType = entityType,
                PortalLink=_configuration[Common.ARIBA_PORTAK_LINK]
            };

            string? toEmail = null;
            string? personName = null;
            List<string> ccEmails = new List<string>();

            if (string.Equals(entityType, Common.TYPE_INVOICE, StringComparison.OrdinalIgnoreCase))
            {
                AribaInvoice invoice = _repoWrapper.AribaInvoice.FindFirstByCondition(x => x.IsActive && x.InvoiceId == aribaId);
                if (invoice == null)
                {
                    _logger.LogInfo($"Invoice not found for id {aribaId}");
                    return;
                }

                AribaTasks aribaTask = _repoWrapper.AribaTask.FindFirstByCondition(t =>
                    t.IsActive &&
                    t.ApprovableUniqueName == aribaId &&
                    t.Approver == approverAribaUsername);

                if (aribaTask == null)
                {
                    _logger.LogInfo($"No active AribaTask found for invoice {aribaId} and approver {approverAribaUsername}");
                    return;
                }

                Person approverPerson = _repoWrapper.Person.FindFirstByCondition(p => p.AribaUserName == approverAribaUsername);
                if (approverPerson == null)
                {
                    _logger.LogInfo($"No person found for AribaUserName {approverAribaUsername}");
                    return;
                }

                toEmail = approverPerson.Email;
                personName = approverPerson.Name;
                emailPlaceholder.EntityId = invoice.Id;
                emailPlaceholder.InvoiceId = invoice.InvoiceId;
                emailPlaceholder.SupplierName = invoice.SupplierName;
                emailPlaceholder.Amount = invoice.Amount;
                emailPlaceholder.DueDate = invoice.DueDate;
                emailPlaceholder.UserName = personName;
            }
            else if (string.Equals(entityType, Common.TYPE_PR, StringComparison.OrdinalIgnoreCase))
            {

                _logger.LogInfo($"Step 1: Checking if PR exists in AribaPR table for id {aribaId}");
                var pr = _repoWrapper.AribaPR.FindFirstByCondition(x => x.IsActive && x.RequisitionsId == aribaId);
                if (pr == null)
                {
                    _logger.LogInfo($"PR not found for id {aribaId} in AribaPR table");
                    return;
                }


                _logger.LogInfo($"Step 2: Checking AribaPRTasks for approvable_unique_name {aribaId} and approver {approverAribaUsername}");
                AribaPRTasks aribaTask = _repoWrapper.AribaPRTask.FindFirstByCondition(t =>
                    t.IsActive &&
                    t.ApprovableUniqueName == aribaId &&
                    t.Approver == approverAribaUsername);

                if (aribaTask == null)
                {
                    _logger.LogInfo($"No active AribaTask found matching approvable_unique_name {aribaId} and approver {approverAribaUsername}");
                    return;
                }

                _logger.LogInfo($"Step 3: Finding Person record with AribaUserName {approverAribaUsername}");
                Person approverPerson = _repoWrapper.Person.FindFirstByCondition(p =>
                    p.IsActive &&
                    p.AribaUserName == approverAribaUsername);

                if (approverPerson == null)
                {
                    _logger.LogInfo($"No active person found with AribaUserName {approverAribaUsername}");
                    return;
                }

                if (string.IsNullOrWhiteSpace(approverPerson.Email))
                {
                    _logger.LogInfo($"Person found but has no email address: {approverAribaUsername}");
                    return;
                }

                toEmail = approverPerson.Email;
                personName = approverPerson.Name;

                emailPlaceholder.EntityId = pr.Id;
                emailPlaceholder.RequisitionId = pr.RequisitionsId;
                emailPlaceholder.RequesterName = pr.RequesterName ?? "";
                emailPlaceholder.SupplierName = pr.SupplierName;
                emailPlaceholder.Amount = pr.Amount;
                emailPlaceholder.DueDate = pr.DueDate ?? pr.NeedBYDate;
                emailPlaceholder.UserName = personName;
            }
            else if (string.Equals(entityType, Common.TYPE_CONTRACT, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInfo($"Step 1: Checking if Contract exists in ContractDetails table for ContractTaskId {aribaId}");
                ContractDetails contract = _repoWrapper.ContractDetails.FindFirstByCondition(x => x.IsActive && x.ContractTaskId == aribaId);
                if (contract == null)
                {
                    _logger.LogInfo($"Contract not found for ContractTaskId {aribaId} in ContractDetails table");
                    return;
                }

                _logger.LogInfo($"Step 2: Checking ContractPending table for UniqueName {contract.ContractTaskId}");
                ContractPending contractPending = _repoWrapper.ContractPending.FindFirstByCondition(t =>
                    t.IsActive &&
                    t.UniqueName == contract.ContractTaskId &&
                    t.Approver == approverAribaUsername);

                if (contractPending == null)
                {
                    _logger.LogInfo($"No active ContractPending record found for ContractTaskId {contract.ContractTaskId} and approver {approverAribaUsername}");
                    return;
                }

                _logger.LogInfo($"Step 3: Finding Person record with AribaUserName {approverAribaUsername}");
                Person approverPerson = _repoWrapper.Person.FindFirstByCondition(p =>
                    p.IsActive &&
                    p.AribaUserName == approverAribaUsername);

                if (approverPerson == null)
                {
                    _logger.LogInfo($"No active person found with AribaUserName {approverAribaUsername}");
                    return;
                }

                if (string.IsNullOrWhiteSpace(approverPerson.Email))
                {
                    _logger.LogInfo($"Person found but has no email address: {approverAribaUsername}");
                    return;
                }

                toEmail = approverPerson.Email;
                personName = approverPerson.Name;


                emailPlaceholder.EntityId = Guid.TryParse(contract.ContractTaskId, out var contractTaskGuid) ? contractTaskGuid : (Guid?)null;
                emailPlaceholder.ContractTaskId = contract.ContractTaskId;
                emailPlaceholder.UserName = personName;
                emailPlaceholder.ContractTaskId = contract.ContractTaskId;
                emailPlaceholder.ContractBeginDate = contract.BeginDate;
                emailPlaceholder.ContractEndDate = contract.EndDate;
                emailPlaceholder.ContractTotalAmount = !string.IsNullOrWhiteSpace(contract.TotalAmount) && decimal.TryParse(contract.TotalAmount, out var totalAmt) ? totalAmt : (decimal?)null;
                emailPlaceholder.ContractCurrency = contract.Currency;
                emailPlaceholder.ContractAmountSpend = !string.IsNullOrWhiteSpace(contract.AmountSpend) && decimal.TryParse(contract.AmountSpend, out var spentAmt) ? spentAmt : (decimal?)null;
                emailPlaceholder.ContractBalanceAmount = !string.IsNullOrWhiteSpace(contract.BalanceAmount) && decimal.TryParse(contract.BalanceAmount, out var balanceAmt) ? balanceAmt : (decimal?)null;
            }


            else
            {
                _logger.LogInfo($"Unknown entity type: {entityType}");
                return;
            }

            if (string.IsNullOrWhiteSpace(toEmail))
            {
                _logger.LogInfo($"No recipient email found for aribaId {aribaId} and entity {entityType}");
                return;
            }

            emailPlaceholder.ToEmail = toEmail;
            emailPlaceholder.PersonName = personName;

            emailPlaceholder.Subject = ReplacePlaceholders(emailContent.Subject!, emailPlaceholder);
            emailPlaceholder.EmailBody = ReplacePlaceholders(emailContent.Body!, emailPlaceholder);

            if (SendEmail(emailPlaceholder.ToEmail!, emailPlaceholder.Subject!, emailPlaceholder.EmailBody!, emailPlaceholder.EmailType!, emailPlaceholder.EntityType!, emailPlaceholder.EntityId, ccEmails.Any() ? ccEmails : null))
            {
                CreateEmailSentDetails(emailPlaceholder);
                _logger.LogInfo($"Ariba approval email notification successfully created and sent for entity {entityType} with Id: {aribaId}");
            }
        }

        public void CreateMaterialPREmailNotification(MaterialPR materialPR, string emailKey, Guid loggedInUserId)
        {
            _logger.LogInfo($"Creating Material PR email notification for PRNumber: {materialPR?.PRNumber}");

            if (materialPR == null)
            {
                _logger.LogInfo($"Material PR is null");
                return;
            }

           
            User? user = _repoWrapper.User.FindFirstByCondition(x => x.IsActive && x.Id == loggedInUserId);
            if (user == null)
            {
                _logger.LogInfo($"No active user found with userId {loggedInUserId}");
                return;
            }

            
            Person? person = _repoWrapper.Person.FindFirstByCondition(x => x.IsActive && x.Id == user.PersonId);
            if (person == null)
            {
                _logger.LogInfo($"No active person found for userId {loggedInUserId}");
                return;
            }

            if (string.IsNullOrWhiteSpace(person.Email))
            {
                _logger.LogInfo($"Person found but has no email address for userId {loggedInUserId}");
                return;
            }

           
            EmailContent? emailContent = _repoWrapper.EmailContent.FindFirstByCondition(x => x.IsActive && x.Key == emailKey);
            if (emailContent == null)
            {
                _logger.LogInfo($"Email template not found for key {emailKey}");
                return;
            }

            var emailPlaceholder = new EmailPlaceholderDto
            {
                EntityId = materialPR.Id,
                EmailType = emailKey,
                ToEmail = person.Email,
                PersonName = person.Name,
                EmailBody = emailContent.Body,
                Subject = emailContent.Subject,
                CompanyName = _configuration[Common.COMPANY_NAME],
                SupportEmail = _configuration[Common.SUPPORT_EMAIL],
                PortalLink = _configuration[Common.ARIBA_PORTAK_LINK],
                CompanyLogo = _configuration[Common.COMPANY_LOGO_URL],
                EntityType = Common.TYPE_MATERIAL_PR_UPLOAD,
                UserName = person.Name,
                PRNumber = materialPR.PRNumber,
                Vendor = materialPR.Vendor,
                VendorNamePR = materialPR.VendorName,
                InvoiceIdPR = materialPR.InvoiceID,
                ReceiptId = materialPR.PRUniqueName,
                StatusPR = materialPR.Status,
                PONumber = materialPR.PRNumber,
                Amount = materialPR.Amount
            };

            emailPlaceholder.Subject = ReplacePlaceholders(emailContent.Subject!, emailPlaceholder);
            emailPlaceholder.EmailBody = ReplacePlaceholders(emailContent.Body!, emailPlaceholder);

            if (SendEmail(emailPlaceholder.ToEmail, emailPlaceholder.Subject, emailPlaceholder.EmailBody, emailPlaceholder.EmailType, emailPlaceholder.EntityType!, emailPlaceholder.EntityId, null))
            {
                CreateEmailSentDetails(emailPlaceholder);
                _logger.LogInfo($"Material PR email notification successfully created and sent for Material PR with PRNumber: {materialPR.PRNumber} to email: {person.Email}");
            }
        }

          public void CreateServicePREmailNotification(ServicePR servicePR, string emailKey, Guid loggedInUserId)
        {
            _logger.LogInfo($"Creating Service PR email notification for PRNumber: {servicePR?.PRNumber}");

            if (servicePR == null)
            {
                _logger.LogInfo($"Service PR is null");
                return;
            }

           
            User? user = _repoWrapper.User.FindFirstByCondition(x => x.IsActive && x.Id == loggedInUserId);
            if (user == null)
            {
                _logger.LogInfo($"No active user found with userId {loggedInUserId}");
                return;
            }

            
            Person? person = _repoWrapper.Person.FindFirstByCondition(x => x.IsActive && x.Id == user.PersonId);
            if (person == null)
            {
                _logger.LogInfo($"No active person found for userId {loggedInUserId}");
                return;
            }

            if (string.IsNullOrWhiteSpace(person.Email))
            {
                _logger.LogInfo($"Person found but has no email address for userId {loggedInUserId}");
                return;
            }

           
            EmailContent? emailContent = _repoWrapper.EmailContent.FindFirstByCondition(x => x.IsActive && x.Key == emailKey);
            if (emailContent == null)
            {
                _logger.LogInfo($"Email template not found for key {emailKey}");
                return;
            }

            var emailPlaceholder = new EmailPlaceholderDto
            {
                EntityId = servicePR.Id,
                EmailType = emailKey,
                ToEmail = person.Email,
                PersonName = person.Name,
                EmailBody = emailContent.Body,
                Subject = emailContent.Subject,
                CompanyName = _configuration[Common.COMPANY_NAME],
                SupportEmail = _configuration[Common.SUPPORT_EMAIL],
                PortalLink = _configuration[Common.ARIBA_PORTAK_LINK],
                CompanyLogo = _configuration[Common.COMPANY_LOGO_URL],
                EntityType = Common.TYPE_SERVICE_PR_UPLOAD,
                UserName = person.Name,
                PRNumber =servicePR.PRNumber,
                Vendor = servicePR.Vendor,
                VendorNamePR = servicePR.VendorName,
                InvoiceIdPR = servicePR.InvoiceID,
                ReceiptId = servicePR.PRServiceUniqueName,
                StatusPR = servicePR.Status,
         
                Amount = servicePR.Amount
            };

            emailPlaceholder.Subject = ReplacePlaceholders(emailContent.Subject!, emailPlaceholder);
            emailPlaceholder.EmailBody = ReplacePlaceholders(emailContent.Body!, emailPlaceholder);

            if (SendEmail(emailPlaceholder.ToEmail, emailPlaceholder.Subject, emailPlaceholder.EmailBody, emailPlaceholder.EmailType, emailPlaceholder.EntityType!, emailPlaceholder.EntityId, null))
            {
                CreateEmailSentDetails(emailPlaceholder);
                _logger.LogInfo($"Service PR email notification successfully created and sent for Service PR with PRNumber: {servicePR.PRNumber} to email: {person.Email}");
            }
        }
    }
}

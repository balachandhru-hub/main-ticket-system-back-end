using Contracts;
using Contracts.IServices;
using Contracts.IRepository;
using Dto;
using ExceptionHandler;
using System;
using Entities.Models;
using System.Collections.Generic;
using System.Linq;
using Entities.Common;
using System.Transactions;
using Microsoft.Extensions.Configuration;

namespace Services
{
    /// <summary>
    /// Ticket Service
    /// </summary>
    public class TicketService : ITicketService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerManager _logger;
        private readonly IAssetService _assetService;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IExcelSafeStoreHelper _excelSafeStoreHelper;

        public TicketService(IRepositoryWrapper repositoryWrapper, ILoggerManager logger, IAssetService assetService, IConfiguration configuration, IEmailService emailService, IExcelSafeStoreHelper excelSafeStoreHelper)
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
            _assetService = assetService;
            _configuration = configuration;
            _emailService = emailService;
            _excelSafeStoreHelper = excelSafeStoreHelper;
        }
        /// <summary>
        /// Returns dashboard summary for tickets based on user role.
        /// </summary>
        /// <param name="userId">The user's ID.</param>
        /// <param name="role">The user's role name.</param>
        public TicketDashboardSummaryDto GetDashboardSummary(Guid userId)
        {
            _logger.LogInfo($"Fetching dashboard summary for userId: {userId}");

            TicketDashboardSummaryDto summary = new TicketDashboardSummaryDto();

            UserRoleMapping userRoleMapping = _repositoryWrapper.UserRoleMapping.FindFirstByCondition(m => m.UserId == userId);
            if (userRoleMapping == null)
            {
                _logger.LogError($"No role mapping found for userId: {userId}");
                throw new PreConditionFailedCustomException("User role not found", $"User with id {userId} does not have a role assigned.");
            }

            Guid roleId = userRoleMapping.RoleId;

            if (roleId == Common.BUSINESS_USER_ROLE_ID || roleId == Common.LEADERSHIP_USER_ROLE_ID || roleId == Common.OPERATIONS_USER_ROLE_ID || roleId == Common.FINANCE_OPERATION_USER_ROLE_ID)
            {
                _logger.LogInfo("User is BUSINESS_USER. Fetching tickets created by this user.");
                IQueryable<Ticket> userTickets = _repositoryWrapper.Ticket.FindByCondition(t => t.IsActive && t.UserId == userId);

                summary.OpenTickets = userTickets.Count(t => t.Status == Common.INITIAL_STATUS_FOR_TICKET);
                summary.InProgress = userTickets.Count(t => t.Status == Common.STATUS_IN_PROGRESS);
                summary.Resolved = userTickets.Count(t => t.Status == Common.STATUS_COMPLETED);
                summary.Approved = userTickets.Count(t => t.Status == Common.STATUS_APPROVE);

            }
            else if (roleId == Common.BUSINESS_ADMIN_ROLE_ID)
            {
                _logger.LogInfo("User is BUSINESS_ADMIN. Fetching tickets by domain.");
                Guid? domainId = (from user in _repositoryWrapper.User.FindByCondition(u => u.IsActive && u.Id == userId)
                                  join person in _repositoryWrapper.Person.FindByCondition(x => x.IsActive)
                                  on user.PersonId equals person.Id
                                  where person.IsActive
                                  select person.DomainId).FirstOrDefault();

                if (domainId == null)
                {
                    _logger.LogError($"DomainId not found or user does not have active person mapping. UserId: {userId}");
                    throw new PreConditionFailedCustomException("Invalid Domain", $"User with id {userId} does not have a valid domain.");
                }

                IQueryable<Ticket> domainTickets = _repositoryWrapper.Ticket.FindByCondition(t => t.IsActive && t.DomainId == domainId);

                summary.OpenTickets = domainTickets.Count(t => t.Status == Common.INITIAL_STATUS_FOR_TICKET);
                summary.InProgress = domainTickets.Count(t => t.Status == Common.STATUS_IN_PROGRESS);
                summary.Resolved = domainTickets.Count(t => t.Status == Common.STATUS_COMPLETED);
                summary.Approved = domainTickets.Count(t => t.Status == Common.STATUS_APPROVE);

            }
            else if (roleId == Common.MANAGER_ROLE_ID || roleId == Common.LEAD_ROLE_ID)
            {
                _logger.LogInfo("User is MANAGER or LEAD. Fetching approval tickets by domain.");
                IQueryable<Ticket> domainTickets = _repositoryWrapper.Ticket.FindByCondition(t => t.IsActive);

                summary.Approved = domainTickets.Count(t => t.Status == Common.STATUS_APPROVE);
                summary.InProgress = domainTickets.Count(t => t.Status == Common.STATUS_IN_PROGRESS);
                summary.Resolved = domainTickets.Count(t => t.Status == Common.STATUS_COMPLETED);
                summary.Rejected = domainTickets.Count(t => t.Status == Common.REJECTED_TICKET_STATUS);
                summary.OpenTickets = null;
            }
            else if (roleId == Common.CONSULTANT_ROLE_ID)
            {
                _logger.LogInfo("User is CONSULTANT. Fetching tickets assigned to them.");
                IQueryable<Ticket> consultantTickets = _repositoryWrapper.Ticket.FindByCondition(t => t.IsActive && t.AssignedTo == userId);

                summary.OpenTickets = null;
                summary.Approved = consultantTickets.Count(t => t.Status == Common.STATUS_APPROVE);
                summary.InProgress = consultantTickets.Count(t => t.Status == Common.STATUS_IN_PROGRESS);
                summary.Resolved = consultantTickets.Count(t => t.Status == Common.STATUS_COMPLETED);
                summary.Rejected = consultantTickets.Count(t => t.Status == Common.REJECTED_TICKET_STATUS);
                summary.OnHold = consultantTickets.Count(t => t.Status == Common.STATUS_ON_HOLD);
            }
            else
            {
                _logger.LogError($"Unknown role for userId: {userId}, roleId: {roleId}");
                throw new PreConditionFailedCustomException("Invalid Role", $"User with id {userId} does not have a valid role for dashboard summary.");
            }

            _logger.LogInfo($"Successfully generated dashboard summary for userId: {userId}");
            return summary;
        }

        /// <summary>
        /// Creates a new ticket.
        /// </summary>
        /// <param name="ticket">TicketRequestDto containing ticket details.</param>
        public Guid CreateTicket(TicketRequestDto ticket)
        {
            _logger.LogInfo($"Creating a new ticket with title: {ticket.Title}");

            using (TransactionScope transaction = new TransactionScope())
            {

                if (ticket.IPAddress == null)
                {
                    _logger.LogError("Unable to Catch the IP Address from the Request");
                    throw new PreConditionFailedCustomException("Missing IP Address", "Unable to Catch the IP Address from the Request");
                }

                User user = _repositoryWrapper.User.FindFirstByCondition(x => x.IsActive && x.Id == ticket.UserId);
                if (user == null)
                {
                    _logger.LogError($"Unable to Find Any Registered User with Given Id : {ticket.UserId}");
                    throw new NotFoundCustomException("User Not Found", $"No user found with Id : {ticket.UserId}");
                }

                DomainPlan? domainPlan = _repositoryWrapper.DomainPlan.GetDomainPlanByUserId((Guid)ticket.UserId!);
                if (domainPlan == null)
                {
                    _logger.LogError($"No domain plan found for user ID: {ticket.UserId}. Cannot create ticket.");
                    throw new PreConditionFailedCustomException("No Plan Found", "You are not subscribed to any plan. Ticket cannot be created.");
                }

                PlanDetails planDetails = _repositoryWrapper.PlanDetails.GetPlanDetailsByDomainId(domainPlan.DomainId);
                if (domainPlan == null)
                {
                    _logger.LogError($"No domain plan found for user ID: {ticket.UserId}. Cannot create ticket.");
                    throw new PreConditionFailedCustomException("No Plan Found", "You are not subscribed to any plan. Ticket cannot be created.");
                }

                double totalHours;
                decimal amount = 0;


                if (domainPlan.PlanName == Common.PAY_AS_USE && domainPlan.ContractEndDate > DateTime.UtcNow)
                {
                    totalHours = Common.DEFAULT_TICKET_HOURS;
                    amount = planDetails.TicketCharge;
                }
                else if (domainPlan.PlanName == Common.SUBSCRIPTION_PLAN_KEY || domainPlan.PlanName == Common.SUPPORT_TEAM_PLAN_KEY && domainPlan.EndDate > DateTime.UtcNow)
                {
                    totalHours = Common.DEFAULT_TICKET_HOURS;

                    DomainPlanCredit credit = _repositoryWrapper.DomainPlanCredit.FindFirstByCondition(x => x.DomainPlanId == domainPlan.Id);

                    if (credit != null && credit.TotalHourCredit < totalHours)
                    {
                        ticket.IsExceed = true;
                    }

                    // If IsExceed is true, calculate amount
                    if (ticket.IsExceed)
                    {

                        decimal chargePerHour = planDetails.AdditionalCRHoursCharge;
                        amount = chargePerHour * (decimal)totalHours;

                        _logger.LogInfo($"IsExceed: true. Calculated additional charge amount: {amount} for hours: {totalHours}");
                    }
                }
                else
                {
                    _logger.LogError($"Invalid plan type: {domainPlan.PlanName} for user ID: {ticket.UserId}");
                }


                Ticket entity = new Ticket
                {
                    Id = Guid.NewGuid(),
                    UserId = (Guid)ticket.UserId,
                    Title = ticket.Title,
                    Description = ticket.Description,
                    Status = Common.INITIAL_STATUS_FOR_TICKET,
                    Plan = domainPlan.PlanName,
                    Priority = ticket.Priority,
                    Type = Common.DEFAULT_TICKET_HOURS_BUCKET,
                    IPAddress = ticket.IPAddress,
                    DomainId = domainPlan.DomainId,
                    TotalHours = Common.DEFAULT_TICKET_HOURS,
                    Amount = amount,
                    Category = ticket.Category,
                    AssignedBy = ticket.UserId,
                    IsExceed = ticket.IsExceed
                };

                _repositoryWrapper.Ticket.Create(entity);

                if (ticket.Asset != null)
                {
                    ticket.Asset.EntityId = entity.Id;
                    _assetService.UploadDocument(ticket.Asset);
                }
                _repositoryWrapper.Save();

                try
                {
                    CreateEmailNotification(entity, Common.TICKET_CREATION_EMAIL_KEY, Common.ENTITY_TYPE_TICKET);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to send ticket creation email notification: {ex.Message}");
                }

                transaction.Complete();

                _logger.LogInfo($"Ticket created successfully. Id: {entity.Id}");

                return entity.Id;
            }
        }

        /// <summary>
        /// Creates a new ticket.
        /// </summary>
        /// <param name="ticket">TicketRequestDto containing ticket details.</param>
        public Guid CreateTicketV2(TicketRequestV2Dto ticket)
        {
            _logger.LogInfo($"Creating a new ticket with title: {ticket.Title}");

            using (TransactionScope transaction = new TransactionScope())
            {

                if (ticket.IPAddress == null)
                {
                    _logger.LogError("Unable to Catch the IP Address from the Request");
                    throw new PreConditionFailedCustomException("Missing IP Address", "Unable to Catch the IP Address from the Request");
                }

                User user = _repositoryWrapper.User.FindFirstByCondition(x => x.IsActive && x.Id == ticket.UserId);
                if (user == null)
                {
                    _logger.LogError($"Unable to Find Any Registered User with Given Id : {ticket.UserId}");
                    throw new NotFoundCustomException("User Not Found", $"No user found with Id : {ticket.UserId}");
                }

                DomainPlan? domainPlan = _repositoryWrapper.DomainPlan.GetDomainPlanByUserId((Guid)ticket.UserId!);
                if (domainPlan == null)
                {
                    _logger.LogError($"No domain plan found for user ID: {ticket.UserId}. Cannot create ticket.");
                    throw new PreConditionFailedCustomException("No Plan Found", "You are not subscribed to any plan. Ticket cannot be created.");
                }

                PlanDetails planDetails = _repositoryWrapper.PlanDetails.GetPlanDetailsByDomainId(domainPlan.DomainId);
                if (domainPlan == null)
                {
                    _logger.LogError($"No domain plan found for user ID: {ticket.UserId}. Cannot create ticket.");
                    throw new PreConditionFailedCustomException("No Plan Found", "You are not subscribed to any plan. Ticket cannot be created.");
                }

                double totalHours;
                decimal amount = 0;


                if (domainPlan.PlanName == Common.PAY_AS_USE && domainPlan.ContractEndDate > DateTime.UtcNow)
                {
                    totalHours = Common.DEFAULT_TICKET_HOURS;
                    amount = planDetails.TicketCharge;
                }
                else if (domainPlan.PlanName == Common.SUBSCRIPTION_PLAN_KEY || domainPlan.PlanName == Common.SUPPORT_TEAM_PLAN_KEY && domainPlan.EndDate > DateTime.UtcNow)
                {
                    totalHours = Common.DEFAULT_TICKET_HOURS;

                    DomainPlanCredit credit = _repositoryWrapper.DomainPlanCredit.FindFirstByCondition(x => x.DomainPlanId == domainPlan.Id);

                    if (credit != null && credit.TotalHourCredit < totalHours)
                    {
                        ticket.IsExceed = true;
                    }

                    // If IsExceed is true, calculate amount
                    if (ticket.IsExceed)
                    {

                        decimal chargePerHour = planDetails.AdditionalCRHoursCharge;
                        amount = chargePerHour * (decimal)totalHours;

                        _logger.LogInfo($"IsExceed: true. Calculated additional charge amount: {amount} for hours: {totalHours}");
                    }
                }
                else
                {
                    _logger.LogError($"Invalid plan type: {domainPlan.PlanName} for user ID: {ticket.UserId}");
                }


                Ticket entity = new Ticket
                {
                    Id = Guid.NewGuid(),
                    UserId = (Guid)ticket.UserId,
                    Title = ticket.Title,
                    Description = ticket.Description,
                    Status = Common.INITIAL_STATUS_FOR_TICKET,
                    Plan = domainPlan.PlanName,
                    Priority = ticket.Priority,
                    Type = Common.DEFAULT_TICKET_HOURS_BUCKET,
                    IPAddress = ticket.IPAddress,
                    DomainId = domainPlan.DomainId,
                    TotalHours = Common.DEFAULT_TICKET_HOURS,
                    Amount = amount,
                    Category = ticket.Category,
                    AssignedBy = ticket.UserId,
                    IsExceed = ticket.IsExceed
                };

                _repositoryWrapper.Ticket.Create(entity);

                if (ticket.Asset != null && ticket.Asset.Any())
                {
                    if (ticket.Asset.Count > 10)
                        throw new PreConditionFailedCustomException(
                            "Maximum 10 files allowed",
                            "You cannot upload more than 10 assets for this ticket."
                        );

                    foreach (var asset in ticket.Asset)
                    {
                        asset.EntityId = entity.Id;
                        _assetService.UploadDocument(asset);
                    }
                }
                _repositoryWrapper.Save();

                try
                {
                    CreateEmailNotification(entity, Common.TICKET_CREATION_EMAIL_KEY, Common.ENTITY_TYPE_TICKET);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to send ticket creation email notification: {ex.Message}");
                }

                transaction.Complete();

                _logger.LogInfo($"Ticket created successfully. Id: {entity.Id}");

                return entity.Id;
            }
        }

        /// <summary> 
        /// Retrieves all tickets for a specific user.
        /// </summary>
        /// <param name="index">The starting index for pagination.</param>  
        /// <param name="limit">The maximum number of tickets to retrieve.</param>
        /// <param name="userId">The ID of the user whose tickets are to be retrieved.</param>
        public List<TicketDetailDto> GetAllTickets(Guid userId, TicketFilterDto filters)
        {
            UserRoleMapping userRoleMapping = _repositoryWrapper.UserRoleMapping
                .FindFirstByCondition(x => x.UserId == userId);

            if (userRoleMapping == null)
            {
                throw new PreConditionFailedCustomException("Role mapping not found", $"User ID: {userId}");
            }

            Guid roleId = userRoleMapping.RoleId;
            Guid domainId = _repositoryWrapper.User.GetDomainIdByUserId(userId);

            IQueryable<Ticket> query = _repositoryWrapper.Ticket.FindByCondition(x => x.IsActive);

            // Role-based filtering
            if (roleId == Common.BUSINESS_ADMIN_ROLE_ID)
            {
                // Business Admin: Tickets in the same domain
                query = query.Where(x => x.DomainId == domainId);
            }
            else if (roleId == Common.BUSINESS_USER_ROLE_ID || roleId == Common.LEADERSHIP_USER_ROLE_ID || roleId == Common.OPERATIONS_USER_ROLE_ID||roleId==Common.FINANCE_OPERATION_USER_ROLE_ID)
            {
                // Business User: Only their own tickets
                query = query.Where(x => x.UserId == userId);
            }
            else if (roleId == Common.BUSINESS_ADMIN_ROLE_ID)
            {
                query = query.Where(x => x.UserId == userId);
            }
            else if (roleId == Common.MANAGER_ROLE_ID || roleId == Common.LEAD_ROLE_ID)
            {
                query = query.Where(x => x.Status != Common.INITIAL_STATUS_FOR_TICKET && x.Status != Common.STATUS_DECLINE);
            }
            else if (roleId == Common.CONSULTANT_ROLE_ID)
            {
                // Consultant: Only assigned tickets
                query = query.Where(x => x.AssignedTo == userId);
            }

            // Search filter (normalized by removing dot and trimming)
            if (!string.IsNullOrEmpty(filters.SearchTerm))
            {
                string lowered = filters.SearchTerm.ToLower().Replace(".", "").Trim();
                query = query.Where(x =>
                    x.Title.ToLower().Replace(".", "").Contains(lowered) ||
                    x.Description.ToLower().Replace(".", "").Contains(lowered) ||
                    x.TicketCode.ToLower().Replace(".", "").Contains(lowered));
            }

            if (!string.IsNullOrEmpty(filters.Status))
            {
                query = query.Where(x => x.Status.ToLower() == filters.Status.ToLower());
            }

            if (!string.IsNullOrEmpty(filters.Category))
            {
                query = query.Where(x => x.Category.ToLower() == filters.Category.ToLower());
            }

            if (!string.IsNullOrEmpty(filters.Priority))
            {
                query = query.Where(x => x.Priority.ToLower() == filters.Priority.ToLower());
            }
            if (filters.FromDate.HasValue)
            {
                query = query.Where(x => x.DateCreated >= filters.FromDate.Value);
            }

            if (filters.ToDate.HasValue)
            {
                query = query.Where(x => x.DateCreated <= filters.ToDate.Value);
            }


            List<TicketDetailDto> result = (
                from ticket in query
                join user in _repositoryWrapper.User.FindByCondition(u => true)
                    on ticket.UserId equals user.Id into userJoin
                from user in userJoin.DefaultIfEmpty()

                join person in _repositoryWrapper.Person.FindByCondition(p => true)
                    on user.PersonId equals person.Id
                    into personJoin
                from person in personJoin.DefaultIfEmpty()

                join asset in _repositoryWrapper.Asset.FindByCondition(a =>
                        a.IsActive && a.AssetType == Common.UPLOADED_FILE_ID)
                    on ticket.Id equals asset.EntityId into assetJoin
                from asset in assetJoin.DefaultIfEmpty()

                join assetTypeMeta in _repositoryWrapper.Metadata.FindByCondition(x => x.IsActive)
                    on asset.AssetType equals assetTypeMeta.Id into assetTypeJoin
                from assetTypeMeta in assetTypeJoin.DefaultIfEmpty()

                join fileTypeMeta in _repositoryWrapper.Metadata.FindByCondition(x => x.IsActive)
                    on asset.FileType equals fileTypeMeta.Id into fileTypeJoin
                from fileTypeMeta in fileTypeJoin.DefaultIfEmpty()

                join pr in _repositoryWrapper.TicketPR.FindByCondition(p => p.IsActive)
                    on ticket.Id equals pr.TicketId into prJoin
                from pr in prJoin.DefaultIfEmpty()
                join domain in _repositoryWrapper.Domain.FindByCondition(d => d.IsActive)
                   on ticket.DomainId equals domain.Id into domainJoin
                from domain in domainJoin.DefaultIfEmpty()


                orderby ticket.DateUpdated descending

                select new
                {
                    Ticket = ticket,
                    PersonEmail = person != null ? person.Email : null,
                    PersonName = person != null ? person.Name : null,
                    OrganizationName = domain != null ? domain.Name : null,
                    AssinedToName = ticket.AssignedTo != null
                    ? _repositoryWrapper.Person.GetPersonNameByUserId((Guid)ticket.AssignedTo!).Name
                    : null,

                    // 🔧 FIX: Capture raw asset instead of projecting inside SQL
                    AssetRaw = asset,
                    AssetTypeKey = assetTypeMeta != null ? assetTypeMeta.Key : null,
                    FileTypeKey = fileTypeMeta != null ? fileTypeMeta.Key : null,

                    PR = pr != null ? new TicketPRDetailDto
                    {
                        Id = pr.Id,
                        TaskName = pr.TaskName,
                        Hrs = pr.Hrs
                    } : null
                }
            )
            .AsEnumerable()
            .GroupBy(x => x.Ticket.Id)
            .Select(g => new TicketDetailDto
            {
                Id = g.Key,
                UserId = g.First().Ticket.UserId,
                Title = g.First().Ticket.Title,
                Description = g.First().Ticket.Description,
                Status = g.First().Ticket.Status,
                Priority = g.First().Ticket.Priority,
                Category = g.First().Ticket.Category,
                AssignedTo = g.First().Ticket.AssignedTo,
                AssignedBy = g.First().Ticket.AssignedBy,
                DateCreated = g.First().Ticket.DateCreated,
                AdminDescription = g.First().Ticket.AdminDescription,
                Name = g.First().PersonName,
                TotalHours = g.First().Ticket.TotalHours,
                Asset = g
                    .Where(x => x.AssetRaw != null)
                    .Select(x => new AssetDto
                    {
                        Id = x.AssetRaw.Id,
                        AssetName = x.AssetRaw.AssetName,
                        FileName = x.AssetRaw.FileName,
                        AssetType = x.AssetTypeKey,
                        FileType = x.FileTypeKey
                    })
                    .ToList(),
                PRDetail = g
                    .Where(x =>
                     x.PR != null)
                    .Select(x => x.PR)
                    .ToList(),
                Email = g.First().PersonEmail,
                Organization = g.First().OrganizationName,
                AssignedToName = g.First().AssinedToName,
                TicketCode = g.First().Ticket.TicketCode
            })
            .Skip(filters.Index)
            .Take(filters.Limit)
            .ToList();

            if (!result.Any())
            {
                _logger.LogInfo($"Unable to Find any List of Ticket for the User : {userId}");
                throw new NoContentCustomException("Unable to Find Any Ticket", $"Unable to Find Any Ticket for User : {userId}");
            }

            return result;
        }



        /// <summary>
        /// Retrieves a ticket by its ID.  
        /// </summary>
        /// <param name="id">The ID of the ticket to retrieve.</param>
        public TicketDetailDto GetTicketById(Guid id)
        {
            _logger.LogInfo($"Fetching ticket with ID: {id}");
            TicketDetailDto? ticketDetail = _repositoryWrapper.Ticket.GetTicketById(id);
            if (ticketDetail == null)
            {
                _logger.LogError($"Ticket data not found with ID: {id}");
                throw new NotFoundCustomException($"Ticket data not found with Id : {id}", $"Ticket data not found with Id : {id}");
            }
            _logger.LogInfo($"Fetched ticket with ID: {id}");
            return ticketDetail;
        }


        /// <summary>
        /// Updates the status of a ticket.                    
        /// </summary>
        /// <param name="dto">The DTO containing updatable fields.</param>                  +
        public Guid UpdateTicket(TicketUpdateDto dto)
        {
            _logger.LogInfo($"Update ticket with id {dto.Id}");

            using (TransactionScope transaction = new TransactionScope())
            {
                Ticket ticket = _repositoryWrapper.Ticket.FindFirstByCondition(x => x.Id == dto.Id && x.IsActive);
                if (ticket == null)
                {
                    _logger.LogError($"Unable to Find the Ticket with Id {dto.Id}");
                    throw new NotFoundCustomException($"Unable to Find the Ticket", $"Unable to Find the Ticket with Id : {dto.Id}");
                }

                UserRoleMapping userRoleMapping = _repositoryWrapper.UserRoleMapping.FindFirstByCondition(x => x.UserId == dto.UserId);
                if (userRoleMapping == null)
                {
                    _logger.LogError($"Unable to Find the User Role for the User with Id : {dto.UserId}");
                    throw new PreConditionFailedCustomException($"Unable to find the User Role", $"Unable to Find the User Role for User with Id : {dto.UserId}");
                }

                DomainPlan? domainPlan = _repositoryWrapper.DomainPlan.GetDomainPlanByUserId((Guid)ticket.UserId!);
                if (domainPlan == null)
                {
                    _logger.LogError($"No domain plan found for user ID: {ticket.UserId}. Cannot create ticket.");
                    throw new PreConditionFailedCustomException("No Plan Found", "You are not subscribed to any plan. Ticket cannot be created.");
                }

                if (userRoleMapping.RoleId == Common.BUSINESS_USER_ROLE_ID || userRoleMapping.RoleId == Common.LEADERSHIP_USER_ROLE_ID || userRoleMapping.RoleId == Common.OPERATIONS_USER_ROLE_ID || userRoleMapping.RoleId == Common.FINANCE_OPERATION_USER_ROLE_ID)
                {
                    ticket.Title = dto.Title ?? ticket.Title;
                    ticket.Description = dto.Description ?? ticket.Description;
                    ticket.Priority = dto.Priority ?? ticket.Priority;
                    ticket.Category = dto.Category ?? ticket.Category;
                }
                else if (userRoleMapping.RoleId == Common.BUSINESS_ADMIN_ROLE_ID)
                {
                    ticket.Title = dto.Title ?? ticket.Title;
                    ticket.Description = dto.Description ?? ticket.Description;
                    ticket.Priority = dto.Priority ?? ticket.Priority;
                    ticket.Category = dto.Category ?? ticket.Category;
                    ticket.AdminDescription = dto.AdminDescription ?? ticket.AdminDescription;
                    if (dto.Status == Common.STATUS_APPROVE || dto.Status == Common.TICKET_DECLINE_STATUS_KEY || dto.Status == Common.REJECTED_TICKET_STATUS)
                        ticket.Status = dto.Status;
                }
                else if (userRoleMapping.RoleId == Common.MANAGER_ROLE_ID || userRoleMapping.RoleId == Common.LEAD_ROLE_ID)
                {
                    ticket.Title = dto.Title ?? ticket.Title;
                    ticket.Category = dto.Category ?? ticket.Category;
                    ticket.Priority = dto.Priority ?? ticket.Priority;
                    ticket.AssignedTo = dto.AssignedTo ?? ticket.AssignedTo;
                    if (dto.Status == Common.STATUS_RE_APPROVE || dto.Status == Common.STATUS_IN_PROGRESS ||
                        dto.Status == Common.STATUS_CLOSED || dto.Status == Common.STATUS_ON_HOLD)
                        ticket.Status = dto.Status;
                    if (dto.Status == Common.STATUS_IN_PROGRESS && ticket.IsCredited == false)
                    {
                        if (domainPlan.PlanName == Common.SUBSCRIPTION_PLAN_KEY || domainPlan.PlanName == Common.SUPPORT_TEAM_PLAN_KEY)
                        {
                            DomainPlanCredit credit = _repositoryWrapper.DomainPlanCredit.GetDomainPlanCredit(ticket.DomainId);
                            if (credit != null && credit.MonthlyHourCredit >= ticket.TotalHours)
                            {
                                credit.MonthlyHourCredit -= ticket.TotalHours;
                            }
                            else if (credit != null && credit.MonthlyHourCredit < ticket.TotalHours)
                            {
                                // Get current UTC time
                                DateTime currentTime = DateTime.UtcNow;
                                if (credit.DomainPlan != null && currentTime > credit.DomainPlan.EndDate)
                                {
                                    _logger.LogInfo("Subscription expired. Ticket hours exceed remaining credit and plan has ended.");
                                    throw new PreConditionFailedCustomException("Subscription expired", "Your subscription has expired. Please renew your plan to continue.");
                                }
                                ticket.IsExceed = true;
                                ticket.IsPayable = true;
                            }
                            else
                            {
                                _logger.LogError($"Insufficient monthly hour credit for domain {ticket.DomainId}");
                                throw new PreConditionFailedCustomException("Insufficient monthly hour credit", $"Insufficient monthly hour credit for domain {ticket.DomainId}");
                            }
                            ticket.IsCredited = true;
                            _repositoryWrapper.DomainPlanCredit.Update(credit);
                        }
                        else if (domainPlan.PlanName == Common.PAY_AS_USE)
                        {
                            ticket.IsCredited = true;
                            ticket.IsPayable = true;
                        }
                    }
                }

                else if (userRoleMapping.RoleId == Common.CONSULTANT_ROLE_ID)
                {
                    ticket.Title = dto.Title ?? ticket.Title;
                    ticket.Description = dto.Description ?? ticket.Description;
                    ticket.Priority = dto.Priority ?? ticket.Priority;
                    if (dto.Status == Common.STATUS_COMPLETED || dto.Status == Common.STATUS_ON_HOLD || dto.Status == Common.STATUS_IN_PROGRESS)
                        ticket.Status = dto.Status;
                }

                _repositoryWrapper.Ticket.Update(ticket);

                // --------------------- ASSET UPDATE LOGIC ------------------------------------
                if (dto.Asset != null)
                {
                    if (dto.Asset.Count > 10)
                    {
                        throw new PreConditionFailedCustomException(
                            "Max 10 files allowed",
                            "You cannot upload more than 10 assets for one ticket."
                        );
                    }

                    // Step 1: Fetch all active assets for ticket
                    var dbAssets = _repositoryWrapper.Asset
                                        .FindByCondition(x => x.EntityId == ticket.Id
                                                   && x.IsActive
                                                   && x.AssetType == Common.UPLOADED_FILE_ID)
                                        .ToList();

                    // Step 2: Extract IDs sent from front-end
                    var incomingIds = dto.Asset
                                        .Where(a => a.Id != null)
                                        .Select(a => a.Id.Value)
                                        .ToList();

                    // Step 3: Deactivate assets not sent by frontend
                    var deactivate = dbAssets
                                        .Where(db => !incomingIds.Contains(db.Id))
                                        .ToList();

                    foreach (var d in deactivate)
                    {
                        d.IsActive = false;
                        _repositoryWrapper.Asset.Update(d);
                    }

                    // Step 4: Insert new assets (Id == null)
                    var newAssets = dto.Asset
                                        .Where(a => a.Id == null)
                                        .ToList();

                    foreach (var newAsset in newAssets)
                    {
                        if (newAsset.FileBytes != null)
                        {
                            var uploadDto = new AssetUploadDto
                            {
                                EntityId = ticket.Id,
                                FileBytes = newAsset.FileBytes,
                                FileName = newAsset.FileName,
                                ContentType = newAsset.ContentType,
                                AssetType = Common.UPLOADED_FILE.ToString()
                            };

                            _assetService.UploadDocument(uploadDto);
                        }
                    }

                }

                _repositoryWrapper.Save();

                try
                {
                    CreateEmailNotification(ticket, Common.TICKET_MODIFICATION_EMAIL_KEY, Common.ENTITY_TYPE_TICKET);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to send ticket creation email notification: {ex.Message}");
                }

                transaction.Complete();
                _logger.LogInfo($"Updated ticket with Id {dto.Id}");
                return ticket.Id;
            }
        }


        /// <summary>
        /// Create Ticket for the Ariba User
        /// </summary>
        /// <param name="ticketCreateAribaDto">Dto Containing Details </param>
        public Guid CreateTicketAriba(TicketCreateAribaDto ticketCreateAribaDto)
        {
            _logger.LogInfo($"Creating ticket for the user with ariba user name : {ticketCreateAribaDto.AribaUserName}");

            if (ticketCreateAribaDto.IPAddress == null)
            {
                _logger.LogError("Unable to Catch the IP Address from the Request");
                throw new PreConditionFailedCustomException("Unable to Catch the IP Address from the Request", "Unable to Catch the IP Address from the Request");
            }
            Person person = _repositoryWrapper.Person.FindFirstByCondition(x => x.IsActive && x.AribaUserName == ticketCreateAribaDto.AribaUserName && x.Email == ticketCreateAribaDto.Email);
            if (person == null)
            {
                _logger.LogError($"Unable to find person with email : {ticketCreateAribaDto.Email} and ariba user name : {ticketCreateAribaDto.AribaUserName}");
                throw new NotFoundCustomException($"Unable to find person with email : {ticketCreateAribaDto.Email} and ariba user name : {ticketCreateAribaDto.AribaUserName} Please Register as User", $"Unable to find person with email : {ticketCreateAribaDto.Email} and ariba user name : {ticketCreateAribaDto.AribaUserName} Please Register as User");
            }
            User user = _repositoryWrapper.User.FindFirstByCondition(x => x.IsActive && x.PersonId == person.Id);
            if (user == null)
            {
                _logger.LogError($"Unable to find User with person id : {person.Id} ");
                throw new NotFoundCustomException($"Unable to find User with person id : {person.Id} Please connect support team.", $"Unable to find User with person id : {person.Id} Please connect support team.");
            }

            Ticket entity = new Ticket
            {
                Id = Guid.NewGuid(),
                Title = ticketCreateAribaDto.Title,
                Description = ticketCreateAribaDto.Description,
                Status = ticketCreateAribaDto.Status,
                UserId = user.Id,

                IPAddress = ticketCreateAribaDto.IPAddress
            };

            return entity.Id;
        }





        public Guid CreateTicketPR(List<TicketPRCreateDto> dto, Guid ticketId)
        {
            _logger.LogInfo($"[START] Creating TicketPR for TicketId: {ticketId}");

            try
            {
                // 1. Validate Ticket
                Ticket ticket = _repositoryWrapper.Ticket.FindFirstByCondition(x => x.IsActive && x.Id == ticketId);
                if (ticket == null)
                {
                    _logger.LogError($"Ticket not found with Id: {ticketId}");
                    throw new NotFoundCustomException("Ticket Not Found", $"Ticket with Id {ticketId} not found.");
                }


                double hours = dto.Sum(x => x.Hrs);
                double totalHours = ticket.TotalHours + hours;
                decimal crBeyondLimit = 0;


                if (ticket.Plan == Common.SUBSCRIPTION_PLAN_KEY || ticket.Plan == Common.SUPPORT_TEAM_PLAN_KEY)
                {
                    ticket.TotalHours = totalHours;
                    _logger.LogInfo($"[INFO] Subscription/Support plan updated. Total Hours: {ticket.TotalHours}");
                }

                else if (ticket.Plan == Common.PAY_AS_USE)
                {
                    PlanDetails planDetails = _repositoryWrapper.PlanDetails.GetPlanDetailsByTicketId(ticketId);
                    if (planDetails == null)
                    {
                        _logger.LogError($"PlanDetails not found for TicketId: {ticketId}");
                        throw new NotFoundCustomException("Plan Details Not Found", $"No PlanDetails found for TicketId: {ticketId}");
                    }

                    decimal chargePerHour = planDetails.CRHoursBeyondLimit;
                    decimal calculatedAmount = chargePerHour * (decimal)hours;
                    crBeyondLimit = planDetails.CRHoursBeyondLimit;

                    ticket.TotalHours = totalHours;
                    ticket.Amount += calculatedAmount;

                    _logger.LogInfo($"[INFO] PAY_AS_USE total hours: {totalHours}, Calculated Amount: {calculatedAmount} (not stored).");
                }
                else
                {
                    _logger.LogError($"TicketPR not allowed. Ticket with Id {ticketId} has unsupported plan: {ticket.Plan}");
                    throw new PreConditionFailedCustomException("Invalid Plan", $"TicketPR is not allowed for plan: {ticket.Plan}");
                }


                List<TicketPR> ticketPRList = dto.Select(x => new TicketPR
                {
                    Id = Guid.NewGuid(),
                    TicketId = ticketId,
                    TaskName = x.TaskName,
                    Amount = ticket.Plan == Common.PAY_AS_USE ? ((decimal)x.Hrs * crBeyondLimit) : x.Amount,
                    Hrs = x.Hrs
                }).ToList();

                _repositoryWrapper.TicketPR.CreateRange(ticketPRList);

                ticket.Status = Common.RE_APPROVE_STATUS;
                ticket.Type = Common.MORETHAN_8_HOURS_BUCKET;
                _repositoryWrapper.Ticket.Update(ticket);
                _repositoryWrapper.Save();

                _logger.LogInfo($"Successfully created TicketPR for ticket with Id: {ticketId}, Logged Hours: {hours}");
                return ticketId;

            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR] Failed to create TicketPR: {ex.Message} | StackTrace: {ex.StackTrace}");
                throw new Exception("Unexpected error occurred while creating TicketPR. Please check logs.");
            }
        }


        /// <summary>
        /// Get tickets Messages and assets for the given ticket with Id                    
        /// </summary>
        /// <param name="ticketId">Ticket Id</param>                  
        public List<TicketMessageDto> GetTicketMessageById(Guid ticketId, int index, int limit)
        {
            _logger.LogInfo($"Fetching the Ticket Message Details for Given Ticket with Id : {ticketId}");
            List<TicketMessageDto> result = _repositoryWrapper.TicketMessage.GetTicketMessageById(ticketId, index, limit);
            if (!result.Any())
            {
                _logger.LogError($"No Messages or Assets are Found for the Given Ticket with Id : {ticketId}");
                throw new NotFoundCustomException("No Messages Found", $"No Messages or Asset Found for the Ticket With Id : {ticketId}");
            }
            _logger.LogInfo($"Fetched the Ticket Message Details for Given Ticket with Id : {ticketId}");
            return result;
        }

        /// <summary>
        /// Create ticket Message for the given ticket with Id                    
        /// </summary>
        /// <param name="ticketMessage">Ticket Message Details</param>                  
        public Guid CreateTicketMessage(TicketMessageRequestDto ticketMessage)
        {
            _logger.LogInfo($"Create ticket message for Given Ticket with Id : {ticketMessage.TicketId}");
            TicketMessage ticketMessageNew = new TicketMessage()
            {
                Id = Guid.NewGuid(),
                Message = ticketMessage.Message,
                TicketId = ticketMessage.TicketId
            };
            _repositoryWrapper.TicketMessage.Create(ticketMessageNew);
            _repositoryWrapper.Save();
            _logger.LogInfo($"Successfully Created Ticket Message for ticket with Id : {ticketMessage.TicketId}");
            return ticketMessageNew.Id;
        }

        /// <summary>
        /// Creates the Email Notification for the Ticket.
        /// </summary>
        public void CreateEmailNotification(Ticket ticket, string emailKey, string entityType)
        {
            _logger.LogInfo($"Creating email notification for ticket with Id : {ticket.Id}");
            Person? person = _repositoryWrapper.Person.GetPersonNameByUserId(ticket.UserId);

            if (person != null)
            {
                EmailContent emailContent = _repositoryWrapper.EmailContent.FindFirstByCondition(x => x.IsActive && x.Key == emailKey);

                if (emailContent != null)
                {
                    EmailPlaceholderDto emailPlaceholder = new EmailPlaceholderDto
                    {
                        EntityId = ticket.Id,
                        TicketId = ticket.TicketCode,
                        EmailType = emailKey,
                        ToEmail = person.Email,
                        PersonName = person.Name,
                        EmailBody = emailContent.Body,
                        Subject = emailContent.Subject,
                        CompanyName = _configuration[Common.COMPANY_NAME],
                        SupportEmail = _configuration[Common.SUPPORT_EMAIL],
                        Title = ticket.Title,
                        Description = ticket.Description,
                        Date = DateOnly.FromDateTime(ticket.DateCreated),
                        PortalLink = _configuration[Common.PORTAL_LINK],
                        CompanyLogo = _configuration[Common.COMPANY_LOGO_URL],
                        EntityType = entityType,
                    };

                    emailPlaceholder.Subject = _emailService.ReplacePlaceholders(emailContent.Subject!, emailPlaceholder);
                    emailPlaceholder.EmailBody = _emailService.ReplacePlaceholders(emailContent.Body!, emailPlaceholder);

                    List<string>? ccEmail = _repositoryWrapper.EmailCCList.FindByCondition(x => x.IsActive && x.EntityType == entityType)
                        .Select(x => x.Email).ToList();

                    if (_emailService.SendEmail(emailPlaceholder.ToEmail, emailPlaceholder.Subject, emailPlaceholder.EmailBody, emailPlaceholder.EmailType, emailPlaceholder.EntityType!, emailPlaceholder.EntityId, ccEmail))
                    {
                        _emailService.CreateEmailSentDetails(emailPlaceholder);
                        _logger.LogInfo($"Email notification successfully created and sent for the ticket with Id: {ticket.Id}");
                    }
                }
            }
        }

        public ExportExcelResultDto ExportTicketDetailsExcel(Guid userId, TicketFilterDto filters)
        {
            _logger.LogInfo($"Exporting Ticket Excel for User: {userId}");

            var tickets = GetAllTickets(userId, filters);

            if (tickets == null || !tickets.Any())
            {
                _logger.LogError($"No tickets found for user {userId}, filters: {filters}");
                throw new NoContentCustomException("No Tickets Found", "No ticket data available for export");
            }

            using var workbook = new ClosedXML.Excel.XLWorkbook();

            var ws = workbook.Worksheets.Add("Tickets");

            if (!tickets.Any())
            {
                ws.Cell(1, 1).Value = "No Tickets Found";

                using var emptyStream = new MemoryStream();
                workbook.SaveAs(emptyStream);

                return new ExportExcelResultDto
                {
                    FileContent = emptyStream.ToArray(),
                    FileName = $"Ticket_Details_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };
            }

            var exportData = tickets.Select(x => new
            {
                x.Id,
                x.TicketCode,
                Title = _excelSafeStoreHelper.ExcelSafe(x.Title),
                Description = _excelSafeStoreHelper.ExcelSafe(x.Description),
                x.Status,
                x.Priority,
                x.Category,
                Name = _excelSafeStoreHelper.ExcelSafe(x.Name),
                x.Email,
                Organization = _excelSafeStoreHelper.ExcelSafe(x.Organization),
                AssignedTo = _excelSafeStoreHelper.ExcelSafe(x.AssignedToName),
                x.DateCreated,
                x.TotalHours
            }).ToList();

            ws.Cell(1, 1).InsertTable(exportData);
            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return new ExportExcelResultDto
            {
                FileContent = stream.ToArray(),
                FileName = $"Ticket_Details_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };
        }

    }
}
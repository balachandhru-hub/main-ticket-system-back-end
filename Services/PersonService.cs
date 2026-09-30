using Contracts.IServices;
using Contracts.IRepository;
using Dto;
using ExceptionHandler;
using System;
using Entities.Models;
using Contracts;
using System.Transactions;
using Entities.Common;
using Microsoft.Extensions.Configuration;

namespace Services
{
    /// <summary>
    /// Person Service
    /// </summary>
    public class PersonService : IPersonService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerManager _logger;
        private readonly IUserService _userService;
        private readonly IUserRoleMappingService _userRoleMappingService;
        private readonly IMetadataService _metadataService;
        private readonly IAssetService _assetService;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public PersonService(
            IRepositoryWrapper repositoryWrapper,
            ILoggerManager logger,
            IUserService userService,
            IUserRoleMappingService userRoleMappingService,
            IMetadataService metadataService,
            IAssetService assetService, IConfiguration configuration, IEmailService emailService)
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
            _userService = userService;
            _userRoleMappingService = userRoleMappingService;
            _metadataService = metadataService;
            _assetService = assetService;
            _configuration = configuration;
            _emailService = emailService;
        }

        /// <summary>
        /// Creates a new person in the system.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public Guid CreatePerson(PersonDto person)
        {
            _logger.LogInfo($"Creating a new person with email: {person.Email}");
            using (TransactionScope transaction = new TransactionScope())
            {
                Person exists = _repositoryWrapper.Person.FindFirstByCondition(x => x.IsActive && x.Email == person.Email);
                User userExist = _repositoryWrapper.User.FindFirstByCondition(x => x.IsActive && x.UserName == person.UserName);
                Person entity;

                if (exists != null)
                {
                    _logger.LogError($"Person with email {person.Email} already exists.");
                    throw new PreConditionFailedCustomException("Person with email address already exists", $"A person with the email '{person.Email}' already exists.");
                }
                if (userExist != null)
                {
                    _logger.LogError($"User with User Name {person.UserName} already exists.");
                    throw new PreConditionFailedCustomException("User with User Name already exists", $"A User with the username {person.UserName} already exists.");
                }
                else
                {
                    bool isDomainSpecificRole = person.DomainId != Common.DEFAULT_DOMAIN_ID && person.RoleId != Common.BUSINESS_ADMIN_ROLE_ID;
                    if (isDomainSpecificRole && !CheckBusinessAdminExist(person.DomainId))
                    {
                        _logger.LogError($"Unable to Find Any Business Admin for the Given Domain : {person.DomainId}");
                        throw new PreConditionFailedCustomException("Business Admin Not Found for the Domain, Please Create a New Business Admin", $"Business Admin Not Found for the Domain : {person.DomainId}, Please Create a New Business Admin");
                    }

                    entity = new Person
                    {
                        Id = Guid.NewGuid(),
                        Name = person.Name,
                        Email = person.Email,

                        TenantId = person.TenantId,
                        Address = person.Address,
                        MobilePhone = person.MobilePhone,
                        TempTenantId = person.TempTenantId,
                        RealmId = person.RealmId,
                        AribaUserName = person.AribaUserName,
                        Designation = person.Designation,
                        EmployeeId = person.EmployeeId,
                        Department = person.Department,
                        DomainId = person.DomainId,
                        Country = person.Country,
                        AribaPassword = person.AribaPassword,
                        orgANId = person.orgANId
                    };
                    _repositoryWrapper.Person.Create(entity);
                    _repositoryWrapper.Save();

                    UserDto userDto = new UserDto
                    {
                        PersonId = entity.Id,
                        UserName = person.UserName,
                        UserSecret = person.UserSecret
                    };
                    Guid userId = _userService.CreateUser(userDto);

                    UserRoleMappingDto userRoleMappingDto = new UserRoleMappingDto
                    {
                        UserId = userId,
                        RoleId = person.RoleId
                    };
                    _userRoleMappingService.CreateUserRole(userRoleMappingDto);
                    if (person.ModelIds != null && person.ModelIds.Any())
                    {
                        List<UserModelMapping> mappings = new List<UserModelMapping>();

                        foreach (var modelId in person.ModelIds)
                        {
                            mappings.Add(new UserModelMapping
                            {
                                Id = Guid.NewGuid(),
                                UserId = userId,
                                ModelId = modelId,
                                IsActive = true
                            });
                        }


                        _repositoryWrapper.UserModelMapping.CreateRange(mappings);
                        _repositoryWrapper.Save();
                    }
                    if (person.AppIds != null && person.AppIds.Any())
                    {
                        List<AppUserMapping> mappings = new List<AppUserMapping>();

                        foreach (var appId in person.AppIds)
                        {
                            mappings.Add(new AppUserMapping
                            {
                                Id = Guid.NewGuid(),
                                UserId = userId,
                                AppId = appId,
                                IsActive = true
                            });
                        }

                        _repositoryWrapper.AppUserMapping.CreateRange(mappings);
                        _repositoryWrapper.Save();
                    }
                    if (person.RequesterNames != null && person.RequesterNames.Any())
                    {
                        List<RequesterName> requesterList = new List<RequesterName>();

                        foreach (var req in person.RequesterNames)
                        {
                            requesterList.Add(new RequesterName
                            {
                                Id = Guid.NewGuid(),
                                DomainId = person.DomainId,
                                UserId = userId,
                                Name = req.Name,
                                Department = req.Department

                            });
                        }

                        _repositoryWrapper.RequesterName.CreateRange(requesterList);
                        _repositoryWrapper.Save();
                    }

                    try
                    {
                        CreateUserEmailNotification(entity, person.UserSecret, Common.ENTITY_TYPE_USER, Common.USER_CREATION_EMAIL_KEY);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Failed to send user creation email notification: {ex.Message}");
                    }
                }
                transaction.Complete();
                return entity.Id;
            }
        }
        /// <summary>
        /// Updates the name, address, and mobile number of the currently logged-in person.
        /// </summary>
        /// <param name="userId">The ID of the logged-in user.</param>
        /// <param name="updateDto">The updated person details.</param>
        public void UpdatePerson(Guid userId, UpdatePersonDto personDto)
        {
            _logger.LogInfo($"Updating person with User ID: {userId}");

            User user = _repositoryWrapper.User.FindFirstByCondition(u => u.IsActive && u.Id == userId);
            if (user == null)
            {
                _logger.LogError($"User with ID {userId} not found.");
                throw new NotFoundCustomException("User Not Found", $"User with ID {userId} not found.");
            }

            Person person = _repositoryWrapper.Person.FindFirstByCondition(p => p.IsActive && p.Id == user.PersonId);
            if (person == null)
            {
                _logger.LogError($"Person with ID {user.PersonId} not found.");
                throw new NotFoundCustomException("Person Not Found", $"Person with ID {user.PersonId} not found.");
            }
            if (personDto.Name != null)
                person.Name = personDto.Name;
            if (personDto.Address != null)
                person.Address = personDto.Address;
            if (personDto.MobilePhone != null)
                person.MobilePhone = personDto.MobilePhone;
            if (personDto.Country != null)
                person.Country = personDto.Country;
            if (personDto.Department != null)
                person.Department = personDto.Department;
            if (personDto.Designation != null)
                person.Designation = personDto.Designation;
            if (personDto.EmployeeId != null)
                person.EmployeeId = personDto.EmployeeId;
            if (personDto.AribaUserName != null)
                person.AribaUserName = personDto.AribaUserName;
            if (personDto.AribaPassword != null)
                person.AribaPassword = personDto.AribaPassword;
            if (personDto.orgANId != null)
                person.orgANId = personDto.orgANId;

            _repositoryWrapper.Person.Update(person);
            _repositoryWrapper.Save();
            if (personDto.ModelIds != null)
            {

                var existing = _repositoryWrapper.UserModelMapping
                    .FindByCondition(x => x.UserId == userId)
                    .ToList();

                List<UserModelMapping> updateList = new List<UserModelMapping>();
                List<UserModelMapping> insertList = new List<UserModelMapping>();


                foreach (var modelId in personDto.ModelIds)
                {

                    var found = existing.FirstOrDefault(x => x.ModelId == modelId);

                    if (found != null)
                    {

                        if (found.IsActive)
                        {
                            continue;
                        }

                        found.IsActive = true;
                        updateList.Add(found);
                        continue;
                    }


                    insertList.Add(new UserModelMapping
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        ModelId = modelId,
                        IsActive = true
                    });
                }
                foreach (var e in existing)
                {
                    if (!personDto.ModelIds.Contains(e.ModelId) && e.IsActive)
                    {
                        e.IsActive = false;
                        updateList.Add(e);
                    }
                }


                if (updateList.Any())
                {
                    _repositoryWrapper.UserModelMapping.UpdateRange(updateList);
                }

                if (insertList.Any())
                {
                    _repositoryWrapper.UserModelMapping.CreateRange(insertList);
                }
                _repositoryWrapper.Save();
            }

            if (personDto.AppIds != null)
            {
                var existing = _repositoryWrapper.AppUserMapping
                    .FindByCondition(x => x.UserId == userId)
                    .ToList();

                List<AppUserMapping> updateList = new List<AppUserMapping>();
                List<AppUserMapping> insertList = new List<AppUserMapping>();

                foreach (var appId in personDto.AppIds)
                {
                    var found = existing.FirstOrDefault(x => x.AppId == appId);

                    if (found != null)
                    {
                        if (found.IsActive)
                        {
                            continue;
                        }

                        found.IsActive = true;
                        updateList.Add(found);
                        continue;
                    }

                    insertList.Add(new AppUserMapping
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        AppId = appId,
                        IsActive = true
                    });
                }

                foreach (var e in existing)
                {
                    if (!personDto.AppIds.Contains(e.AppId) && e.IsActive)
                    {
                        e.IsActive = false;
                        updateList.Add(e);
                    }
                }

                if (updateList.Any())
                {
                    _repositoryWrapper.AppUserMapping.UpdateRange(updateList);
                }

                if (insertList.Any())
                {
                    _repositoryWrapper.AppUserMapping.CreateRange(insertList);
                }

                _repositoryWrapper.Save();
            }

            if (personDto.RequesterNames != null)
            {
                var existing = _repositoryWrapper.RequesterName
                    .FindByCondition(x => x.UserId == userId)
                    .ToList();

                List<RequesterName> updateList = new List<RequesterName>();
                List<RequesterName> insertList = new List<RequesterName>();

                foreach (var req in personDto.RequesterNames)
                {
                    var found = existing.FirstOrDefault(x =>
                        x.Name == req.Name &&
                        x.Department == req.Department);

                    if (found != null)
                    {
                        if (found.IsActive)
                        {
                            continue;
                        }

                        found.IsActive = true;
                        updateList.Add(found);
                        continue;
                    }

                    insertList.Add(new RequesterName
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        DomainId = person.DomainId,
                        Name = req.Name,
                        Department = req.Department,
                        IsActive = true
                    });
                }

                foreach (var e in existing)
                {
                    if (!personDto.RequesterNames.Any(x =>
                        x.Name == e.Name &&
                        x.Department == e.Department) && e.IsActive)
                    {
                        e.IsActive = false;
                        updateList.Add(e);
                    }
                }

                if (updateList.Any())
                {
                    _repositoryWrapper.RequesterName.UpdateRange(updateList);
                }

                if (insertList.Any())
                {
                    _repositoryWrapper.RequesterName.CreateRange(insertList);
                }




                _repositoryWrapper.Save();
            }

            _logger.LogInfo($"Successfully updated person with ID: {person.Id}");
        }


        /// <summary>
        /// Retrieves the profile of the logged-in person.
        /// </summary>
        /// <param name="userId">The User ID of the person.</param>
        /// <returns>MyProfileDto or null if not found</returns>
        public MyProfileDto GetMyProfile(Guid userId)
        {
            MyProfileDto? profileDto = _repositoryWrapper.Person.GetMyProfileByUserId(userId);
            if (profileDto == null)
                throw new NotFoundCustomException("Unable to find user", $"User with ID {userId} not found.");

            List<AssetDto> asset = _assetService.GetAssetDetail(_metadataService.GetRefTermIdByRefTermKey(Common.ENTITY_TYPE_USER), userId, null);
            profileDto.Asset = asset;


            User user = _repositoryWrapper.User.FindFirstByCondition(u => u.IsActive && u.Id == userId);
            if (user == null)
                throw new NotFoundCustomException("User Not Found", $"User with ID {userId} not found.");

            Person person = _repositoryWrapper.Person.FindFirstByCondition(p => p.IsActive && p.Id == user.PersonId);
            if (person == null)
                throw new NotFoundCustomException("Person Not Found", $"Person with ID {user.PersonId} not found.");
            Guid domainId = person.DomainId;

            DateTime utcNow = DateTime.UtcNow;
            DateTime monthStart = new DateTime(utcNow.Year, utcNow.Month, 1);
            DateTime monthEnd = monthStart.AddMonths(1);

            List<Ticket> exceededTickets = _repositoryWrapper.Ticket
                .FindByCondition(t =>
                    t.IsActive &&
                    t.DomainId == domainId &&
                    t.IsExceed &&
                    t.DateCreated >= monthStart &&
                    t.DateCreated < monthEnd)
                .ToList();

            double monthlyExceededHours = exceededTickets.Sum(t => t.TotalHours);

            profileDto.MonthlyExceededHours = monthlyExceededHours;

            return profileDto;
        }

        /// <summary>
        /// Removes a person from the system by their User ID.  
        /// </summary>
        /// <param name="id">The User ID of the person to remove.</param>
        public bool RemovePerson(Guid id)
        {
            _logger.LogInfo($"Removing person with User ID: {id}");
            User user = _repositoryWrapper.User.FindFirstByCondition(x => x.Id == id);
            if (user != null)
            {
                Person person = _repositoryWrapper.Person.FindFirstByCondition(x => x.Id == user.PersonId);
                _repositoryWrapper.User.Delete(user);
                _repositoryWrapper.Person.Delete(person);
                _repositoryWrapper.Save();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Fetches the list of Person Details for the Given Domain Id
        /// </summary>
        /// <param name="domainId">Domain Id</param>
        /// <returns></returns>
        public List<PersonPreviewDto> GetPersonDetialByDomainId(int index, int limit, Guid domainId, string? searchTerm)
        {
            _logger.LogInfo($"Fetching the List of Person Details for Domain: {domainId} with SearchTerm: {searchTerm}");

            List<PersonPreviewDto> result = _repositoryWrapper.Person
                .GetPersonDetailByDomainId(index, limit, domainId, searchTerm);

            if (!result.Any())
            {
                _logger.LogError($"No users found for Domain: {domainId} with SearchTerm: {searchTerm}");
                throw new NotFoundCustomException("No User is Available", $"No Person Details found for Domain: {domainId}");
            }

            _logger.LogInfo($"Successfully fetched Person Details for Domain: {domainId}");
            return result;
        }

        /// <summary>
        /// Checks for the Given Domain One Business Admin Exist or Not
        /// </summary>
        /// <param name="domainId"></param>
        /// <returns></returns>
        private bool CheckBusinessAdminExist(Guid domainId)
        {
            _logger.LogInfo($"Checking for Business Admin for the Given Domain : {domainId}");
            List<Person> person = _repositoryWrapper.Person.GetBusinessAdminByDomainId(domainId);
            if (person.Any())
            {
                return true;
            }
            return false;
        }

        public List<ConsultantTicketCountDto> GetConsultantsWithTicketCount(int index, int limit, string searchTerm)
        {
            _logger.LogInfo($"[START] Fetching consultants - Page: {index}, Limit: {limit}, Search: {searchTerm}");
            var data = _repositoryWrapper.Person.GetConsultantsWithTicketCount(index, limit, searchTerm);
            _logger.LogInfo($"[END] Returned {data.Count} consultants");
            return data;
        }


        public void CreateUserEmailNotification(Person person, string Password, string entityType, string emailKey)
        {
            _logger.LogInfo($"Creating user creation email notification for user: {person.Email}");

            EmailContent emailContent = _repositoryWrapper.EmailContent.FindFirstByCondition(x => x.IsActive && x.Key == emailKey);

            if (emailContent != null)
            {
                EmailPlaceholderDto emailPlaceholder = new EmailPlaceholderDto
                {
                    ToEmail = person.Email,
                    PersonName = person.Name,
                    EntityId = person.Id,
                    EmailType = emailKey,
                    EntityType = entityType,
                    EmailBody = emailContent.Body,
                    Subject = emailContent.Subject,
                    CompanyName = _configuration[Common.COMPANY_NAME],
                    SupportEmail = _configuration[Common.SUPPORT_EMAIL],
                    PortalLink = _configuration[Common.PORTAL_LINK],
                    CompanyLogo = _configuration[Common.COMPANY_LOGO_URL],
                    UserName = person.Name,
                    UserSecret = Password
                };

                emailPlaceholder.Subject = _emailService.ReplacePlaceholders(emailContent.Subject!, emailPlaceholder);
                emailPlaceholder.EmailBody = _emailService.ReplacePlaceholders(emailContent.Body!, emailPlaceholder);
                List<string>? ccEmail = _repositoryWrapper.EmailCCList.FindByCondition(x => x.IsActive && x.EntityType == entityType)
                                .Select(x => x.Email).ToList();
                if (_emailService.SendEmail(emailPlaceholder.ToEmail, emailPlaceholder.Subject, emailPlaceholder.EmailBody, emailPlaceholder.EmailType, emailPlaceholder.EntityType!, emailPlaceholder.EntityId, ccEmail))
                {
                    _emailService.CreateEmailSentDetails(emailPlaceholder);
                    _logger.LogInfo($"User creation email notification sent to: {person.Email}");
                }
            }
        }
        /// <summary>
        /// Get all model names from UserModelName table
        /// </summary>
        /// <returns>List of distinct model names</returns>
        public List<UserModelNameDto> GetAllModelNames()
        {
            _logger.LogInfo("[START] Fetching all model names from UserModelName table");

            var modelNames = _repositoryWrapper.UserModelName.GetAllModelNames();

            if (modelNames == null || !modelNames.Any())
            {
                _logger.LogError("No model names found in UserModelName table");
                throw new NotFoundCustomException(
                    "No model names found",
                    "No model names found"
                );
            }

            _logger.LogInfo($"[END] Retrieved {modelNames.Count} model names");

            return modelNames;
        }

        /// <summary>
        /// Get all model names created by a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of model names</returns>

        public List<UserModelNameDto> GetModelNamesByUserId(Guid userId)
        {
            _logger.LogInfo($"Fetching model names for user: {userId}");

            var modelNames = _repositoryWrapper.UserModelName.GetModelNamesByUserId(userId);

            if (modelNames == null || !modelNames.Any())
            {
                _logger.LogError($"No model names found for user: {userId}");
                throw new NotFoundCustomException(
                    $"No model names found for user: {userId}",
                    $"No model names found for user: {userId}"
                );
            }

            _logger.LogInfo($"Fetched {modelNames.Count} model names for user: {userId}");

            return modelNames;
        }

        /// <summary>
        /// Deactivate a person by their User ID. This will prevent the person from logging in or accessing the system.
        /// </summary> 
        /// <param name="userId">The User ID of the person to deactivate.</param>
        public void DeactivatePerson(Guid userId)
        {
            _logger.LogInfo($"Deactivating person with User ID: {userId}");
            User? user = _repositoryWrapper.User.FindFirstByCondition(x => x.IsActive && x.Id == userId);
            if (user == null)
            {
                _logger.LogError($"User not found with ID: {userId}");
                throw new NotFoundCustomException("User not found", $"User not found with ID: {userId}");
            }

            Person? person = _repositoryWrapper.Person.FindFirstByCondition(x => x.IsActive && x.Id == user.PersonId);

            if (person == null)
            {
                _logger.LogError($"Person not found for User ID: {userId}");
                throw new NotFoundCustomException("Person not found", $"Person not found for User ID: {userId}");
            }
            person.IsActive = false;
            user.IsActive = false;
            _repositoryWrapper.Person.Update(person);
            _repositoryWrapper.User.Update(user);
            _repositoryWrapper.Save();
            _logger.LogInfo($"Successfully deactivated person with User ID: {userId}");
        }
        /// <summary>
        /// Get all currency names from UserCurrencyName table
        /// </summary>
        /// <returns>List of distinct currency names</returns>
        public List<CurrencyNameDto> GetAllCurrencyNames()
        {
            _logger.LogInfo("[START] Fetching all currency names from UserCurrencyName table");

            var modelNames = _repositoryWrapper.AribaCurrency.GetAllCurrencyNames();

            if (modelNames == null || !modelNames.Any())
            {
                _logger.LogError("No currency names found in UserCurrencyName table");
                throw new NotFoundCustomException(
                    "No currency names found",
                    "No currency names found"
                );
            }

            _logger.LogInfo($"[END] Retrieved {modelNames.Count} currency names");

            return modelNames;
        }

        public List<AppNameDto> GetAllAppNames()
        {
            var apps = _repositoryWrapper.UserAppMapping
                .FindByCondition(x => x.IsActive)
                .Select(x => new AppNameDto
                {
                    Id = x.Id,
                    Key = x.Key,
                    Name = x.Name,
                    Description = x.Description,
                    Link = x.Link
                })
                .ToList();

            return apps;
        }

        public List<AppNameDto> GetAppNamesByUserId(Guid userId)
        {
            _logger.LogInfo($"Fetching app names for user: {userId}");
            List<AppNameDto> appNames = _repositoryWrapper.UserAppMapping.GetAppNamesByUserId(userId);
            if (appNames == null || !appNames.Any())
            {
                _logger.LogError($"No app names found for user: {userId}");
                throw new NotFoundCustomException($"No app names found for user",$"No app names found for user: {userId}");
            }
            _logger.LogInfo($"Fetched {appNames.Count} app names for user: {userId}");
            return appNames;
        }
        public void UpdateAppModelMapping(Guid domainId, List<Guid> appIds)
        {
            _logger.LogInfo($"Updating app model mapping for Domain ID: {domainId}");

            var existing = _repositoryWrapper.AppModelMapping
                .FindByCondition(x => x.DomainId == domainId)
                .ToList();

            List<AppModelMapping> updateList = new List<AppModelMapping>();
            List<AppModelMapping> insertList = new List<AppModelMapping>();

            foreach (var appId in appIds)
            {
                var found = existing.FirstOrDefault(x => x.AppId == appId);

                if (found != null)
                {
                    continue;
                }

                insertList.Add(new AppModelMapping
                {
                    Id = Guid.NewGuid(),
                    DomainId = domainId,
                    AppId = appId
                });
            }

            foreach (var e in existing)
            {
                if (!appIds.Contains(e.AppId))
                {
                    updateList.Add(e);
                }
            }

            if (updateList.Any())
            {
                _repositoryWrapper.AppModelMapping.DeleteRange(updateList);
            }

            if (insertList.Any())
            {
                _repositoryWrapper.AppModelMapping.CreateRange(insertList);
            }

            _repositoryWrapper.Save();

            _logger.LogInfo(
                $"Successfully updated app model mapping for Domain ID: {domainId}");
        }

        public List<AppNameDto> GetAppNamesByDomainId(Guid domainId)
        {
            return _repositoryWrapper.AppModelMapping.GetAppNamesByDomainId(domainId);
        }

      
    }
}

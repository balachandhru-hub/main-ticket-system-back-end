using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Contracts;
using Contracts.IRepository;
using Swashbuckle.AspNetCore.Annotations;
using Dto;
using TicketSystemAPI.Attributes;
using Contracts.IServices;
using System.ComponentModel.DataAnnotations;

namespace TicketSystemAPI.Controllers
{
    public class PersonController : BaseController
    {
        private readonly ILoggerManager _logger;
        private readonly IPersonService _personService;
        private readonly IRepositoryWrapper _repositoryWrapper;

        public PersonController(ILoggerManager logger, IPersonService personService, IRepositoryWrapper repositoryWrapper)
        {
            _logger = logger;
            _personService = personService;
            _repositoryWrapper = repositoryWrapper;
        }

        /// <summary>
        /// Creates a new person in the system.
        /// </summary>
        [HttpPost]
        [Route("api/v1/person")]
        [ValidateModelState]
        [SwaggerOperation("CreatePerson")]
        [SwaggerResponse(201, type: typeof(SuccessResponseDto), description: "Person created successfully")]
        [SwaggerResponse(400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Person Not Found")]
        public IActionResult CreatePerson([FromBody] PersonDto body)
        {
            _logger.LogInfo("Creating a new person.");
            Guid result = _personService.CreatePerson(body);
            return StatusCode(201, new SuccessResponseDto
            {
                StatusCode = 201,
                Id = result.ToString(),
                Message = "Person created successfully",
                Description = "Person created successfully",
            });
        }
        /// <summary>
        /// Updates the logged-in person's profile.
        /// </summary>
        [HttpPut]
        [Route("api/v1/updateperson")]
        [ValidateModelState]
        [ApiAuthorization(Name = "UPDATE_PERSON")]
        [SwaggerOperation("UpdatePerson")]
        [SwaggerResponse(statusCode: 200, type: typeof(SuccessResponseDto), description: "Person updated successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorResponseDto), description: "Person Not Found")]
        public IActionResult UpdatePerson([FromBody] UpdatePersonDto updateDto)
        {
            Guid userId = getLoggedUser();
            _personService.UpdatePerson(userId, updateDto);
            return Ok(new SuccessResponseDto
            {
                StatusCode = 200,
                Message = "Person updated successfully",
                Description = "Person Details Updated successfully",
            });
        }
        /// <summary>
        /// Updates a person's profile by user ID.
        /// </summary>
        [HttpPut]
        [Route("api/v1/updateperson/{userId}")]
        [ValidateModelState]
        [ApiAuthorization(Name = "UPDATE_PERSON_BY_ID")]
        [SwaggerOperation("UpdatePersonById")]
        [SwaggerResponse(statusCode: 200, type: typeof(SuccessResponseDto), description: "Person updated successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorResponseDto), description: "Person Not Found")]
        public IActionResult UpdatePersonById([FromRoute] Guid userId, [FromBody] UpdatePersonDto updateDto)
        {
            _personService.UpdatePerson(userId, updateDto);
            return Ok(new SuccessResponseDto
            {
                StatusCode = 200,
                Message = "Person updated successfully",
                Description = "Person Details Updated successfully",
            });
        }

        /// <summary>
        /// Retrieves the profile of the logged-in person.
        /// </summary>
        [HttpGet]
        [Route("api/v1/person/myprofile")]
        [ApiAuthorization(Name = "GET_MY_PROFILE")]
        [SwaggerOperation("GetMyProfile")]
        [SwaggerResponse(200, "Profile fetched successfully")]
        [SwaggerResponse(401, "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, "Profile Not Found")]
        public IActionResult GetMyProfile()
        {
            Guid userId = getLoggedUser();
            var profile = _personService.GetMyProfile(userId);
            return Ok(profile);
        }

        /// <summary>
        /// Retrieves the details of the logged-in person.      
        /// </summary>
        [HttpDelete]
        [Route("api/v1/person")]
        [SwaggerOperation("RemovePerson")]
        [SwaggerResponse(statusCode: 201, type: typeof(SuccessResponseDto), description: "Person delete successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorResponseDto), description: "Person Not Found")]
        public IActionResult RemovePerson()
        {
            Guid userId = getLoggedUser();
            _logger.LogInfo($"Remove person with User ID: {userId}");
            _personService.RemovePerson(userId);
            _logger.LogInfo($"Remove person with User ID: {userId} successfully");
            return Ok("Person Remove successfully");
        }

        /// <summary>
        /// Retrieves the details of person for the Given Domain.      
        /// </summary>
        [HttpGet]
        [Route("api/v1/person/domain/{domainId}")]
        [ApiAuthorization(Name = "GET_PERSON_DOMAIN")]
        [SwaggerOperation("GetPersonDetail")]
        [SwaggerResponse(200, type: typeof(SuccessResponseDto), description: "Fetching the List of Person for the Domain")]
        [SwaggerResponse(400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Person Not Found")]
        public IActionResult GetPersonDetail(
        [FromQuery][Required] int index,
        [FromQuery][Required] int limit,
        [FromRoute][Required] Guid domainId,
        [FromQuery] string? searchTerm)
        {
            _logger.LogInfo($"Fetching Person List for DomainId: {domainId}, SearchTerm: {searchTerm}");
            List<PersonPreviewDto> result = _personService.GetPersonDetialByDomainId(index, limit, domainId, searchTerm);
            _logger.LogInfo($"Fetched Person List for DomainId: {domainId}");
            return Ok(result);
        }


        /// <summary>
        /// Retrieves the list of consultants with their ticket counts.
        /// </summary>
        [HttpGet]
        [Route("api/v1/person/consultants")]
        [ApiAuthorization(Name = "GET_CONSULTANTS_WITH_TICKET_COUNT")]
        [SwaggerOperation("GetConsultantsWithTicketCount")]
        [SwaggerResponse(200, type: typeof(SuccessResponseDto), description: "Fetching the List of Consultants with Ticket Count")]
        [SwaggerResponse(400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Consultants Not Found")]
        public IActionResult GetConsultantsWithTicketCount(
            [FromQuery][Required] int index,
            [FromQuery][Required] int limit,
            [FromQuery] string searchTerm)
        {
            _logger.LogInfo($"Fetching consultants with ticket count - Page: {index}, Limit: {limit}, Search: {searchTerm}");
            List<ConsultantTicketCountDto> result = _personService.GetConsultantsWithTicketCount(index, limit, searchTerm);
            _logger.LogInfo($"Fetched {result.Count} consultants with ticket count");
            return Ok(result);
        }
        [HttpGet]
        [Route("api/v1/modelname/all")]
        [ApiAuthorization(Name = "GET_ALL_MODEL_NAMES")]
        [SwaggerOperation("GetAllModelNames")]
        [SwaggerResponse(200, type: typeof(List<UserModelNameDto>), description: "Model names fetched successfully")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "No model names found")]
        public IActionResult GetAllModelNames()
        {
            // Guid userId = getLoggedUser();
            var modelNames = _personService.GetAllModelNames();
            _logger.LogInfo($"End: Fetched {modelNames.Count} model names");
            return Ok(modelNames);


        }

        /// <summary>
        /// Get all model names created by the logged-in user
        /// </summary>
        [HttpGet]
        [Route("api/v1/models/model-names-by-user")]
        [ApiAuthorization(Name = "GET_MODEL_NAMES_BY_USER")]
        [SwaggerOperation("GetModelNamesByUser")]
        [SwaggerResponse(200, type: typeof(List<string>), description: "Model names for user fetched successfully")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public IActionResult GetModelNamesByUser()
        {
            _logger.LogDebug("Fetching Models for the given user");
            Guid userId = getLoggedUser();
            var modelNames = _personService.GetModelNamesByUserId(userId);
            return Ok(modelNames);
        }

        /// <summary>
        /// Get all app names assigned to the logged-in user
        /// </summary>
        [HttpGet]
        [Route("api/v1/apps/app-names-by-user")]
        [ApiAuthorization(Name = "GET_APP_NAMES_BY_USER")]
        [SwaggerOperation("GetAppNamesByUser")]
        [SwaggerResponse(200, type: typeof(List<AppNameDto>), description: "App names for user fetched successfully")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public IActionResult GetAppNamesByUser()
        {
            Guid userId = getLoggedUser();
            _logger.LogDebug($"Fetching app names for user: {userId}");
            var appNames = _personService.GetAppNamesByUserId(userId);
            return Ok(appNames);
        }

        /// <summary>
        /// De-Activates the given user by user ID.
        /// </summary>
        [HttpPut]
        [Route("api/v1/person/deactivate-user/{userId}")]
        [ApiAuthorization(Name = "DEACTIVATE_USER")]
        [SwaggerOperation("DeactivateUser")]
        [SwaggerResponse(201, type: typeof(SuccessResponseDto), description: "User deactivated successfully")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "User Not Found")]
        [SwaggerResponse(403, type: typeof(ErrorResponseDto), description: "Forbidden - User does not have permission to deactivate users.")]
        public IActionResult DeactivateUser([FromRoute] Guid userId)
        {
            _logger.LogDebug($"Attempting to deactivate person with ID: {userId}");
            _personService.DeactivatePerson(userId);
            _logger.LogDebug($"Successfully deactivated person with ID: {userId}");
            return Ok(new SuccessResponseDto { Id = userId.ToString(), Message = "User deactivated successfully", Description = $"User with ID {userId} has been deactivated." });
        }

        [HttpGet]
        [Route("api/v1/currencyname/all")]
        [ApiAuthorization(Name = "GET_ALL_CURRENCY_NAMES")]
        [SwaggerOperation("GetAllCurrencyNames")]
        [SwaggerResponse(200, type: typeof(List<CurrencyNameDto>), description: "Currency names fetched successfully")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "No currency names found")]
        public IActionResult GetAllCurrencyNames()
        {
            _logger.LogDebug("Fetching all currency names.");
            var currencyNames = _personService.GetAllCurrencyNames();
            _logger.LogDebug($"End: Fetched {currencyNames.Count} currency names");
            return Ok(currencyNames);
        }

        [HttpGet]
        [Route("api/v1/appname/all")]
        [ApiAuthorization(Name = "GET_ALL_APP_NAME")]
        [SwaggerOperation("GetAllAppNames")]
        [SwaggerResponse(200, type: typeof(List<AppNameDto>), description: "App names fetched successfully")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public IActionResult GetAllAppNames()
        {
            _logger.LogDebug("Fetching all app names.");
            var apps = _personService.GetAllAppNames();
            _logger.LogDebug($"Fetched {apps.Count} app names");
            return Ok(apps);
        }
        [HttpPut]
        [Route("api/v1/appmodelmapping/update")]
        [ApiAuthorization(Name = "UPDATE_APP_MODEL_MAPPING")]
        [SwaggerOperation("UpdateAppModelMapping")]
        [SwaggerResponse(200, type: typeof(bool), description: "App model mapping updated successfully")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public IActionResult UpdateAppModelMapping(UpdateAppModelMappingDto request)
        {
            _logger.LogDebug($"Updating app model mapping for Domain ID: {request.DomainId}");

            _personService.UpdateAppModelMapping(
                request.DomainId,
                request.AppIds);

            _logger.LogDebug(
                $"App model mapping updated successfully for Domain ID: {request.DomainId}");

            return Ok(true);
        }
        [HttpGet]
        [Route("api/v1/appname/by-domain")]
        [ApiAuthorization(Name = "GET_APP_NAME_BY_DOMAIN")]
        [SwaggerOperation("GetAppNamesByDomain")]
        [SwaggerResponse(200, type: typeof(List<AppNameDto>), description: "App names fetched successfully")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public IActionResult GetAppNamesByDomain()
        {
            _logger.LogDebug("Fetching app names by domain.");

            var domainId = getUserDomainId();

            var apps = _personService.GetAppNamesByDomainId(domainId);

            _logger.LogDebug($"Fetched {apps.Count} app names for domain.");

            return Ok(apps);
        }

    }
}
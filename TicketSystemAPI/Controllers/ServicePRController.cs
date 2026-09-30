using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Contracts.IServices;
using Dto;
using Contracts;
using TicketSystemAPI.Attributes;
using TicketSystemAPI.Controllers;
using Contracts.IRepository;

namespace TicketSystemAPI.Controllers
{
    /// <summary>
    /// Controller for managing assets.
    /// </summary>
    [ApiController]
    public class ServicePRController : BaseController
    {
        private readonly ILoggerManager _logger;
        private readonly IAribaServicePRService _aribaServicePRService;
        private readonly IRepositoryWrapper _repoWrapper;


        /// <summary>
        /// Constructor for Metadata Controller.
        /// </summary>
        /// <param name="_metadataService">The asset service.</param>
        /// <param name="logger">The logger manager.</param>
        public ServicePRController(ILoggerManager logger, IAribaServicePRService aribaServicePRService, IRepositoryWrapper repositoryWrapper)
        {
            _logger = logger;
            _aribaServicePRService = aribaServicePRService;
            _repoWrapper = repositoryWrapper;
        }
        [HttpPost]
        [Route("api/v1/ServicePR")]
        [ValidateModelState]
        [ApiAuthorization(Name = "CREATESERVICEPR")]
        [SwaggerOperation("SERVICEPR")]
        [SwaggerResponse(statusCode: 201, type: typeof(ServicePRDto), description: "Service PR  created successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]

         public async Task<IActionResult> PostServicePR([FromBody] ServicePRFormRequest request, [FromQuery] DateOnly? date)
        {
            _logger.LogDebug("Posting Service PR details to Ariba");

            Guid domainId = getUserDomainId();
            Guid userId = getLoggedUser();

            var profileDto = _repoWrapper.Person.GetMyProfileByUserId(userId);
            var PRRequests = request.PRRequests;
           var result= await _aribaServicePRService.PostMaterialPRToAriba(PRRequests,domainId,profileDto,userId, request.Asset,request.IsUpdate );

            _logger.LogDebug($"Posted Service PR Details SuccessFully");
           return Ok(new
            {
                StatusCode = 200,
                Message = request.IsUpdate ? "Service PR updated successfully" : "Service PR created successfully",
                Data = result   
            });
        }

        
    
           //Summary:
        //Get Service PR details by PR number
        //Summary:
        [HttpPost]
        [Route("api/v1/GetAllServicePR")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GETALLSERVICEPR")]
        [SwaggerOperation("GET_ALL_SERVICE_PR")]
        [SwaggerResponse(statusCode: 200, type: typeof(List<ServicePRDto>), description: "Service PR details retrieved successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> GetAllServicePRDetails([FromBody] ServicePRFilterRequestDto filterRequest = null)
        {
            _logger.LogDebug($"Retrieving Service PR details with filters");
            Guid domainId = getUserDomainId();
            Guid userId = getLoggedUser();

            var result = await _aribaServicePRService.GetServicePRsGroupedAsync(userId, domainId, filterRequest);

            return Ok(result);
        }

  
    }
}
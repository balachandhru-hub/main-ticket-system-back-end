using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Contracts.IServices;
using Dto;
using Contracts;
using TicketSystemAPI.Attributes;
using TicketSystemAPI.Controllers;
using Contracts.IRepository;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TicketSystemAPI.Controllers
{
    /// <summary>
    /// Controller for managing assets.
    /// </summary>
    [ApiController]
    public class PublicRelationServicePRController : BaseController
    {
        private readonly ILoggerManager _logger;
        private readonly IPublicRelationServicePRService _publicRelationServicePRService;
        private readonly IRepositoryWrapper _repoWrapper;


        /// <summary>
        /// Constructor for Metadata Controller.
        /// </summary>
        /// <param name="_metadataService">The asset service.</param>
        /// <param name="logger">The logger manager.</param>
        public PublicRelationServicePRController(ILoggerManager logger, IPublicRelationServicePRService publicRelationServicePRService, IRepositoryWrapper repositoryWrapper)
        {
            _logger = logger;
            _publicRelationServicePRService = publicRelationServicePRService;
            _repoWrapper = repositoryWrapper;
        }
        [HttpPost]
        [Route("api/v1/PublicRelationServicePR")]
        [ValidateModelState]
        [ApiAuthorization(Name = "CREATE_PUBLIC_RELATION_SERVICE_PR")]
        [SwaggerOperation("CREATE_PUBLIC_RELATION_SERVICE_PR")]
         [SwaggerResponse(statusCode: 201, type: typeof(MaterialPRDto), description: "Public Relation Service PR created successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> PostPublicRelationServicePR([FromBody] ServicePRFormRequest request, [FromQuery] DateOnly? date)
        {
            _logger.LogDebug("Posting Digital Quick PR details to Ariba");

            Guid domainId = getUserDomainId();
            Guid userId = getLoggedUser();

            var profileDto = _repoWrapper.Person.GetMyProfileByUserId(userId);
            var PRRequests = request.PRRequests;
           var result= await _publicRelationServicePRService.PostPublicQuickPRToAriba(PRRequests,domainId,profileDto,userId, request.Asset,request.IsUpdate );

            _logger.LogDebug($"Posted PO Details SuccessFully");
           return Ok(new
            {
                StatusCode = 200,
                Message = request.IsUpdate ? "Material PR updated successfully" : "Material PR created successfully",
                Data = result   
            });
        }

          
      
   
    }
}
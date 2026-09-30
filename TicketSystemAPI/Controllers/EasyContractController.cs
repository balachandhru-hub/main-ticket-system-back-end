using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Contracts;
using Contracts.IServices;
using Dto;
using Entities.Common;
using ExceptionHandler;
using TicketSystemAPI.Attributes;
using Entities.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using Contracts.IRepository;

namespace TicketSystemAPI.Controllers
{


    /// <summary>
    /// Controller for managing Ariba Invoice Uploads
    /// </summary>
    public class EasyContractController : BaseController
    {
        private readonly IContractExcelService _contractExcelService;
        private readonly ILoggerManager _logger;
        private readonly IRepositoryWrapper _repositoryWrapper;

        public EasyContractController(IContractExcelService contractExcelService, ILoggerManager logger, IRepositoryWrapper repositoryWrapper)
        {
            _contractExcelService = contractExcelService;
            _logger = logger;
            _repositoryWrapper = repositoryWrapper;
        }

        /// <summary>
        /// Get all venues for a domain
        /// </summary>
        [HttpGet]
        [Route("api/v1/ariba/GetAllContractDetails")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GET_CONTRACT_DETAILS")]
        [SwaggerOperation("GetAllContractDetails")]
        [SwaggerResponse(statusCode: 200, type: typeof(SuccessResponseDto), description: "File Upload successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorResponseDto), description: "File Not Found")]
        public async Task<ActionResult> GetVenueSupplier(
    [FromQuery] string type,
    [FromQuery] string? searchTerm,
    [FromQuery] int index = 0,
    [FromQuery] int limit = 10)
        {
            Guid domainId = getUserDomainId();

            var request = new GetVenueSupplierRequestDto
            {
                Type = type,
                SearchTerm = searchTerm,
                Index = index,
                Limit = limit
            };

            var result = await _contractExcelService.GetAllContractDetailsAsync(domainId, request);

            return Ok(result);

        }

        [HttpGet]
        [Route("api/v1/contract-suppliers")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GET_CONTRACT_SUPPLIERS")]
        [SwaggerOperation("GET CONTRACT SUPPLIERS")]
        [SwaggerResponse(statusCode: 200, type: typeof(SuccessResponseDto), description: "Contract suppliers retrieved successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> GetContractSuppliers([FromQuery] string? searchTerm, [FromQuery] int index = 0, [FromQuery] int limit = 10)
        {
            _logger.LogDebug($"Retrieving Contract Suppliers with search term: {searchTerm}");
            Guid domainId = getUserDomainId();

            var result = await _contractExcelService.GetContractSupplierNamesAsync(domainId, searchTerm, index, limit);
            _logger.LogDebug($"Retrieved Contract Suppliers with search term: {searchTerm}");

            return Ok(result);
        }

        [HttpGet]
        [Route("api/v1/contract-custom-types")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GET_CONTRACT_CUSTOM_TYPES")]
        [SwaggerOperation("GET CONTRACT CUSTOM TYPES")]
        [SwaggerResponse(statusCode: 200, type: typeof(SuccessResponseDto), description: "Contract custom types retrieved successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<ActionResult> GetCustomTypeNames(
   [FromQuery] string type,
   [FromQuery] string? searchTerm,
   [FromQuery] int index = 0,
   [FromQuery] int limit = 10)
        {
            _logger.LogDebug($"Retrieving Contract Custom Types with search term: {searchTerm}");
            Guid domainId = getUserDomainId();


            var request = new GetVenueSupplierRequestDto
            {
                Type = type,
                SearchTerm = searchTerm,
                Index = index,
                Limit = limit
            };

            var result = await _contractExcelService.GetCustomTypeNamesAsync(domainId, request);

            return Ok(result);

        }

        [HttpPost]
        [Route("api/v1/ariba/create/easy-contract")]
        [ValidateModelState]
        [ApiAuthorization(Name = "CREATE_EASY_CONTRACT")]
        [SwaggerOperation("CreateEasyContract")]
        [SwaggerResponse(200, type: typeof(SuccessResponseDto), description: "Easy contract created successfully")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Domain Not Found")]
        public async Task<IActionResult> PatchRequisition([FromBody][Required] ContractWorkspaceRequestDto requestDto)
        {
            Guid domainId = getUserDomainId();
            Guid userId = getLoggedUser();
            MyProfileDto aribauser = _repositoryWrapper.Person.GetMyProfileByUserId(userId)!;
            var result = await _contractExcelService.CreateEasyContract(domainId, requestDto, aribauser);
            if (!result.IsSuccess)
            {
                return BadRequest(result); 
            }
            return Ok(result);
        }
        [HttpPost]
[Route("api/v1/get-all-easy-contracts")]
[ValidateModelState]
[ApiAuthorization(Name = "GETALLEASYCONTRACTS")]
[SwaggerOperation("GET_ALL_EASY_CONTRACTS")]
[SwaggerResponse(statusCode: 200, type: typeof(List<EasyContractDto>), description: "Easy Contracts retrieved successfully")]
[SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
[SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized")]
public async Task<IActionResult> GetAllEasyContracts([FromBody] EasyContractFilterRequestDto filter)
{
    _logger.LogDebug("Retrieving Easy Contracts");

    Guid domainId = getUserDomainId();
    Guid userId = getLoggedUser();

    var result = await _contractExcelService.GetEasyContractsAsync(
        userId,
        domainId,
        filter?.Index ?? 0,
        filter?.Limit ?? 10,
        filter?.SearchTerm
    );

    return Ok(result);
}
        [HttpGet]
        [Route("api/v1/contractid")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GET_CONTRACT_ID")]
        [SwaggerOperation("GET CONTRACT ID")]
        [SwaggerResponse(statusCode: 200, type: typeof(SuccessResponseDto), description: "Contract Id retrieved successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        public async Task<IActionResult> GetContractIdName([FromQuery] string? searchTerm, [FromQuery] int index = 0, [FromQuery] int limit = 10)
        {
            _logger.LogDebug($"Retrieving Concierge Venue Names with search term: {searchTerm}");
            Guid domainId = getUserDomainId();

            var result = await _contractExcelService.GetCOntractId(domainId, searchTerm, index, limit);
            _logger.LogDebug($"Retrieved Concierge Venue Names with search term: {searchTerm}");

            return Ok(result);
        }
    }
}           

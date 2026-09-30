using Microsoft.AspNetCore.Mvc;
using Dto;
using Swashbuckle.AspNetCore.Annotations;
using Contracts.IServices;
using Contracts;
using TicketSystemAPI.Attributes;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.SignalR;

namespace TicketSystemAPI.Controllers
{
    public class TicketController : BaseController
    {
        private readonly ILoggerManager _logger;
        private readonly ITicketService _ticketService;
        private readonly IHubContext<TicketMessageHub> _hubContext;
        private readonly IAssetService _assetService;

        public TicketController(ILoggerManager logger, ITicketService ticketService, IHubContext<TicketMessageHub> hubContext, IAssetService assetService)
        {
            _logger = logger;
            _ticketService = ticketService;
            _hubContext = hubContext;
            _assetService = assetService;
        }

        /// <summary>
        /// Create a new ticket
        /// </summary>
        [HttpPost]
        [Route("api/v1/ticket")]
        [ValidateModelState]
        [ApiAuthorization(Name = "CREATE_TICKET_USER")]
        [SwaggerOperation("CreateTicket")]
        [SwaggerResponse(statusCode: 201, type: typeof(SuccessResponseDto), description: "Ticket create successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorResponseDto), description: "Ticket Not Found")]
        public IActionResult CreateTicket([FromBody] TicketRequestDto request)
        {
            _logger.LogInfo("Start: Creating a new ticket.");

            Guid userId = getLoggedUser();
            request.UserId = userId;
            request.IPAddress = GetIPAddress();

            Guid ticketId = _ticketService.CreateTicket(request);

            _logger.LogInfo($"End: Ticket created successfully with ID: {ticketId}");

            return StatusCode(201, new SuccessResponseDto
            {
                StatusCode = 201,
                Id = ticketId.ToString(),
                Message = "Ticket created successfully",
                Description = "Ticket has been created and stored."
            });
        }

        [HttpPost]
        [Route("api/v2/ticket")]
        [ValidateModelState]
        [ApiAuthorization(Name = "CREATE_TICKET_USER")]
        [SwaggerOperation("CreateTicketV2")]
        [SwaggerResponse(statusCode: 201, type: typeof(SuccessResponseDto), description: "Ticket created successfully (V2)")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorResponseDto), description: "Ticket Not Found")]
        public IActionResult CreateTicketV2([FromBody] TicketRequestV2Dto request)
        {
            _logger.LogInfo("Start: Creating a new ticket (V2).");

            Guid userId = getLoggedUser();
            request.UserId = userId;
            request.IPAddress = GetIPAddress();

            Guid ticketId = _ticketService.CreateTicketV2(request);

            _logger.LogInfo($"End: Ticket V2 created successfully with ID: {ticketId}");

            return StatusCode(201, new SuccessResponseDto
            {
                StatusCode = 201,
                Id = ticketId.ToString(),
                Message = "Ticket created successfully (V2)",
                Description = "Ticket has been created using the updated V2 structure."
            });
        }

        [HttpGet]
        [Route("api/v1/ticket/dashboard-summary")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GET_DASHBOARD_SUMMARY")]
        [SwaggerOperation("GetTicketDashboardSummary")]
        [SwaggerResponse(200, type: typeof(TicketDashboardSummaryDto), description: "Ticket dashboard summary")]
        [SwaggerResponse(401, "Unauthorized")]
        public IActionResult GetTicketDashboardSummary()
        {
            Guid userId = getLoggedUser();
            _logger.LogInfo("Start: Fetching ticket dashboard summary.");
            TicketDashboardSummaryDto summary = _ticketService.GetDashboardSummary(userId);
            _logger.LogInfo("End: Fetched ticket dashboard summary.");
            return Ok(summary);
        }


        /// <summary>
        /// Get all tickets for the logged-in user
        /// </summary>
        [HttpGet]
        [Route("api/v1/ticket")]
        [SwaggerOperation("GetAllTickets")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GET_ALL_TICKET_USER")]
        [SwaggerResponse(statusCode: 200, type: typeof(List<TicketDetailDto>), description: "List of all tickets")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorResponseDto), description: "Ticket Not Found")]
        public IActionResult GetAllTickets([FromQuery] TicketFilterDto filters)
        {
            Guid userId = getLoggedUser();
            _logger.LogInfo("Start: Fetching all tickets.");

            List<TicketDetailDto> tickets = _ticketService.GetAllTickets(userId, filters);

            _logger.LogInfo("End: Fetched all tickets.");
            return Ok(tickets);
        }



        /// <summary>
        /// Get tickets for the logged-in user with the given Id
        /// </summary>
        [HttpGet]
        [Route("api/v1/ticket/{id}")]
        [ValidateModelState]

        [SwaggerOperation("GetMyTickets")]
        [ApiAuthorization(Name = "GET_TICKET_USER")]
        [SwaggerResponse(statusCode: 200, type: typeof(TicketDetailDto), description: "Fetches the Ticket Details for the given Id")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorResponseDto), description: "Ticket Not Found")]
        public IActionResult GetMyTickets([FromRoute][Required] Guid id)
        {
            Guid userId = getLoggedUser();
            _logger.LogInfo($"Start: Fetching tickets for user: {userId}");
            TicketDetailDto result = _ticketService.GetTicketById(id);
            _logger.LogInfo($"End: Fetched tickets for user: {userId}");
            return Ok(result);
        }

        /// <summary>
        /// Updates the status of a ticket.         
        /// </summary>
        [HttpPut]
        [Route("api/v1/ticket")]
        [ValidateModelState]
        [ApiAuthorization(Name = "UPDATE_TICKET")]
        [SwaggerOperation("UpdateTicket")]
        [SwaggerResponse(statusCode: 200, type: typeof(SuccessResponseDto), description: "Ticket updated successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorResponseDto), description: "Ticket Not Found")]
        public IActionResult UpdateTicket([FromBody] TicketUpdateDto request)
        {
            _logger.LogInfo($"Start: Updating ticket with Id : {request.Id}");
            request.UserId = getLoggedUser();
            Guid result = _ticketService.UpdateTicket(request);
            _logger.LogInfo($"End: Ticket updated successfully for ticket with Id {request.Id}");
            return Ok(new SuccessResponseDto
            {
                StatusCode = 200,
                Id = result.ToString(),
                Message = "Ticket updated successfully",
                Description = "Ticket fields updated based on user role."
            });
        }

        /// <summary>
        /// Creates ticket from the ariba extention
        /// </summary>
        [HttpPost]
        [Route("api/v1/ticket-ariba")]
        [ValidateModelState]
        [ApiKeyAuthorization(Name = "CREATE_TICKET_USER_ARIBA")]
        [SwaggerOperation("CreateTicketAriba")]
        [SwaggerResponse(statusCode: 201, type: typeof(TicketDetailDto), description: "Fetches the Ticket Details for the given Id")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorResponseDto), description: "Ticket Not Found")]
        public IActionResult CreateTicketAriba([FromBody] TicketCreateAribaDto ticketCreateAribaDto)
        {
            _logger.LogInfo($"Start: Creating a new ticket for user with Email : {ticketCreateAribaDto.Email}.");
            ticketCreateAribaDto.IPAddress = GetIPAddress();
            Guid ticketId = _ticketService.CreateTicketAriba(ticketCreateAribaDto);

            _logger.LogInfo($"End: Ticket created successfully with ID: {ticketId}");

            return StatusCode(201, new SuccessResponseDto
            {
                StatusCode = 201,
                Id = ticketId.ToString(),
                Message = "Ticket created successfully",
                Description = "Ticket has been created and stored."
            });
        }

        /// <summary>
        /// Get tickets Messages and assets for the given ticket with Id
        /// </summary>
        [HttpGet]
        [Route("api/v1/ticket/message/{id}")]
        [ValidateModelState]
        [ApiAuthorization(Name = "GET_TICKET_MESSAGE_USER")]
        [SwaggerOperation("GetTicketMessage")]
        [SwaggerResponse(statusCode: 200, type: typeof(List<TicketMessageDto>), description: "Fetches the Ticket Message Details for the given Id")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorResponseDto), description: "Ticket Not Found")]
        public IActionResult GetTicketMessage([FromRoute][Required] Guid id, [FromQuery][Required] int index, [FromQuery][Required] int limit)
        {
            Guid userId = getLoggedUser();
            _logger.LogInfo($"Start: Fetching ticket Messages for given ticket with Id : {id} for user: {userId}");
            List<TicketMessageDto> result = _ticketService.GetTicketMessageById(id, index, limit);
            _logger.LogInfo($"End: Fetched ticket Messages for given ticket with Id : {id} for user: {userId}");
            return Ok(result);
        }

        [HttpPost]
        [Route("api/v1/ticket/ticket-PR")]
        [ValidateModelState]
        [ApiAuthorization(Name = "CREATE_TICKET_PR")]
        [SwaggerOperation("CreateTicketPR")]
        [SwaggerResponse(statusCode: 200, type: typeof(Guid), description: "Ticket PR created successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorResponseDto), description: "Ticket not found")]
        public IActionResult CreateTicketPR([FromBody][Required] List<TicketPRCreateDto> dto, [FromQuery][Required] Guid ticketId)
        {
            Guid userId = getLoggedUser();
            _logger.LogInfo($"Start: Creating Ticket PR for TicketId : {ticketId} by user: {userId}");

            Guid prId = _ticketService.CreateTicketPR(dto, ticketId);

            _logger.LogInfo($"End: Created Ticket PR with Id : {prId} for TicketId : {ticketId} by user: {userId}");

            return Ok(new SuccessResponseDto
            {
                StatusCode = 200,
                Id = prId.ToString(),
                Message = "Ticket PR created successfully",
                Description = $"Successfully created Ticket PR with Id : {prId} for TicketId : {ticketId}"
            });
        }


        /// <summary>
        /// Create Messages for the given ticket with Id
        /// </summary>
        [HttpPost]
        [Route("api/v1/ticket/message")]
        [ValidateModelState]
        [ApiAuthorization(Name = "CREATE_TICKET_MESSAGE_USER")]
        [SwaggerOperation("CreateTicketMessage")]
        [SwaggerResponse(statusCode: 201, type: typeof(SuccessResponseDto), description: "Created the Ticket Message for the given Id")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorResponseDto), description: "Ticket Not Found")]
        public async Task<IActionResult> CreateTicketMessageAsync([FromBody] TicketMessageRequestDto body)
        {
            Guid userId = getLoggedUser();
            _logger.LogInfo($"Start: Creating ticket Messages for given ticket with Id : {body.TicketId} for user: {userId}");
            Guid result = _ticketService.CreateTicketMessage(body);
            _logger.LogInfo($"Start: Created ticket Messages for given ticket with Id : {body.TicketId} for user: {userId}");
            List<TicketMessageDto> latestMessages = _ticketService.GetTicketMessageById(body.TicketId, 0, 1);
            await _hubContext.Clients.Group(body.TicketId.ToString())
                                     .SendAsync("ReceiveMessage", latestMessages);
            return StatusCode(201, new SuccessResponseDto { StatusCode = 201, Id = result.ToString(), Message = "Created the Ticket Message", Description = $"Successfully Created the Ticket Message with Id : {result}" });
        }

        /// <summary>
        /// Create Messages for the given ticket with Id
        /// </summary>
        [HttpPost]
        [Route("api/v1/ticket/asset")]
        [ValidateModelState]
        [ApiAuthorization(Name = "CREATE_TICKET_ASSET_USER")]
        [SwaggerOperation("CreateTicketAssetAsync")]
        [SwaggerResponse(statusCode: 201, type: typeof(SuccessResponseDto), description: "Created the Ticket Message for the given Id")]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorResponseDto), description: "Bad request")]
        [SwaggerResponse(statusCode: 401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(statusCode: 404, type: typeof(ErrorResponseDto), description: "Ticket Not Found")]
        public async Task<IActionResult> CreateTicketAssetAsync([FromBody] AssetUploadDto asset)
        {
            _logger.LogDebug($"Uploading documents for entity with Id : {asset.EntityId}");
            Guid result = _assetService.UploadDocument(asset);
            _logger.LogDebug($"Document uploaded successfully for the file with Id: {result}");
            List<TicketMessageDto> latestMessages = _ticketService.GetTicketMessageById(asset.EntityId, 0, 1);
            await _hubContext.Clients.Group(asset.EntityId.ToString())
                                     .SendAsync("ReceiveMessage", latestMessages);
            return StatusCode(201, new SuccessResponseDto { StatusCode = 201, Id = result.ToString(), Message = "Created the Ticket Message", Description = $"Successfully Created the Ticket Message with Id : {result}" });
        }

        [HttpGet]
        [Route("api/v1/ticket/details-excel")]
        [ApiAuthorization(Name = "DOWNLOAD_TICKET_EXCEL")]
        [SwaggerOperation("ExportTicketDetailsExcel")]
        [SwaggerResponse(200, type: typeof(File), description: "Ticket details downloaded successfully")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Ticket not found")]
        public IActionResult ExportTicketDetailsExcel([FromQuery] TicketFilterDto filters)
        {
            _logger.LogDebug("Starting Ticket Excel Export...");
            Guid userId = getLoggedUser();
            var result =  _ticketService.ExportTicketDetailsExcel(userId, filters);

            return File(result.FileContent,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                result.FileName);
        }

    }
}
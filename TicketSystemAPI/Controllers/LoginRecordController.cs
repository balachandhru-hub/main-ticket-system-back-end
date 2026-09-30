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
    public class LoginRecordController : BaseController
    {
        private readonly ILoginRecordService _loginRecordService;
        private readonly ILoggerManager _logger;

        public LoginRecordController(ILoginRecordService loginRecordService, ILoggerManager logger)
        {
            _loginRecordService = loginRecordService;
            _logger = logger;
        }

        [HttpGet]
        [Route("api/v1/loginrecords/log")]
        [ApiAuthorization(Name = "LOG_LOGIN")]
         [SwaggerOperation("ExportPendingApprovalsDetailsExcel")]
        [SwaggerResponse(200, type: typeof(List<AribaInvoiceDetailDto>), description: "login details downloaded  successfully")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Domain Not Found")]
      public async Task<IActionResult> DownloadLoginRecordsExcel([FromQuery] int limit = 100)
{
    
        var result = await _loginRecordService.ExportLoginRecordsExcelAsync(limit);

        return File(
            result.FileContent,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            result.FileName
        );
    
  
}
    }
}

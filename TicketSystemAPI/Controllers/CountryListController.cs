using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using Dto;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Entities.Common;
using Contracts.IRepository;
using Contracts.IServices;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Contracts;
using TicketSystemAPI.Attributes;
using System.Net.Mail;
using Services;
using Entities.Models;
using NPOI.POIFS.FileSystem;
using ExceptionHandler;
using NPOI.SS.Formula.Functions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Diagnostics.Contracts;


namespace TicketSystemAPI.Controllers
{

    public class CountryListController : BaseController
    {
        private readonly ICountryListService _countryListService;
        private readonly ILoggerManager _logger;

        public CountryListController(ICountryListService countryListService, ILoggerManager logger)
        {
            _countryListService = countryListService;
            _logger = logger;
        }

        [HttpGet]
        [Route("api/v1/countries")]
        [SwaggerResponse(200, type: typeof(List<CountryListDto>), description: "Countries fetched successfully")]
        [SwaggerResponse(401, type: typeof(ErrorResponseDto), description: "Unauthorized - JWT token is missing or invalid.")]
        [SwaggerResponse(404, type: typeof(ErrorResponseDto), description: "Domain Not Found")]  
        public async Task<IActionResult> GetCountries([FromQuery] string? search)
        {
            _logger.LogInfo("Fetching country list.");
                var countries = await _countryListService.GetCountriesAsync(search);
                return Ok(countries);
       
        }
    }
}
    
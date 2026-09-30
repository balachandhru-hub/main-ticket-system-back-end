using Contracts;
using Contracts.IRepository;
using Contracts.IServices;
using Dto;
using Newtonsoft.Json.Linq;
using Entities.Models;
using Newtonsoft.Json;
using Entities.Util;
using Microsoft.Extensions.Hosting;
using NPOI.SS.Formula.Functions;
using Quartz.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using Entities.Common;
using System.Net.Mail;
using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;
using ExceptionHandler;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace Services
{
    public class CountryListService : ICountryListService
    {
        private readonly IRepositoryWrapper _repoWrapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILoggerManager _logger;
       
      
        
        public CountryListService(IRepositoryWrapper repositoryWrapper, ILoggerManager logger)
        {
            _repoWrapper = repositoryWrapper;
            _logger = logger;
     

        }

   
        public async Task<List<CountryListDto>> GetCountriesAsync(string? search )
    {
        var countries = await _repoWrapper.CountryList.GetCountriesAsync();
            if (!string.IsNullOrWhiteSpace(search))
    {
        search = search.Trim().ToLower();

        countries = countries
            .Where(x =>
                (!string.IsNullOrEmpty(x.CountryName) &&
                 x.CountryName.ToLower().Contains(search))
                ||
                (!string.IsNullOrEmpty(x.CountryCode) &&
                 x.CountryCode.ToLower().Contains(search))
            )
            .ToList();
    }

        return countries.Select(x => new CountryListDto
        {
           
            CountryName = x.CountryName,
            CountryCode = x.CountryCode,
            MobileCountryCode = x.MobileCountryCode
        }).ToList();
    }
    }
}
using Dto;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.IServices
{
    public interface ICountryListService
    {
        Task<List<CountryListDto>> GetCountriesAsync(string? search);
    }
}
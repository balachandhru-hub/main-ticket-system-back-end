using Entities.Models;

namespace Contracts.IRepository
{
    public interface ICountryListRepository : IRepositoryBase<CountryList>
    {
        Task<List<CountryList>> GetCountriesAsync();
    }
}
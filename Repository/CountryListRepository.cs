using Contracts.IRepository;
using Entities.DbContext;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    public class CountryListRepository : RepositoryBase<CountryList>, ICountryListRepository
    {
        private readonly RepositoryContext _context;

        public CountryListRepository(RepositoryContext context) : base(context)
        {
            _context = context;
        }
            public async Task<List<CountryList>> GetCountriesAsync()
    {
        return await _context.CountryList
            .OrderBy(x => x.CountryName)
            .ToListAsync();
    }
    }
    
}
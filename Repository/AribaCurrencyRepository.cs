using Contracts.IRepository;
using Entities.Models;
using Entities.DbContext;
using Dto;
 
namespace Repository
{
public class AribaCurrencyRepository : RepositoryBase<AribaCurrency>, IAribaCurrencyRepository
{
    public AribaCurrencyRepository(RepositoryContext repositoryContext)
        : base(repositoryContext)
    {
    }
      /// <summary>
        /// Get all distinct currency names
        /// </summary>
        /// <returns>List of currency names</returns>
        public List<CurrencyNameDto> GetAllCurrencyNames()
{
   return FindByCondition(x => x.IsActive)
        .OrderBy(x => x.SortNumber) 
        .Select(x => new CurrencyNameDto
        {
            CurrencyName = x.CurrencyName,
   
        })
        .ToList();
}


 

}
}
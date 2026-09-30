using Entities.Models;
using Dto;

namespace Contracts.IRepository
{
    public interface IAribaCurrencyRepository : IRepositoryBase<AribaCurrency>
    {
        List<CurrencyNameDto> GetAllCurrencyNames();
    }
}

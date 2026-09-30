using Entities.Models;
using Dto;

namespace Contracts.IRepository
{
    public interface ILegalMaterialCodeRepository : IRepositoryBase<LegalMaterialCode>
    {
      Task<LegalMaterialCodeListDto?> GetLegalMaterialCodeDetailsByNameAsync(
    Guid domainId,
    string materialCode);
    }
}

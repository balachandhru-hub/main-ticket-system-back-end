using System.Threading.Tasks;
using Dto;

namespace Contracts.IServices
{
    public interface ILinkService
    {
        Task<LinkDto> GetLinkByTypeAsync(string type);
    }
}

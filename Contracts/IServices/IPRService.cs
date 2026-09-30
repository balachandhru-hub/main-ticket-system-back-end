using Dto;

namespace Contracts.IServices
{
    /// <summary>
    /// Interface IPRService
    /// </summary>
    public interface IPRService
    {
        Task<MaterialPRResultDto> UpdatePRDetailInAriba(PRUpdateDto prUpdateDto, Guid domainId, Guid userId);
        //Task<MaterialPRSoapEnvelope> PostPRToAriba(ServicePRDto prRequest);
    }
}
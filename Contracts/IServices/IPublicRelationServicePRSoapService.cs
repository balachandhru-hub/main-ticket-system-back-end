using System.Collections.Generic;
using Dto;
using System;
using Contracts.IServices;

namespace Contracts.IServices
{
    /// <summary>
    /// Interface IDigitalQuickPRSoapService
    /// </summary>
    public interface IPublicRelationServicePRSoapService
    {
        Task<MaterialPRSoapEnvelope> PostPublicServicesPRToAriba(ServicePRDto aribaReq);
    }
}
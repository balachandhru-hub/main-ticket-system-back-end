using System.Collections.Generic;
using Dto;
using System;
using Contracts.IServices;

namespace Contracts.IServices
{
    /// <summary>
    /// Interface IServicePRService
    /// </summary>
    public interface IServicesPRService
    {
        Task<MaterialPRSoapEnvelope> PostServicesPRToAriba(ServicePRDto prRequest);
        
    }
}
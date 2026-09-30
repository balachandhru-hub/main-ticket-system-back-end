using System.Collections.Generic;
using Dto;
using System;
using Contracts.IServices;

namespace Contracts.IServices
{
    /// <summary>
    /// Interface IDigitalQuickPRSoapService
    /// </summary>
    public interface IPublicRelationSoapService
    {
        Task<MaterialPRSoapEnvelope> PostDigitalQuickPRToAriba(MaterialPRDto prRequest);
    }
}
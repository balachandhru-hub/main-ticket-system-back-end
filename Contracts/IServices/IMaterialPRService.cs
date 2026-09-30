using System.Collections.Generic;
using Dto;
using System;
using Contracts.IServices;

namespace Contracts.IServices
{
    /// <summary>
    /// Interface IMaterialPRService
    /// </summary>
    public interface IMaterialPRService
    {
        Task<MaterialPRSoapEnvelope> PostMaterialPRToAriba(MaterialPRDto prRequest);
    }
}
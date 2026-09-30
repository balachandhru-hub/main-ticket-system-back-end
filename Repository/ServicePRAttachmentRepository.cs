using Contracts.IRepository;
using Dto;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Entities.Models;
using Entities.DbContext;
using Contracts;
using Entities.Common;
using Microsoft.Data.SqlClient;

namespace Repository
{

    /// <summary>
    /// Repository for Person entity, implementing IPersonRepository interface.
    /// </summary>
    public class  ServicePRAttachmentRepository : RepositoryBase<ServicePRAttachment>, IServicePRAttachmentRepository
    {
        private ILoggerManager _logger;
        private IConfiguration _configuration;




        public ServicePRAttachmentRepository(RepositoryContext repositoryContext, ILoggerManager logger, IConfiguration configuration)
            : base(repositoryContext)
        {
            _logger = logger;
            _configuration = configuration;

        }
    }
}
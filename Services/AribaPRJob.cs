using Quartz;
using Microsoft.Extensions.DependencyInjection;
using Contracts;
using Contracts.IRepository;
using Contracts.IServices;
using Dto;
using Entities.Models;
using Entities.Common;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Services
{
    [DisallowConcurrentExecution]
    public class AribaPRJob : IJob
    {
        private readonly ILoggerManager _logger;
        private readonly IServiceProvider _serviceProvider;
        private IRepositoryWrapper _repositoryWrapper;
        private readonly IPRPendingApprovableService _pRPendingApprovableService;

        public AribaPRJob(ILoggerManager logger, IServiceProvider serviceProvider, IRepositoryWrapper repositoryWrapper, IPRPendingApprovableService pRPendingApprovableService)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _repositoryWrapper = repositoryWrapper;
            _pRPendingApprovableService = pRPendingApprovableService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await _pRPendingApprovableService.ProcessAsync("");         
        }
    }
}

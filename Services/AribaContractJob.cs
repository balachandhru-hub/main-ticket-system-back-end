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
    public class AribaContractJob : IJob
    {
        private readonly ILoggerManager _logger;
        private readonly IServiceProvider _serviceProvider;
        private IRepositoryWrapper _repositoryWrapper;
        private readonly IContractPendingService _contractPendingService;

        public AribaContractJob(ILoggerManager logger, IServiceProvider serviceProvider, IRepositoryWrapper repositoryWrapper, IContractPendingService contractPendingService)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _repositoryWrapper = repositoryWrapper;
            _contractPendingService = contractPendingService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await _contractPendingService.ProcessAsync("");         
        }
    }
}

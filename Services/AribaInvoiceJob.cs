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
    public class AribaInvoiceJob : IJob
    {
        private readonly ILoggerManager _logger;
        private readonly IServiceProvider _serviceProvider;
        private IRepositoryWrapper _repositoryWrapper;
        private readonly IInvoicePendingApprovableService _invoicePendingApprovableService;

        public AribaInvoiceJob(ILoggerManager logger, IServiceProvider serviceProvider, IRepositoryWrapper repositoryWrapper, IInvoicePendingApprovableService invoicePendingApprovableService)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _repositoryWrapper = repositoryWrapper;
            _invoicePendingApprovableService = invoicePendingApprovableService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await _invoicePendingApprovableService.ProcessAsync("");         
        }
    }
}

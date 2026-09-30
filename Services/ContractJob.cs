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
    public class ContractJob : IJob
    {
        private readonly ILoggerManager _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IInvoicePendingApprovableService _invoicePendingApprovableService;

        public ContractJob(
            ILoggerManager logger,
            IServiceProvider serviceProvider,
            IRepositoryWrapper repositoryWrapper,
            IInvoicePendingApprovableService invoicePendingApprovableService)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _repositoryWrapper = repositoryWrapper;
            _invoicePendingApprovableService = invoicePendingApprovableService;
        }

        public Task Execute(IJobExecutionContext context)
        {

            TriggerContract();
            return Task.CompletedTask;
        }

        private void TriggerContract()
        {
            // _logger.LogInfo("ContractJob execution started...");

            using (var scope = _serviceProvider.CreateScope()){
                var aribaService = scope.ServiceProvider.GetRequiredService<IAribaService>();

                // 1. Hardcoded old DTO list
                var oldDtos = new List<ContractDto>
                {
                    new ContractDto
                    {
                        ContractId = "C123",
                        Title = "Laptop Purchase FY24",
                        Amount = 50000,
                        Description = "Purchase of 50 laptops",
                        Currency = "USD",
                        AmountAlreadyPaid = "20000",
                        BalanceAfterCurrentPayment = "30000",
                    },
                    new ContractDto
                    {
                        ContractId = "C124",
                        Title = "Desktop Purchase FY24",
                        Amount = 75000,
                        Description = "Purchase of 100 desktops",
                        Currency = "USD",
                        AmountAlreadyPaid = "25000",
                        BalanceAfterCurrentPayment = "50000",
                    }
                };

                // 2. Hardcoded new DTO list
                var newDtos = new List<ContractDto>
                {
                    new ContractDto
                    {
                        ContractId = "C123",
                        Title = "Laptop Purchase FY25",
                        Amount = 60000,
                        Description = "Purchase of 60 laptops",
                        Currency = "USD",
                        AmountAlreadyPaid = "25000",
                        BalanceAfterCurrentPayment = "35000",
                    },
                    new ContractDto
                    {
                        ContractId = "C124",
                        Title = "Desktop Purchase FY25",
                        Amount = 80000,
                        Description = "Purchase of 110 desktops",
                        Currency = "USD",
                        AmountAlreadyPaid = "30000",
                        BalanceAfterCurrentPayment = "50000",
                    }
                };

                // 3. Loop through both lists (assuming same order by ContractId)
                for (int i = 0; i < oldDtos.Count; i++)
                {
                    ContractDto newUpdate = newDtos.Where(n => n.ContractId == oldDtos[i].ContractId).FirstOrDefault();
                    newUpdate.Title= oldDtos[i].Title;
                    newUpdate.Description = oldDtos[i].Description;
                    newUpdate.Currency = oldDtos[i].Currency;
                }

                Console.WriteLine(JsonConvert.SerializeObject(newDtos));


                _logger.LogInfo("ContractJob execution completed successfully.");
            }
        }
    }
}

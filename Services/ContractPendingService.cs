using Contracts;
using Contracts.IRepository;
using Contracts.IServices;
using Dto;
using Entities.Common;
using Entities.DbContext;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Diagnostics.Contracts;

namespace Services
{
    public class ContractPendingService : IContractPendingService
    {
        private readonly IRepositoryWrapper _repoWrapper;
        private readonly ILoggerManager _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly IEmailService _emailService;

        public ContractPendingService(IRepositoryWrapper repoWrapper, ILoggerManager logger, IServiceProvider serviceProvider, IPurchaseOrderService purchaseOrderService, IEmailService emailService)
        {
            _repoWrapper = repoWrapper;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _emailService = emailService;

        }

        public async Task ProcessAsync(string TaskId = "")
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _repositoryWrapper = scope.ServiceProvider.GetRequiredService<IRepositoryWrapper>();
                var _contractService = scope.ServiceProvider.GetRequiredService<IContractService>();

                try
                {
                    _logger.LogInfo(" [ContractJob] Job started...");



                    var contractSettings = _repositoryWrapper.DomainSetting
                        .FindByCondition(ds => ds.SettingsKey == Common.DOMAIN_SETTINGS_CONTRACT_URL && ds.IsActive)
                        .ToList();

                    if (!contractSettings.Any() || contractSettings == null)
                    {
                        _logger.LogInfo("⚠️ No active Ariba contract settings found.");
                        return;
                    }

                    foreach (var setting in contractSettings)
                    {
                        DomainSetting? tokenConfigSetting = _repositoryWrapper.DomainSetting
                            .FindByCondition(t => t.DomainId == setting.DomainId && t.SettingsKey.Contains(Common.DOMAIN_SETTINGS_ARIBA_CONTRACT_TOKEN))
                            .FirstOrDefault();

                        if (tokenConfigSetting == null)
                        {
                            _logger.LogError($"[AribaContractDataScheduler] No Ariba token settings found for DomainId: {setting.DomainId}");
                            continue;
                        }

                        AribaPurchaseOrderSettings? tokenConfig = JsonConvert.DeserializeObject<AribaPurchaseOrderSettings>(tokenConfigSetting.SettingsValue);
                        AribaPurchaseOrderSettings? aribaContractJobSetting = JsonConvert.DeserializeObject<AribaPurchaseOrderSettings>(setting.SettingsValue ?? string.Empty);

                        if (tokenConfig != null && aribaContractJobSetting != null)
                        {
                            var domainId = setting.DomainId;

                            _logger.LogInfo("[AribaInvoiceDataScheduler] Successfully retrieved Ariba configuration settings.");
                            string token = await _contractService.GetAribaToken(tokenConfig.AribaAuthToken, tokenConfig.AribaTokenUrl!);

                            SchedularHistory? existingRecordInJobTable = _repositoryWrapper.SchedularHistory
                                .FindByCondition(t => t.SchedularKey == Common.SCHEDULAR_KEY_FETCH_ARIBA_CONTRACT && t.DomainId == setting.DomainId)
                                .AsNoTracking()
                                .FirstOrDefault();


                            var tempStartDate = DateTime.Parse(aribaContractJobSetting.StartDateForContractJob!);

                            if (existingRecordInJobTable != null && existingRecordInJobTable.EndDate.HasValue)
                            {
                                tempStartDate = existingRecordInJobTable.EndDate.Value;
                            }

                            var tempEndDate = DateTime.UtcNow;

                            var contracts = await _contractService.GetPendingContracts(
                                tempStartDate,
                                tempEndDate,
                                token,
                                aribaContractJobSetting.ApiKey,
                                aribaContractJobSetting.Realm,
                                aribaContractJobSetting.ContractPendingApprovablesUrl,
                                aribaContractJobSetting.ContractWorkspaceUrl,
                                domainId,
                                TaskId
                            );



                            BatchProcessingResultDto result = new BatchProcessingResultDto();
                            List<ContractDetails> newContract = new List<ContractDetails>();
                            List<ContractDetails> ContractToUpdate = new List<ContractDetails>();

                            IQueryable<ContractDetails> query = _repositoryWrapper.ContractDetails
                               .FindByCondition(x => x.IsActive && contracts.Select(t => t.ContractTaskId).Contains(x.ContractTaskId));


                            List<ContractDetails> existingContracts = await query.ToListAsync();

                            if (contracts != null && contracts.Any())
                            {
                                List<string> insertedapprovablehistories = new List<string>();
                                List<string> processedcontracts = new List<string>();
                                foreach (var dto in contracts)
                                {
                                    try
                                    {
                                        if (!IsContract(dto.Description))
                                        {
                                            _logger.LogInfo($"Skipping ContractDetails for non-contract task: {dto.ContractTaskId}");
                                            continue;
                                        }

                                        ContractDetails? existingContract = await _repositoryWrapper.ContractDetails.FindFirstByConditionAsync(x => x.ContractTaskId == dto.ContractTaskId);
                                        if (!insertedapprovablehistories.Contains(dto.ContractTaskId))
                                        {
                                            // Ensure ContractId is set in DTO
                                            if (existingContract != null)
                                            {
                                                existingContract.SupplierName = dto.SupplierName;
                                                existingContract.Description = dto.Description;
                                                existingContract.BeginDate = dto.BeginDate;
                                                existingContract.EndDate = dto.EndDate;
                                                existingContract.SupplierID = dto.SupplierID;
                                                existingContract.EndDate = dto.EndDate;
                                                existingContract.TotalAmount = dto.TotalAmount;
                                                existingContract.Currency = dto.Currency;
                                                existingContract.WorkspaceID = dto.WorkspaceID;
                                                existingContract.Department = dto.Department;
                                                existingContract.CompanyUniqueName = dto.CompanyUniqueName;
                                                // existingContract.AttachmentID= dto.AttachmentID;
                                                ContractToUpdate.Add(existingContract);

                                            }
                                            else
                                            {
                                                // Create new
                                                ContractDetails entity = new ContractDetails
                                                {
                                                    Id = Guid.NewGuid(),
                                                    DomainId = domainId,
                                                    ContractTaskId = dto.ContractTaskId,
                                                    ContractId = dto.ContractId,
                                                    BeginDate = dto.BeginDate,
                                                    EndDate = dto.EndDate,
                                                    TotalAmount = dto.TotalAmount,
                                                    Currency = dto.Currency,
                                                    WorkspaceID = dto.WorkspaceID,
                                                    Description = dto.Description,
                                                    SupplierName = dto.SupplierName,
                                                    Department = dto.Department,
                                                    // AttachmentID = dto.AttachmentID,
                                                    SupplierID = dto.SupplierID,

                                                    CompanyUniqueName = dto.CompanyUniqueName

                                                };
                                                newContract.Add(entity);

                                            }
                                        }
                                        insertedapprovablehistories.Add(dto.ContractTaskId);


                                        result.SuccessfulCount++;
                                    }
                                    catch (Exception ex)
                                    {
                                        result.FailedCount++;
                                        result.Errors.Add(new ErrorResponseDto
                                        {
                                            StatusCode = 500,
                                            Message = "Error processing Contract",
                                            Description = $"Error processing Contract {dto.ContractTaskId}: {ex.Message}"
                                        });
                                        _logger.LogError($"Error processing row {dto.ContractTaskId}: {ex.Message}");
                                    }
                                }
                            }

                            if (newContract.Any())
                            {
                                _logger.LogInfo($" Saving {newContract.Count} new Ariba contracts for domain {domainId}.");
                                await _repositoryWrapper.ContractDetails.CreateRangeAsync(newContract);


                                try
                                {
                                    var approvalData =
                                        from c in newContract
                                        join cp in _repositoryWrapper.ContractPending.FindByCondition(x => x.IsActive)
                                            on c.ContractTaskId equals cp.UniqueName
                                        join p in _repositoryWrapper.Person.FindByCondition(x => x.IsActive)
                                            on cp.Approver equals p.AribaUserName
                                        select new
                                        {
                                            Contract = c,
                                            Person = new Person
                                            {
                                                Email = p.Email,
                                                Name = p.Name,
                                                AribaUserName = p.AribaUserName
                                            },
                                            cp.Status
                                        };

                                    foreach (var Approver in approvalData)
                                    {
                                        _emailService.SendGatekeeperApprovalNotification(Approver.Person, Common.TYPE_CONTRACT, Approver.Status, null, null, Approver.Contract);

                                        _logger.LogInfo(
                                            $"Sent approval notification for contract {Approver.Contract.ContractTaskId} to {Approver.Person.Email}");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError($"Error sending approval notifications. Message: {ex.Message}, StackTrace: {ex.StackTrace}");
                                }


                            }

                            if (ContractToUpdate.Any())
                            {
                                _logger.LogInfo($" Updating {ContractToUpdate.Count} existing Ariba Contract for domain {domainId}.");
                                _repositoryWrapper.ContractDetails.UpdateRange(ContractToUpdate);
                            }



                            if (existingRecordInJobTable != null)
                            {
                                existingRecordInJobTable.StartDate = tempStartDate;
                                existingRecordInJobTable.EndDate = tempEndDate;
                                existingRecordInJobTable.DomainId = domainId;

                                _repositoryWrapper.SchedularHistory.Update(existingRecordInJobTable);
                            }
                            else
                            {
                                var newScheduleRecord = new SchedularHistory
                                {
                                    SchedularKey = Common.SCHEDULAR_KEY_FETCH_ARIBA_CONTRACT,
                                    StartDate = tempStartDate,
                                    EndDate = tempEndDate,
                                    DomainId = domainId
                                };
                                await _repositoryWrapper.SchedularHistory.CreateAsync(newScheduleRecord);
                            }

                            await _repositoryWrapper.SaveAsync();

                            _logger.LogInfo($" Saved {contracts?.Count ?? 0} Ariba contracts for domain {domainId}.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($" [AribaContractJob] Job failed: {ex.Message}");
                }
            }
        }
        public async Task ProcessContractApprovalsByUserIDAsync(string aribaUserName, string aribaPassword, Guid DomainId, string TaskId = "")
        {

            using (var scope = _serviceProvider.CreateScope())
            {
                var _repositoryWrapper = scope.ServiceProvider.GetRequiredService<IRepositoryWrapper>();
                var _contractService = scope.ServiceProvider.GetRequiredService<IContractService>();
                var _repositoryContext = scope.ServiceProvider.GetRequiredService<RepositoryContext>();

                try
                {
                    _logger.LogInfo(" [ContractJob] Job started...");



                    var contractSettings = _repositoryWrapper.DomainSetting
                        .FindByCondition(ds => ds.SettingsKey == Common.DOMAIN_SETTINGS_CONTRACT_URL && ds.IsActive && ds.DomainId == DomainId)
                        .ToList();

                    if (!contractSettings.Any() || contractSettings == null)
                    {
                        _logger.LogInfo("⚠️ No active Ariba contract settings found.");
                        return;
                    }

                    foreach (var setting in contractSettings)
                    {
                        DomainSetting? tokenConfigSetting = _repositoryWrapper.DomainSetting
                            .FindByCondition(t => t.DomainId == setting.DomainId && t.SettingsKey.Contains(Common.DOMAIN_SETTINGS_ARIBA_CONTRACT_TOKEN))
                            .FirstOrDefault();

                        if (tokenConfigSetting == null)
                        {
                            _logger.LogError($"[AribaContractDataScheduler] No Ariba token settings found for DomainId: {setting.DomainId}");
                            continue;
                        }

                        AribaPurchaseOrderSettings? tokenConfig = JsonConvert.DeserializeObject<AribaPurchaseOrderSettings>(tokenConfigSetting.SettingsValue);
                        AribaPurchaseOrderSettings? aribaContractJobSetting = JsonConvert.DeserializeObject<AribaPurchaseOrderSettings>(setting.SettingsValue ?? string.Empty);

                        if (tokenConfig != null && aribaContractJobSetting != null)
                        {
                            var domainId = setting.DomainId;

                            _logger.LogInfo("[AribaInvoiceDataScheduler] Successfully retrieved Ariba configuration settings.");
                            string token = await _contractService.GetAribaToken(tokenConfig.AribaAuthToken, tokenConfig.AribaTokenUrl!);

                            SchedularHistory? existingRecordInJobTable = _repositoryWrapper.SchedularHistory
                                .FindByCondition(t => t.SchedularKey == Common.SCHEDULAR_KEY_FETCH_ARIBA_CONTRACT && t.DomainId == setting.DomainId)
                                .AsNoTracking()
                                .FirstOrDefault();


                            var tempStartDate = DateTime.Parse(aribaContractJobSetting.StartDateForContractJob!);

                            if (existingRecordInJobTable != null && existingRecordInJobTable.EndDate.HasValue)
                            {
                                tempStartDate = existingRecordInJobTable.EndDate.Value;
                            }

                            var tempEndDate = DateTime.UtcNow;

                            var contracts = await _contractService.GetPendingContractsByUserId(
                                tempStartDate,
                                tempEndDate,
                                token,
                                aribaContractJobSetting.ApiKey,
                                aribaContractJobSetting.Realm,
                                aribaContractJobSetting.ContractPendingApprovablesUrl,
                                aribaContractJobSetting.ContractWorkspaceUrl,
                                aribaPassword,
                                aribaUserName,
                                domainId,
                                TaskId
                            );


                            var cleanupRepo = scope.ServiceProvider.GetRequiredService<IRepositoryWrapper>();
                            await CleanupPendingRecords(cleanupRepo, domainId, contracts, aribaUserName, _repositoryContext);


                            BatchProcessingResultDto result = new BatchProcessingResultDto();
                            List<ContractDetails> newContract = new List<ContractDetails>();
                            List<ContractDetails> ContractToUpdate = new List<ContractDetails>();

                            IQueryable<ContractDetails> query = _repositoryWrapper.ContractDetails
                               .FindByCondition(x => x.IsActive && contracts.Select(t => t.ContractTaskId).Contains(x.ContractTaskId));


                            List<ContractDetails> existingContracts = await query.ToListAsync();

                            if (contracts != null && contracts.Any())
                            {
                                List<string> insertedapprovablehistories = new List<string>();
                                List<string> processedcontracts = new List<string>();
                                foreach (var dto in contracts)
                                {
                                    try
                                    {
                                        if (!IsContract(dto.Description))
                                        {
                                            _logger.LogInfo($"Skipping ContractDetails for non-contract task: {dto.ContractTaskId}");
                                            continue;
                                        }

                                        ContractDetails? existingContract = await _repositoryWrapper.ContractDetails.FindFirstByConditionAsync(x => x.ContractTaskId == dto.ContractTaskId);
                                        if (!insertedapprovablehistories.Contains(dto.ContractTaskId))
                                        {
                                            // Ensure ContractId is set in DTO
                                            if (existingContract != null)
                                            {
                                                existingContract.SupplierName = dto.SupplierName;
                                                existingContract.Description = dto.Description;
                                                existingContract.BeginDate = dto.BeginDate;
                                                existingContract.EndDate = dto.EndDate;
                                                existingContract.SupplierID = dto.SupplierID;
                                                existingContract.EndDate = dto.EndDate;
                                                existingContract.TotalAmount = dto.TotalAmount;
                                                existingContract.Currency = dto.Currency;
                                                existingContract.WorkspaceID = dto.WorkspaceID;
                                                existingContract.Department = dto.Department;
                                                existingContract.CompanyUniqueName = dto.CompanyUniqueName;

                                                ContractToUpdate.Add(existingContract);

                                            }
                                            else
                                            {
                                                // Create new
                                                ContractDetails entity = new ContractDetails
                                                {
                                                    Id = Guid.NewGuid(),
                                                    DomainId = domainId,
                                                    ContractTaskId = dto.ContractTaskId,
                                                    ContractId = dto.ContractId,
                                                    BeginDate = dto.BeginDate,
                                                    EndDate = dto.EndDate,
                                                    TotalAmount = dto.TotalAmount,
                                                    Currency = dto.Currency,
                                                    WorkspaceID = dto.WorkspaceID,
                                                    Description = dto.Description,
                                                    SupplierName = dto.SupplierName,
                                                    Department = dto.Department,
                                                    SupplierID = dto.SupplierID,

                                                    CompanyUniqueName = dto.CompanyUniqueName

                                                };
                                                newContract.Add(entity);

                                            }
                                        }
                                        insertedapprovablehistories.Add(dto.ContractTaskId);


                                        result.SuccessfulCount++;
                                    }
                                    catch (Exception ex)
                                    {
                                        result.FailedCount++;
                                        result.Errors.Add(new ErrorResponseDto
                                        {
                                            StatusCode = 500,
                                            Message = "Error processing Contract",
                                            Description = $"Error processing Contract {dto.ContractTaskId}: {ex.Message}"
                                        });
                                        _logger.LogError($"Error processing row {dto.ContractTaskId}: {ex.Message}");
                                    }
                                }
                            }

                            if (newContract.Any())
                            {
                                _logger.LogInfo($" Saving {newContract.Count} new Ariba contracts for domain {domainId}.");
                                await _repositoryWrapper.ContractDetails.CreateRangeAsync(newContract);
                            }

                            if (ContractToUpdate.Any())
                            {
                                _logger.LogInfo($" Updating {ContractToUpdate.Count} existing Ariba Contract for domain {domainId}.");
                                _repositoryWrapper.ContractDetails.UpdateRange(ContractToUpdate);
                            }



                            if (existingRecordInJobTable != null)
                            {
                                existingRecordInJobTable.StartDate = tempStartDate;
                                existingRecordInJobTable.EndDate = tempEndDate;
                                existingRecordInJobTable.DomainId = domainId;

                                _repositoryWrapper.SchedularHistory.Update(existingRecordInJobTable);
                            }
                            else
                            {
                                var newScheduleRecord = new SchedularHistory
                                {
                                    SchedularKey = Common.SCHEDULAR_KEY_FETCH_ARIBA_CONTRACT,
                                    StartDate = tempStartDate,
                                    EndDate = tempEndDate,
                                    DomainId = domainId
                                };
                                await _repositoryWrapper.SchedularHistory.CreateAsync(newScheduleRecord);
                            }

                            await _repositoryWrapper.SaveAsync();

                            _logger.LogInfo($" Saved {contracts?.Count ?? 0} Ariba contracts for domain {domainId}.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($" [AribaContractJob] Job failed: {ex.Message}");
                }
            }
        }


        private static bool IsContract(string? text)
        {
            return !string.IsNullOrWhiteSpace(text) &&
                   text.Contains("contract", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Cleans up pending AribaTask records and orphaned AribaInvoice records
        /// </summary>
        private async Task CleanupPendingRecords(IRepositoryWrapper repositoryWrapper, Guid domainId, List<AribaContractDto> latestPendingContracts, string aribaUserName, RepositoryContext repositoryContext)
        {
            try
            {
                _logger.LogInfo($"[Cleanup] Starting cleanup for DomainId: {domainId}");

                // Step 1: Get all pending tasks for this domain
                var pendingTasks = await repositoryWrapper.ContractPending
                      .FindByCondition(t => t.IsActive &&
                                           t.DomainId == domainId &&
                                           t.Approver == aribaUserName &&
                                           t.Status == Common.STATUS_PENDING)
                      .AsNoTracking()
                      .ToListAsync();

                if (!pendingTasks.Any())
                {
                    _logger.LogInfo($"[Cleanup] No pending tasks found for DomainId: {domainId}");
                    return;
                }

                // Step 2: Get all current pending invoice IDs from Ariba
                var latestPendingTaskIds = latestPendingContracts?
                   .Where(i => i.Tasks != null)
                   .SelectMany(i => i.Tasks.Where(t => !string.IsNullOrEmpty(i.ContractTaskId) && !string.IsNullOrEmpty(t.Approver))
                       .Select(t => $"{i.ContractTaskId.Trim()}|{t.Approver.Trim().ToLower()}"))
                       .Distinct()
                       .ToHashSet() ?? new HashSet<string>();

                var tasksToClose = pendingTasks.Where(t => !latestPendingTaskIds.Contains($"{t.UniqueName.Trim()}|{t.Approver.Trim().ToLower()}")).ToList();

                List<ContractPending> closedTask = new List<ContractPending>();

                if (tasksToClose.Any())
                {
                    _logger.LogInfo($"[Cleanup] Found {tasksToClose.Count} stale pending tasks to mark as Closed.");

                    foreach (var task in tasksToClose)
                    {
                        task.Status = Common.CLOSED_STATUS_ARIBA;
                        closedTask.Add(task);
                    }
                }

                if (closedTask.Any())
                {
                    var ids = closedTask.Select(x => x.Id).ToHashSet();

                    var trackedEntries = repositoryContext.ChangeTracker
                        .Entries<ContractPending>()
                        .Where(e => ids.Contains(e.Entity.Id))
                        .ToList();

                    foreach (var entry in trackedEntries)
                    {
                        entry.State = EntityState.Detached;
                    }
                    repositoryWrapper.ContractPending.UpdateRange(closedTask);
                }

                await repositoryWrapper.SaveAsync();
                _logger.LogInfo($"[Cleanup] Cleanup completed successfully for DomainId: {domainId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[Cleanup] Error during cleanup for DomainId {domainId}: {ex.Message}");
                throw;
            }
        }

    }
}
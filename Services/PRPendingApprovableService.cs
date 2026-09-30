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
    public class PRPendingApprovableService : IPRPendingApprovableService
    {
        private readonly ILoggerManager _logger;
        private readonly IServiceProvider _serviceProvider;

        public PRPendingApprovableService(ILoggerManager logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;            
        }

        public async Task ProcessAsync(string TaskId = "")
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _repositoryWrapper = scope.ServiceProvider.GetRequiredService<IRepositoryWrapper>();
                var _aribaPRService = scope.ServiceProvider.GetRequiredService<IAribaPRService>();
                var _purchaseOrderService = scope.ServiceProvider.GetRequiredService<IPurchaseOrderService>();
                var _aribaService = scope.ServiceProvider.GetRequiredService<IAribaService>();
                var _emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                try
                {
                    _logger.LogInfo(" [AribaInvoiceJob] Job started...");



                    var invoiceSettings = _repositoryWrapper.DomainSetting
                        .FindByCondition(ds => ds.SettingsKey == Common.DOMAIN_SETTINGS_ARIBA_INVOICE && ds.IsActive)
                        .ToList();

                    if (invoiceSettings == null || !invoiceSettings.Any())
                    {
                        _logger.LogInfo("⚠️ No active Ariba invoice settings found.");
                        return;
                    }

                    foreach (var setting in invoiceSettings)
                    {
                        DomainSetting? tokenConfigSetting = _repositoryWrapper.DomainSetting
                            .FindByCondition(t => t.DomainId == setting.DomainId && t.SettingsKey.Contains(Common.DOMAIN_SETTINGS_ARIBA_TOKEN_INVOICE))
                            .FirstOrDefault();

                        if (tokenConfigSetting == null)
                        {
                            _logger.LogError($"[AribaInvoiceDataScheduler] No Ariba token settings found for DomainId: {setting.DomainId}");
                            continue;
                        }

                        AribaPurchaseOrderSettings? tokenConfig = JsonConvert.DeserializeObject<AribaPurchaseOrderSettings>(tokenConfigSetting.SettingsValue);
                        AribaPurchaseOrderSettings? aribaRequisitionJobSetting = JsonConvert.DeserializeObject<AribaPurchaseOrderSettings>(setting.SettingsValue ?? string.Empty);

                        if (tokenConfig != null && aribaRequisitionJobSetting != null)
                        {
                            var domainId = setting.DomainId;

                            _logger.LogInfo("[AribaInvoiceDataScheduler] Successfully retrieved Ariba configuration settings.");
                            string token = await _aribaService.GetValidToken(domainId, tokenConfig);

                            SchedularHistory? existingRecordInJobTable = _repositoryWrapper.SchedularHistory
                                .FindByCondition(t => t.SchedularKey == Common.SCHEDULAR_KEY_FETCH_ARIBA_REQUISITIONS && t.DomainId == setting.DomainId)
                                .AsNoTracking()
                                .FirstOrDefault();


                            var tempStartDate = DateTime.Parse(aribaRequisitionJobSetting.StartDateForInvoiceJob!);

                            if (existingRecordInJobTable != null && existingRecordInJobTable.EndDate.HasValue)
                            {
                                tempStartDate = existingRecordInJobTable.EndDate.Value;
                            }

                            var tempEndDate = DateTime.UtcNow;


                            var requisitions = await _aribaPRService.GetPendingApprovables(
                                tempStartDate,
                                tempEndDate,
                                token,
                                aribaRequisitionJobSetting.ApiKey,
                                aribaRequisitionJobSetting.Realm,
                                aribaRequisitionJobSetting.PendingApprovablesUrl,
                                aribaRequisitionJobSetting.RequisitionTaskUrl,
                                domainId,
                                TaskId
                            );

                           //using (var cleanupScope = _serviceProvider.CreateScope())
                           // {
                           //     var cleanupRepo = cleanupScope.ServiceProvider.GetRequiredService<IRepositoryWrapper>();
                           //     await CleanupPendingRecords(cleanupRepo, domainId, requisitions);
                           // }

                            
                           

                            BatchProcessingResultDto result = new BatchProcessingResultDto();
                            List<AribaPR> newRequisitions = new List<AribaPR>();
                            List<AribaPR> requisitionsToUpdate = new List<AribaPR>();

                            IQueryable<AribaPR> query = _repositoryWrapper.AribaPR
                               .FindByCondition(x => x.IsActive && requisitions.Select(t => t.RequisitionsId).Contains(x.RequisitionsId));


                            List<AribaPR> existingRequistions = await query.ToListAsync();

                            if (requisitions != null && requisitions.Any())
                            {
                                List<string> insertedapprovablehistories = new List<string>();
                                List<string> processedcontracts = new List<string>();
                                foreach (var dto in requisitions)
                                {
                                    try
                                    {

                                        _logger.LogInfo($"Trying to fetch ContractId for DomainId: {domainId}, RequisitionId: {dto.RequisitionsId}");
                                        InvoicePODto? poDetails = null;
                                        if (!string.IsNullOrEmpty(dto.RequisitionsId))
                                        {
                                            poDetails = _purchaseOrderService.GetPODetailsByRequisitionsId(domainId, dto.RequisitionsId);
                                        }


                                        AribaPR? existingPR = await _repositoryWrapper.AribaPR.FindFirstByConditionAsync(x => x.RequisitionsId == dto.RequisitionsId && x.AribaTaskId == dto.AribaTaskId);
                                        if (!insertedapprovablehistories.Contains(dto.RequisitionsId))
                                        {
                                            // Ensure ContractId is set in DTO
                                            if (existingPR != null)
                                            {
                                                existingPR.SupplierName = dto.SupplierName;
                                                existingPR.Amount = dto.Amount;
                                                existingPR.DueDate = dto.DueDate;
                                                existingPR.RequisitionsId = dto.RequisitionsId;
                                                existingPR.ContractId = poDetails?.ContractID;
                                                existingPR.PRTaxAmount = dto.PRTaxAmount;
                                             
                                                existingPR.CompanyUniqueName= dto.CompanyUniqueName;
                                                existingPR.NeedBYDate = dto.NeedBYDate;
                                                existingPR.RequesterName = dto.RequesterName;
                                                existingPR.Department = dto.Department;
                                                // existingPR.Description = dto.Description;
                                        
                                                existingPR.PRNetAmount = dto.PRNetAmount;

                                                requisitionsToUpdate.Add(existingPR);

                                            }
                                            else
                                            {
                                                // Create new
                                                AribaPR entity = new AribaPR
                                                {
                                                    Id = Guid.NewGuid(),
                                                    DomainId = domainId,
                                                    RequisitionsId = dto.RequisitionsId,

                                                    SupplierName = dto.SupplierName,
                                                    Amount = dto.Amount,
                                                    Currency = dto.Currency,
                                                    DueDate = dto.DueDate,
                                                    RequisitionsDate = dto.RequisitionsDate,
                                                 
                                                    CompanyUniqueName = dto.CompanyUniqueName,
                                                    AribaTaskId = dto.AribaTaskId,
                                                    ContractId = poDetails?.ContractID,
                                                    Department = dto.Department,
Description = dto.Description,  
                                                    PRTaxAmount = dto.PRTaxAmount,
                                                    RequesterName = dto.RequesterName,
                                                    NeedBYDate = dto.NeedBYDate,
                                                    PRNetAmount = dto.PRNetAmount


                                                };
                                                newRequisitions.Add(entity);

                                            }
                                        }
                                        insertedapprovablehistories.Add(dto.RequisitionsId);

                                        if (poDetails != null)
                                        {

                                            if (!processedcontracts.Contains(poDetails.ContractID))
                                            {
                                                _logger.LogInfo($"Updating the Contract Details for the Id : {poDetails.ContractID}");
                                                InvoiceContractDto? contractResult = await _aribaService.GetContractById(domainId, poDetails.ContractID!);

                                                AribaContract? aribaContract = await _repositoryWrapper.AribaContract.FindByConditionAsync(x => x.IsActive && x.DomainId == domainId && x.ContractId == poDetails.ContractID).FirstOrDefaultAsync();
                                                if (contractResult != null && aribaContract != null)
                                                {
                                                    aribaContract.ContractName = contractResult.ContractName;
                                                    _repositoryWrapper.AribaContract.Update(aribaContract);
                                                    await _repositoryWrapper.SaveAsync();
                                                    processedcontracts.Add(poDetails.ContractID);
                                                }
                                            }
                                        }
                                        result.SuccessfulCount++;
                                    }
                                    catch (Exception ex)
                                    {
                                        result.FailedCount++;
                                        result.Errors.Add(new ErrorResponseDto
                                        {
                                            StatusCode = 500,
                                            Message = "Error processing Invoice",
                                            Description = $"Error processing  Requisitions {dto.RequisitionsId}: {ex.Message}"
                                        });
                                        _logger.LogError($"Error processing row {dto.RequisitionsId}: {ex.Message}");
                                    }
                                }
                            }

                            if (newRequisitions.Any())
                            {
                                _logger.LogInfo($" Saving {newRequisitions.Count} new Ariba Requisitions for domain {domainId}.");
                                await _repositoryWrapper.AribaPR.CreateRangeAsync(newRequisitions);
try
{
    var approvalData =
        from pr in newRequisitions
        join task in _repositoryWrapper.AribaTask
            .FindByCondition(x => x.IsActive)
            on pr.RequisitionsId equals task.ApprovableUniqueName
        join p in _repositoryWrapper.Person
            .FindByCondition(x => x.IsActive)
            on task.Approver equals p.AribaUserName
        select new
        {
            PR = pr,
            Person = new Person
            {
                Email = p.Email,
                Name = p.Name,
                AribaUserName = p.AribaUserName
            },
            task.Status
        };

    foreach (var approver in approvalData)
    {
        _emailService.SendGatekeeperApprovalNotification(
            approver.Person,
            Common.TYPE_PR,
            approver.Status,
            null,
            approver.PR
        );

        _logger.LogInfo(
            $"Sent approval notification for PR {approver.PR.RequisitionsId} to {approver.Person.Email}");
    }
}
catch (Exception ex)
{
    _logger.LogError(
        $"Error sending approval notifications for new PRs. Message: {ex.Message}, StackTrace: {ex.StackTrace}");
}

                            }

                            if (requisitionsToUpdate.Any())
                            {
                                _logger.LogInfo($" Updating {requisitionsToUpdate.Count} existing Ariba Requisitions for domain {domainId}.");
                                _repositoryWrapper.AribaPR.UpdateRange(requisitionsToUpdate);
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
                                    SchedularKey = Common.SCHEDULAR_KEY_FETCH_ARIBA_REQUISITIONS,
                                    StartDate = tempStartDate,
                                    EndDate = tempEndDate,
                                    DomainId = domainId
                                };
                                await _repositoryWrapper.SchedularHistory.CreateAsync(newScheduleRecord);
                            }

                            await _repositoryWrapper.SaveAsync();

                            _logger.LogInfo($" Saved {requisitions?.Count ?? 0} Ariba Requisitions for domain {domainId}.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($" [AribaInvoiceJob] Job failed: {ex.Message}");
                }
            }
        }

        public async Task ProcessPRApprovalsByUserIDAsync(string aribaUserName, string aribaPassword, string DomainId, string TaskId = "")
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _repositoryWrapper = scope.ServiceProvider.GetRequiredService<IRepositoryWrapper>();
                var _aribaPRService = scope.ServiceProvider.GetRequiredService<IAribaPRService>();
                var domainId = new Guid(DomainId);
                var _repositoryContext = scope.ServiceProvider.GetRequiredService<RepositoryContext>();
                var _aribaService = scope.ServiceProvider.GetRequiredService<IAribaService>();
                var _purchaseOrderService = scope.ServiceProvider.GetRequiredService<IPurchaseOrderService>();
                try
                {
                    _logger.LogInfo(" [AribaInvoiceJob] Job started...");



                    var invoiceSettings = _repositoryWrapper.DomainSetting
                        .FindByCondition(ds => ds.SettingsKey == Common.DOMAIN_SETTINGS_ARIBA_INVOICE && ds.IsActive && ds.DomainId== domainId)
                        .ToList();

                    if (!invoiceSettings.Any() || invoiceSettings == null)
                    {
                        _logger.LogInfo("⚠️ No active Ariba invoice settings found.");
                        return;
                    }

                    foreach (var setting in invoiceSettings)
                    {
                        DomainSetting? tokenConfigSetting = _repositoryWrapper.DomainSetting
                            .FindByCondition(t => t.DomainId == setting.DomainId && t.SettingsKey.Contains(Common.DOMAIN_SETTINGS_ARIBA_TOKEN_INVOICE))
                            .FirstOrDefault();

                        if (tokenConfigSetting == null)
                        {
                            _logger.LogError($"[AribaInvoiceDataScheduler] No Ariba token settings found for DomainId: {setting.DomainId}");
                            continue;
                        }

                        AribaPurchaseOrderSettings? tokenConfig = JsonConvert.DeserializeObject<AribaPurchaseOrderSettings>(tokenConfigSetting.SettingsValue);
                        AribaPurchaseOrderSettings? aribaRequisitionJobSetting = JsonConvert.DeserializeObject<AribaPurchaseOrderSettings>(setting.SettingsValue ?? string.Empty);

                        if (tokenConfig != null && aribaRequisitionJobSetting != null)
                        {
                            //var domainId = setting.DomainId;

                            _logger.LogInfo("[AribaInvoiceDataScheduler] Successfully retrieved Ariba configuration settings.");
                            string token = await _aribaService.GetValidToken(domainId, tokenConfig);

                            SchedularHistory? existingRecordInJobTable = _repositoryWrapper.SchedularHistory
                                .FindByCondition(t => t.SchedularKey == Common.SCHEDULAR_KEY_FETCH_ARIBA_REQUISITIONS && t.DomainId == setting.DomainId)
                                .AsNoTracking()
                                .FirstOrDefault();


                            var tempStartDate = DateTime.Parse(aribaRequisitionJobSetting.StartDateForInvoiceJob!);

                            if (existingRecordInJobTable != null && existingRecordInJobTable.EndDate.HasValue)
                            {
                                tempStartDate = existingRecordInJobTable.EndDate.Value;
                            }

                            var tempEndDate = DateTime.UtcNow;


                            var requisitions = await _aribaPRService.GetPendingApprovablesByUserId(
                                tempStartDate,
                                tempEndDate,
                                token,
                                aribaRequisitionJobSetting.ApiKey,
                                aribaRequisitionJobSetting.Realm,
                                aribaRequisitionJobSetting.PendingApprovablesUrl,
                                aribaRequisitionJobSetting.RequisitionTaskUrl,
                                aribaUserName,
                                aribaPassword,
                                domainId,
                                TaskId
                            );


                            await CleanupPendingRecords(_repositoryWrapper, domainId, requisitions, aribaUserName, _repositoryContext);

                            BatchProcessingResultDto result = new BatchProcessingResultDto();
                            List<AribaPR> newRequisitions = new List<AribaPR>();
                            List<AribaPR> requisitionsToUpdate = new List<AribaPR>();

                            IQueryable<AribaPR> query = _repositoryWrapper.AribaPR
                               .FindByCondition(x => x.IsActive && requisitions.Select(t => t.RequisitionsId).Contains(x.RequisitionsId));


                            List<AribaPR> existingRequistions = await query.ToListAsync();

                            if (requisitions != null && requisitions.Any())
                            {
                                List<string> insertedapprovablehistories = new List<string>();
                                List<string> processedcontracts = new List<string>();
                                foreach (var dto in requisitions)
                                {
                                    try
                                    {

                                        _logger.LogInfo($"Trying to fetch ContractId for DomainId: {domainId}, RequisitionId: {dto.RequisitionsId}");
                                        InvoicePODto? poDetails = null;
                                        if (!string.IsNullOrEmpty(dto.RequisitionsId))
                                        {
                                            poDetails = _purchaseOrderService.GetPODetailsByRequisitionsId(domainId, dto.RequisitionsId);
                                        }


                                        AribaPR? existingPR = await _repositoryWrapper.AribaPR.FindFirstByConditionAsync(x => x.RequisitionsId == dto.RequisitionsId && x.AribaTaskId == dto.AribaTaskId);
                                        if (!insertedapprovablehistories.Contains(dto.RequisitionsId))
                                        {
                                            // Ensure ContractId is set in DTO
                                            if (existingPR != null)
                                            {
                                                existingPR.SupplierName = dto.SupplierName;
                                                existingPR.Amount = dto.Amount;
                                                existingPR.DueDate = dto.DueDate;
                                                existingPR.RequisitionsId = dto.RequisitionsId;
                                                existingPR.ContractId = poDetails?.ContractID;
                                                existingPR.PRTaxAmount = dto.PRTaxAmount;

                                                existingPR.CompanyUniqueName = dto.CompanyUniqueName;
                                                existingPR.NeedBYDate = dto.NeedBYDate;
                                                existingPR.RequesterName = dto.RequesterName;
                                                existingPR.Department = dto.Department;
                                                // existingPR.Description = dto.Description;

                                                existingPR.PRNetAmount = dto.PRNetAmount;

                                                requisitionsToUpdate.Add(existingPR);

                                            }
                                            else
                                            {
                                                // Create new
                                                AribaPR entity = new AribaPR
                                                {
                                                    Id = Guid.NewGuid(),
                                                    DomainId = domainId,
                                                    RequisitionsId = dto.RequisitionsId,

                                                    SupplierName = dto.SupplierName,
                                                    Amount = dto.Amount,
                                                    Currency = dto.Currency,
                                                    DueDate = dto.DueDate,
                                                    RequisitionsDate = dto.RequisitionsDate,

                                                    CompanyUniqueName = dto.CompanyUniqueName,
                                                    AribaTaskId = dto.AribaTaskId,
                                                    ContractId = poDetails?.ContractID,
                                                    Department = dto.Department,
                                                    Description = dto.Description,
                                                    PRTaxAmount = dto.PRTaxAmount,
                                                    RequesterName = dto.RequesterName,
                                                    NeedBYDate = dto.NeedBYDate,
                                                    PRNetAmount = dto.PRNetAmount


                                                };
                                                newRequisitions.Add(entity);

                                            }
                                        }
                                        insertedapprovablehistories.Add(dto.RequisitionsId);

                                        if (poDetails != null)
                                        {

                                            if (!processedcontracts.Contains(poDetails.ContractID))
                                            {
                                                _logger.LogInfo($"Updating the Contract Details for the Id : {poDetails.ContractID}");
                                                InvoiceContractDto? contractResult = await _aribaService.GetContractById(domainId, poDetails.ContractID!);

                                                AribaContract? aribaContract = await _repositoryWrapper.AribaContract.FindByConditionAsync(x => x.IsActive && x.DomainId == domainId && x.ContractId == poDetails.ContractID).FirstOrDefaultAsync();
                                                if (contractResult != null && aribaContract != null)
                                                {
                                                    aribaContract.ContractName = contractResult.ContractName;
                                                    _repositoryWrapper.AribaContract.Update(aribaContract);
                                                    await _repositoryWrapper.SaveAsync();
                                                    processedcontracts.Add(poDetails.ContractID);
                                                }
                                            }
                                        }
                                        result.SuccessfulCount++;
                                    }
                                    catch (Exception ex)
                                    {
                                        result.FailedCount++;
                                        result.Errors.Add(new ErrorResponseDto
                                        {
                                            StatusCode = 500,
                                            Message = "Error processing Invoice",
                                            Description = $"Error processing  Requisitions {dto.RequisitionsId}: {ex.Message}"
                                        });
                                        _logger.LogError($"Error processing row {dto.RequisitionsId}: {ex.Message}");
                                    }
                                }
                            }

                            if (newRequisitions.Any())
                            {
                                _logger.LogInfo($" Saving {newRequisitions.Count} new Ariba Requisitions for domain {domainId}.");
                                await _repositoryWrapper.AribaPR.CreateRangeAsync(newRequisitions);
                            }

                            if (requisitionsToUpdate.Any())
                            {
                                _logger.LogInfo($" Updating {requisitionsToUpdate.Count} existing Ariba Requisitions for domain {domainId}.");
                                _repositoryWrapper.AribaPR.UpdateRange(requisitionsToUpdate);
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
                                    SchedularKey = Common.SCHEDULAR_KEY_FETCH_ARIBA_REQUISITIONS,
                                    StartDate = tempStartDate,
                                    EndDate = tempEndDate,
                                    DomainId = domainId
                                };
                                await _repositoryWrapper.SchedularHistory.CreateAsync(newScheduleRecord);
                            }

                            await _repositoryWrapper.SaveAsync();

                            _logger.LogInfo($" Saved {requisitions?.Count ?? 0} Ariba Requisitions for domain {domainId}.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($" [AribaInvoiceJob] Job failed: {ex.Message}");
                }
            }
        }

        private async Task CleanupPendingRecords(IRepositoryWrapper repositoryWrapper, Guid domainId, List<AribaPRDto> latestPendingRequisitions, string aribaUserName, RepositoryContext repositoryContext)
        {
            try
            {
                _logger.LogInfo($"[Cleanup] Starting cleanup for DomainId: {domainId}");

                // Step 1: Get all pending tasks for this domain
              var pendingTasks = await repositoryWrapper.AribaPRTask
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
                var latestPendingKeys = latestPendingRequisitions
                    .Where(i => i.Tasks != null)
                    .SelectMany(i => i.Tasks.Where(t => !string.IsNullOrEmpty(i.RequisitionsId) && !string.IsNullOrEmpty(t.approver))
                        .Select(t => $"{i.RequisitionsId.Trim()}|{t.approver.Trim().ToLower()}"))
                        .Distinct()
                        .ToHashSet() ?? new HashSet<string>();

                // Step 3: Find tasks in DB that are pending but not in latest Ariba list
                var tasksToClose = pendingTasks.Where(t => !latestPendingKeys.Contains($"{t.ApprovableUniqueName.Trim()}|{t.Approver.Trim().ToLower()}")).ToList();

                List<AribaPRTasks> closedTask = new List<AribaPRTasks>();

                if (tasksToClose.Any())
                {
                    _logger.LogInfo($"[Cleanup] Found {tasksToClose.Count} stale pending tasks to mark as Closed.");

                    foreach (var tasks in tasksToClose)
                    {
                        tasks.Status = Common.CLOSED_STATUS_ARIBA;
                        closedTask.Add(tasks);
                    }
                }

                if (closedTask.Any())
                {
                    var ids = closedTask.Select(x => x.Id).ToHashSet();

                    var trackedEntries = repositoryContext.ChangeTracker
                        .Entries<AribaPRTasks>()
                        .Where(e => ids.Contains(e.Entity.Id))
                        .ToList();

                    foreach (var entry in trackedEntries)
                    {
                        entry.State = EntityState.Detached;
                    }

                    repositoryWrapper.AribaPRTask.UpdateRange(closedTask);
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
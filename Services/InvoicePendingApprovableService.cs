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
    public class InvoicePendingApprovableService : IInvoicePendingApprovableService
    {
        private readonly IRepositoryWrapper _repoWrapper;
        private readonly ILoggerManager _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly IEmailService _emailService;
        public InvoicePendingApprovableService(IRepositoryWrapper repoWrapper, ILoggerManager logger, IServiceProvider serviceProvider, IPurchaseOrderService purchaseOrderService, IEmailService emailService)
        {
            _repoWrapper = repoWrapper;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _purchaseOrderService = purchaseOrderService;
            _emailService = emailService;
        }

        public async Task ProcessAsync(string TaskId = "")
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _repositoryWrapper = scope.ServiceProvider.GetRequiredService<IRepositoryWrapper>();
                var _aribaService = scope.ServiceProvider.GetRequiredService<IAribaService>();

                try
                {
                    _logger.LogInfo(" [AribaInvoiceJob] Job started...");

                    var invoiceSettings = _repositoryWrapper.DomainSetting
                        .FindByCondition(ds => ds.SettingsKey == Common.DOMAIN_SETTINGS_ARIBA_INVOICE && ds.IsActive)
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
                        AribaPurchaseOrderSettings? aribaInvoiceJobSetting = JsonConvert.DeserializeObject<AribaPurchaseOrderSettings>(setting.SettingsValue ?? string.Empty);

                        if (tokenConfig != null && aribaInvoiceJobSetting != null)
                        {
                            var domainId = setting.DomainId;

                            _logger.LogInfo("[AribaInvoiceDataScheduler] Successfully retrieved Ariba configuration settings.");
                            string token = await _aribaService.GetValidToken(domainId, tokenConfig);

                            SchedularHistory? existingRecordInJobTable = _repositoryWrapper.SchedularHistory
                                .FindByCondition(t => t.SchedularKey == Common.SCHEDULAR_KEY_FETCH_ARIBA_INVOICES && t.DomainId == setting.DomainId)
                                .AsNoTracking()
                                .FirstOrDefault();


                            var tempStartDate = DateTime.Parse(aribaInvoiceJobSetting.StartDateForInvoiceJob!);

                            if (existingRecordInJobTable != null && existingRecordInJobTable.EndDate.HasValue)
                            {
                                tempStartDate = existingRecordInJobTable.EndDate.Value;
                            }

                            var tempEndDate = DateTime.UtcNow;


                            var invoices = await _aribaService.GetPendingApprovables(
                                tempStartDate,
                                tempEndDate,
                                token,
                                aribaInvoiceJobSetting.ApiKey,
                                aribaInvoiceJobSetting.Realm,
                                aribaInvoiceJobSetting.PendingApprovablesUrl,
                                aribaInvoiceJobSetting.RequisitionDetailsUrl,
                                domainId,
                                TaskId
                            );

                            //using (var cleanupScope = _serviceProvider.CreateScope())
                            //{
                            //    var cleanupRepo = cleanupScope.ServiceProvider.GetRequiredService<IRepositoryWrapper>();
                            //    await CleanupPendingRecords(cleanupRepo, domainId, invoices);
                            //}

                            BatchProcessingResultDto result = new BatchProcessingResultDto();
                            List<AribaInvoice> newInvoices = new List<AribaInvoice>();
                            List<AribaInvoice> invoicesToUpdate = new List<AribaInvoice>();

                            IQueryable<AribaInvoice> query = _repositoryWrapper.AribaInvoice
                               .FindByCondition(x => x.IsActive && invoices.Select(t => t.InvoiceId).Contains(x.InvoiceId));


                            List<AribaInvoice> existingInvoices = await query.ToListAsync();

                            if (invoices != null && invoices.Any())
                            {
                                List<string> insertedapprovablehistories = new List<string>();
                                List<string> processedcontracts = new List<string>();
                                foreach (var dto in invoices)
                                {
                                    try
                                    {

                                        _logger.LogInfo($"Trying to fetch ContractId for DomainId: {domainId}, PONumber: {dto.PONumber}");
                                        InvoicePODto? poDetails = null;
                                        if (!string.IsNullOrEmpty(dto.PONumber))
                                        {
                                            poDetails = _purchaseOrderService.GetPODetailsByPONumber(domainId, dto.PONumber);
                                        }


                                        AribaInvoice? existingInvoice = await _repositoryWrapper.AribaInvoice.FindFirstByConditionAsync(x => x.InvoiceId == dto.InvoiceId && x.AribaTaskId == dto.AribaTaskId);
                                        if (!insertedapprovablehistories.Contains(dto.InvoiceId))
                                        {
                                            // Ensure ContractId is set in DTO
                                            // string requesterName = null;
                                            // if (!string.IsNullOrEmpty(dto.requester))
                                            //     requesterName = dto.requester;
                                            if (existingInvoice != null)
                                            {
                                                existingInvoice.SupplierName = dto.SupplierName;
                                                existingInvoice.Amount = dto.Amount;
                                                existingInvoice.DueDate = dto.DueDate;
                                                existingInvoice.PONumber = dto.PONumber;
                                                existingInvoice.ContractId = poDetails?.ContractID;
                                                existingInvoice.InvoiceNetAmount = dto.InvoiceNetAmount;
                                                existingInvoice.InvoiceTaxAmount = dto.InvoiceTaxAmount;
                                                existingInvoice.RequesterUser = dto.RequesterName;
                                                
                                                // existingInvoice.Department = poDetails?.Department;
                                                //Don't update invoicedescription 
                                                invoicesToUpdate.Add(existingInvoice);
                                                // _repositoryWrapper.AribaInvoice.Update(existingInvoice);
                                                // await _repositoryWrapper.SaveAsync();
                                            }
                                            else
                                            {
                                                // Create new
                                                AribaInvoice entity = new AribaInvoice
                                                {
                                                    Id = Guid.NewGuid(),
                                                    DomainId = domainId,
                                                    InvoiceId = dto.InvoiceId,
                                                    InvoiceNumber = dto.InvoiceNumber,
                                                    SupplierName = dto.SupplierName,
                                                    Amount = dto.Amount,
                                                    Currency = dto.Currency,
                                                    DueDate = dto.DueDate,
                                                    InvoiceDate = dto.InvoiceDate,
                                                    Description = poDetails?.HeaderTittle,
                                                    CompanyUniqueName = dto.CompanyUniqueName,
                                                    PONumber = dto.PONumber,
                                                    AribaTaskId = dto.AribaTaskId,
                                                    ContractId = poDetails?.ContractID,
                                                    InvoiceNetAmount = dto.InvoiceNetAmount,
                                                    InvoiceTaxAmount = dto.InvoiceTaxAmount,
                                                    RequesterUser = dto.RequesterName
                                                    //  Department = poDetails?.Department
                                                };
                                                newInvoices.Add(entity);
                                                // await _repositoryWrapper.AribaInvoice.CreateAsync(entity);
                                                // await _repositoryWrapper.SaveAsync();
                                            }
                                        }
                                        insertedapprovablehistories.Add(dto.InvoiceId);

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
                                            Description = $"Error processing Invoice {dto.InvoiceNumber}: {ex.Message}"
                                        });
                                        _logger.LogError($"Error processing row {dto.InvoiceNumber}: {ex.Message}");
                                    }
                                }
                            }

                            if (newInvoices.Any())
                            {
                                _logger.LogInfo($" Saving {newInvoices.Count} new Ariba invoices for domain {domainId}.");
                                await _repositoryWrapper.AribaInvoice.CreateRangeAsync(newInvoices);
                            }
                          try
{
    var approvalData =
        from invoice in newInvoices
        join task in _repositoryWrapper.AribaTask
            .FindByCondition(x => x.IsActive)
            on invoice.InvoiceId equals task.ApprovableUniqueName
        join p in _repositoryWrapper.Person
            .FindByCondition(x => x.IsActive)
            on task.Approver equals p.AribaUserName
        select new
        {
            Invoice = invoice,
            Person = new Person
            {
                Email = p.Email,
                Name = p.Name,
                AribaUserName = p.AribaUserName
            },
            task.Status
        };

    foreach (var approver in approvalData){
        _emailService.SendGatekeeperApprovalNotification(approver.Person,Common.TYPE_INVOICE,approver.Status,approver.Invoice
        );

        _logger.LogInfo( $"Sent approval notification for invoice {approver.Invoice.InvoiceNumber} to {approver.Person.Email}");
                             }
                         }
                      catch (Exception ex)
                              {
                           _logger.LogError(
                  $"Error sending approval notifications for invoices. Message: {ex.Message}, StackTrace: {ex.StackTrace}");
                               }

                            if (invoicesToUpdate.Any())
                            {
                                _logger.LogInfo($" Updating {invoicesToUpdate.Count} existing Ariba invoices for domain {domainId}.");
                                _repositoryWrapper.AribaInvoice.UpdateRange(invoicesToUpdate);
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
                                    SchedularKey = Common.SCHEDULAR_KEY_FETCH_ARIBA_INVOICES,
                                    StartDate = tempStartDate,
                                    EndDate = tempEndDate,
                                    DomainId = domainId
                                };
                                await _repositoryWrapper.SchedularHistory.CreateAsync(newScheduleRecord);
                            }

                            await _repositoryWrapper.SaveAsync();

                            _logger.LogInfo($" Saved {invoices?.Count ?? 0} Ariba invoices for domain {domainId}.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($" [AribaInvoiceJob] Job failed: {ex.Message}");
                }
            }
        }

        public async Task ProcessInvoiceAsyncByUserId(string aribaUserName,string aribaPassword,string domainId, string TaskId = "")
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _repositoryWrapper = scope.ServiceProvider.GetRequiredService<IRepositoryWrapper>();
                var _aribaService = scope.ServiceProvider.GetRequiredService<IAribaService>();
                var domainGuid = new Guid(domainId);
                var _repositoryContext = scope.ServiceProvider.GetRequiredService<RepositoryContext>();
                try
                {
                    _logger.LogInfo(" [AribaInvoiceJob] Job started...");

                    var invoiceSettings = _repositoryWrapper.DomainSetting
                        .FindByCondition(ds => ds.SettingsKey == Common.DOMAIN_SETTINGS_ARIBA_INVOICE && ds.IsActive && ds.DomainId== domainGuid)
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
                        AribaPurchaseOrderSettings? aribaInvoiceJobSetting = JsonConvert.DeserializeObject<AribaPurchaseOrderSettings>(setting.SettingsValue ?? string.Empty);

                        if (tokenConfig != null && aribaInvoiceJobSetting != null)
                        {
                            //var domainsId = setting.DomainId;

                            _logger.LogInfo("[AribaInvoiceDataScheduler] Successfully retrieved Ariba configuration settings.");
                            string token = await _aribaService.GetValidToken(domainGuid, tokenConfig);

                            SchedularHistory? existingRecordInJobTable = _repositoryWrapper.SchedularHistory
                                .FindByCondition(t => t.SchedularKey == Common.SCHEDULAR_KEY_FETCH_ARIBA_INVOICES && t.DomainId == setting.DomainId)
                                .AsNoTracking()
                                .FirstOrDefault();


                            var tempStartDate = DateTime.Parse(aribaInvoiceJobSetting.StartDateForInvoiceJob!);

                            if (existingRecordInJobTable != null && existingRecordInJobTable.EndDate.HasValue)
                            {
                                tempStartDate = existingRecordInJobTable.EndDate.Value;
                            }

                            var tempEndDate = DateTime.UtcNow;


                            var invoices = await _aribaService.GetPendingApprovablesByUserId(
                                tempStartDate,
                                tempEndDate,
                                token,
                                aribaInvoiceJobSetting.ApiKey,
                                aribaInvoiceJobSetting.Realm,
                                aribaInvoiceJobSetting.PendingApprovablesUrl,
                                aribaInvoiceJobSetting.RequisitionDetailsUrl,
                                domainGuid,
                                TaskId,
                                aribaUserName,
                                aribaPassword
                            );

                           
                            var cleanupRepo = scope.ServiceProvider.GetRequiredService<IRepositoryWrapper>();
                            await CleanupPendingRecords(cleanupRepo, domainGuid, invoices,aribaUserName, _repositoryContext);

                            BatchProcessingResultDto result = new BatchProcessingResultDto();
                            List<AribaInvoice> newInvoices = new List<AribaInvoice>();
                            List<AribaInvoice> invoicesToUpdate = new List<AribaInvoice>();

                            IQueryable<AribaInvoice> query = _repositoryWrapper.AribaInvoice
                               .FindByCondition(x => x.IsActive && invoices.Select(t => t.InvoiceId).Contains(x.InvoiceId));


                            List<AribaInvoice> existingInvoices = await query.ToListAsync();

                            if (invoices != null && invoices.Any())
                            {
                                List<string> insertedapprovablehistories = new List<string>();
                                List<string> processedcontracts = new List<string>();
                                foreach (var dto in invoices)
                                {
                                    try
                                    {

                                        _logger.LogInfo($"Trying to fetch ContractId for DomainId: {domainId}, PONumber: {dto.PONumber}");
                                        InvoicePODto? poDetails = null;
                                        if (!string.IsNullOrEmpty(dto.PONumber))
                                        {
                                            poDetails = _purchaseOrderService.GetPODetailsByPONumber(domainGuid, dto.PONumber);
                                        }


                                        AribaInvoice? existingInvoice = await _repositoryWrapper.AribaInvoice.FindFirstByConditionAsync(x => x.InvoiceId == dto.InvoiceId && x.AribaTaskId == dto.AribaTaskId);
                                        if (!insertedapprovablehistories.Contains(dto.InvoiceId))
                                        {
                                            // Ensure ContractId is set in DTO
                                            // string requesterName = null;
                                            // if (!string.IsNullOrEmpty(dto.requester))
                                            //     requesterName = dto.requester;
                                            if (existingInvoice != null)
                                            {
                                                existingInvoice.SupplierName = dto.SupplierName;
                                                existingInvoice.Amount = dto.Amount;
                                                existingInvoice.DueDate = dto.DueDate;
                                                existingInvoice.PONumber = dto.PONumber;
                                                existingInvoice.ContractId = poDetails?.ContractID;
                                                existingInvoice.InvoiceNetAmount = dto.InvoiceNetAmount;
                                                existingInvoice.InvoiceTaxAmount = dto.InvoiceTaxAmount;
                                                existingInvoice.RequesterUser = dto.RequesterName;

                                                // existingInvoice.Department = poDetails?.Department;
                                                //Don't update invoicedescription 
                                                invoicesToUpdate.Add(existingInvoice);
                                                // _repositoryWrapper.AribaInvoice.Update(existingInvoice);
                                                // await _repositoryWrapper.SaveAsync();
                                            }
                                            else
                                            {
                                                // Create new
                                                AribaInvoice entity = new AribaInvoice
                                                {
                                                    Id = Guid.NewGuid(),
                                                    DomainId = domainGuid,
                                                    InvoiceId = dto.InvoiceId,
                                                    InvoiceNumber = dto.InvoiceNumber,
                                                    SupplierName = dto.SupplierName,
                                                    Amount = dto.Amount,
                                                    Currency = dto.Currency,
                                                    DueDate = dto.DueDate,
                                                    InvoiceDate = dto.InvoiceDate,
                                                    Description = poDetails?.HeaderTittle,
                                                    CompanyUniqueName = dto.CompanyUniqueName,
                                                    PONumber = dto.PONumber,
                                                    AribaTaskId = dto.AribaTaskId,
                                                    ContractId = poDetails?.ContractID,
                                                    InvoiceNetAmount = dto.InvoiceNetAmount,
                                                    InvoiceTaxAmount = dto.InvoiceTaxAmount,
                                                    RequesterUser = dto.RequesterName
                                                    //  Department = poDetails?.Department
                                                };
                                                newInvoices.Add(entity);
                                                // await _repositoryWrapper.AribaInvoice.CreateAsync(entity);
                                                // await _repositoryWrapper.SaveAsync();
                                            }
                                        }
                                        insertedapprovablehistories.Add(dto.InvoiceId);

                                        if (poDetails != null)
                                        {

                                            if (!processedcontracts.Contains(poDetails.ContractID))
                                            {
                                                _logger.LogInfo($"Updating the Contract Details for the Id : {poDetails.ContractID}");
                                                InvoiceContractDto? contractResult = await _aribaService.GetContractById(domainGuid, poDetails.ContractID!);

                                                AribaContract? aribaContract = await _repositoryWrapper.AribaContract.FindByConditionAsync(x => x.IsActive && x.DomainId == domainGuid && x.ContractId == poDetails.ContractID).FirstOrDefaultAsync();
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
                                            Description = $"Error processing Invoice {dto.InvoiceNumber}: {ex.Message}"
                                        });
                                        _logger.LogError($"Error processing row {dto.InvoiceNumber}: {ex.Message}");
                                    }
                                }
                            }

                            if (newInvoices.Any())
                            {
                                _logger.LogInfo($" Saving {newInvoices.Count} new Ariba invoices for domain {domainId}.");
                                await _repositoryWrapper.AribaInvoice.CreateRangeAsync(newInvoices);
                            }

                            if (invoicesToUpdate.Any())
                            {
                                _logger.LogInfo($" Updating {invoicesToUpdate.Count} existing Ariba invoices for domain {domainId}.");
                                _repositoryWrapper.AribaInvoice.UpdateRange(invoicesToUpdate);
                            }



                            if (existingRecordInJobTable != null)
                            {
                                existingRecordInJobTable.StartDate = tempStartDate;
                                existingRecordInJobTable.EndDate = tempEndDate;
                                existingRecordInJobTable.DomainId = domainGuid;

                                _repositoryWrapper.SchedularHistory.Update(existingRecordInJobTable);
                            }
                            else
                            {
                                var newScheduleRecord = new SchedularHistory
                                {
                                    SchedularKey = Common.SCHEDULAR_KEY_FETCH_ARIBA_INVOICES,
                                    StartDate = tempStartDate,
                                    EndDate = tempEndDate,
                                    DomainId = domainGuid
                                };
                                await _repositoryWrapper.SchedularHistory.CreateAsync(newScheduleRecord);
                            }

                            await _repositoryWrapper.SaveAsync();

                            _logger.LogInfo($" Saved {invoices?.Count ?? 0} Ariba invoices for domain {domainId}.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($" [AribaInvoiceJob] Job failed: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Cleans up pending AribaTask records and orphaned AribaInvoice records
        /// </summary>
        private async Task CleanupPendingRecords(IRepositoryWrapper repositoryWrapper, Guid domainId, List<AribaInvoiceDto> latestPendingInvoices, string aribaUserName, RepositoryContext repositoryContext)
        {
            try
            {
                _logger.LogInfo($"[Cleanup] Starting cleanup for DomainId: {domainId}");

                // Step 1: Get all pending tasks for this domain
              var pendingTasks = await repositoryWrapper.AribaTask
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
                var latestPendingKeys = latestPendingInvoices?
                    .Where(i => i.Tasks != null)
                    .SelectMany(i => i.Tasks.Where(t => !string.IsNullOrEmpty(i.InvoiceId) && !string.IsNullOrEmpty(t.approver))
                        .Select(t => $"{i.InvoiceId.Trim()}|{t.approver.Trim().ToLower()}"))
                        .Distinct()
                        .ToHashSet() ?? new HashSet<string>();
                
                var tasksToClose = pendingTasks.Where(t => !latestPendingKeys.Contains($"{t.ApprovableUniqueName.Trim()}|{t.Approver.Trim().ToLower()}")).ToList();

                List<AribaTasks> closedTask = new List<AribaTasks>();

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
                        .Entries<AribaTasks>()
                        .Where(e => ids.Contains(e.Entity.Id))
                        .ToList();

                    foreach (var entry in trackedEntries)
                    {
                        entry.State = EntityState.Detached;
                    }
                    repositoryWrapper.AribaTask.UpdateRange(closedTask);
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
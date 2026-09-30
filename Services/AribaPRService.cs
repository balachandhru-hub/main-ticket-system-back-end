using Contracts;
using Contracts.IRepository;
using Contracts.IServices;
using Dto;
using Newtonsoft.Json.Linq;
using Entities.Models;
using Newtonsoft.Json;
using Entities.Util;
using Microsoft.Extensions.Hosting;
using NPOI.SS.Formula.Functions;
using Quartz.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using Entities.Common;
using System.Net.Mail;
using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;
using ExceptionHandler;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace Services
{
    public class AribaPRService : IAribaPRService
    {
        private readonly IRepositoryWrapper _repoWrapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILoggerManager _logger;
        private readonly IDomainService _domainService;
        private readonly IAribaService _aribaService;


        /// <param name="repoWrapper">Repository wrapper.</param>
        /// <param name="logger">Logger manager.</param>

        public AribaPRService(IRepositoryWrapper repoWrapper, ILoggerManager logger, IDomainService domainService, IAribaService aribaService, IHttpContextAccessor httpContextAccessor)
        {
            _repoWrapper = repoWrapper;
            _logger = logger;
            _domainService = domainService;
            _aribaService = aribaService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<AribaPRDto>> GetPendingApprovables(
              DateTime fromDate,
              DateTime toDate,
              string token,
              string apiKey,
              string realm,
              string pendingApprovablesUrl,
              string RequisitionTaskUrl,
              Guid domainid,
              string TaskId)
        {
            _logger.LogInfo($"Fetching pending approvables for realm {realm} from {fromDate} to {toDate}.");

            // Build filter JSON
            string filterString = $"$filter=approvableType eq 'requisitions'";

            // Build final URL
            string url = pendingApprovablesUrl;
            if (!url.Contains("?"))
                url += "?";
            else if (!url.EndsWith("&"))
                url += "&";

            url += $"{filterString}&realm={realm}";

            _logger.LogInfo($"Calling Ariba PendingApprovables API: {url}");

            using var client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("apiKey", apiKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Ariba API error: {response.StatusCode} - {responseContent}");

            // Deserialize tasks
            List<AribaPRTask> tasks = new List<AribaPRTask>();
            JToken json = JsonConvert.DeserializeObject<JToken>(responseContent);

            if (json.Type == JTokenType.Object && json["value"] != null)
            {
                if (json["value"].Type == JTokenType.Array)
                    tasks = json["value"].ToObject<List<AribaPRTask>>();
                else
                    tasks.Add(json["value"].ToObject<AribaPRTask>());
            }
            else if (json.Type == JTokenType.Array)
            {
                tasks = json.ToObject<List<AribaPRTask>>();
            }

            if (tasks.Count == 0)
                return new List<AribaPRDto>();

            // --- REQUISITIONS ---
            tasks = tasks.Where(x => x.approvableType == "requisitions").ToList();
            if (TaskId != "")
                tasks = tasks.Where(x => x.approvableType == "requisitions" && x.approvableId == TaskId).ToList();

            List<RequisitionDto> requisitions = new List<RequisitionDto>();
            foreach (var t in tasks)
            {
                try
                {
                    AribaPRTasks taskexists = _repoWrapper.AribaPRTask.FindFirstByCondition(x =>
                        x.IsActive &&
                        x.ApprovableUniqueName == t.approvableUniqueName &&
                        x.Approver == t.approver &&
                        x.AssignedDate == t.assignedDate);

                    if (taskexists == null)
                    {
                        AribaPRTasks task = new AribaPRTasks();
                        task.Id = Guid.NewGuid();
                        task.ApprovalId = t.approvableId;
                        task.Approver = t.approver;
                        task.Status = Common.STATUS_PENDING;
                        task.Description = t.description;
                        task.AssignedDate = t.assignedDate;
                        task.Email = t.email;
                        task.DomainId = domainid;
                        task.FullUrl = t.fullUrl;
                        task.ApprovableType = t.approvableType;
                        task.ApprovableUniqueName = t.approvableUniqueName;
                        await _repoWrapper.AribaPRTask.CreateAsync(task);

                        var req = await GetRequisitionById(t.approvableId, token, apiKey, realm,
                               RequisitionTaskUrl.Replace("{aribaTaskId}", task.ApprovalId));
                        if (req != null)
                        {
                            requisitions.Add(req);

                            if (req.Attachments != null && req.Attachments.Count > 0)
                            {
                                List<AribaPRAttachment> attachments = new List<AribaPRAttachment>();
                                foreach (var att in req.Attachments)
                                {
                                    var exists = _repoWrapper.AribaPRAttachment.FindFirstByCondition(x =>
                                        x.IsActive &&
                                        x.AribaTaskId == t.approvableId &&
                                        x.RequisitionsId == t.approvableUniqueName &&
                                        x.AttachmentId == att.Id);

                                    if (exists == null)
                                    {
                                        attachments.Add(new AribaPRAttachment
                                        {
                                            Id = Guid.NewGuid(),
                                            AttachmentId = att.Id,
                                            RequisitionsId = t.approvableUniqueName,
                                            AribaTaskId = t.approvableId
                                        });
                                    }
                                }

                                if (attachments.Any())
                                    await _repoWrapper.AribaPRAttachment.CreateRangeAsync(attachments);
                            }
                        }


                        await _repoWrapper.SaveAsync();
                    }
                    else
                    {
                        taskexists.Status = Common.STATUS_PENDING;
                        taskexists.Description = t.description;
                        taskexists.AssignedDate = t.assignedDate;
                        taskexists.Email = t.email;
                        taskexists.ApprovableType = t.approvableType;
                        taskexists.ApprovableUniqueName = t.approvableUniqueName;
                        _repoWrapper.AribaPRTask.Update(taskexists);


                        var req = await GetRequisitionById(t.approvableId, token, apiKey, realm,
                            RequisitionTaskUrl.Replace("{aribaTaskId}", taskexists.ApprovalId));
                        if (req != null)
                        {
                            requisitions.Add(req);

                            if (req.Attachments != null)
                            {
                                List<AribaPRAttachment> newAtt = new List<AribaPRAttachment>();
                                List<AribaPRAttachment> updateAtt = new List<AribaPRAttachment>();

                                foreach (var a in req.Attachments)
                                {
                                    var ex = _repoWrapper.AribaPRAttachment.FindFirstByCondition(x =>
                                        x.IsActive &&
                                        x.AribaTaskId == t.approvableId &&
                                        x.RequisitionsId == t.approvableUniqueName &&
                                        x.AttachmentId == a.Id);

                                    if (ex != null)
                                    {
                                        updateAtt.Add(ex);
                                    }
                                    else
                                    {
                                        newAtt.Add(new AribaPRAttachment
                                        {
                                            Id = Guid.NewGuid(),
                                            AttachmentId = a.Id,
                                            RequisitionsId = t.approvableUniqueName,
                                            AribaTaskId = t.approvableId
                                        });
                                    }
                                }

                                if (newAtt.Any())
                                    await _repoWrapper.AribaPRAttachment.CreateRangeAsync(newAtt);

                                if (updateAtt.Any())
                                    _repoWrapper.AribaPRAttachment.UpdateRange(updateAtt);
                            }
                        }

                        await _repoWrapper.SaveAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to fetch requisition for approvableId={t.approvableId}: {ex.Message}");
                }
            }

            try
            {
                List<string> insertedapprovablehistories = new List<string>();
                if (requisitions.Count > 0)
                {
                    foreach (var a in requisitions)
                    {
                        if (!insertedapprovablehistories.Contains(a.ApprovableId))
                        {
                            var existingApprovalHistories = _repoWrapper.AribaPRHistory
                                .FindByCondition(x => x.IsActive && x.AribaTaskId == a.ApprovableId && x.RequisitionsId == a.UniqueName)
                                .AsNoTracking()
                                .ToList();

                            if (existingApprovalHistories.Any())
                            {
                                existingApprovalHistories.ForEach(x => x.IsActive = false);
                                _repoWrapper.AribaPRHistory.UpdateRange(existingApprovalHistories);
                            }

                            var approvalRequests = a.ApprovalRequests
                                .OrderBy(x => DateTimeOffset.FromUnixTimeMilliseconds(a.SubmitDate).UtcDateTime)

                                .ThenBy(x => a.SubmitDate)
                                .ToList();

                            var newApprovalHistories = new List<AribaPRHistory>();
                            int approvedLevelCounter = 0;

                            foreach (var approvalrequest in approvalRequests)
                            {
                                var newHistory = new AribaPRHistory
                                {
                                    Id = Guid.NewGuid(),
                                    RequisitionsId = a.UniqueName,
                                    AribaTaskId = a.ApprovableId,
                                    Comments = approvalrequest.ApproverComments,
                                    Status = approvalrequest.ApprovalState,
                                    IsActive = true
                                };

                                if (approvalrequest.ApprovalState == "Approved")
                                {
                                    newHistory.Status = Common.STATUS_ARIBA_APPROVE;
                                    newHistory.ApprovalDate = DateTimeOffset.FromUnixTimeMilliseconds(a.LastModified).UtcDateTime;
                                    approvedLevelCounter++;
                                    newHistory.Level = "Level " + approvedLevelCounter;
                                    newHistory.AribaUser = approvalrequest.Approver;


                                }
                                else if (approvalrequest.ApprovalState == "Ready for approval")
                                {
                                    newHistory.Status = Common.STATUS_PENDING;
                                    approvedLevelCounter++;
                                    newHistory.Level = "Level " + approvedLevelCounter;
                                    newHistory.AribaUser = approvalrequest.Approver;
                                }
                                else
                                {

                                    newHistory.AribaUser = approvalrequest.Approver;
                                }
                                insertedapprovablehistories.Add(a.ApprovableId);
                                newApprovalHistories.Add(newHistory);
                            }
                            await _repoWrapper.AribaPRHistory.CreateRangeAsync(newApprovalHistories);
                        }
                    }
                }
                await _repoWrapper.SaveAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to insert invoice history for : {ex.Message}");
            }

            return GroupTasksIntoRequisitions(tasks, requisitions);
        }

        public async Task<List<AribaPRDto>> GetPendingApprovablesByUserId(DateTime fromDate, DateTime toDate, string token, string apiKey, string realm, string pendingApprovablesUrl, string RequisitionTaskUrl, string aribaUserName, string aribaPassword, Guid domainid, string TaskId)
        {
            _logger.LogInfo($"Fetching pending approvables for realm {realm} from {fromDate} to {toDate}.");

            // Build filter JSON
            //string filterString = $"$filter=approvableType eq 'requisitions'";
            string filterString = $"$filter=user eq '{aribaUserName}' and passwordAdapter eq '{aribaPassword}' and approvableType eq 'requisitions'";

            // Build final URL
            string url = pendingApprovablesUrl;
            if (!url.Contains("?"))
                url += "?";
            else if (!url.EndsWith("&"))
                url += "&";

            url += $"{filterString}&realm={realm}";

            _logger.LogInfo($"Calling Ariba PendingApprovables API: {url}");

            using var client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("apiKey", apiKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Ariba API error: {response.StatusCode} - {responseContent}");

            // Deserialize tasks
            List<AribaPRTask> tasks = new List<AribaPRTask>();
            JToken json = JsonConvert.DeserializeObject<JToken>(responseContent);

            if (json.Type == JTokenType.Object && json["value"] != null)
            {
                if (json["value"].Type == JTokenType.Array)
                    tasks = json["value"].ToObject<List<AribaPRTask>>();
                else
                    tasks.Add(json["value"].ToObject<AribaPRTask>());
            }
            else if (json.Type == JTokenType.Array)
            {
                tasks = json.ToObject<List<AribaPRTask>>();
            }

            if (tasks.Count == 0)
                return new List<AribaPRDto>();

            // --- REQUISITIONS ---
            tasks = tasks.Where(x => x.approvableType == "requisitions").ToList();
            if (TaskId != "")
                tasks = tasks.Where(x => x.approvableType == "requisitions" && x.approvableId == TaskId).ToList();

            List<RequisitionDto> requisitions = new List<RequisitionDto>();
            foreach (var t in tasks)
            {
                try
                {
                    AribaPRTasks taskexists = _repoWrapper.AribaPRTask.FindFirstByCondition(x =>
                        x.IsActive &&
                        x.ApprovableUniqueName == t.approvableUniqueName &&
                        x.Approver == t.approver &&
                        x.AssignedDate == t.assignedDate);

                    if (taskexists == null)
                    {
                        AribaPRTasks task = new AribaPRTasks();
                        task.Id = Guid.NewGuid();
                        task.ApprovalId = t.approvableId;
                        task.Approver = t.approver;
                        task.Status = Common.STATUS_PENDING;
                        task.Description = t.description;
                        task.AssignedDate = t.assignedDate;
                        task.Email = t.email;
                        task.DomainId = domainid;
                        task.FullUrl = t.fullUrl;
                        task.ApprovableType = t.approvableType;
                        task.ApprovableUniqueName = t.approvableUniqueName;
                        await _repoWrapper.AribaPRTask.CreateAsync(task);

                        var req = await GetRequisitionById(t.approvableId, token, apiKey, realm,
                            RequisitionTaskUrl.Replace("{aribaTaskId}", task.ApprovalId));
                        if (req != null)
                        {
                            requisitions.Add(req);

                            if (req.Attachments != null && req.Attachments.Count > 0)
                            {
                                List<AribaPRAttachment> attachments = new List<AribaPRAttachment>();
                                foreach (var att in req.Attachments)
                                {
                                    var exists = _repoWrapper.AribaPRAttachment.FindFirstByCondition(x =>
                                        x.IsActive &&
                                        x.AribaTaskId == t.approvableId &&
                                        x.RequisitionsId == t.approvableUniqueName &&
                                        x.AttachmentId == att.Id);

                                    if (exists == null)
                                    {
                                        attachments.Add(new AribaPRAttachment
                                        {
                                            Id = Guid.NewGuid(),
                                            AttachmentId = att.Id,
                                            RequisitionsId = t.approvableUniqueName,
                                            AribaTaskId = t.approvableId
                                        });
                                    }
                                }

                                if (attachments.Any())
                                    await _repoWrapper.AribaPRAttachment.CreateRangeAsync(attachments);
                            }
                        }


                        await _repoWrapper.SaveAsync();
                    }
                    else
                    {
                        taskexists.Status = Common.STATUS_PENDING;
                        taskexists.Description = t.description;
                        taskexists.AssignedDate = t.assignedDate;
                        taskexists.Email = t.email;
                        taskexists.ApprovableType = t.approvableType;
                        taskexists.ApprovableUniqueName = t.approvableUniqueName;
                        _repoWrapper.AribaPRTask.Update(taskexists);



                        var req = await GetRequisitionById(t.approvableId, token, apiKey, realm,
                            RequisitionTaskUrl.Replace("{aribaTaskId}", taskexists.ApprovalId));
                        if (req != null)
                        {
                            requisitions.Add(req);

                            if (req.Attachments != null)
                            {
                                List<AribaPRAttachment> newAtt = new List<AribaPRAttachment>();
                                List<AribaPRAttachment> updateAtt = new List<AribaPRAttachment>();

                                foreach (var a in req.Attachments)
                                {
                                    var ex = _repoWrapper.AribaPRAttachment.FindFirstByCondition(x =>
                                        x.IsActive &&
                                        x.AribaTaskId == t.approvableId &&
                                        x.RequisitionsId == t.approvableUniqueName &&
                                        x.AttachmentId == a.Id);

                                    if (ex != null)
                                    {
                                        updateAtt.Add(ex);
                                    }
                                    else
                                    {
                                        newAtt.Add(new AribaPRAttachment
                                        {
                                            Id = Guid.NewGuid(),
                                            AttachmentId = a.Id,
                                            RequisitionsId = t.approvableUniqueName,
                                            AribaTaskId = t.approvableId
                                        });
                                    }
                                }

                                if (newAtt.Any())
                                    await _repoWrapper.AribaPRAttachment.CreateRangeAsync(newAtt);

                                if (updateAtt.Any())
                                    _repoWrapper.AribaPRAttachment.UpdateRange(updateAtt);
                            }
                        }


                        await _repoWrapper.SaveAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to fetch requisition for approvableId={t.approvableId}: {ex.Message}");
                }
            }

            try
            {
                List<string> insertedapprovablehistories = new List<string>();
                if (requisitions.Count > 0)
                {
                    foreach (var a in requisitions)
                    {
                        if (!insertedapprovablehistories.Contains(a.ApprovableId))
                        {
                            var existingApprovalHistories = _repoWrapper.AribaPRHistory
                                .FindByCondition(x => x.IsActive && x.AribaTaskId == a.ApprovableId && x.RequisitionsId == a.UniqueName)
                                .AsNoTracking()
                                .ToList();

                            if (existingApprovalHistories.Any())
                            {
                                existingApprovalHistories.ForEach(x => x.IsActive = false);
                                _repoWrapper.AribaPRHistory.UpdateRange(existingApprovalHistories);
                            }

                            var approvalRequests = a.ApprovalRequests
                                .OrderBy(x => DateTimeOffset.FromUnixTimeMilliseconds(a.SubmitDate).UtcDateTime)

                                .ThenBy(x => a.SubmitDate)
                                .ToList();

                            var newApprovalHistories = new List<AribaPRHistory>();
                            int approvedLevelCounter = 0;

                            foreach (var approvalrequest in approvalRequests)
                            {
                                var newHistory = new AribaPRHistory
                                {
                                    Id = Guid.NewGuid(),
                                    RequisitionsId = a.UniqueName,
                                    AribaTaskId = a.ApprovableId,
                                    Comments = approvalrequest.ApproverComments,
                                    Status = approvalrequest.ApprovalState,
                                    IsActive = true
                                };

                                if (approvalrequest.ApprovalState == "Approved")
                                {
                                    newHistory.Status = Common.STATUS_ARIBA_APPROVE;
                                    newHistory.ApprovalDate = DateTimeOffset.FromUnixTimeMilliseconds(a.LastModified).UtcDateTime;
                                    approvedLevelCounter++;
                                    newHistory.Level = "Level " + approvedLevelCounter;
                                    newHistory.AribaUser = approvalrequest.Approver;


                                }
                                else if (approvalrequest.ApprovalState == "Ready for approval")
                                {
                                    newHistory.Status = Common.STATUS_PENDING;
                                    approvedLevelCounter++;
                                    newHistory.Level = "Level " + approvedLevelCounter;
                                    newHistory.AribaUser = approvalrequest.Approver;
                                }
                                else
                                {

                                    newHistory.AribaUser = approvalrequest.Approver;
                                }
                                insertedapprovablehistories.Add(a.ApprovableId);
                                newApprovalHistories.Add(newHistory);
                            }
                            await _repoWrapper.AribaPRHistory.CreateRangeAsync(newApprovalHistories);
                        }
                    }
                }
                await _repoWrapper.SaveAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to insert invoice history for : {ex.Message}");
            }

            return GroupTasksIntoRequisitions(tasks, requisitions);
        }

        private List<AribaPRDto> GroupTasksIntoRequisitions(List<AribaPRTask> tasks, List<RequisitionDto> requisitions)
        {
            _logger.LogInfo($"Grouping {tasks.Count} tasks into requisitions.");

            return tasks
                .GroupBy(t => new { t.approvableUniqueName, t.approver, t.approvableId })
                .Select(g =>
                {
                    var requisition = requisitions
                        .FirstOrDefault(r => r.ApprovableId == g.Key.approvableId);
                    var taxDetail = _repoWrapper.AribaPR.GetCompanyTaxDetailByRequisition(g.Key.approvableUniqueName);

                    return new AribaPRDto
                    {

                        RequisitionsId = g.Key.approvableUniqueName,
                        SupplierName = requisition?.LineItems?.FirstOrDefault()?.Supplier?.Name ?? requisition?.RequesterName,
                        Currency = requisition?.TotalCost?.Currency,
                        DueDate = requisition?.SubmitDate != null ? DateTimeOffset.FromUnixTimeMilliseconds(requisition.SubmitDate).UtcDateTime : (DateTime?)null ?? g.Min(t => t.assignedDate),

                        RequisitionsDate = requisition?.SubmitDate != null ? DateTimeOffset.FromUnixTimeMilliseconds(requisition.SubmitDate).UtcDateTime : (DateTime?)null,
                        NeedBYDate = requisitions?.FirstOrDefault()?.LineItems?.FirstOrDefault()?.NeedBy > 0
    ? DateTimeOffset.FromUnixTimeMilliseconds(requisitions.FirstOrDefault().LineItems.FirstOrDefault().NeedBy).UtcDateTime
    : (DateTime?)null,
                        RequesterName = requisition?.Requester?.Name,
                        Description = requisition?.LineItems?.FirstOrDefault()?.Description?.Description,

                        PRTaxAmount = taxDetail.TaxAmount,
                        PRNetAmount = taxDetail.TaxAmount == 0 ? 0.0 : (double)requisition.TotalCost.Amount - taxDetail.TaxAmount,
                        AribaTaskId = requisition?.ApprovableId,
                        Amount = requisition?.TotalCost?.Amount ?? 0,
                        CompanyUniqueName = requisition?.CompanyUniqueName ?? taxDetail.CompanyCode,
                        Attachments = requisition?.Attachments?
                    .Where(a => a != null)
                    .Select(a => a.Id)
                    .ToList()
                ?? new List<string>(),
                        Tasks = g.ToList()
                    };
                })
                .ToList().Where(x => x.AribaTaskId != null).ToList();
        }
        public async Task<RequisitionDto> GetRequisitionById(
       string approvableId,
       string token,
       string apiKey,
       string realm,
       string requisitionTaskUrl)

        {
            _logger.LogInfo($"Fetching requisition details for approvableId {approvableId}.");


            var filters = new
            {
                approvableId = approvableId,
                realm = realm
            };

            string filterString = JsonConvert.SerializeObject(filters);
            string url = requisitionTaskUrl;

            if (!url.Contains("?"))
                url += "?";
            else if (!url.EndsWith("&"))
                url += "&";

            url += $"realm={realm}&filters={Uri.EscapeDataString(filterString)}";

            _logger.LogInfo($"[ARIBA] Final Request URL: {url}");

            HttpClientHandler handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback =
            (httpRequestMessage, cert, cetChain, policyErrors) =>
            {
                return true;
            };

            using var client = new HttpClient(handler);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("apiKey", apiKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await client.SendAsync(request);
            string content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Ariba API error: {response.StatusCode} - {content}");

            var jsonObject = JObject.Parse(content);

            var lineItems = jsonObject["lineItems"] as JArray;

            if (lineItems != null)
            {
                var filteredItems = lineItems
                    .Where(item =>
                    {
                        var shortName = item["description"]?["shortName"]?.ToString();

                        return shortName == null ||
                            !shortName.StartsWith("VAT", StringComparison.OrdinalIgnoreCase);
                    })
                    .ToList();

                jsonObject["lineItems"] = new JArray(filteredItems);
            }

            return jsonObject.ToObject<RequisitionDto>();
        }


        public async Task<string> PatchRequisition(Guid domainId, string requisitionId, AribaPatchRequestDto patchRequest, MyProfileDto aribauser, Guid userId)
        {
            try
            {

                DomainSetting configSetting = _repoWrapper.DomainSetting
                    .FindFirstByCondition(ds => ds.DomainId == domainId &&
                                                ds.SettingsKey == Common.DOMAIN_SETTINGS_ARIBA_INVOICE &&
                                                ds.IsActive);

                if (configSetting == null)
                    throw new Exception("Missing Ariba config for domain.");

                AribaPurchaseOrderSettings config = JsonConvert.DeserializeObject<AribaPurchaseOrderSettings>(configSetting.SettingsValue);

                if (string.IsNullOrWhiteSpace(config.User) || string.IsNullOrWhiteSpace(config.PasswordAdapter))
                    throw new Exception("User or passwordAdapter not found in Ariba config.");

                DomainSetting tokenConfigSetting = _repoWrapper.DomainSetting
                    .FindFirstByCondition(ds => ds.DomainId == domainId &&
                                                ds.SettingsKey == Common.DOMAIN_SETTINGS_ARIBA_TOKEN_INVOICE &&
                                                ds.IsActive);

                if (tokenConfigSetting == null)
                    throw new Exception("Missing Ariba Token config for domain.");

                AribaPurchaseOrderSettings tokenConfig = JsonConvert.DeserializeObject<AribaPurchaseOrderSettings>(tokenConfigSetting.SettingsValue);


                string token = await _aribaService.GetValidToken(domainId, tokenConfig);
                if (aribauser.Role == Common.BUSINESS_ADMIN_USER_KEY)
                {
                    if (!string.IsNullOrWhiteSpace(aribauser.AribaUserName) &&
                        !string.IsNullOrWhiteSpace(aribauser.AribaPassword))
                    {
                        _logger.LogInfo("Using AribaUserName and AribaPassword from MyProfileDto (Business Admin)");
                    }
                    else
                    {

                        DomainSetting defaultUserSetting = _repoWrapper.DomainSetting.FindFirstByCondition(
                            ds => ds.DomainId == domainId &&
                                ds.SettingsKey == Common.DOMAIN_SETTINGS_ARIBA_CONTRACT_URL &&
                                ds.IsActive);
                        if (defaultUserSetting != null)
                        {
                            AribaPurchaseOrderSettings defaultUserConfig =
                                JsonConvert.DeserializeObject<AribaPurchaseOrderSettings>(defaultUserSetting.SettingsValue);

                            aribauser.AribaUserName = defaultUserConfig?.AribaContractDefaultUser;
                            aribauser.AribaPassword = defaultUserConfig.AribaContractDefaultPassword;

                            _logger.LogInfo("Using default BusinessAdmin Ariba credentials from config");
                        }
                    }
                }
                string url = config.AcceptRejectPRUrl;
                if (url.Contains("{requisitionId}"))
                    url = url.Replace("{requisitionId}", requisitionId);
                if (url.Contains("{user}"))
                    url = url.Replace("{user}", aribauser.AribaUserName);
                if (url.Contains("{passwordAdapter}"))
                    url = url.Replace("{passwordAdapter}", aribauser.AribaPassword);
                if (url.Contains("{realm}"))
                    url = url.Replace("{realm}", config.Realm);

                _logger.LogInfo($"Patching Ariba requisition at {url}");

                HttpClient client = new HttpClient();

                string body = JsonConvert.SerializeObject(patchRequest);

                ByteArrayContent content = new ByteArrayContent(Encoding.UTF8.GetBytes(body));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Patch, url)
                {
                    Content = content
                };

                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                requestMessage.Headers.Add("apiKey", config.ApiKey);


                _logger.LogInfo($"PATCH Body: {body}");
                _logger.LogInfo($"Final Content-Type: {requestMessage.Content.Headers.ContentType}");

                HttpResponseMessage response = await client.SendAsync(requestMessage);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error patching requisition: {responseContent}");
                    throw new PreConditionFailedCustomException($"Ariba Patch failed: {responseContent}", $"Ariba Patch failed: {responseContent}");
                }
                else
                {
                    var pendingapprovable = _repoWrapper.AribaPRTask.FindByCondition(x => x.ApprovalId == requisitionId && x.Status == "Pending" && x.Approver == aribauser.AribaUserName).ToList();
                    pendingapprovable.ForEach(x => x.Status = patchRequest.State);
                    _repoWrapper.AribaPRTask.UpdateRange(pendingapprovable);
                    string ipAddress = _httpContextAccessor.HttpContext?.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim() ?? _httpContextAccessor.HttpContext?.Request.Headers["X-Real-IP"].FirstOrDefault() ?? _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.MapToIPv4()?.ToString();

                    LoginRecord log = new LoginRecord
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,         // coming from MyProfileDto
                        DomainId = domainId,
                        AttributeId = requisitionId,        // invoiceId / requisitionId
                        EntityType = Common.TYPE_PR,   // or Common.TYPE_PR based on context
                        IpAddress = ipAddress,
                        LoginTime = DateTime.UtcNow
                    };

                    await _repoWrapper.LoginRecord.CreateAsync(log);
                    await _repoWrapper.SaveAsync();
                }

                return responseContent;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception in PatchRequisitionAsync: {ex.Message}");
                throw new PreConditionFailedCustomException($"Unable to Update in Ariba : {ex.Message}", $"Unable to Update in Ariba : {ex.Message}");
            }
        }

        public SplitAttachmentResponseDto GetFinalPRAttachmentBase64(string requisitionId)
        {
            _logger.LogInfo($"Fetching the Attachments for the Requisition with Id : {requisitionId}");

            if (string.IsNullOrWhiteSpace(requisitionId))
                throw new PreConditionFailedCustomException("Requisition ID cannot be null or empty", "Requisition ID cannot be null or empty");

            List<FinalPRAttachment> attachments = _repoWrapper.FinalPRAttachment
                .FindByCondition(f => f.AribaId.ToLower() == requisitionId.ToLower() && f.IsActive)
                .ToList();
            if (attachments == null || attachments.Count == 0)
                throw new NotFoundCustomException($"No final attachment found for Requisition ID: {requisitionId}", $"No final attachment found for Requisition ID: {requisitionId}");


            SplitAttachmentResponseDto result = new SplitAttachmentResponseDto();
            result.PRAttachments = attachments
                .Where(a => a.type != null && a.type.Equals("ARIBA_PR", StringComparison.OrdinalIgnoreCase))
                .Select(a => Convert.ToBase64String(a.Attachment))
                .ToList();

            result.ContractAttachments = attachments
                .Where(a => a.type != null && a.type.Equals("ARIBA_CONTRACT", StringComparison.OrdinalIgnoreCase))
                .Select(a => Convert.ToBase64String(a.Attachment))
                .ToList();
            return result;
        }

        public async Task<List<AribaAttachmentDTO>> GetRequisitionAttachments(Guid domainId, string requisitionId, string attachmentId)
        {
            _logger.LogInfo($"Fetching requisition attachments for domain {domainId}, requisition {requisitionId}, attachment {attachmentId}.");

            DomainSetting configSetting = _repoWrapper.DomainSetting.FindFirstByCondition(
                ds => ds.DomainId == domainId &&
                      ds.SettingsKey == Common.DOMAIN_SETTINGS_ARIBA_INVOICE &&
                      ds.IsActive);

            if (configSetting == null)
                throw new Exception("Missing Ariba-Invoice config for domain.");

            AribaPurchaseOrderSettings invoiceConfig = JsonConvert.DeserializeObject<AribaPurchaseOrderSettings>(configSetting.SettingsValue);

            DomainSetting tokenSetting = _repoWrapper.DomainSetting.FindFirstByCondition(
                ds => ds.DomainId == domainId &&
                      ds.SettingsKey == Common.DOMAIN_SETTINGS_ARIBA_TOKEN_INVOICE &&
                      ds.IsActive);

            if (tokenSetting == null)
                throw new Exception("Missing Ariba-Token config for domain.");

            AribaPurchaseOrderSettings tokenConfig = JsonConvert.DeserializeObject<AribaPurchaseOrderSettings>(tokenSetting.SettingsValue);


            string token = await _aribaService.GetAribaToken(tokenConfig.AribaAuthToken, tokenConfig.AribaTokenUrl);

            string requestUrl = invoiceConfig.RequisitionPRAttachmentUrl;

            if (requestUrl.Contains("{requisitionId}"))
                requestUrl = requestUrl.Replace("{requisitionId}", requisitionId);
            if (requestUrl.Contains("{attachmentId}"))
                requestUrl = requestUrl.Replace("{attachmentId}", attachmentId);


            if (!requestUrl.Contains("realm="))
            {
                if (!requestUrl.Contains("?"))
                    requestUrl += "?";
                else if (!requestUrl.EndsWith("&"))
                    requestUrl += "&";

                requestUrl += $"realm={invoiceConfig.Realm}";
            }

            _logger.LogInfo($"Patching Ariba requisition at {requestUrl}");

            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

                request.Headers.TryAddWithoutValidation("apikey", tokenConfig.ApiKey);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Ariba API error: {response.StatusCode} - {errorResponse}");
                }

                string contentType = response.Content.Headers.ContentType?.MediaType;

                if (contentType != null && contentType.Contains("json"))
                {
                    string responseContent = await response.Content.ReadAsStringAsync();

                    try
                    {
                        JToken json = JToken.Parse(responseContent);

                        if (json.Type == JTokenType.Object)
                        {
                            JObject obj = (JObject)json;

                            if (obj["error"] != null)
                            {
                                string code = obj["error"]["code"]?.ToString();
                                string message = obj["error"]["message"]?.ToString();
                                throw new Exception($"Ariba API error: {code} - {message}");
                            }

                            if (obj["value"] != null && obj["value"].Type == JTokenType.Array)
                                return obj["value"].ToObject<List<AribaAttachmentDTO>>();

                            AribaAttachmentDTO single = obj.ToObject<AribaAttachmentDTO>();
                            return new List<AribaAttachmentDTO> { single };
                        }
                        else if (json.Type == JTokenType.Array)
                        {
                            return json.ToObject<List<AribaAttachmentDTO>>();
                        }
                    }
                    catch (JsonReaderException jex)
                    {
                        throw new Exception($"Invalid JSON from Ariba: {jex.Message}");
                    }
                }
                else
                {
                    byte[] bytes = await response.Content.ReadAsByteArrayAsync();
                    string fileName = $"Ariba_Attachment_{attachmentId}";

                    return new List<AribaAttachmentDTO>
            {
                new AribaAttachmentDTO
                {
                    Id = attachmentId,
                    FileName = fileName,
                    ContentType = contentType,
                    FileBytes = bytes
                }
            };
                }
            }

            throw new Exception("Unexpected Ariba attachment response format.");
        }

        public async Task<byte[]> GetAttachmentbyContractID(Guid domainId, string contractId, MyProfileDto aribaUser)
        {

            var contractDetail = _repoWrapper.AribaContractDetails
                .FindFirstByCondition(x => x.DomainId == domainId && x.ContractId == contractId && x.IsActive);

            string? workspaceId = contractDetail?.WorkspaceID;

            if (string.IsNullOrEmpty(workspaceId))
            {
                _logger.LogInfo($"WorkspaceId not found in DB. Fetching from Ariba for ContractId: {contractId}");
                workspaceId = await _aribaService.GetContractWorkspaceId(domainId, contractId);
                if (string.IsNullOrEmpty(workspaceId))
                {
                    _logger.LogError($"WorkspaceId could not be fetched from Ariba for ContractId: {contractId}");
                    return null;
                }
            }
            else
            {
                _logger.LogInfo($"WorkspaceId found in DB for ContractId: {contractId} = {workspaceId}");
            }

            string? documentId = await _aribaService.GetAttachmentIdByWorkspaceIdAsync(domainId, workspaceId, aribaUser);
            if (string.IsNullOrEmpty(documentId))
            {
                _logger.LogError($"No document ID found for WorkspaceId: {workspaceId}");
                return null;
            }

            _logger.LogInfo($"DocumentId for WorkspaceId {workspaceId}: {documentId}");
            byte[]? documentBytes = await _aribaService.GetContractDocumentById(domainId, documentId, aribaUser);

            if (documentBytes == null || documentBytes.Length == 0)
            {
                _logger.LogError($"Failed to download document for DocumentId={documentId}, WorkspaceId={workspaceId}");
                return null;
            }

            _logger.LogInfo($"Successfully downloaded document for ContractId={contractId}, size={documentBytes.Length} bytes");
            return documentBytes;
        }

        public async Task<List<AribaPRDetailDto>> GetPendingApprovalsAsync(
      Guid userId,
      Guid domainId,
      int index,
      int limit,
      string? searchTerm = null,
      DateTime? fromDate = null,
      DateTime? toDate = null,
      string? status = null,
      List<string>? glAccount = null)
        {
            _logger.LogInfo($"Fetching pending PR for UserId: {userId} in DomainId: {domainId}");
            if (glAccount != null)
            {
                glAccount = glAccount
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();

                if (glAccount.Count == 0)
                    glAccount = null;
            }
            MyProfileDto? profileDto = _repoWrapper.Person.GetMyProfileByUserId(userId);
            string aribausername = null;
            string role = null;
            if (profileDto != null)
            {
                aribausername = profileDto.AribaUserName;
                role = profileDto.Role;
            }

            var query = from task in _repoWrapper.AribaPRTask
                               .FindByConditionAsync(x => x.DomainId == domainId)
                        join requisitions in _repoWrapper.AribaPR.FindByCondition(x => x.IsActive)
                            on task.ApprovalId equals requisitions.AribaTaskId
                        select new { task, requisitions };

            // Step 2: Role / approver filter
            if (role != Common.BUSINESS_ADMIN_USER_KEY)
            {
                if (string.IsNullOrEmpty(aribausername))
                {
                    _logger.LogError($"Unable to Find Any Records for User with Id : {userId}");
                    throw new NotFoundCustomException("Unable to Find Any Records", "Unable to Find Any Records");
                }

                query = query.Where(x => x.task.Approver == aribausername);
            }
            if (!string.IsNullOrEmpty(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                string normalizedStatus = status.Trim();
                List<string> validStatus = new List<string>();

                if (normalizedStatus.Equals(Common.STATUS_ARIBA_APPROVE, StringComparison.OrdinalIgnoreCase))
                {
                    validStatus.Add(Common.STATUS_APPROVE);
                    validStatus.Add(Common.STATUS_ARIBA_APPROVE);
                }
                else
                {
                    validStatus.Add(normalizedStatus);
                }
                query = query.Where(x => x.task.Status != null && validStatus.Contains(x.task.Status));
            }
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(x =>
                    x.requisitions.RequisitionsId.Contains(searchTerm) ||
                    x.task.Approver.Contains(searchTerm) ||
                    x.requisitions.SupplierName.Contains(searchTerm));
            }

            if (fromDate.HasValue)
            {
                query = query.Where(x => x.requisitions.RequisitionsDate >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                query = query.Where(x => x.requisitions.RequisitionsDate <= toDate.Value);
            }
            // GL Account filter using JOIN
            if (glAccount != null && glAccount.Any())
            {
                query =
                    from q in query
                    join po in _repoWrapper.PurchaseOrderMaster
                        .FindByCondition(x => x.IsActive && glAccount.Contains(x.GLAccount))
                        on q.requisitions.RequisitionsId equals po.RequisitionId
                    select q;
            }

            // Pagination (async)
            var pagedData = await query
                .OrderByDescending(x => x.task.DateUpdated)
                .Skip(index)
                .Take(limit)
                .ToListAsync();

            if (!pagedData.Any())
            {
                _logger.LogError($"Unable to find Any PR");
                throw new NotFoundCustomException("Unable to Find Any PR", "Unable to Find Any PR");
            }

            // PRE-FETCH ALL RELATED DATA IN BULK TO AVOID N+1 QUERIES

            // 1. Collect unique keys
            var requisitionIds = pagedData.Select(x => x.requisitions.RequisitionsId).Distinct().ToList();
            var contractIds = pagedData.Where(x => x.requisitions.ContractId != null)
                                       .Select(x => x.requisitions.ContractId)
                                       .Distinct()
                                       .ToList();
            var requesters = pagedData.Where(x => !string.IsNullOrEmpty(x.requisitions.RequesterName))
                                      .Select(x => x.requisitions.RequesterName)
                                      .Distinct()
                                      .ToList();
            var companyCodes = pagedData.Where(x => x.requisitions.CompanyUniqueName != null)
                                        .Select(x => x.requisitions.CompanyUniqueName)
                                        .Distinct()
                                        .ToList();
            var taskIds = pagedData.Select(x => x.requisitions.AribaTaskId).Distinct().ToList();

            // 2. Bulk fetch PO details
            var poDetailsList = (await _repoWrapper.PurchaseOrderMaster
                        .FindByConditionAsync(x => x.IsActive && requisitionIds.Contains(x.RequisitionId))
                    .ToListAsync());

            var poDetailsDict = poDetailsList
                    .GroupBy(x => x.RequisitionId)
                    .ToDictionary(g => g.Key, g => g.First());

            // 3. Bulk fetch approval history
            var approvalHistoryDict =
                (await _repoWrapper.AribaPRHistory
                    .FindByConditionAsync(x => x.IsActive && taskIds.Contains(x.AribaTaskId) && requisitionIds.Contains(x.RequisitionsId))
                    .ToListAsync())
                .GroupBy(x => new { x.AribaTaskId, x.RequisitionsId })
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.DateCreated).ToList());

            // 4. Bulk fetch contracts
            var contractsList = (await _repoWrapper.AribaContract
                    .FindByConditionAsync(x => contractIds.Contains(x.ContractId))
                    .ToListAsync());

            var contractsDict = contractsList
                .GroupBy(x => x.ContractId)
                .ToDictionary(g => g.Key, g => g.First());

            // 5. Bulk fetch contract details
            var contractDetailsList = (await _repoWrapper.AribaContractDetails
                    .FindByConditionAsync(x => contractIds.Contains(x.ContractId))
                    .ToListAsync());

            var contractDetailsDict = contractDetailsList
                .GroupBy(x => x.ContractId)
                .ToDictionary(g => g.Key, g => g.First());

            // 6. Bulk fetch company names
            var companyList = (await _repoWrapper.AribaCompanyMapping
                    .FindByConditionAsync(x => x.IsActive && companyCodes.Contains(x.CompanyCode))
                    .ToListAsync());

            var companyDict = companyList
                .GroupBy(x => x.CompanyCode)
                .ToDictionary(g => g.Key, g => g.First());

            // 7. Bulk fetch attachments
            var attachmentsList = (await _repoWrapper.FinalPRAttachment
                    .FindByConditionAsync(x => x.IsActive && requisitionIds.Contains(x.AribaId))
                     .Select(x => x.AribaId)
                    .ToListAsync());
            var attachmentsDict = attachmentsList
                .Distinct()
                .ToDictionary(x => x, x => true);

            // 8. Bulk fetch departments
            Dictionary<string, string> departmentDict = new();
            foreach (var req in requesters)
            {
                var dept = _repoWrapper.AribaPR.GetDepartmentDetail(req);
                if (dept != null)
                    departmentDict[req] = dept;
            }

            List<AribaPRDetailDto> aribarequisitions = new List<AribaPRDetailDto>();
            foreach (var item in pagedData)
            {
                var requisitions = item.requisitions;
                var task = item.task;

                var po = requisitions?.RequisitionsId != null && poDetailsDict.TryGetValue(requisitions.RequisitionsId, out var p) ? p : null;

                var dto = new AribaPRDetailDto
                {
                    RequisitionsId = requisitions.RequisitionsId,
                    AribaTaskId = requisitions.AribaTaskId,
                    Currency = requisitions.Currency,
                    DueDate = requisitions.DueDate,
                    Amount = requisitions.Amount,
                    SupplierName = requisitions.SupplierName,
                    Description = requisitions.Description,
                    Department = !string.IsNullOrEmpty(requisitions?.RequesterName) && departmentDict.TryGetValue(requisitions.RequesterName, out var dept) ? dept : null,
                    RequisitionsDate = requisitions.RequisitionsDate,
                    PRTaxAmount = _repoWrapper.AribaPR.GetCompanyTaxDetailByRequisition(requisitions.RequisitionsId).TaxAmount,
                    ContractId = requisitions.ContractId,
                    Approver = task.Approver,
                    PRNetAmount = requisitions.PRNetAmount,
                    RequesterName = requisitions.RequesterName,
                    NeedBYDate = requisitions.NeedBYDate,
                    HasAttachment = attachmentsDict.ContainsKey(requisitions.RequisitionsId),
                    CompanyUniqueName = requisitions.CompanyUniqueName != null && companyDict.TryGetValue(requisitions.CompanyUniqueName, out var mapping)
                        ? mapping.CompanyName
                        : null,
                    Status = task.Status,
                    FullUrl = task.FullUrl,
                    PaymentStatus = task.PaymentStatus,
                    GLAccount = po?.GLAccount,
                    CostCenter = po?.CostCenter,
                };
                if (role == Common.BUSINESS_ADMIN_USER_KEY || role == Common.BUSINESS_USER_KEY)
                {
                    dto.Approver = task.Approver;
                }

                // Get approval history from dictionary
                var historyKey = new { AribaTaskId = dto.AribaTaskId, RequisitionsId = dto.RequisitionsId };
                dto.ApprovalHistory = approvalHistoryDict.TryGetValue(historyKey, out var history)
                    ? history
                    : new List<AribaPRHistory>();

                // Get contract details from dictionary
                if (dto.ContractId != null &&
                    contractsDict.TryGetValue(dto.ContractId, out var aribacontract) &&
                    contractDetailsDict.TryGetValue(dto.ContractId, out var aribacontractdetails))
                {
                    dto.ContractValue = aribacontract.TotalAmount;
                    dto.ValueAlreadyPaid = aribacontractdetails.AmountSpend;
                    dto.RemainingBalance = aribacontractdetails.BalanceAmount;
                    dto.WorkSpaceId = aribacontractdetails.WorkspaceID;
                    dto.AttachmentId = aribacontractdetails.AttachmentID;
                    dto.ContractCurrency = aribacontract.Currency;
                }
                aribarequisitions.Add(dto);
            }

            if (!aribarequisitions.Any())
            {
                _logger.LogError($"Unable to find Any PR");
                throw new NotFoundCustomException("Unable to Find Any PR", "Unable to Find Any PR");
            }

            return aribarequisitions;
        }
        public async Task<ExportExcelResultDto> ExportPendingApprovalsExcelAsync(
            Guid userId, Guid domainId, int limit,
            string? searchTerm, DateTime? fromDate, DateTime? toDate,
            string? status, List<string>? glAccount)
        {
            if (limit > 500)
                throw new PreConditionFailedCustomException("Limit cannot exceed 500 records", "Limit cannot exceed 500 records");

            var requisitions = await GetPendingApprovalsAsync(
                userId, domainId, 0, limit, searchTerm, fromDate, toDate, status, glAccount);

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("Pending Approvals");

            if (!requisitions.Any())
            {
                ws.Cell(1, 1).Value = "No Records Found";

                using var emptyStream = new MemoryStream();
                workbook.SaveAs(emptyStream);

                return new ExportExcelResultDto
                {
                    FileContent = emptyStream.ToArray(),
                    FileName = $"PendingApprovalsPRDetails_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };
            }


            var exportData = requisitions.Select(x => new
            {
                Entity = x.CompanyUniqueName,
                x.Approver,
                PRNumber = x.RequisitionsId,
                x.NeedBYDate,
                x.RequisitionsDate,
                x.Amount,
                x.Currency,
                x.SupplierName,
                x.Status,
                x.Description,
                x.RequesterName,
                x.ContractId,
                x.Department,
                x.GLAccount,
                x.CostCenter,
                x.ContractValue,
                x.ValueAlreadyPaid,
                x.RemainingBalance,
                x.FullUrl
            }).ToList();

            ws.Cell(1, 1).InsertTable(exportData);
            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            string fileName = $"PendingApprovalsPRDetails_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return new ExportExcelResultDto
            {
                FileContent = stream.ToArray(),
                FileName = fileName
            };


        }
        public async Task<bool> UpdatePaymentStatusAsync(string PRNumber, string paymentStatus)
        {
            var validStatuses = new[] { Common.UNPAID_PAYMENT_STATUS, Common.PAID_PAYMENT_STATUS, Common.PAYING_STATUS, Common.PARTIAL_PAYMENT_STATUS };
            if (!validStatuses.Contains(paymentStatus))
            {
                throw new ArgumentException($"Invalid payment status: {paymentStatus}. Valid statuses are: {string.Join(", ", validStatuses)}");
            }
            // Find record by InvoiceNumber
            var paymentRecord = _repoWrapper.AribaPRTask
                .FindByCondition(p => p.ApprovableUniqueName == PRNumber && p.IsActive)
                .FirstOrDefault();

            if (paymentRecord == null)
                throw new Exception($"No payment record found for PRNumber: {PRNumber}");

            paymentRecord.PaymentStatus = paymentStatus;
            paymentRecord.DateUpdated = DateTime.Now;

            _repoWrapper.AribaPRTask.Update(paymentRecord);
            await _repoWrapper.SaveAsync();

            return true;
        }



    }
}






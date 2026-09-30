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
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Services
{
    public class ContractService : IContractService
    {
        private readonly IRepositoryWrapper _repoWrapper;

        private readonly ILoggerManager _logger;
        private readonly IDomainService _domainService;
        private readonly IAribaService _aribaService;


        /// <param name="repoWrapper">Repository wrapper.</param>
        /// <param name="logger">Logger manager.</param>

        public ContractService(IRepositoryWrapper repoWrapper, ILoggerManager logger, IDomainService domainService, IAribaService aribaService)
        {
            _repoWrapper = repoWrapper;
            _logger = logger;
            _domainService = domainService;
            _aribaService = aribaService;
        }

        public async Task<string> GetAribaToken(string authToken, string tokenUrl)
        {
            using var client = new HttpClient();

            // Request body
            var body = new Dictionary<string, string> { { "grant_type", "openapi_2lo" } };

            using var content = new FormUrlEncodedContent(body);

            // Add headers
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            client.DefaultRequestHeaders.Connection.Add("keep-alive");
            client.DefaultRequestHeaders.Add("cache-control", "no-cache");

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

            // Send request
            var response = await client.PostAsync(tokenUrl, content);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to fetch token: {response.StatusCode} - {responseContent}");
            }

            var responseDTO = JsonConvert.DeserializeObject<AribaToken>(responseContent);

            if (string.IsNullOrEmpty(responseDTO?.access_token))
                throw new Exception("Failed to retrieve Ariba access token.");

            _logger.LogInfo("Ariba token retrieved successfully.");
            return responseDTO.access_token;
        }
        private static bool IsContract(string? text)
        {
            return !string.IsNullOrWhiteSpace(text) &&
                   text.Contains("contract", StringComparison.OrdinalIgnoreCase);
        }



        public async Task<List<AribaContractDto>> GetPendingContracts(
    DateTime fromDate,
    DateTime toDate,
    string token,
    string apiKey,
    string realm,
    string ContractPendingApprovablesUrl,
    string ContractWorkspaceUrl,

    Guid domainId,
    string taskId)
        {
            _logger.LogInfo($"[AribaContract] Fetching pending contract approvables for realm {realm} from {fromDate} to {toDate}.");

            // string filterString = "$filter=documentType eq 'ariba.collaborate.core.Task'";
            string url = ContractPendingApprovablesUrl;

            if (!url.Contains("?"))
                url += "?";
            else if (!url.EndsWith("&"))
                url += "&";

            url += $"realm={realm}";
            _logger.LogInfo($"Calling Ariba Pending Contracts API: {url}");

            using var client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("apiKey", apiKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Ariba API error: {response.StatusCode} - {responseContent}");

            // Parse JSON
            List<ContractTaskDto> tasks = new List<ContractTaskDto>();
            JToken json = JsonConvert.DeserializeObject<JToken>(responseContent);

            if (json.Type == JTokenType.Object && json["value"] != null)
            {
                if (json["value"].Type == JTokenType.Array)
                    tasks = json["value"].ToObject<List<ContractTaskDto>>();
                else
                    tasks.Add(json["value"].ToObject<ContractTaskDto>());
            }
            else if (json.Type == JTokenType.Array)
            {
                tasks = json.ToObject<List<ContractTaskDto>>();
            }

            if (tasks.Count == 0)
                return new List<AribaContractDto>();

            if (!string.IsNullOrEmpty(taskId))
                tasks = tasks.Where(x => x.UniqueName == taskId).ToList();

            List<ContractDetailDto> contracts = new List<ContractDetailDto>();
            foreach (var t in tasks)
            {
                try
                {
                    if (!IsContract(t.Description))
                    {
                        _logger.LogInfo($"Skipping pending task (not contract): {t.UniqueName} - {t.Description}");
                        continue;
                    }

                    ContractPending taskExists = _repoWrapper.ContractPending.FindFirstByCondition(x =>
                        x.IsActive &&
                        x.UniqueName == t.UniqueName &&
                        x.Approver == t.Approver &&
                        x.AssignedDate == t.AssignedDate);

                    if (taskExists == null)
                    {
                        ContractPending newTask = new ContractPending
                        {
                            Id = Guid.NewGuid(),
                            UniqueName = t.UniqueName,
                            Description = t.Description,
                            Approver = t.Approver,
                            Email = t.Email,
                            AssignedDate = t.AssignedDate,
                            DomainId = domainId,
                            Status = Common.STATUS_PENDING,
                            FullUrl = t.FullUrl,
                            DocumentType = t.DocumentType ?? "ariba.collaborate.core.Task"
                        };

                        await _repoWrapper.ContractPending.CreateAsync(newTask);


                        var contract = await GetContractTaskById(t.UniqueName, token, apiKey, realm,
                            ContractWorkspaceUrl.Replace("{TaskId}", t.UniqueName));
                        if (contract != null)
                        {
                            if (contract.Document?.Content?.SubContentDoc != null)
                            {
                                var newAttachments = new List<ContractAttachment>();

                                foreach (var doc in contract.Document.Content.SubContentDoc)
                                {
                                    if (string.IsNullOrEmpty(doc.Id))
                                        continue;

                                    var existing = _repoWrapper.ContractAttachment
                                        .FindFirstByCondition(x =>
                                            x.IsActive &&
                                            x.TaskId == t.UniqueName &&
                                            x.AttachmentId == doc.Id);

                                    if (existing == null)
                                    {
                                        newAttachments.Add(new ContractAttachment
                                        {
                                            Id = Guid.NewGuid(),
                                            TaskId = t.UniqueName,
                                            AttachmentId = doc.Id
                                        });
                                    }
                                }

                                if (newAttachments.Any())
                                    await _repoWrapper.ContractAttachment.CreateRangeAsync(newAttachments);
                            }


                            if (!string.IsNullOrEmpty(contract.WorkspaceId))
                            {
                                try
                                {
                                    var workspaceDetails = await GetContractWorkspaceById(
                                        contract.WorkspaceId,
                                        domainId);

                                    if (workspaceDetails != null)
                                    {

                                        contract.WorkspaceDetails = workspaceDetails;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError($"Failed to fetch workspace details for {contract.WorkspaceId}: {ex.Message}");
                                }
                            }
                        }
                        contracts.Add(contract);

                        await _repoWrapper.SaveAsync();
                    }
                    else
                    {
                        // Update task
                        taskExists.Description = t.Description;
                        taskExists.Approver = t.Approver;
                        taskExists.Email = t.Email;
                        taskExists.AssignedDate = t.AssignedDate;
                        taskExists.DocumentType = t.DocumentType;
                        _repoWrapper.ContractPending.Update(taskExists);



                        var contract = await GetContractTaskById(t.UniqueName, token, apiKey, realm,
                            ContractWorkspaceUrl.Replace("{TaskId}", t.UniqueName));
                        if (contract != null)
                        {


                            if (contract.Document?.Content?.SubContentDoc != null)
                            {
                                var newAttachments = new List<ContractAttachment>();

                                foreach (var doc in contract.Document.Content.SubContentDoc)
                                {
                                    if (string.IsNullOrEmpty(doc.Id))
                                        continue;

                                    var existing = _repoWrapper.ContractAttachment
                                        .FindFirstByCondition(x =>
                                            x.IsActive &&
                                            x.TaskId == t.UniqueName &&
                                            x.AttachmentId == doc.Id);

                                    if (existing == null)
                                    {
                                        newAttachments.Add(new ContractAttachment
                                        {
                                            Id = Guid.NewGuid(),
                                            TaskId = t.UniqueName,
                                            AttachmentId = doc.Id,

                                        });
                                    }
                                }

                                if (newAttachments.Any())
                                    await _repoWrapper.ContractAttachment.CreateRangeAsync(newAttachments);
                            }


                            if (!string.IsNullOrEmpty(contract.WorkspaceId))
                            {
                                try
                                {
                                    var workspaceDetails = await GetContractWorkspaceById(
                                        contract.WorkspaceId,
                                        domainId);

                                    if (workspaceDetails != null)
                                    {

                                        contract.WorkspaceDetails = workspaceDetails;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError($"Failed to fetch workspace details for {contract.WorkspaceId}: {ex.Message}");
                                }
                            }
                        }
                        contracts.Add(contract);
                        await _repoWrapper.SaveAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to fetch contract for task={t.UniqueName}: {ex.Message}");
                }
            }

            try
            {
                List<string> insertedapprovablehistories = new List<string>();
                if (contracts.Count > 0)
                {
                    foreach (var c in contracts)
                    {
                        var existingHistories = _repoWrapper.AribaContractHistory
                            .FindByCondition(x => x.IsActive && x.TaskId == c.Id)
                            .ToList();

                        if (existingHistories.Any())
                        {
                            existingHistories.ForEach(x => x.IsActive = false);
                            _repoWrapper.AribaContractHistory.UpdateRange(existingHistories);
                        }

                        if (c.ApprovalRequests != null)
                        {
                            var newApprovalHistories = new List<AribaContractHistory>();
                            int approvedLevelCounter = 0;

                            foreach (var req in c.ApprovalRequests)
                            {
                                var newHistory = new AribaContractHistory
                                {
                                    Id = Guid.NewGuid(),
                                    TaskId = req.TaskId,
                                    // Comments = req.Comments?
                                    //     .Where(c => c.TaskId == req.TaskId)
                                    //     .Select(c => c.Body)
                                    //     .FirstOrDefault(),
                                    Status = req.ApprovalState,
                                    IsActive = true,
                                    AribaUser = req.Approvers?.FirstOrDefault()?.User?.UniqueName
                                };


                                if (req.ApprovalState == "Approved")
                                {
                                    newHistory.Status = Common.STATUS_ARIBA_APPROVE;
                                    newHistory.ApprovalDate = DateTimeOffset.FromUnixTimeMilliseconds(req.ActivationDate).UtcDateTime;
                                    approvedLevelCounter++;
                                    newHistory.Level = "Level " + approvedLevelCounter;
                                    newHistory.AribaUser = req.Approvers?.FirstOrDefault()?.User?.UniqueName;


                                }
                                else if (req.ApprovalState == "Ready for approval")
                                {
                                    newHistory.Status = Common.STATUS_PENDING;
                                    approvedLevelCounter++;
                                    newHistory.Level = "Level " + approvedLevelCounter;
                                    newHistory.AribaUser = req.Approvers?.FirstOrDefault()?.User?.UniqueName;
                                }
                                else
                                {

                                    newHistory.AribaUser = req.Approvers?.FirstOrDefault()?.User?.UniqueName;
                                }
                                insertedapprovablehistories.Add(c.Id);
                                newApprovalHistories.Add(newHistory);
                            }
                            await _repoWrapper.AribaContractHistory.CreateRangeAsync(newApprovalHistories);
                        }
                    }

                    await _repoWrapper.SaveAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to insert contract history: {ex.Message}");
            }

            return GroupTasksIntoContracts(tasks, contracts);
        }

        private List<AribaContractDto> GroupTasksIntoContracts(List<ContractTaskDto> tasks, List<ContractDetailDto> contracts)
        {
            _logger.LogInfo($"Grouping {tasks.Count} contract tasks.");

            return tasks
                .GroupBy(t => new { t.UniqueName, t.Approver })
                .Select(g =>
                {
                    var contract = contracts.FirstOrDefault(c => c.Id == g.Key.UniqueName);
                    var workspace = contract?.WorkspaceDetails?.Value?.FirstOrDefault();

                    return new AribaContractDto
                    {
                        ContractTaskId = g.Key.UniqueName,
                        Description = contract?.Title,
                        WorkspaceID = contract?.WorkspaceId,
                        BeginDate = contract != null ? DateTimeOffset.FromUnixTimeMilliseconds(contract.BeginDate).UtcDateTime : (DateTime?)null,
                        EndDate = workspace?.ExpirationDate,
                        TotalAmount = workspace?.ContractAmount?.Amount.ToString(),
                        Currency = workspace?.ContractAmount?.Currency,
                        SupplierID = workspace?.Supplier?.SmVendorID,
                        SupplierName = workspace?.Supplier?.Name,
                        Department = workspace?.Departments != null && workspace.Departments.Count > 0 ? string.Join(", ", workspace.Departments.Select(d => d.Name)) : null,
                        CompanyUniqueName = workspace?.CustomFields?
        .FirstOrDefault(f => f.FieldName == "Property")
        ?.TextMultiSelectValue != null
            ? string.Join(", ", workspace.CustomFields
                .FirstOrDefault(f => f.FieldName == "Property")
                .TextMultiSelectValue)
            : null,
                        AttachmentID = contract?.Document?.Content?.SubContentDoc?
    .Where(d => !string.IsNullOrEmpty(d.Id))
    .Select(d => d.Id!)
    .Distinct()
    .ToList()
    ?? new List<string>(),


                        Tasks = g.ToList()
                    };
                })
                .Where(x => x.ContractTaskId != null)
                .ToList();
        }

        public async Task<ContractDetailDto> GetContractTaskById(
    string TaskId,
    string token,
    string apiKey,
    string realm,
    string ContractWorkspaceUrl)
        {
            _logger.LogInfo($"Fetching contract task details for Task ID: {TaskId}.");


            string url = ContractWorkspaceUrl.TrimEnd('/');
            url += $"/Task/{TaskId}?realm={realm}";

            _logger.LogInfo($"[ARIBA] Final Request URL: {url}");
            HttpClientHandler handler = new HttpClientHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true
            };

            using var client = new HttpClient(handler);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("apiKey", apiKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Send request
            HttpResponseMessage response = await client.SendAsync(request);
            string content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"[ARIBA] Error fetching contract task: {response.StatusCode} - {content}");
                throw new Exception($"Ariba API error: {response.StatusCode} - {content}");
            }

            _logger.LogInfo("[ARIBA] Successfully fetched contract task details.");

            var result = JsonConvert.DeserializeObject<ContractDetailDto>(content);
            if (result == null)
                throw new Exception("Failed to deserialize contract task details");

            return result;
        }

        public async Task<AribaWorkspaceResponseDto> GetContractWorkspaceById(
           string WorkspaceID,
           Guid domainId)
        {
            _logger.LogInfo($"[AribaContract] Fetching Contract Workspace for Domain: {domainId}, WorkspaceId: {WorkspaceID}.");

            // 🔹 1. Fetch contract configuration
            var configSetting = _repoWrapper.DomainSetting
                .FindFirstByCondition(ds =>
                    ds.DomainId == domainId &&
                    ds.SettingsKey == Common.DOMAIN_SETTINGS_CONTRACT_URL &&
                    ds.IsActive);

            if (configSetting == null)
            {
                _logger.LogError($"[AribaContract] Missing Ariba Contract configuration for domain: {domainId}");
                return null;
            }

            var contractConfig = JsonConvert.DeserializeObject<AribaPurchaseOrderSettings>(configSetting.SettingsValue);
            if (contractConfig == null ||
                string.IsNullOrWhiteSpace(contractConfig.User) ||
                string.IsNullOrWhiteSpace(contractConfig.PasswordAdapter))
            {
                _logger.LogError($"[AribaContract] Invalid Ariba Contract configuration for domain: {domainId}");
                return null;
            }

            // 🔹 2. Fetch Token configuration
            var tokenConfigSetting = _repoWrapper.DomainSetting
                .FindFirstByCondition(ds =>
                    ds.DomainId == domainId &&
                    ds.SettingsKey == Common.DOMAIN_SETTINGS_ARIBA_CONTRACT_TOKEN &&
                    ds.IsActive);

            if (tokenConfigSetting == null)
            {
                _logger.LogError($"[AribaContract] Missing Ariba Token configuration for domain: {domainId}");
                return null;
            }

            var tokenConfig = JsonConvert.DeserializeObject<AribaPurchaseOrderSettings>(tokenConfigSetting.SettingsValue);
            if (tokenConfig == null)
            {
                _logger.LogError($"[AribaContract] Invalid Ariba Token configuration for domain: {domainId}");
                return null;
            }

            // 🔹 3. Get valid token
            string token = await GetAribaToken(tokenConfig.WorkspaceDetailsAuthToken, tokenConfig.WorkspaceDetailsTokenUrl);
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError($"[AribaContract] Failed to obtain token for domain: {domainId}");
                return null;
            }
            string aribaUserName = contractConfig.User;
            string passwordAdapter = contractConfig.PasswordAdapter;

            // 🔹 4. Build final URL
            string url = contractConfig.ContractWorkspaceDetailsUrl;
            if (url.Contains("{workspaceId}"))
                url = url.Replace("{workspaceId}", WorkspaceID);
            if (url.Contains("{realm}"))
                url = url.Replace("{realm}", contractConfig.Realm);
            if (url.Contains("{user}"))
                url = url.Replace("{user}", aribaUserName);
            if (url.Contains("{passwordAdapter}"))
                url = url.Replace("{passwordAdapter}", passwordAdapter);

            _logger.LogInfo($"Posting Ariba Task Action at {url}");


            using var client = new HttpClient();
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            requestMessage.Headers.TryAddWithoutValidation("apiKey", tokenConfig.WorkspaceDetailsApiKey);
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _logger.LogInfo($"[AribaContract] Sending GET request to Ariba API...");


            HttpResponseMessage response = await client.SendAsync(requestMessage);
            string responseContent = await response.Content.ReadAsStringAsync();
            await Task.Delay(10000);



            if (!response.IsSuccessStatusCode)
            {

                _logger.LogError($"[AribaContract] API request failed. Status: {response.StatusCode}, Response: {responseContent}");
                return null;
            }

            var workspaceResponse = JsonConvert.DeserializeObject<AribaWorkspaceResponseDto>(responseContent);

            if (workspaceResponse == null)
            {
                _logger.LogError("[ARIBA] Empty or invalid response received from Ariba API.");
                return null;
            }


            _logger.LogError($"[ARIBA] Successfully fetched workspace for ID: {WorkspaceID}");

            return workspaceResponse;
        }


        public async Task<string> PostRequisitionActionAsync(Guid domainId, string taskId, AribaActionRequestDto actionRequest, MyProfileDto aribaUser)
        {
            try
            {
                // 🔹 1. Fetch Ariba Config
                DomainSetting configSetting = _repoWrapper.DomainSetting
                    .FindFirstByCondition(ds => ds.DomainId == domainId &&
                                                ds.SettingsKey == Common.DOMAIN_SETTINGS_CONTRACT_URL &&
                                                ds.IsActive);

                if (configSetting == null)
                    throw new Exception("Missing Ariba config for domain.");

                AribaPurchaseOrderSettings config = JsonConvert.DeserializeObject<AribaPurchaseOrderSettings>(configSetting.SettingsValue);

                if (string.IsNullOrWhiteSpace(config.User) || string.IsNullOrWhiteSpace(config.PasswordAdapter))
                    throw new Exception("User or passwordAdapter not found in Ariba config.");


                DomainSetting tokenConfigSetting = _repoWrapper.DomainSetting
                    .FindFirstByCondition(ds => ds.DomainId == domainId &&
                                                ds.SettingsKey == Common.DOMAIN_SETTINGS_ARIBA_CONTRACT_TOKEN &&
                                                ds.IsActive);

                if (tokenConfigSetting == null)
                    throw new Exception("Missing Ariba Token config for domain.");

                AribaPurchaseOrderSettings tokenConfig = JsonConvert.DeserializeObject<AribaPurchaseOrderSettings>(tokenConfigSetting.SettingsValue);
                string token = await GetAribaToken(tokenConfig.AribaAuthToken, tokenConfig.AribaTokenUrl);

                if (aribaUser.Role == Common.BUSINESS_ADMIN_USER_KEY)
                {

                    if (!string.IsNullOrWhiteSpace(aribaUser.AribaUserName) &&
                        !string.IsNullOrWhiteSpace(aribaUser.AribaPassword))
                    {
                        _logger.LogInfo("Using MyProfileDto AribaUserName/Password (Business Admin)");
                    }
                    else
                    {

                        DomainSetting defaultUserSetting = _repoWrapper.DomainSetting.FindFirstByCondition(
                            ds => ds.DomainId == domainId &&
                                ds.SettingsKey == Common.DOMAIN_SETTINGS_CONTRACT_URL &&
                                ds.IsActive);

                        if (defaultUserSetting != null)
                        {
                            AribaPurchaseOrderSettings defaultUserConfig =
                                JsonConvert.DeserializeObject<AribaPurchaseOrderSettings>(defaultUserSetting.SettingsValue);

                            aribaUser.AribaUserName = defaultUserConfig?.AribaContractDefaultUser;
                            aribaUser.AribaPassword = defaultUserConfig.AribaContractDefaultPassword;

                            _logger.LogInfo("Using default BusinessAdmin Ariba credentials from config");
                        }
                    }
                }

                // 🔹 4. Build final URL
                string url = config.ContractAcceptRejectUrl;
                if (url.Contains("{realm}"))
                    url = url.Replace("{realm}", config.Realm);
                if (url.Contains("{user}"))
                    url = url.Replace("{user}", aribaUser.AribaUserName);
                if (url.Contains("{passwordAdapter}"))
                    url = url.Replace("{passwordAdapter}", aribaUser.AribaPassword);

                _logger.LogInfo($"Posting Ariba Task Action at {url}");

                // 🔹 5. Prepare request body
                if (string.IsNullOrEmpty(actionRequest.UniqueName))
                    actionRequest.UniqueName = taskId;

                string body = JsonConvert.SerializeObject(actionRequest);
                _logger.LogInfo($"POST Body: {body}");

                // 🔹 6. Send POST Request
                using (HttpClient client = new HttpClient())
                {
                    var content = new StringContent(body, Encoding.UTF8, "application/json");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    client.DefaultRequestHeaders.Add("apiKey", config.ApiKey);

                    HttpResponseMessage response = await client.PostAsync(url, content);
                    string responseContent = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError($"Error posting action: {responseContent}");
                        throw new PreConditionFailedCustomException($"Ariba Action failed: {responseContent}", $"Ariba Action failed: {responseContent}");
                    }
                    else
                    {

                        var pendingTasks = _repoWrapper.ContractPending.FindByCondition(
                            x => x.UniqueName == taskId &&
                                 x.Status == "Pending" &&
                                 x.Approver == aribaUser.AribaUserName).ToList();

                        pendingTasks.ForEach(x => x.Status = Common.ARIBA_APPROVED_STATUS_DISPLAY);
                        _repoWrapper.ContractPending.UpdateRange(pendingTasks);

                        await _repoWrapper.SaveAsync();
                    }

                    return responseContent;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception in PostRequisitionActionAsync: {ex.Message}");
                throw new PreConditionFailedCustomException($"Unable to Post Action in Ariba: {ex.Message}", $"Unable to Post Action in Ariba: {ex.Message}");
            }
        }

        public async Task<AribaAttachmentDTO> GetContractDocumentById(Guid domainId, string documentId, MyProfileDto aribauser)
        {
            try
            {
                var contractSetting = _repoWrapper.DomainSetting
                    .FindFirstByCondition(x =>
                        x.DomainId == domainId &&
                        x.SettingsKey == Common.DOMAIN_SETTINGS_ARIBA_CONTRACT_URL &&
                        x.IsActive);

                if (contractSetting == null)
                {
                    _logger.LogError($"[Domain: {domainId}] No contract URL settings found.");
                    return null;
                }

                var contractConfig = JsonConvert.DeserializeObject<AribaPurchaseOrderSettings>(contractSetting.SettingsValue);
                if (contractConfig == null)
                {
                    _logger.LogError($"[Domain: {domainId}] Failed to deserialize contract settings.");
                    return null;
                }

                var tokenSetting = _repoWrapper.DomainSetting
                    .FindFirstByCondition(x =>
                        x.DomainId == domainId &&
                        x.SettingsKey == Common.DOMAIN_SETTINGS_CONTRACT_CONFIG &&
                        x.IsActive);

                if (tokenSetting == null)
                {
                    _logger.LogError($"[Domain: {domainId}] No contract token settings found.");
                    return null;
                }

                var tokenConfig = JsonConvert.DeserializeObject<AribaPurchaseOrderSettings>(tokenSetting.SettingsValue);
                if (tokenConfig == null)
                {
                    _logger.LogError($"[Domain: {domainId}] Failed to deserialize token settings.");
                    return null;
                }

                string contractToken = await GetAribaToken(tokenConfig.AttachmentAuthToken, tokenConfig.AttachmentTokenUrl);
                if (string.IsNullOrWhiteSpace(contractToken))
                {
                    _logger.LogError($"[Domain: {domainId}] Failed to retrieve contract token.");
                    return null;
                }

                string url = contractConfig.ContractAttachmentUrl
                      .Replace("{documentId}", documentId)
                      .Replace("{user}", aribauser.AribaUserName)
                      .Replace("{passwordAdapter}", aribauser.AribaPassword)
                      .Replace("{Realm}", contractConfig.Realm);

                _logger.LogInfo($"[ARIBA] Final Document Download URL: {url}");

                using var client = new HttpClient
                {
                    Timeout = TimeSpan.FromMinutes(10)
                };

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", contractToken);
                request.Headers.Add("apiKey", contractConfig.AttachmentApiKey);

                HttpResponseMessage response = await client.SendAsync(request);
                var contentBytes = await response.Content.ReadAsByteArrayAsync();

                if (!response.IsSuccessStatusCode)
                {
                    string errorText = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"[Domain: {domainId}] Failed to download document {documentId}: {response.StatusCode} - {errorText}");
                    return null;
                }

                _logger.LogInfo($"[Domain: {domainId}] Successfully downloaded document {documentId}. File size: {contentBytes.Length} bytes");


                    return new AribaAttachmentDTO
            {
                attachmentId = documentId,
                Base64File = Convert.ToBase64String(contentBytes),
                FileName = $"{documentId}.pdf"   
            };

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetContractDocumentById for DocumentId={documentId}, Domain={domainId}: {ex}");
                return null;
            }
        }

        public async Task<List<AribaContractDto>> GetContractPendingApprovalsAsync(
    Guid userId,
    Guid domainId,
    int index,
    int limit,
    string? searchTerm = null,
    DateTime? fromDate = null,
    DateTime? toDate = null,
    string? status = null)
        {
            _logger.LogInfo($"Fetching contract pending approvals for UserId: {userId}, DomainId: {domainId}");

            MyProfileDto? profileDto = _repoWrapper.Person.GetMyProfileByUserId(userId);
            string aribaUsername = profileDto?.AribaUserName;
            string role = profileDto?.Role;

            // Step 1: Base query
            var query =
                from task in _repoWrapper.ContractPending
                    .FindByConditionAsync(x => x.DomainId == domainId)
                join contract in _repoWrapper.ContractDetails
                    .FindByCondition(x => x.IsActive)
                    on task.UniqueName equals contract.ContractTaskId
                select new { task, contract };

            // Step 2: Role-based filtering
            if (role != Common.BUSINESS_ADMIN_USER_KEY)
            {
                if (string.IsNullOrEmpty(aribaUsername))
                {
                    _logger.LogError("Ariba username not found");
                    throw new NotFoundCustomException("Unable to Find Any Records", "Unable to Find Any Records");
                }

                query = query.Where(x => x.task.Approver == aribaUsername);
            }

            // Step 3: Status filter
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

            // Step 4: Search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(x =>
                    (x.contract.SupplierName != null && x.contract.SupplierName.Contains(searchTerm)) ||
                    (x.contract.ContractTaskId != null && x.contract.ContractTaskId.Contains(searchTerm)) ||
                    (x.task.Approver != null && x.task.Approver.Contains(searchTerm))
                );
            }

            // Step 5: Date filter
            if (fromDate.HasValue)
                query = query.Where(x => x.contract.BeginDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(x => x.contract.BeginDate <= toDate.Value);

            // Step 6: Pagination
            var pagedData = await query
                .OrderByDescending(x => x.task.DateUpdated)
                .Skip(index)
                .Take(limit)
                .ToListAsync();

            if (!pagedData.Any())
            {
                throw new NotFoundCustomException("Unable to Find Any Contracts", "Unable to Find Any Contracts");
            }

            // Step 7: Bulk fetch approval history
            var taskIds = pagedData.Select(x => x.contract.ContractTaskId).Distinct().ToList();

            var approvalHistoryDict = (await _repoWrapper.AribaContractHistory
                    .FindByConditionAsync(x => x.IsActive && taskIds.Contains(x.TaskId))
                    .ToListAsync())
                .GroupBy(x => x.TaskId)
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.DateCreated).ToList());

            // Step 8: Build DTO
            List<AribaContractDto> result = new();

            foreach (var item in pagedData)
            {
                var dto = new AribaContractDto
                {
                    ContractTaskId = item.contract.ContractTaskId,
                    ContractId = item.contract.ContractId,
                    BeginDate = item.contract.BeginDate,
                    EndDate = item.contract.EndDate,
                    TotalAmount = item.contract.TotalAmount,
                    Currency = item.contract.Currency,
                    WorkspaceID = item.contract.WorkspaceID,
               
                    Status = item.task.Status,
                    Description = item.contract.Description,
                    SupplierID = item.contract.SupplierID,
                    SupplierName = item.contract.SupplierName,
                    CompanyUniqueName = item.contract.CompanyUniqueName,
                    Department = item.contract.Department,
                    HasAttachment = item.contract.IsAttachmentProcessed,
                    FullUrl = item.task.FullUrl,
                    Approver =  item.task.Approver,
                               
                    ApprovalHistory = approvalHistoryDict.TryGetValue(item.contract.ContractTaskId, out var history)
                        ? history
                        : new List<AribaContractHistory>()
                };
                if (role == Common.BUSINESS_ADMIN_USER_KEY || role == Common.BUSINESS_USER_KEY)
                {
                    dto.Approver = item.task.Approver;
                }

                result.Add(dto);
            }

            return result;
        }
        public SplitAttachmentResponseDto GetFinalContractAttachmentBase64(string contractTaskId)
        {
            _logger.LogInfo($"Fetching the Attachments for the Contract with TaskId : {contractTaskId}");

            if (string.IsNullOrWhiteSpace(contractTaskId))
                throw new PreConditionFailedCustomException(
                    "Contract Task ID cannot be null or empty",
                    "Contract Task ID cannot be null or empty");

            List<FinalContractAttachment> attachments = _repoWrapper.FinalContractAttachment
                .FindByCondition(f =>
                    f.AribaId.ToLower() == contractTaskId.ToLower() &&
                    f.IsActive)
                .ToList();

            if (attachments == null || attachments.Count == 0)
                throw new NotFoundCustomException(
                    $"No final attachment found for Contract Task ID: {contractTaskId}",
                    $"No final attachment found for Contract Task ID: {contractTaskId}");

            SplitAttachmentResponseDto result = new SplitAttachmentResponseDto
            {

                ContractAttachments = attachments
                    .Where(a => a.type != null &&
                                a.type.Equals("ARIBA_CONTRACT", StringComparison.OrdinalIgnoreCase))
                    .Select(a => Convert.ToBase64String(a.Attachment))
                    .ToList()
            };

            return result;
        }




        public async Task<List<AribaContractDto>> GetPendingApprovalsDetails(
            Guid userId, Guid domainId, int index, int limit,
            string? searchTerm, DateTime? fromDate, DateTime? toDate,
            string? status)
        {
            MyProfileDto? profileDto = _repoWrapper.Person.GetMyProfileByUserId(userId);

            string aribausername = profileDto.AribaUserName;
            string role = profileDto.Role;

            // Step 1: Build contract query with task join
            var query = from task in _repoWrapper.ContractPending
                            .FindByConditionAsync(x => x.DomainId == domainId)
                        join contract in _repoWrapper.ContractDetails.FindByCondition(x => x.IsActive)
                            on task.UniqueName equals contract.ContractTaskId
                        select new { task, contract };

            // Step 2: Role / approver filter
            if (role != Common.BUSINESS_ADMIN_USER_KEY)
            {
                query = query.Where(x => x.task.Approver == aribausername);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string lowerSearchTerm = searchTerm.ToLower();
                query = query.Where(x =>
                    (x.contract.SupplierName != null && x.contract.SupplierName.ToLower().Contains(lowerSearchTerm)) ||
                    (x.contract.ContractTaskId != null && x.contract.ContractTaskId.ToLower().Contains(lowerSearchTerm)) ||
                    (x.task.Approver != null && x.task.Approver.ToLower().Contains(lowerSearchTerm))
                );
            }
            if (!string.IsNullOrWhiteSpace(status) &&
    !status.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                string lowerStatus = status.ToLower();

                query = query.Where(x =>
                    (
                        x.task.Status.ToLower() == Common.STATUS_APPROVE.ToLower() &&
                        lowerStatus == Common.STATUS_CONTRACT_APPROVE.ToLower()
                    )
                    ||
                    (
                        x.task.Status.ToLower() == Common.STATUS_DENY.ToLower() &&
                        lowerStatus == Common.STATUS_CONTRACT_DENY.ToLower()
                    )
                    ||
                    (
                        x.task.Status.ToLower() == Common.STATUS_ARIBA_APPROVE.ToLower() &&
                        lowerStatus == Common.ARIBA_APPROVED_STATUS_DISPLAY.ToLower()
                    )
                    ||
                    x.task.Status.ToLower() == lowerStatus
                );
            }


            if (fromDate.HasValue)
            {
                query = query.Where(x => x.contract.BeginDate >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                query = query.Where(x => x.contract.BeginDate <= toDate.Value);
            }

            query = query.OrderByDescending(x => x.task.DateUpdated).Skip(index).Take(limit);

            var pagedData = query.ToList();

            List<AribaContractDto> aribaContract = new List<AribaContractDto>();
            foreach (var item in pagedData)
            {
                var contract = item.contract;
                var task = item.task;

                var dto = new AribaContractDto
                {
                    ContractTaskId = contract.ContractTaskId,
                    ContractId = contract.ContractId,
                    BeginDate = contract.BeginDate,
                    EndDate = contract.EndDate,
                    TotalAmount = contract.TotalAmount,
                    Currency = contract.Currency,
                    WorkspaceID = contract.WorkspaceID,
                   
                    Status = task.Status == Common.STATUS_APPROVE ? Common.STATUS_CONTRACT_APPROVE :
                              task.Status == Common.STATUS_ARIBA_APPROVE ? Common.STATUS_CONTRACT_APPROVE :
                             task.Status == Common.STATUS_DENY || task.Status == Common.STATUS_ARIBA_DENY ? Common.STATUS_CONTRACT_DENY : task.Status,
                    Description = contract.Description,
                    SupplierID = contract.SupplierID,
                    SupplierName = contract.SupplierName,
                    FullUrl = task.FullUrl,
                    CompanyUniqueName = contract.CompanyUniqueName,
                    Department = contract.Department
                };

                if (role == Common.BUSINESS_ADMIN_USER_KEY || role == Common.BUSINESS_USER_KEY)
                {
                    dto.Approver = task.Approver;
                }

                dto.ApprovalHistory = _repoWrapper.AribaContractHistory
                    .FindByCondition(x => x.IsActive && x.TaskId == dto.ContractTaskId)
                    .OrderBy(x => x.DateCreated).ToList();

                aribaContract.Add(dto);
            }
            if (!aribaContract.Any())
            {
                _logger.LogError($"Unable to find Any Contracts ");
                throw new NotFoundCustomException("Unable to Find Any Contract", "Unable to Find Any Contracts");
            }

            return aribaContract;
        }

        public async Task<ExportExcelResultDto> ExportContractPendingApprovalsExcelAsync(
           Guid userId, Guid domainId, int limit,
           string? searchTerm, DateTime? fromDate, DateTime? toDate,
           string? status)
        {
            if (limit > 500)
                throw new PreConditionFailedCustomException("Limit cannot exceed 500 records", "Limit cannot exceed 500 records");

            var aribaContract = await GetPendingApprovalsDetails(
                      userId, domainId, 0, limit, searchTerm, fromDate, toDate, status);

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("Pending Approvals");

            if (!aribaContract.Any())
            {
                ws.Cell(1, 1).Value = "No Records Found";

                using var emptyStream = new MemoryStream();
                workbook.SaveAs(emptyStream);

                return new ExportExcelResultDto
                {
                    FileContent = emptyStream.ToArray(),
                    FileName = $"PendingApprovalsDetails_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };
            }

            var exportData = aribaContract.Select(x => new
            {
                ContractTaskId = x.ContractTaskId,
                ContractId = x.ContractId,
                BeginDate = x.BeginDate,
                EndDate = x.EndDate,
                TotalAmount = x.TotalAmount,
                Currency = x.Currency,
                WorkspaceID = x.WorkspaceID,
                AttachmentID = x.AttachmentID,
                Status = x.Status,
                Description = x.Description,
                SupplierID = x.SupplierID,
                SupplierName = x.SupplierName,
                FullUrl = x.FullUrl,
                CompanyUniqueName = x.CompanyUniqueName,
                Department = x.Department
            }).ToList();

            ws.Cell(1, 1).InsertTable(exportData);
            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            string fileName = $"PendingApprovalsDetails_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return new ExportExcelResultDto
            {
                FileContent = stream.ToArray(),
                FileName = fileName
            };
        }
         public async Task<List<AribaContractDto>> GetPendingContractsByUserId(
    DateTime fromDate,
    DateTime toDate,
    string token,
    string apiKey,
    string realm,
    string ContractPendingApprovablesUrl,
    string ContractWorkspaceUrl,
    string aribaPassword,
    string aribaUserName,Guid domainId,
    string taskId)
        {
            _logger.LogInfo($"[AribaContract] Fetching pending contract approvables for realm {realm} from {fromDate} to {toDate}.");
                        //string filterString = $"$filter=approvableType eq 'requisitions'";
            string filterString = $"$filter=user eq '{aribaUserName}' and passwordAdapter eq '{aribaPassword}' and approvableType eq 'requisitions'";

            // string filterString = "$filter=documentType eq 'ariba.collaborate.core.Task'";
            string url = ContractPendingApprovablesUrl;

            if (!url.Contains("?"))
                url += "?";
            else if (!url.EndsWith("&"))
                url += "&";

               url += $"{filterString}&realm={realm}";
            _logger.LogInfo($"Calling Ariba Pending Contracts API: {url}");

            using var client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("apiKey", apiKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Ariba API error: {response.StatusCode} - {responseContent}");

            // Parse JSON
            List<ContractTaskDto> tasks = new List<ContractTaskDto>();
            JToken json = JsonConvert.DeserializeObject<JToken>(responseContent);

            if (json.Type == JTokenType.Object && json["value"] != null)
            {
                if (json["value"].Type == JTokenType.Array)
                    tasks = json["value"].ToObject<List<ContractTaskDto>>();
                else
                    tasks.Add(json["value"].ToObject<ContractTaskDto>());
            }
            else if (json.Type == JTokenType.Array)
            {
                tasks = json.ToObject<List<ContractTaskDto>>();
            }

            if (tasks.Count == 0)
                return new List<AribaContractDto>();

            if (!string.IsNullOrEmpty(taskId))
                tasks = tasks.Where(x => x.UniqueName == taskId).ToList();

            List<ContractDetailDto> contracts = new List<ContractDetailDto>();
            foreach (var t in tasks)
            {
                try
                {
                    if (!IsContract(t.Description))
                    {
                        _logger.LogInfo($"Skipping pending task (not contract): {t.UniqueName} - {t.Description}");
                        continue;
                    }

                    ContractPending taskExists = _repoWrapper.ContractPending.FindFirstByCondition(x =>
                        x.IsActive &&
                        x.UniqueName == t.UniqueName &&
                        x.Approver == t.Approver &&
                        x.AssignedDate == t.AssignedDate);

                    if (taskExists == null)
                    {
                        ContractPending newTask = new ContractPending
                        {
                            Id = Guid.NewGuid(),
                            UniqueName = t.UniqueName,
                            Description = t.Description,
                            Approver = t.Approver,
                            Email = t.Email,
                            AssignedDate = t.AssignedDate,
                            DomainId = domainId,
                            Status = Common.STATUS_PENDING,
                            FullUrl = t.FullUrl,
                            DocumentType = t.DocumentType ?? "ariba.collaborate.core.Task"
                        };

                        await _repoWrapper.ContractPending.CreateAsync(newTask);


                        var contract = await GetContractTaskById(t.UniqueName, token, apiKey, realm,
                            ContractWorkspaceUrl.Replace("{TaskId}", t.UniqueName));
                        if (contract != null)
                        {
                            if (contract.Document?.Content?.SubContentDoc != null)
                            {
                                var newAttachments = new List<ContractAttachment>();

                                foreach (var doc in contract.Document.Content.SubContentDoc)
                                {
                                    if (string.IsNullOrEmpty(doc.Id))
                                        continue;

                                    var existing = _repoWrapper.ContractAttachment
                                        .FindFirstByCondition(x =>
                                            x.IsActive &&
                                            x.TaskId == t.UniqueName &&
                                            x.AttachmentId == doc.Id);

                                    if (existing == null)
                                    {
                                        newAttachments.Add(new ContractAttachment
                                        {
                                            Id = Guid.NewGuid(),
                                            TaskId = t.UniqueName,
                                            AttachmentId = doc.Id
                                        });
                                    }
                                }

                                if (newAttachments.Any())
                                    await _repoWrapper.ContractAttachment.CreateRangeAsync(newAttachments);
                            }


                            if (!string.IsNullOrEmpty(contract.WorkspaceId))
                            {
                                try
                                {
                                    var workspaceDetails = await GetContractWorkspaceById(
                                        contract.WorkspaceId,
                                        domainId);

                                    if (workspaceDetails != null)
                                    {

                                        contract.WorkspaceDetails = workspaceDetails;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError($"Failed to fetch workspace details for {contract.WorkspaceId}: {ex.Message}");
                                }
                            }
                        }
                        contracts.Add(contract);

                        await _repoWrapper.SaveAsync();
                    }
                    else
                    {
                        // Update task
                        taskExists.Description = t.Description;
                        taskExists.Approver = t.Approver;
                        taskExists.Email = t.Email;
                        taskExists.AssignedDate = t.AssignedDate;
                        taskExists.DocumentType = t.DocumentType;
                        _repoWrapper.ContractPending.Update(taskExists);



                        var contract = await GetContractTaskById(t.UniqueName, token, apiKey, realm,
                            ContractWorkspaceUrl.Replace("{TaskId}", t.UniqueName));
                        if (contract != null)
                        {


                            if (contract.Document?.Content?.SubContentDoc != null)
                            {
                                var newAttachments = new List<ContractAttachment>();

                                foreach (var doc in contract.Document.Content.SubContentDoc)
                                {
                                    if (string.IsNullOrEmpty(doc.Id))
                                        continue;

                                    var existing = _repoWrapper.ContractAttachment
                                        .FindFirstByCondition(x =>
                                            x.IsActive &&
                                            x.TaskId == t.UniqueName &&
                                            x.AttachmentId == doc.Id);

                                    if (existing == null)
                                    {
                                        newAttachments.Add(new ContractAttachment
                                        {
                                            Id = Guid.NewGuid(),
                                            TaskId = t.UniqueName,
                                            AttachmentId = doc.Id,

                                        });
                                    }
                                }

                                if (newAttachments.Any())
                                    await _repoWrapper.ContractAttachment.CreateRangeAsync(newAttachments);
                            }


                            if (!string.IsNullOrEmpty(contract.WorkspaceId))
                            {
                                try
                                {
                                    var workspaceDetails = await GetContractWorkspaceById(
                                        contract.WorkspaceId,
                                        domainId);

                                    if (workspaceDetails != null)
                                    {

                                        contract.WorkspaceDetails = workspaceDetails;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError($"Failed to fetch workspace details for {contract.WorkspaceId}: {ex.Message}");
                                }
                            }
                        }
                        contracts.Add(contract);
                        await _repoWrapper.SaveAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to fetch contract for task={t.UniqueName}: {ex.Message}");
                }
            }

            try
            {
                List<string> insertedapprovablehistories = new List<string>();
                if (contracts.Count > 0)
                {
                    foreach (var c in contracts)
                    {
                        var existingHistories = _repoWrapper.AribaContractHistory
                            .FindByCondition(x => x.IsActive && x.TaskId == c.Id)
                            .ToList();

                        if (existingHistories.Any())
                        {
                            existingHistories.ForEach(x => x.IsActive = false);
                            _repoWrapper.AribaContractHistory.UpdateRange(existingHistories);
                        }

                        if (c.ApprovalRequests != null)
                        {
                            var newApprovalHistories = new List<AribaContractHistory>();
                            int approvedLevelCounter = 0;

                            foreach (var req in c.ApprovalRequests)
                            {
                                var newHistory = new AribaContractHistory
                                {
                                    Id = Guid.NewGuid(),
                                    TaskId = req.TaskId,
                                    // Comments = req.Comments?
                                    //     .Where(c => c.TaskId == req.TaskId)
                                    //     .Select(c => c.Body)
                                    //     .FirstOrDefault(),
                                    Status = req.ApprovalState,
                                    IsActive = true,
                                    AribaUser = req.Approvers?.FirstOrDefault()?.User?.UniqueName
                                };


                                if (req.ApprovalState == "Approved")
                                {
                                    newHistory.Status = Common.STATUS_ARIBA_APPROVE;
                                    newHistory.ApprovalDate = DateTimeOffset.FromUnixTimeMilliseconds(req.ActivationDate).UtcDateTime;
                                    approvedLevelCounter++;
                                    newHistory.Level = "Level " + approvedLevelCounter;
                                    newHistory.AribaUser = req.Approvers?.FirstOrDefault()?.User?.UniqueName;


                                }
                                else if (req.ApprovalState == "Ready for approval")
                                {
                                    newHistory.Status = Common.STATUS_PENDING;
                                    approvedLevelCounter++;
                                    newHistory.Level = "Level " + approvedLevelCounter;
                                    newHistory.AribaUser = req.Approvers?.FirstOrDefault()?.User?.UniqueName;
                                }
                                else
                                {

                                    newHistory.AribaUser = req.Approvers?.FirstOrDefault()?.User?.UniqueName;
                                }
                                insertedapprovablehistories.Add(c.Id);
                                newApprovalHistories.Add(newHistory);
                            }
                            await _repoWrapper.AribaContractHistory.CreateRangeAsync(newApprovalHistories);
                        }
                    }

                    await _repoWrapper.SaveAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to insert contract history: {ex.Message}");
            }

            return GroupTasksIntoContracts(tasks, contracts);
        }

    }
}
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
    public class AribaService : IAribaService
    {
        private readonly IRepositoryWrapper _repoWrapper;

        private readonly ILoggerManager _logger;
        private readonly IDomainService _domainService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;

        private static AribaToken _cachedToken;
        private static readonly Dictionary<Guid, (AribaToken Token, DateTime Expiry)> _tokenCache = new();
        private static DateTime _tokenExpiry;
        /// <param name="repoWrapper">Repository wrapper.</param>
        /// <param name="logger">Logger manager.</param>

        public AribaService(IRepositoryWrapper repoWrapper, ILoggerManager logger, IDomainService domainService, IHttpContextAccessor httpContextAccessor, IEmailService emailService)
        {
            _repoWrapper = repoWrapper;
            _logger = logger;
            _domainService = domainService;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
        }

        public async Task<ExternalReceiptImportReply> PostPoDetailsToAriba(PurchaseOrderRqToAribaDto poRequest)
        {
            var ret = await SendPORequestAsync(poRequest, poRequest.AribaEndPoint, poRequest.Username, poRequest.Password);

            return ret;
        }

        public async Task<ExternalReceiptImportReply> SendPORequestAsync(PurchaseOrderRqToAribaDto request, string? aribaUrl, string? username, string? password)
        {

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("SOAPAction", "ExternalReceiptImport");

                var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

                // SOAPAction header (Ariba specific)
                client.DefaultRequestHeaders.Add("SOAPAction", "ExternalReceiptImport");

                string cont = BuildContractRequest(request);
                var content = new StringContent(cont, Encoding.UTF8, "text/xml");
                try
                {
                    var response = await client.PostAsync(aribaUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseXml = await response.Content.ReadAsStringAsync();

                        // Deserialize response
                        var serializer = new XmlSerializer(typeof(AribaSoapEnvelope));

                        using (var reader = new StringReader(responseXml))
                        {
                            if (responseXml.Contains("<Fault") || responseXml.Contains("errors") || !responseXml.Contains("<StatusString>Submitted</StatusString>"))
                            {
                                throw new Exception("Error: " + responseXml);
                            }

                            var doc = XDocument.Parse(responseXml);
                            var cleanXml = XmlNamespaceRemover.RemoveAllNamespaces(doc.Root);

                            var serializerSuccess = new XmlSerializer(typeof(AribaSoapEnvelope));
                            using var readerSuccess = cleanXml.CreateReader();

                            var envelope = (AribaSoapEnvelope)serializer.Deserialize(readerSuccess);
                            return envelope.Body.Reply; // contains the Items list
                        }
                    }
                    else
                    {
                        _logger.LogError("failed to post the ariba data with status code" + response.StatusCode.ToString());
                        throw new Exception("failed to post the ariba data with status code" + response.StatusCode.ToString());
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    throw new Exception("failed to post the ariba data", ex);
                }

            }
        }


        public static string GenerateERPReceiptNumber()
        {
            var random = new Random();
            return "ERP" + random.Next(10000, 99990).ToString();
        }
        private static string BuildContractRequest(PurchaseOrderRqToAribaDto poRequest)
        {
            // Generate unique ERP Receipt Number
            var erpReceiptNumber = GenerateERPReceiptNumber();
            string date = poRequest.Date != null ? poRequest.Date.Value.ToString("yyyyMMdd") : DateTime.UtcNow.ToString("yyyyMMdd");

            // Build ExternalReceiptItems dynamically
            var externalReceiptItemsXml = new StringBuilder();
            foreach (var lineItem in poRequest.LineItems)
            {
                externalReceiptItemsXml.Append($@"
            <urn:item>
                <urn:AmountAccepted>0</urn:AmountAccepted>
                <urn:AmountCurrency></urn:AmountCurrency>
                <urn:AmountRejected>0</urn:AmountRejected>
                <urn:Comment>Reposting Via BOT</urn:Comment>
                <urn:IsReceivingByCount>true</urn:IsReceivingByCount>
                <urn:NumberAccepted>{(double)(lineItem.AcceptedQuantity ?? null)}</urn:NumberAccepted>
                <urn:ReceivableLineItemId>{lineItem.LineItem}</urn:ReceivableLineItemId>
                <urn:ReceivedDate>{date}</urn:ReceivedDate>
                <urn:ReceivingType>1</urn:ReceivingType>
            </urn:item>");
            }

            // Plug everything into the SOAP envelope
            return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:urn=""urn:Ariba:Buyer:{poRequest.Server}"">
                <soapenv:Header>
                    <urn:Headers>
                        <urn:variant>{poRequest.Variant}</urn:variant>
                        <urn:partition>{poRequest.Partition}</urn:partition>
                    </urn:Headers>
                </soapenv:Header>
                <soapenv:Body>
                    <urn:ExternalReceiptImportRequest partition=""{poRequest.Partition}"" variant=""{poRequest.Variant}"">
                        <urn:ExternalReceiptInputBean_Item>
                            <urn:item>
                                <urn:ERPReceiptNumber>{erpReceiptNumber}</urn:ERPReceiptNumber>
                                <urn:ExternalReceiptItems>
                                    {externalReceiptItemsXml}
                                </urn:ExternalReceiptItems>
                                <urn:OriginatingSystem>ERP</urn:OriginatingSystem>
                                <urn:ReceivableId>{poRequest.ReceivableId}</urn:ReceivableId>
                            </urn:item>
                        </urn:ExternalReceiptInputBean_Item>
                    </urn:ExternalReceiptImportRequest>
                </soapenv:Body>
              </soapenv:Envelope>";
        }

        private async Task<AribaToken> GenerateAribaToken(AribaPurchaseOrderSettings dto)
        {
            _logger.LogInfo("Generating new Ariba token...");
            HttpClientHandler handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                };

            HttpClient client = new HttpClient(handler);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, dto.AribaTokenUrl);

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", dto.AribaAuthToken);

            request.Content = new FormUrlEncodedContent(new[]
            {
             new KeyValuePair<string, string>("grant_type", "openapi_2lo")});

            _logger.LogInfo($"Sending token request to {dto.AribaTokenUrl}");
            HttpResponseMessage response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string tokenResponse = await response.Content.ReadAsStringAsync();
            AribaToken responseDTO = JsonConvert.DeserializeObject<AribaToken>(tokenResponse);

            if (string.IsNullOrEmpty(responseDTO?.access_token))
                throw new Exception("Failed to retrieve Ariba access token.");

            _logger.LogInfo("Ariba token retrieved successfully.");
            return responseDTO;
        }



        public async Task<string> GetValidToken(Guid domainId, AribaPurchaseOrderSettings config)
        {
            if (_tokenCache.TryGetValue(domainId, out var cached) && DateTime.UtcNow < cached.Expiry)
            {
                _logger.LogInfo($"Using cached Ariba token for domain {domainId}.");
                return cached.Token.access_token;
            }

            _logger.LogInfo($"Fetching new Ariba token for domain {domainId}.");
            AribaToken newToken = await GenerateAribaToken(config);
            DateTime expiry = DateTime.UtcNow.AddSeconds(newToken.expires_in - 60);

            _tokenCache[domainId] = (newToken, expiry);

            return newToken.access_token;
        }



        public async Task<List<AribaInvoiceDto>> GetPendingApprovables(
         DateTime fromDate,
         DateTime toDate,
         string token,
         string apiKey,
         string realm,
         string pendingApprovablesUrl,
         string requisitionDetailsUrl,
         Guid domainid,
         string TaskId)
        {


            _logger.LogInfo($"Fetching pending approvables for realm {realm} from {fromDate} to {toDate}.");

            // Build filter JSON
            string filterString = $"$filter=approvableType eq 'invoices'";

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
            List<AribaTask> tasks = new List<AribaTask>();
            JToken json = JsonConvert.DeserializeObject<JToken>(responseContent);

            if (json.Type == JTokenType.Object && json["value"] != null)
            {
                if (json["value"].Type == JTokenType.Array)
                    tasks = json["value"].ToObject<List<AribaTask>>();
                else
                    tasks.Add(json["value"].ToObject<AribaTask>());
            }
            else if (json.Type == JTokenType.Array)
            {
                tasks = json.ToObject<List<AribaTask>>();
            }

            if (tasks.Count == 0)
                return new List<AribaInvoiceDto>();

            // --- INVOICES ---
            tasks = tasks.Where(x => x.approvableType == "invoices").ToList();
            if (TaskId != "")
                tasks = tasks.Where(x => x.approvableType == "invoices" && x.approvableId == TaskId).ToList();

            List<RequisitionDetailDto> requisitions = new List<RequisitionDetailDto>();
            foreach (var t in tasks)
            {
                try
                {
                    AribaTasks taskexists = _repoWrapper.AribaTask.FindFirstByCondition(x =>
                        x.IsActive &&
                        x.ApprovableUniqueName == t.approvableUniqueName &&
                        x.Approver == t.approver &&
                        x.AssignedDate == t.assignedDate);

                    if (taskexists == null)
                    {

                        AribaTasks task = new AribaTasks();

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


                        await _repoWrapper.AribaTask.CreateAsync(task);

                        var req = await GetRequisitionById(t.approvableId, token, apiKey, realm,
                            requisitionDetailsUrl.Replace("{invoiceId}", task.ApprovalId));

                        if (req != null)
                        {
                            requisitions.Add(req);

                            if (req.Attachments != null && req.Attachments.Count > 0)
                            {
                                List<AribaAttachment> attachments = new List<AribaAttachment>();
                                foreach (var att in req.Attachments)
                                {
                                    var exists = _repoWrapper.AribaAttachment.FindFirstByCondition(x =>
                                        x.IsActive &&
                                        x.AribaTaskId == t.approvableId &&
                                        x.InvoiceID == t.approvableUniqueName &&
                                        x.AttachmentId == att.Id);

                                    if (exists == null)
                                    {
                                        attachments.Add(new AribaAttachment
                                        {
                                            Id = Guid.NewGuid(),
                                            AttachmentId = att.Id,
                                            InvoiceID = t.approvableUniqueName,
                                            AribaTaskId = t.approvableId
                                        });
                                    }
                                }

                                if (attachments.Any())
                                    await _repoWrapper.AribaAttachment.CreateRangeAsync(attachments);
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
                        _repoWrapper.AribaTask.Update(taskexists);

                        var req = await GetRequisitionById(t.approvableId, token, apiKey, realm,
                            requisitionDetailsUrl.Replace("{invoiceId}", taskexists.ApprovalId));
                        if (req != null)
                        {
                            requisitions.Add(req);

                            if (req.Attachments != null)
                            {
                                List<AribaAttachment> newAtt = new List<AribaAttachment>();
                                List<AribaAttachment> updateAtt = new List<AribaAttachment>();

                                foreach (var a in req.Attachments)
                                {
                                    var ex = _repoWrapper.AribaAttachment.FindFirstByCondition(x =>
                                        x.IsActive &&
                                        x.AribaTaskId == t.approvableId &&
                                        x.InvoiceID == t.approvableUniqueName &&
                                        x.AttachmentId == a.Id);

                                    if (ex != null)
                                    {
                                        updateAtt.Add(ex);
                                    }
                                    else
                                    {
                                        newAtt.Add(new AribaAttachment
                                        {
                                            Id = Guid.NewGuid(),
                                            AttachmentId = a.Id,
                                            InvoiceID = t.approvableUniqueName,
                                            AribaTaskId = t.approvableId
                                        });
                                    }
                                }

                                if (newAtt.Any())
                                    await _repoWrapper.AribaAttachment.CreateRangeAsync(newAtt);

                                if (updateAtt.Any())
                                    _repoWrapper.AribaAttachment.UpdateRange(updateAtt);
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
                            var existingApprovalHistories = _repoWrapper.AribaInvoiceApprovalHistory
                                .FindByCondition(x => x.IsActive && x.AribaTaskId == a.ApprovableId && x.InvoiceID == a.UniqueName)
                                .AsNoTracking()
                                .ToList();

                            if (existingApprovalHistories.Any())
                            {
                                existingApprovalHistories.ForEach(x => x.IsActive = false);
                                _repoWrapper.AribaInvoiceApprovalHistory.UpdateRange(existingApprovalHistories);
                            }

                            var approvalRequests = a.ApprovalRequests
                                .OrderBy(x => x.ActivationDate == null)
                                .ThenBy(x => x.ActivationDate)
                                .ToList();

                            var newApprovalHistories = new List<AribaInvoiceApprovalHistory>();
                            int approvedLevelCounter = 0;

                            foreach (var approvalrequest in approvalRequests)
                            {
                                var newHistory = new AribaInvoiceApprovalHistory
                                {
                                    Id = Guid.NewGuid(),
                                    InvoiceID = a.UniqueName,
                                    AribaTaskId = a.ApprovableId,
                                    Comments = approvalrequest.ApproverComment,
                                    Status = approvalrequest.State,
                                    IsActive = true
                                };

                                if (approvalrequest.State == "Approved")
                                {
                                    newHistory.Status = Common.ARIBA_APPROVED_STATUS_DISPLAY;
                                    newHistory.ApprovalDate = approvalrequest.LastModified;
                                    approvedLevelCounter++;
                                    newHistory.Level = "Level " + approvedLevelCounter;
                                    newHistory.AribaUser = approvalrequest.ApprovedBy?.UniqueName;
                                }
                                else if (approvalrequest.State == "ActiveButNoActionYet")
                                {
                                    newHistory.Status = Common.STATUS_PENDING;
                                    approvedLevelCounter++;
                                    newHistory.Level = "Level " + approvedLevelCounter;
                                    newHistory.AribaUser = approvalrequest.Approver?.UniqueName;
                                }
                                else
                                {
                                    newHistory.AribaUser = approvalrequest.Approver?.UniqueName;
                                }
                                insertedapprovablehistories.Add(a.ApprovableId);
                                newApprovalHistories.Add(newHistory);
                            }
                            await _repoWrapper.AribaInvoiceApprovalHistory.CreateRangeAsync(newApprovalHistories);
                        }
                    }
                }
                await _repoWrapper.SaveAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to insert invoice history for : {ex.Message}");
            }

            return GroupTasksIntoInvoices(tasks, requisitions);
        }

        public async Task<List<AribaInvoiceDto>> GetPendingApprovablesByUserId(
            DateTime fromDate,
            DateTime toDate,
            string token,
            string apiKey,
            string realm,
            string pendingApprovablesUrl,
            string requisitionDetailsUrl,
            Guid domainid,
            string TaskId,
            string userName,
            string password)
        {


            _logger.LogInfo($"Fetching pending approvables for realm {realm} from {fromDate} to {toDate}.");

            // Build filter JSON
            string filterString = $"$filter=user eq '{userName}' and passwordAdapter eq '{password}' and approvableType eq 'invoices'";

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
            List<AribaTask> tasks = new List<AribaTask>();
            JToken json = JsonConvert.DeserializeObject<JToken>(responseContent);

            if (json.Type == JTokenType.Object && json["value"] != null)
            {
                if (json["value"].Type == JTokenType.Array)
                    tasks = json["value"].ToObject<List<AribaTask>>();
                else
                    tasks.Add(json["value"].ToObject<AribaTask>());
            }
            else if (json.Type == JTokenType.Array)
            {
                tasks = json.ToObject<List<AribaTask>>();
            }

            if (tasks.Count == 0)
                return new List<AribaInvoiceDto>();

            // --- INVOICES ---
            tasks = tasks.Where(x => x.approvableType == "invoices").ToList();
            if (TaskId != "")
                tasks = tasks.Where(x => x.approvableType == "invoices" && x.approvableId == TaskId).ToList();

            List<RequisitionDetailDto> requisitions = new List<RequisitionDetailDto>();
            foreach (var t in tasks)
            {
                try
                {
                    AribaTasks taskexists = _repoWrapper.AribaTask.FindFirstByCondition(x =>
                        x.IsActive &&
                        x.ApprovableUniqueName == t.approvableUniqueName &&
                        x.Approver == t.approver &&
                        x.AssignedDate == t.assignedDate);

                    if (taskexists == null)
                    {

                        AribaTasks task = new AribaTasks();

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


                        await _repoWrapper.AribaTask.CreateAsync(task);

                        var req = await GetRequisitionById(t.approvableId, token, apiKey, realm,
                            requisitionDetailsUrl.Replace("{invoiceId}", task.ApprovalId));
                        if (req != null)
                        {
                            requisitions.Add(req);

                            if (req.Attachments != null && req.Attachments.Count > 0)
                            {
                                List<AribaAttachment> attachments = new List<AribaAttachment>();
                                foreach (var att in req.Attachments)
                                {
                                    var exists = _repoWrapper.AribaAttachment.FindFirstByCondition(x =>
                                        x.IsActive &&
                                        x.AribaTaskId == t.approvableId &&
                                        x.InvoiceID == t.approvableUniqueName &&
                                        x.AttachmentId == att.Id);

                                    if (exists == null)
                                    {
                                        attachments.Add(new AribaAttachment
                                        {
                                            Id = Guid.NewGuid(),
                                            AttachmentId = att.Id,
                                            InvoiceID = t.approvableUniqueName,
                                            AribaTaskId = t.approvableId
                                        });
                                    }
                                }

                                if (attachments.Any())
                                    await _repoWrapper.AribaAttachment.CreateRangeAsync(attachments);
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
                        _repoWrapper.AribaTask.Update(taskexists);


                        var req = await GetRequisitionById(t.approvableId, token, apiKey, realm,
                            requisitionDetailsUrl.Replace("{invoiceId}", taskexists.ApprovalId));
                        if (req != null)
                        {
                            requisitions.Add(req);

                            if (req.Attachments != null)
                            {
                                List<AribaAttachment> newAtt = new List<AribaAttachment>();
                                List<AribaAttachment> updateAtt = new List<AribaAttachment>();

                                foreach (var a in req.Attachments)
                                {
                                    var ex = _repoWrapper.AribaAttachment.FindFirstByCondition(x =>
                                        x.IsActive &&
                                        x.AribaTaskId == t.approvableId &&
                                        x.InvoiceID == t.approvableUniqueName &&
                                        x.AttachmentId == a.Id);

                                    if (ex != null)
                                    {
                                        updateAtt.Add(ex);
                                    }
                                    else
                                    {
                                        newAtt.Add(new AribaAttachment
                                        {
                                            Id = Guid.NewGuid(),
                                            AttachmentId = a.Id,
                                            InvoiceID = t.approvableUniqueName,
                                            AribaTaskId = t.approvableId
                                        });
                                    }
                                }

                                if (newAtt.Any())
                                    await _repoWrapper.AribaAttachment.CreateRangeAsync(newAtt);

                                if (updateAtt.Any())
                                    _repoWrapper.AribaAttachment.UpdateRange(updateAtt);
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
                            var existingApprovalHistories = _repoWrapper.AribaInvoiceApprovalHistory
                                .FindByCondition(x => x.IsActive && x.AribaTaskId == a.ApprovableId && x.InvoiceID == a.UniqueName)
                                .AsNoTracking()
                                .ToList();

                            if (existingApprovalHistories.Any())
                            {
                                existingApprovalHistories.ForEach(x => x.IsActive = false);
                                _repoWrapper.AribaInvoiceApprovalHistory.UpdateRange(existingApprovalHistories);
                            }

                            var approvalRequests = a.ApprovalRequests
                                .OrderBy(x => x.ActivationDate == null)
                                .ThenBy(x => x.ActivationDate)
                                .ToList();

                            var newApprovalHistories = new List<AribaInvoiceApprovalHistory>();
                            int approvedLevelCounter = 0;

                            foreach (var approvalrequest in approvalRequests)
                            {
                                var newHistory = new AribaInvoiceApprovalHistory
                                {
                                    Id = Guid.NewGuid(),
                                    InvoiceID = a.UniqueName,
                                    AribaTaskId = a.ApprovableId,
                                    Comments = approvalrequest.ApproverComment,
                                    Status = approvalrequest.State,
                                    IsActive = true
                                };

                                if (approvalrequest.State == "Approved")
                                {
                                    newHistory.Status = Common.ARIBA_APPROVED_STATUS_DISPLAY;
                                    newHistory.ApprovalDate = approvalrequest.LastModified;
                                    approvedLevelCounter++;
                                    newHistory.Level = "Level " + approvedLevelCounter;
                                    newHistory.AribaUser = approvalrequest.ApprovedBy?.UniqueName;
                                }
                                else if (approvalrequest.State == "ActiveButNoActionYet")
                                {
                                    newHistory.Status = Common.STATUS_PENDING;
                                    approvedLevelCounter++;
                                    newHistory.Level = "Level " + approvedLevelCounter;
                                    newHistory.AribaUser = approvalrequest.Approver?.UniqueName;
                                }
                                else
                                {
                                    newHistory.AribaUser = approvalrequest.Approver?.UniqueName;
                                }
                                insertedapprovablehistories.Add(a.ApprovableId);
                                newApprovalHistories.Add(newHistory);
                            }
                            await _repoWrapper.AribaInvoiceApprovalHistory.CreateRangeAsync(newApprovalHistories);
                        }
                    }
                }
                await _repoWrapper.SaveAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to insert invoice history for : {ex.Message}");
            }

            return GroupTasksIntoInvoices(tasks, requisitions);
        }

        private List<AribaInvoiceDto> GroupTasksIntoInvoices(List<AribaTask> tasks, List<RequisitionDetailDto> requisitions)
        {
            _logger.LogInfo($"Grouping {tasks.Count} tasks into invoices.");

            return tasks
                .GroupBy(t => new { t.approvableUniqueName, t.approver, t.approvableId })
                .Select(g =>
                {
                    var requisition = requisitions
                        .FirstOrDefault(r => r.ApprovableId == g.Key.approvableId);

                    return new AribaInvoiceDto
                    {
                        InvoiceNumber = requisition?.InvoiceNumber,
                        InvoiceId = g.Key.approvableUniqueName,
                        SupplierName = requisition?.Supplier.Name ?? requisition?.SupplierName,
                        Currency = requisition?.TotalCost?.Currency?.UniqueName,
                        DueDate = requisition?.InvoiceDate ?? g.Min(t => t.assignedDate),
                        InvoiceDate = requisition?.InvoiceDate,
                        AribaTaskId = requisition?.ApprovableId,
                        Amount = requisition?.TotalCost?.Amount ?? 0,
                        InvoiceNetAmount = requisition?.TotalInvoicedLessTax?.Amount ?? 0,
                        InvoiceTaxAmount = requisition?.TaxAmount?.Amount ?? 0,
                        RequesterName = requisition?.Requester?.UniqueName,
                        CompanyUniqueName = requisition?.CompanyCode?.UniqueName ?? requisition?.CompanyUniqueName,
                        Attachments = requisition?.Attachments?
                    .Where(a => a != null)
                    .Select(a => a.Id)
                    .ToList()
                ?? new List<string>(),
                        PONumber = requisition?.Order.UniqueName,
                        Tasks = g.ToList()
                    };
                })
                .ToList().Where(x => x.AribaTaskId != null).ToList();
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

            string token = await GetValidToken(domainId, tokenConfig);

            string requestUrl = invoiceConfig.RequisitionAttachmentUrl;

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

        public async Task<RequisitionDetailDto> GetRequisitionById(
        string approvableId,
        string token,
        string apiKey,
        string realm,
        string requisitionDetailsUrl)
        {


            _logger.LogInfo($"Fetching requisition details for approvableId {approvableId}.");

            var filters = new
            {
                approvableId = approvableId,

                realm = realm
            };

            string filterString = JsonConvert.SerializeObject(filters);
            string url = requisitionDetailsUrl;
            if (!url.Contains("?"))
                url += "?";
            else if (!url.EndsWith("&"))
                url += "&";

            url += $"realm={realm}&filters={Uri.EscapeDataString(filterString)}";

            _logger.LogInfo($"[ARIBA] Final Request URL: {url}");

            using var client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("apiKey", apiKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await client.SendAsync(request);
            string content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Ariba API error: {response.StatusCode} - {content}");

            return JsonConvert.DeserializeObject<RequisitionDetailDto>(content);
        }

        public async Task<string> PatchRequisitionAsync(Guid domainId, string requisitionId, AribaPatchRequestDto patchRequest, MyProfileDto aribauser, Guid userId)
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


                string token = await GetValidToken(domainId, tokenConfig);


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

                string url = config.AcceptRejectUrl;
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
                    var pendingapprovable = _repoWrapper.AribaTask.FindByCondition(x => x.ApprovalId == requisitionId && x.Status == "Pending" && x.Approver == aribauser.AribaUserName).ToList();
                    pendingapprovable.ForEach(x => x.Status = (patchRequest.State == Common.STATUS_ARIBA_APPROVE ? Common.ARIBA_APPROVED_STATUS_DISPLAY : patchRequest.State == Common.STATUS_ARIBA_DENY ? Common.ARIBA_DENY_STATUS_DISPLAY : Common.STATUS_PENDING));
                    _repoWrapper.AribaTask.UpdateRange(pendingapprovable);
                    string ipAddress = _httpContextAccessor.HttpContext?.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim() ?? _httpContextAccessor.HttpContext?.Request.Headers["X-Real-IP"].FirstOrDefault() ?? _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.MapToIPv4()?.ToString();

                    LoginRecord log = new LoginRecord
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,          // coming from MyProfileDto
                        DomainId = domainId,
                        AttributeId = requisitionId,        // invoiceId / requisitionId
                        EntityType = Common.TYPE_INVOICE,   // or Common.TYPE_PR based on context
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


        //contract token
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
        public async Task<List<ContractDto>> FetchAndMatchContracts(Guid domainId, List<AribaInvoiceDto> invoices)
        {
            List<ContractDto> matchedContracts = new List<ContractDto>();

            try
            {
                if (invoices == null || !invoices.Any())
                {
                    _logger.LogInfo($"[Domain: {domainId}] No invoices provided for contract fetching.");
                    return matchedContracts;
                }

                // Fetch contract settings (URL, realm, etc.)
                var contractSetting = _repoWrapper.DomainSetting
                    .FindFirstByCondition(x =>
                        x.DomainId == domainId &&
                        x.SettingsKey == Common.DOMAIN_SETTINGS_ARIBA_CONTRACT_URL &&
                        x.IsActive);

                if (contractSetting == null)
                {
                    _logger.LogError($"[Domain: {domainId}] No contract settings found.");
                    return matchedContracts;
                }

                var contractConfig = JsonConvert.DeserializeObject<AribaPurchaseOrderSettings>(contractSetting.SettingsValue);
                if (contractConfig == null)
                {
                    _logger.LogError($"[Domain: {domainId}] Failed to deserialize contract settings.");
                    return matchedContracts;
                }

                // Fetch token settings
                var tokenSetting = _repoWrapper.DomainSetting
                    .FindFirstByCondition(x =>
                        x.DomainId == domainId &&
                        x.SettingsKey == Common.DOMAIN_SETTINGS_CONTRACT_CONFIG &&
                        x.IsActive);

                if (tokenSetting == null)
                {
                    _logger.LogError($"[Domain: {domainId}] No contract token settings found.");
                    return matchedContracts;
                }

                var tokenConfig = JsonConvert.DeserializeObject<AribaPurchaseOrderSettings>(tokenSetting.SettingsValue);
                if (tokenConfig == null)
                {
                    _logger.LogError($"[Domain: {domainId}] Failed to deserialize token settings.");
                    return matchedContracts;
                }

                // Use UriBuilder to safely handle existing query parameters
                var uriBuilder = new UriBuilder(contractConfig.ContractAmountUrl.Trim());
                var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);

                // Only add realm if not already present
                if (string.IsNullOrEmpty(query["realm"]))
                    query["realm"] = contractConfig.Realm;

                uriBuilder.Query = query.ToString()!;
                string contractAmountUrl = uriBuilder.ToString();

                // Get contract token using tokenConfig
                string contractToken = await GetAribaToken(tokenConfig.ContractAuthToken, tokenConfig.ContractTokenUrl);

                using var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, contractAmountUrl);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", contractToken);
                request.Headers.Add("apiKey", contractConfig.ApiKey);

                HttpResponseMessage response = await client.SendAsync(request);
                string content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to fetch contracts: {response.StatusCode}");
                    return matchedContracts;
                }

                var contractJson = JsonConvert.DeserializeObject<JToken>(content);
                List<ContractDto> contracts = new List<ContractDto>();

                // Inline parsing logic (merged ParseContracts)
                if (contractJson.Type == JTokenType.Object && contractJson["value"] != null)
                {
                    if (contractJson["value"].Type == JTokenType.Array)
                        contracts = contractJson["value"].ToObject<List<ContractDto>>();
                    else
                        contracts.Add(contractJson["value"].ToObject<ContractDto>());
                }
                else if (contractJson.Type == JTokenType.Array)
                {
                    contracts = contractJson.ToObject<List<ContractDto>>();
                }

                var invoiceApprovalIds = invoices.Select(x => x.AribaTaskId).ToList();

                matchedContracts = contracts
                    .Where(c => invoiceApprovalIds.Contains(c.ContractId))
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in FetchAndMatchContracts: {ex.Message}");
            }

            return matchedContracts;
        }

        public async Task<InvoiceContractDto> GetContractById(Guid domainId, string contractId)
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

                string contractToken = await GetAribaToken(tokenConfig.ContractWorkspaceAuthToken, tokenConfig.ContractTokenUrl);
                contractId = AddR(contractId);

                if (string.IsNullOrWhiteSpace(contractToken))
                {
                    _logger.LogError($"[Domain: {domainId}] Failed to retrieve contract token.");
                    return null;
                }

                contractId = AddR(contractId);
                string url = contractConfig.ContractWorkspaceUrl
                                        .Replace("{contractId}", contractId)
                                        .Replace("{ContractRealm}", contractConfig.ContractRealm);
                string contractcontent;
                {
                    using var client = new HttpClient();
                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", contractToken);
                    request.Headers.Add("apiKey", contractConfig.ContractApiKey);

                    HttpResponseMessage response = await client.SendAsync(request);
                    contractcontent = await response.Content.ReadAsStringAsync();

                    _logger.LogInfo($"[Domain: {domainId}] Raw contract API response for {contractId}: {contractcontent}");

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError($"[Domain: {domainId}] Failed to fetch contract {contractId}: {response.StatusCode} - {contractcontent}");
                        return null;
                    }
                }

                var contractDetailMain = JsonConvert.DeserializeObject<ContractDetailsDto>(contractcontent);

                InvoiceContractDto invoiceContractDto = new InvoiceContractDto();
                invoiceContractDto.ContractName = contractDetailMain.Name;

                string urlContract = contractConfig.ContractAmountUrl.Replace("{ContractId}", contractId)
                .Replace("{Realm}", contractConfig.ContractRealm); ;
                _logger.LogInfo($"[ARIBA] Final Contract Request URL: {urlContract}");
                string content;
                {
                    using var client = new HttpClient();
                    var request = new HttpRequestMessage(HttpMethod.Get, urlContract);
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", contractToken);
                    request.Headers.Add("apiKey", contractConfig.ContractApiKey);

                    HttpResponseMessage response = await client.SendAsync(request);
                    content = await response.Content.ReadAsStringAsync();

                    _logger.LogInfo($"[Domain: {domainId}] Raw contract API response for {contractId}: {content}");

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError($"[Domain: {domainId}] Failed to fetch contract {contractId}: {response.StatusCode} - {content}");
                        return null;
                    }

                }
                var contractDetail = JsonConvert.DeserializeObject<ContractDto>(content);

                if (contractDetail != null)
                {
                    _logger.LogInfo($"[Domain: {domainId}] Parsed Contract => Id={contractDetail.ContractId}, Title={contractDetail.Title}, Supplier={contractDetail.SupplierName}");


                    if (contractDetail.LineItems != null && contractDetail.LineItems.Any())
                    {
                        foreach (var li in contractDetail.LineItems)
                        {
                            string desc = li.Description?.Description ?? "N/A";
                            decimal amt = li.Amount?.Amount ?? 0;
                            string currency = li.Amount?.Currency?.UniqueName ?? "NULL";
                            string currencyName = li.Amount?.Currency?.Name ?? "NULL";

                            _logger.LogInfo($"[Domain: {domainId}] LineItem => Description='{desc}', Amount={amt}, Currency={currency}, CurrencyName={currencyName}");
                        }
                    }

                    else
                    {
                        _logger.LogInfo($"[Domain: {domainId}] No line items found in contract {contractId}.");
                    }
                }
                else
                {
                    _logger.LogError($"[Domain: {domainId}] Could not map contract response into ContractDto.");
                }

                return invoiceContractDto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetContractById for ContractId={contractId}, Domain={domainId}: {ex}");
                return null;
            }
        }

        private static string AddR(string contractId)
        {
            if (string.IsNullOrEmpty(contractId))
                return contractId;

            var match = Regex.Match(contractId, @"^([A-Za-z]+)(\d+)$");
            if (match.Success)
            {
                string letters = match.Groups[1].Value;
                string numbers = match.Groups[2].Value;
                return letters + "R" + numbers;
            }


            return contractId;
        }

        public async Task<byte[]> GetContractDocumentById(Guid domainId, string documentId, MyProfileDto aribauser)
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
                return contentBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetContractDocumentById for DocumentId={documentId}, Domain={domainId}: {ex}");
                return null;
            }
        }

        public async Task<AribaInvoiceDetailDto> GetContractDetailsFromReportAsync(Guid domainId, MyProfileDto aribaUser, DateTime updatedDateFrom, DateTime updatedDateTo)
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

                string contractToken = await GetAribaToken(tokenConfig.ContractAuthToken, tokenConfig.ContractTokenUrl);
                if (string.IsNullOrWhiteSpace(contractToken))
                {
                    _logger.LogError($"[Domain: {domainId}] Failed to retrieve contract token.");
                    return null;
                }


                var filters = JsonConvert.SerializeObject(new
                {
                    updatedDateFrom = updatedDateFrom.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    updatedDateTo = updatedDateTo.ToString("yyyy-MM-ddTHH:mm:ssZ")
                });


                string fullUrl = contractConfig.ContractUrl
                                    // .Replace("{ContractId}", contractId)
                                    .Replace("{Realm}", tokenConfig.Realm)
                                    .Replace("{Filters}", Uri.EscapeDataString(filters));

                _logger.LogInfo($"[Domain: {domainId}] Final ARIBA report URL: {fullUrl}");


                using var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, fullUrl);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", contractToken);
                request.Headers.Add("apiKey", contractConfig.ApiKey);
                request.Headers.Add("accept", "application/json");

                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"[Domain: {domainId}] ARIBA report fetch failed: {response.StatusCode} - {content}");
                    return null;
                }


                dynamic json = JsonConvert.DeserializeObject(content);
                var list = json.data ?? json.records;
                if (list == null)
                {
                    _logger.LogError($"[Domain: {domainId}] No records found in JSON response.");
                    return null;
                }

                dynamic record = null;
                decimal origMaxCommitment = 0;
                decimal origAmountLeft = 0;

                if (decimal.TryParse((string)record.OrigMaxCommitment?.ToString(), out var parsed1))
                    origMaxCommitment = parsed1;

                if (decimal.TryParse((string)record.OrigAmountLeft?.ToString(), out var parsed2))
                    origAmountLeft = parsed2;

                decimal amountSpend = origMaxCommitment - origAmountLeft;


                var details = new AribaInvoiceDetailDto
                {
                    // ContractId = contractId,
                    ContractValue = origMaxCommitment.ToString(),
                    RemainingBalance = origAmountLeft.ToString(),
                    ValueAlreadyPaid = amountSpend.ToString()
                };

                return details;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[Domain: {domainId}] Error in CreateContractDetailsFromReportAsync: {ex}");
                return null;
            }

        }

        public async Task<string> GetContractWorkspaceId(Guid domainId, string contractId)
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


                string contractToken = await GetAribaToken(tokenConfig.ContractWorkspaceAuthToken, tokenConfig.ContractTokenUrl);
                if (string.IsNullOrWhiteSpace(contractToken))
                {
                    _logger.LogError($"[Domain: {domainId}] Failed to retrieve contract token.");
                    return null;
                }

                contractId = AddR(contractId);


                string url = contractConfig.ContractWorkspaceUrl
                                 .Replace("{contractId}", contractId)
                                 .Replace("{ContractRealm}", contractConfig.ContractRealm);


                string contractContent;
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", contractToken);
                    request.Headers.Add("apiKey", contractConfig.ContractApiKey);

                    HttpResponseMessage response = await client.SendAsync(request);
                    contractContent = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError($"[Domain: {domainId}] Failed to fetch contract {contractId}: {response.StatusCode} - {contractContent}");
                        return null;
                    }
                }


                var match = Regex.Match(contractContent, @"CW\d+");
                if (match.Success)
                {
                    string workspaceId = match.Value;
                    _logger.LogInfo($"[Domain: {domainId}] Extracted WorkspaceId = {workspaceId}");
                    return workspaceId;
                }

                _logger.LogInfo($"[Domain: {domainId}] No WorkspaceId found in response for contract {contractId}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetContractWorkspaceId for ContractId={contractId}, Domain={domainId}: {ex}");
                return null;
            }
        }



        public async Task<byte[]> GetAttachmentbyContractID(Guid domainId, string contractId, MyProfileDto aribaUser)
        {

            var contractDetail = _repoWrapper.AribaContractDetails
                .FindFirstByCondition(x => x.DomainId == domainId && x.ContractId == contractId && x.IsActive);

            string? workspaceId = contractDetail?.WorkspaceID;

            if (string.IsNullOrEmpty(workspaceId))
            {
                _logger.LogInfo($"WorkspaceId not found in DB. Fetching from Ariba for ContractId: {contractId}");
                workspaceId = await GetContractWorkspaceId(domainId, contractId);
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

            string? documentId = await GetAttachmentIdByWorkspaceIdAsync(domainId, workspaceId, aribaUser);
            if (string.IsNullOrEmpty(documentId))
            {
                _logger.LogError($"No document ID found for WorkspaceId: {workspaceId}");
                return null;
            }

            _logger.LogInfo($"DocumentId for WorkspaceId {workspaceId}: {documentId}");
            byte[]? documentBytes = await GetContractDocumentById(domainId, documentId, aribaUser);

            if (documentBytes == null || documentBytes.Length == 0)
            {
                _logger.LogError($"Failed to download document for DocumentId={documentId}, WorkspaceId={workspaceId}");
                return null;
            }

            _logger.LogInfo($"Successfully downloaded document for ContractId={contractId}, size={documentBytes.Length} bytes");
            return documentBytes;
        }

        public async Task<string?> GetAttachmentIdByWorkspaceIdAsync(Guid domainId, string workspaceId, MyProfileDto aribaUser)
        {

            var attachmentRecord = _repoWrapper.AribaContractDetails
                .FindFirstByCondition(x => x.DomainId == domainId && x.WorkspaceID == workspaceId && x.IsActive);

            if (attachmentRecord != null && !string.IsNullOrEmpty(attachmentRecord.AttachmentID))
            {
                _logger.LogInfo($"AttachmentId found in DB for WorkspaceId: {workspaceId} = {attachmentRecord.AttachmentID}");
                return attachmentRecord.AttachmentID;
            }

            _logger.LogInfo($"AttachmentId not found in DB. Fetching from Ariba for WorkspaceId: {workspaceId}");

            string attachmentId = await GetAttachmentIdFromAribaAsync(domainId, workspaceId, aribaUser);//method 4 
            if (string.IsNullOrEmpty(attachmentId))
                throw new PreConditionFailedCustomException(
                     $"No attachment/document found in Ariba for WorkspaceId {workspaceId}",
                     $"No attachment/document found in Ariba for WorkspaceId {workspaceId}"
     );

            _logger.LogInfo($"Fetched AttachmentId from Ariba: {attachmentId}");
            return attachmentId;
        }

        public async Task<string?> GetAttachmentIdFromAribaAsync(Guid domainId, string workspaceId, MyProfileDto aribaUser)
        {
            var contractSetting = _repoWrapper.DomainSetting
                .FindFirstByCondition(x => x.DomainId == domainId &&
                                           x.SettingsKey == Common.DOMAIN_SETTINGS_ARIBA_CONTRACT_URL &&
                                           x.IsActive);
            if (contractSetting == null)
                throw new PreConditionFailedCustomException(
                    "Missing Ariba contract config for domain.",
                    "Missing Ariba contract config for domain."
    );
            var contractConfig = JsonConvert.DeserializeObject<DomainSettingPurchaseOrderDto>(contractSetting.SettingsValue);

            var tokenSetting = _repoWrapper.DomainSetting
                .FindFirstByCondition(x => x.DomainId == domainId &&
                                           x.SettingsKey == Common.DOMAIN_SETTINGS_CONTRACT_CONFIG &&
                                           x.IsActive);
            if (tokenSetting == null)
                throw new PreConditionFailedCustomException("Missing Ariba token config for domain.", "Missing Ariba token config for domain.");

            var tokenConfig = JsonConvert.DeserializeObject<DomainSettingPurchaseOrderDto>(tokenSetting.SettingsValue);

            string attachmentToken = await GetAribaToken(tokenConfig.AttachmentAuthToken, tokenConfig.AttachmentTokenUrl);
            if (string.IsNullOrWhiteSpace(attachmentToken))
                throw new PreConditionFailedCustomException("Failed to retrieve Ariba attachment token.", "Failed to retrieve Ariba attachment token.");

            string apiUrl = contractConfig.ContractDocumentUrl
                .Replace("{documentId}", workspaceId)
                .Replace("{user}", aribaUser.AribaUserName)
                .Replace("{passwordAdapter}", aribaUser.AribaPassword)
                .Replace("{realm}", contractConfig.Realm);

            _logger.LogInfo($"[ARIBA] Final Attachment API URL: {apiUrl}");

            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, apiUrl.Replace("&amp;", "&"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", attachmentToken);
            request.Headers.Add("apiKey", contractConfig.AttachmentApiKey);

            var response = await client.SendAsync(request);
            string content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new PreConditionFailedCustomException(
        $"No document found for this workspace: {response.StatusCode} - {content}",
        $"No document found for this workspace: {response.StatusCode} - {content}");

            var contractDocument = JsonConvert.DeserializeObject<ContractDocumentDto>(content);

            if (contractDocument?.ChildrenDocument == null || contractDocument.ChildrenDocument.Count == 0)
                throw new PreConditionFailedCustomException("No children document found in Ariba response.", "No children document found in Ariba response.");

            {
                return contractDocument.ChildrenDocument[0].Id;
            }
        }


        public SplitAttachmentResponseDto GetFinalAttachmentBase64(string invoiceId)
        {
            _logger.LogInfo($"Fetching the Attachments for the Invoice with Id : {invoiceId}");

            if (string.IsNullOrWhiteSpace(invoiceId))
                throw new PreConditionFailedCustomException("Invoice ID cannot be null or empty", "Invoice ID cannot be null or empty");

            List<FinalAttachment> attachments = _repoWrapper.FinalAttachment
                .FindByCondition(f => f.AribaId.ToLower() == invoiceId.ToLower() && f.IsActive)
                .ToList();

            if (attachments == null || attachments.Count == 0)
                throw new NotFoundCustomException($"No final attachment found for Invoice ID: {invoiceId}", $"No final attachment found for Invoice ID: {invoiceId}");


            SplitAttachmentResponseDto result = new SplitAttachmentResponseDto();
            result.InvoiceAttachments = attachments
                .Where(a => a.type != null && a.type.Equals("ARIBA_INVOICE", StringComparison.OrdinalIgnoreCase))
                .Select(a => Convert.ToBase64String(a.Attachment))
                .ToList();

            result.ContractAttachments = attachments
                .Where(a => a.type != null && a.type.Equals("ARIBA_CONTRACT", StringComparison.OrdinalIgnoreCase))
                .Select(a => Convert.ToBase64String(a.Attachment))
                .ToList();
            return result;
        }
        public async Task DeleteAribaTaskByInvoiceAndApproverAsync(string invoiceId, string approver)
        {
            _logger.LogInfo($"[DeleteAribaTask] Start: invoiceId={invoiceId}, approver={approver}");


            AribaInvoice invoice = _repoWrapper.AribaInvoice.FindFirstByCondition(x => x.InvoiceId == invoiceId);
            if (invoice == null)
            {
                _logger.LogError($"[DeleteAribaTask] Invoice not found for InvoiceId={invoiceId}");
                throw new NotFoundCustomException("Invoice not found", $"No invoice found with InvoiceId={invoiceId}");
            }

            string aribaTaskId = invoice.AribaTaskId;
            _logger.LogInfo($"[DeleteAribaTask] Found invoice. AribaTaskId={aribaTaskId}");

            AribaTasks task = _repoWrapper.AribaTask.FindFirstByCondition(x => x.ApprovalId == aribaTaskId && x.Approver == approver);
            if (task == null)
            {
                _logger.LogError($"[DeleteAribaTask] Task not found for ApprovalId={aribaTaskId} and Approver={approver}");
                throw new NotFoundCustomException("Task not found", $"No task found with ApprovalId={aribaTaskId} and Approver={approver}");
            }

            _logger.LogInfo($"[DeleteAribaTask] Deleting task with Id={task.Id}");
            _repoWrapper.AribaTask.Delete(task);
            await _repoWrapper.SaveAsync();
            _logger.LogInfo($"[DeleteAribaTask] Successfully deleted task with Id={task.Id}");
        }

        /// <summary>
        /// Generates a summary of Ariba approval tasks for leadership users.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>

        public AribaApprovalDashboardSummaryDto GetLeadershipApprovalDashboardSummary(Guid userId, Guid domainId, string type = Common.TYPE_INVOICE)

        {
            _logger.LogInfo($"Fetching Ariba approval dashboard summary for userId: {userId}");

            string normalizedType = (type ?? Common.TYPE_INVOICE).Trim().ToUpper();
            if (!new[] { Common.TYPE_INVOICE, Common.TYPE_CONTRACT, Common.TYPE_PR, Common.TYPE_PAYMENT }.Contains(normalizedType))
            {
                throw new PreConditionFailedCustomException(
                    "Invalid type parameter",
                    "Type must be one of: Invoice, Contract, PR, Payment"
                );
            }

            AribaApprovalDashboardSummaryDto summary = new AribaApprovalDashboardSummaryDto();
            UserRoleMapping userRoleMapping = _repoWrapper.UserRoleMapping.FindFirstByCondition(m => m.UserId == userId);
            if (userRoleMapping == null)
            {
                _logger.LogError($"No role mapping found for userId: {userId}");
                throw new PreConditionFailedCustomException("User role not found", $"User with id {userId} does not have a role assigned.");
            }

            Guid roleId = userRoleMapping.RoleId;

            if (roleId == Common.LEADERSHIP_USER_ROLE_ID || roleId == Common.BUSINESS_ADMIN_ROLE_ID || roleId == Common.BUSINESS_USER_ROLE_ID || roleId == Common.OPERATIONS_USER_ROLE_ID || roleId==Common.FINANCE_OPERATION_USER_ROLE_ID)
            {
                string aribaUserName = string.Empty;


                if (roleId != Common.BUSINESS_ADMIN_ROLE_ID)
                {
                    MyProfileDto profile = _repoWrapper.Person.GetMyProfileByUserId(userId);

                    if (profile == null || string.IsNullOrEmpty(profile.AribaUserName))
                    {
                        _logger.LogError($"No Ariba username found for userId: {userId}");
                        throw new PreConditionFailedCustomException(
                            "Ariba username not found",
                            $"User with id {userId} does not have an Ariba username assigned."
                        );
                    }

                    aribaUserName = profile.AribaUserName;
                }

                summary.Type = normalizedType;

                // =========================
                // 🔹 CONTRACT TYPE
                // =========================
                if (normalizedType == Common.TYPE_CONTRACT)
                {
                

                    var prTasksQuery =
                        from task in _repoWrapper.ContractPending.FindByCondition(t => t.IsActive && t.DomainId == domainId)
                        join pr in _repoWrapper.ContractDetails.FindByCondition(p => p.IsActive && p.DomainId == domainId)
                            on task.UniqueName equals pr.ContractTaskId
                        select task;


                    if (roleId == Common.LEADERSHIP_USER_ROLE_ID || roleId == Common.OPERATIONS_USER_ROLE_ID||roleId == Common.BUSINESS_USER_ROLE_ID)
                    {
                        prTasksQuery = prTasksQuery.Where(t => t.Approver == aribaUserName);
                    }
      


                    var prTasks = prTasksQuery.ToList();
                    summary.Total = prTasks.Count();
                    summary.Approved = prTasks.Count(t => !string.IsNullOrEmpty(t.Status) &&
                        (t.Status.Equals(Common.STATUS_ARIBA_APPROVE, StringComparison.OrdinalIgnoreCase) ||
                        t.Status.Equals(Common.STATUS_APPROVE, StringComparison.OrdinalIgnoreCase)));
                    summary.Deny = prTasks.Count(t => !string.IsNullOrEmpty(t.Status) &&
                        t.Status.Equals(Common.STATUS_DENY, StringComparison.OrdinalIgnoreCase));
                    summary.Pending = prTasks.Count(t => !string.IsNullOrEmpty(t.Status) &&
                        t.Status.Equals(Common.STATUS_PENDING, StringComparison.OrdinalIgnoreCase));
                    summary.Closed = prTasks.Count(t => !string.IsNullOrEmpty(t.Status) &&
                        t.Status.Equals(Common.CLOSED_STATUS_ARIBA, StringComparison.OrdinalIgnoreCase));
                }

                // =========================
                // 🔹 PR TYPE
                // =========================

               else if (normalizedType == Common.TYPE_PR)
                {

                    var prTasksQuery =
                        from task in _repoWrapper.AribaPRTask.FindByCondition(t => t.IsActive && t.DomainId == domainId)
                        join pr in _repoWrapper.AribaPR.FindByCondition(p => p.IsActive && p.DomainId == domainId)
                            on task.ApprovalId equals pr.AribaTaskId
                        select task;


                    if (roleId == Common.LEADERSHIP_USER_ROLE_ID || roleId == Common.OPERATIONS_USER_ROLE_ID)
                    {
                        prTasksQuery = prTasksQuery.Where(t => t.Approver == aribaUserName);
                    }
                    else if (roleId == Common.BUSINESS_USER_ROLE_ID)
                    {
                        var userPRs = _repoWrapper.AribaPR
                            .FindByCondition(p => p.IsActive && p.DomainId == domainId && p.RequesterName == aribaUserName)
                            .Select(p => p.AribaTaskId)
                            .ToList();

                        prTasksQuery = prTasksQuery.Where(t => userPRs.Contains(t.ApprovableUniqueName));
                    }


                    var prTasks = prTasksQuery.ToList();
                    summary.Total = prTasks.Count();
                    summary.Approved = prTasks.Count(t => !string.IsNullOrEmpty(t.Status) &&
                        (t.Status.Equals(Common.STATUS_ARIBA_APPROVE, StringComparison.OrdinalIgnoreCase) ||
                        t.Status.Equals(Common.STATUS_APPROVE, StringComparison.OrdinalIgnoreCase)));
                    summary.Deny = prTasks.Count(t => !string.IsNullOrEmpty(t.Status) &&
                        t.Status.Equals(Common.STATUS_DENY, StringComparison.OrdinalIgnoreCase));
                    summary.Pending = prTasks.Count(t => !string.IsNullOrEmpty(t.Status) &&
                        t.Status.Equals(Common.STATUS_PENDING, StringComparison.OrdinalIgnoreCase));
                    summary.Closed = prTasks.Count(t => !string.IsNullOrEmpty(t.Status) &&
                        t.Status.Equals(Common.CLOSED_STATUS_ARIBA, StringComparison.OrdinalIgnoreCase));
                }

                // =========================
                // 🔹 PAYMENT TYPE
                // =========================

                else if (normalizedType == Common.TYPE_PAYMENT)
                {
                    IQueryable<AribaPayment> paymentQuery =
                        _repoWrapper.AribaPayment.FindByCondition(x =>
                            x.IsActive &&
                            x.DomainId == domainId);

                    if (roleId == Common.LEADERSHIP_USER_ROLE_ID || roleId == Common.OPERATIONS_USER_ROLE_ID)
                    {
                        paymentQuery = paymentQuery.Where(p => _repoWrapper.AribaInvoice.FindByCondition(i => i.IsActive && i.InvoiceNumber == p.InvoiceNumber)
                                        .Any(i => _repoWrapper.AribaTask.FindByCondition(t => t.IsActive && t.ApprovalId == i.AribaTaskId && t.Approver == aribaUserName).Any()));
                    }
                    else if (roleId == Common.BUSINESS_USER_ROLE_ID)
                    {
                        paymentQuery = paymentQuery.Where(p => _repoWrapper.AribaInvoice.FindByCondition(i => i.IsActive && i.InvoiceNumber == p.InvoiceNumber && i.RequesterUser == aribaUserName).Any());
                    }

                    var paymentCount = paymentQuery.ToList();
                    summary.Total = paymentCount.Count();
                    summary.Paid = paymentCount.Count(t => !string.IsNullOrEmpty(t.PaymentStatus) &&
                        t.PaymentStatus.Equals(Common.PAID_STATUS, StringComparison.OrdinalIgnoreCase));
                    summary.Paying = paymentCount.Count(t => !string.IsNullOrEmpty(t.PaymentStatus) &&
                        t.PaymentStatus.Equals(Common.PAYING_STATUS, StringComparison.OrdinalIgnoreCase));
                }

                // =========================
                // 🔹 INVOICE TYPE (DEFAULT)
                // =========================


                else
                {

                    var invoiceTasksQuery =
                         from task in _repoWrapper.AribaTask.FindByCondition(t => t.DomainId == domainId)
                         join inv in _repoWrapper.AribaInvoice.FindByCondition(i => i.IsActive)
                             on task.ApprovalId equals inv.AribaTaskId
                         where task.IsActive
                         select task;





                    if (roleId == Common.LEADERSHIP_USER_ROLE_ID || roleId == Common.OPERATIONS_USER_ROLE_ID)
                    {
                        invoiceTasksQuery = invoiceTasksQuery.Where(t => t.Approver == aribaUserName);
                    }
                    else if (roleId == Common.BUSINESS_USER_ROLE_ID)
                    {
                        var userInvoices = _repoWrapper.AribaInvoice
                            .FindByCondition(i => i.IsActive && i.DomainId == domainId && i.RequesterUser == aribaUserName)
                            .Select(i => i.InvoiceId)
                            .ToList();

                        invoiceTasksQuery = invoiceTasksQuery.Where(t => userInvoices.Contains(t.ApprovableUniqueName));
                    }

                    var invoiceTasks = invoiceTasksQuery.ToList();

                    summary.Total = invoiceTasks.Count();
                    summary.Approved = invoiceTasks.Count(t => !string.IsNullOrEmpty(t.Status) &&
                        (t.Status.Equals(Common.STATUS_ARIBA_APPROVE, StringComparison.OrdinalIgnoreCase) ||
                        t.Status.Equals(Common.STATUS_APPROVE, StringComparison.OrdinalIgnoreCase)));
                    summary.Deny = invoiceTasks.Count(t => !string.IsNullOrEmpty(t.Status) &&
                        t.Status.Equals(Common.STATUS_DENY, StringComparison.OrdinalIgnoreCase));
                    summary.Pending = invoiceTasks.Count(t => !string.IsNullOrEmpty(t.Status) &&
                        t.Status.Equals(Common.STATUS_PENDING, StringComparison.OrdinalIgnoreCase));
                    summary.Closed = invoiceTasks.Count(t => !string.IsNullOrEmpty(t.Status) &&
                        t.Status.Equals(Common.CLOSED_STATUS_ARIBA, StringComparison.OrdinalIgnoreCase));
                }
            }
            return summary;

        }

        /// <summary>
        /// Save or update an invoice extension for a domain. If an existing active record exists for the same domain
        /// and invoiceId it will be updated, otherwise a new record is created.
        /// </summary>
        public async Task SaveOrUpdateExtensionAsync(Guid domainId, ExtensionUploadDto dto, Guid userId)
        {
            _logger.LogInfo($"[SaveOrUpdateInvoiceExtension] domain={domainId},  file={dto?.FileName}");
            try
            {
                if (dto == null || dto.FileBytes == null || dto.FileBytes.Length == 0)
                {
                    throw new PreConditionFailedCustomException("Invalid upload payload", "File bytes are required");
                }

                AribaExtension existing = _repoWrapper.AribaExtension
                    .FindFirstByCondition(x => x.DomainId == domainId && x.IsActive);

                if (existing != null)
                {
                    existing.Extension = dto.FileBytes;
                    existing.ExtensionFileName = dto.FileName;
                    existing.ExtensionContentType = dto.ContentType;
                    _repoWrapper.AribaExtension.Update(existing);
                }
                else
                {
                    AribaExtension ext = new AribaExtension
                    {
                        Id = Guid.NewGuid(),
                        DomainId = domainId,
                        Extension = dto.FileBytes,
                        ExtensionFileName = dto.FileName,
                        ExtensionContentType = dto.ContentType,
                    };

                    await _repoWrapper.AribaExtension.CreateAsync(ext);
                }

                await _repoWrapper.SaveAsync();
                _logger.LogInfo($"[SaveOrUpdateInvoiceExtension] saved for domain={domainId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[SaveOrUpdateInvoiceExtension] failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Fetch the latest extension for a domain.
        /// </summary>
        public async Task<ExtensionDownloadDto> GetLatestInvoiceExtensionAsync(Guid domainId)
        {
            _logger.LogInfo($"[GetLatestInvoiceExtension] domain={domainId}");
            try
            {
                AribaExtension extension = _repoWrapper.AribaExtension
                        .FindByCondition(x => x.DomainId == domainId && x.IsActive)
                        .OrderByDescending(x => x.DateCreated)
                        .FirstOrDefault();

                if (extension == null)
                {
                    _logger.LogInfo($"[GetLatestInvoiceExtension] No extension found for domain={domainId}");
                    return null;
                }

                ExtensionDownloadDto result = new ExtensionDownloadDto
                {
                    FileBytes = extension.Extension,
                    FileName = extension.ExtensionFileName,
                    ContentType = extension.ExtensionContentType ?? "application/octet-stream"
                };

                _logger.LogInfo($"[GetLatestInvoiceExtension] Found extension for domain={domainId}, fileName={extension.ExtensionFileName}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[GetLatestInvoiceExtension] failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get extension by domain ID.
        /// </summary>
        public async Task<ExtensionDownloadDto> GetExtensionByDomainIdAsync(Guid domainId)
        {
            _logger.LogInfo($"[GetExtensionByDomainId] domain={domainId}");
            try
            {
                AribaExtension extension = _repoWrapper.AribaExtension
                        .FindFirstByCondition(x => x.DomainId == domainId && x.IsActive);

                if (extension == null)
                {
                    _logger.LogInfo($"[GetExtensionByDomainId] No extension found for domain={domainId}");
                    return null;
                }

                ExtensionDownloadDto result = new ExtensionDownloadDto
                {
                    FileBytes = extension.Extension,
                    FileName = extension.ExtensionFileName,
                    ContentType = extension.ExtensionContentType ?? "application/octet-stream"
                };

                _logger.LogInfo($"[GetExtensionByDomainId] Found extension for domain={domainId}, fileName={extension.ExtensionFileName}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[GetExtensionByDomainId] failed: {ex.Message}");
                throw;
            }
        }

        public async Task<List<AribaInvoiceDetailDto>> GetInvoicePendingApprovals(Guid userId, Guid domainId, int index, int limit, string? searchTerm = null, DateTime? fromDate = null, DateTime? toDate = null, string? status = null, List<string>? glAccount = null)
        {
            _logger.LogInfo($"Fetching pending approvals for UserId: {userId} in DomainId: {domainId}");

            if (glAccount != null)
            {
                glAccount = glAccount
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();

                if (glAccount.Count == 0)
                    glAccount = null;
            }

            MyProfileDto? profileDto = _repoWrapper.Person.GetMyProfileByUserId(userId);

            string aribausername = profileDto?.AribaUserName;
            string role = profileDto?.Role;

            // Step 1: Build invoice query with task join
            var query = from task in _repoWrapper.AribaTask
                            .FindByConditionAsync(x => x.DomainId == domainId)
                        join invoice in _repoWrapper.AribaInvoice.FindByCondition(x => x.IsActive)
                            on task.ApprovalId equals invoice.AribaTaskId
                        select new { task, invoice };



            if (role == Common.BUSINESS_USER_KEY)
            {
                if (string.IsNullOrEmpty(aribausername))
                {
                    _logger.LogError($"Unable to Find Any Records for User with Id : {userId}");
                    throw new NotFoundCustomException("Unable to Find Any Records", "Unable to Find Any Records");
                }

                query = query.Where(x => x.invoice.RequesterUser != null && x.invoice.RequesterUser.ToLower() == aribausername.ToLower());
            }
            // For non-admins (except business users handled above), show tasks assigned to the approver
            else if (role != Common.BUSINESS_ADMIN_USER_KEY)
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
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(x =>
                    (x.invoice.InvoiceId != null && x.invoice.InvoiceId.Contains(searchTerm)) ||
                    (x.invoice.PONumber != null && x.invoice.PONumber.Contains(searchTerm)) ||
                    (x.invoice.SupplierName != null && x.invoice.SupplierName.Contains(searchTerm)) ||
                    (x.task.Approver != null && x.task.Approver.Contains(searchTerm))
                );
            }


            if (fromDate.HasValue)
            {
                query = query.Where(x => x.invoice.InvoiceDate >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                query = query.Where(x => x.invoice.InvoiceDate <= toDate.Value);
            }
            if (glAccount != null && glAccount.Any())
            {
                query =
                    from q in query
                    join po in _repoWrapper.PurchaseOrderMaster
                        .FindByCondition(x => x.IsActive && glAccount.Contains(x.GLAccount))
                        on q.invoice.PONumber equals po.EPID
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
                _logger.LogError($"Unable to find Any Invoices");
                throw new NotFoundCustomException("Unable to Find Any Invoices", "Unable to Find Any Invoices");
            }

            // PRE-FETCH ALL RELATED DATA IN BULK TO AVOID N+1 QUERIES
            var poNumbers = pagedData.Select(x => x.invoice.PONumber).Where(x => x != null).Distinct().ToList();
            var invoiceIds = pagedData.Select(x => x.invoice.InvoiceId).Distinct().ToList();
            var aribaTaskIds = pagedData.Select(x => x.invoice.AribaTaskId).Distinct().ToList();
            var contractIds = pagedData.Select(x => x.invoice.ContractId).Where(x => x != null).Distinct().ToList();
            var companyCodes = pagedData.Select(x => x.invoice.CompanyUniqueName).Where(x => x != null).Distinct().ToList();

            // Fetch all PO details at once
            var poDetailsList = await _repoWrapper.PurchaseOrderMaster
                .FindByConditionAsync(x => x.IsActive && poNumbers.Contains(x.EPID))
                .ToListAsync();

            var poDetailsDict = poDetailsList
                .GroupBy(x => x.EPID)
                .ToDictionary(g => g.Key, g => g.First()); // Take first if duplicates exist

            // Fetch all approval history at once
            var approvalHistoryDict = (await _repoWrapper.AribaInvoiceApprovalHistory
                .FindByConditionAsync(x => x.IsActive && aribaTaskIds.Contains(x.AribaTaskId) && invoiceIds.Contains(x.InvoiceID))
                .ToListAsync())
                .GroupBy(x => new { x.AribaTaskId, x.InvoiceID })
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(x => x.DateCreated).ToList()
                );

            // Fetch all contracts at once
            var aribaContractsList = await _repoWrapper.AribaContract
                .FindByConditionAsync(x => contractIds.Contains(x.ContractId))
                .ToListAsync();

            var aribaContractsDict = aribaContractsList
                .GroupBy(x => x.ContractId)
                .ToDictionary(g => g.Key, g => g.First());

            // Fetch all contract details at once
            var aribaContractDetailsList = await _repoWrapper.AribaContractDetails
                .FindByConditionAsync(x => contractIds.Contains(x.ContractId))
                .ToListAsync();

            var aribaContractDetailsDict = aribaContractDetailsList
                .GroupBy(x => x.ContractId)
                .ToDictionary(g => g.Key, g => g.First());

            // Fetch all company mappings at once
            var companyMappingList = await _repoWrapper.AribaCompanyMapping
                .FindByConditionAsync(x => x.IsActive && companyCodes.Contains(x.CompanyCode))
                .ToListAsync();

            var companyMappingDict = companyMappingList
                .GroupBy(x => x.CompanyCode)
                .ToDictionary(g => g.Key, g => g.First());

            // Fetch all attachments at once
            var attachmentsList = await _repoWrapper.FinalAttachment
                .FindByConditionAsync(x => x.IsActive && invoiceIds.Contains(x.AribaId))
                .Select(x => x.AribaId)
                .ToListAsync();

            var attachmentsDict = attachmentsList
                .Distinct()
                .ToDictionary(x => x, x => true);

            // Get unique requisition requesters for department lookup
            var requisitionRequesters = poDetailsDict.Values
                .Where(x => x.RequisitionRequester != null)
                .Select(x => x.RequisitionRequester)
                .Distinct()
                .ToList();

            // Fetch all departments at once (assuming GetDepartmentDetail can be batched)
            var departmentDict = new Dictionary<string, string>();
            foreach (var requester in requisitionRequesters)
            {
                var dept = _repoWrapper.Person.GetDepartmentDetail(requester);
                if (dept != null)
                {
                    departmentDict[requester] = dept;
                }
            }

            // Now build the DTOs using pre-fetched data
            List<AribaInvoiceDetailDto> aribaInvoices = new List<AribaInvoiceDetailDto>();
            foreach (var item in pagedData)
            {
                var invoice = item.invoice;
                var task = item.task;

                var poDetails = invoice?.PONumber != null && poDetailsDict.TryGetValue(invoice.PONumber, out var p) ? p : null;

                var dto = new AribaInvoiceDetailDto
                {
                    InvoiceId = invoice.InvoiceId,
                    AribaTaskId = invoice.AribaTaskId,
                    InvoiceNumber = invoice.InvoiceNumber,
                    InvoiceDate = invoice.InvoiceDate,
                    Currency = invoice.Currency,
                    DueDate = invoice.DueDate,
                    InvoiceAmount = invoice.Amount,
                    SupplierName = invoice.SupplierName,
                    InvoiceTaxAmount = invoice.InvoiceTaxAmount,
                    InvoiceNetAmount = invoice.InvoiceNetAmount,
                    PurchaseOrderId = poDetails?.PurchaseOrderId,
                    Description = invoice.Description ?? poDetails?.HeaderTittle,
                    Department = poDetails?.RequisitionRequester != null && departmentDict.TryGetValue(poDetails.RequisitionRequester, out var dept)
                        ? dept
                        : null,
                    ContractId = invoice.ContractId,
                    GLAccount = poDetails?.GLAccount,
                    CostCenter = poDetails?.CostCenter,
                    PONumber = poDetails?.PurchaseOrderId,
                    EPID = invoice.PONumber,
                    CompanyUniqueName = invoice.CompanyUniqueName != null && companyMappingDict.TryGetValue(invoice.CompanyUniqueName, out var mapping)
                        ? mapping.CompanyName
                        : null,
                    HasAttachment = attachmentsDict.ContainsKey(invoice.InvoiceId),
                    Status = task.Status,
                    PaymentStatus = task.PaymentStatus,
                    Approver = task.Approver,
                    FullUrl = task.FullUrl
                };

                if (role == Common.BUSINESS_ADMIN_USER_KEY || role == Common.BUSINESS_USER_KEY)
                {
                    dto.Approver = task.Approver;
                }

                // Get approval history from pre-fetched dictionary
                var historyKey = new { AribaTaskId = dto.AribaTaskId, InvoiceID = dto.InvoiceId };
                dto.ApprovalHistory = approvalHistoryDict.TryGetValue(historyKey, out var history)
                    ? history
                    : new List<AribaInvoiceApprovalHistory>();

                // Get contract details from pre-fetched dictionaries
                if (dto.ContractId != null &&
                    aribaContractsDict.TryGetValue(dto.ContractId, out var aribacontract) &&
                    aribaContractDetailsDict.TryGetValue(dto.ContractId, out var aribacontractdetails))
                {
                    dto.ContractValue = aribacontract.TotalAmount;
                    dto.ValueAlreadyPaid = aribacontractdetails.AmountSpend;
                    dto.RemainingBalance = aribacontractdetails.BalanceAmount;
                    dto.WorkSpaceId = aribacontractdetails.WorkspaceID;
                    dto.AttachmentId = aribacontractdetails.AttachmentID;
                    dto.ContractCurrency = aribacontract.Currency;
                }

                aribaInvoices.Add(dto);
            }

            if (!aribaInvoices.Any())
            {
                _logger.LogError($"Unable to find Any Invoices");
                throw new NotFoundCustomException("Unable to Find Any Invoices", "Unable to Find Any Invoices");
            }

           
           
            return aribaInvoices;
        }

        public async Task<ExportExcelResultDto> ExportInvoicePendingApprovalsExcelAsync(
    Guid userId, Guid domainId, int limit,
    string? searchTerm, DateTime? fromDate, DateTime? toDate,
    string? status, List<string>? glAccount)
        {
            if (limit > 500)
                throw new PreConditionFailedCustomException("Limit cannot exceed 500 records", "Limit cannot exceed 500 records");

            var aribaInvoices = await GetInvoicePendingApprovals(
                userId, domainId, 0, limit, searchTerm, fromDate, toDate, status, glAccount);

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("Pending Approvals");

            if (!aribaInvoices.Any())
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


            var exportData = aribaInvoices.Select(x => new
            {
                Entity = x.CompanyUniqueName,
                x.Approver,
                x.InvoiceId,
                x.InvoiceNumber,
                x.InvoiceDate,
                x.InvoiceAmount,
                x.Currency,
                x.SupplierName,
                x.Status,
                x.DueDate,
                x.ContractId,
                x.Department,
                x.GLAccount,
                x.CostCenter,
                x.ContractValue,
                x.ValueAlreadyPaid,
                x.RemainingBalance,
                x.EPID,
                x.Description,
                x.PONumber,
                x.FullUrl
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

        public async Task<bool> UpdatePaymentStatusAsync(string invoiceId, string paymentStatus)
        {
            // Validate payment status
            var validStatuses = new[] { Common.PENDING_PAYMENT_STATUS, Common.PAID_PAYMENT_STATUS ,Common.PARTIAL_PAYMENT_STATUS};
            if (!validStatuses.Contains(paymentStatus))
            {
                throw new ArgumentException($"Invalid payment status: {paymentStatus}. Valid statuses are: {string.Join(", ", validStatuses)}");
            }

            // Find ALL records by InvoiceNumber
            var paymentRecords = _repoWrapper.AribaTask
                .FindByCondition(p => p.Description == invoiceId && p.IsActive)
                .ToList();

            if (!paymentRecords.Any())
                throw new Exception($"No payment record found for invoiceId: {invoiceId}");

            
            foreach (var paymentRecord in paymentRecords)
            {
                paymentRecord.PaymentStatus = paymentStatus;
                paymentRecord.DateUpdated = DateTime.Now;
                _repoWrapper.AribaTask.Update(paymentRecord);
            }

            await _repoWrapper.SaveAsync();

            return true;
        }

        public  Task<List<PaymentStatusDto>> GetPaymentStatusesAsync()
        {
            var statuses = new List<PaymentStatusDto>
            {
                new PaymentStatusDto { Value = Common.PENDING_PAYMENT_STATUS},
                new PaymentStatusDto { Value = Common.PAID_PAYMENT_STATUS},
          
                new PaymentStatusDto {Value=Common.PARTIAL_PAYMENT_STATUS}
            };

            return  Task.FromResult(statuses);
        }
    }

}






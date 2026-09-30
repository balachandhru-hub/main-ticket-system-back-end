using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using Contracts;
using Contracts.IRepository;
using Contracts.IServices;
using Dto;
using Entities.Common;
using ExceptionHandler;
using Newtonsoft.Json;

namespace Services
{
    public class PRService : IPRService
    {
        private readonly ILoggerManager _logger;
        private readonly IRepositoryWrapper _repositoryWrapper;

        public PRService(ILoggerManager loggerManager, IRepositoryWrapper repositoryWrapper)
        {
            _logger = loggerManager;
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<MaterialPRResultDto> UpdatePRDetailInAriba(PRUpdateDto prUpdateDto, Guid domainId, Guid userId)
        {
            _logger.LogInfo("Updating PR details in Ariba for PR Number: " + prUpdateDto.PRNumber);

            MyProfileDto person = _repositoryWrapper.Person.GetMyProfileByUserId(userId);

            if (person == null)
            {
                _logger.LogError("User not found for userId: " + userId);
                throw new PreConditionFailedCustomException("User not found.", $"User not found for id : {userId}.");
            }

            List<PRDetailsDto> prDetailsList = await _repositoryWrapper.MaterialPR.GetPRDetailsByPRNumberAsync(prUpdateDto.PRNumber);

            if (!prDetailsList.Any() || prDetailsList == null)
            {
                _logger.LogError("PR details not found for PR Number: " + prUpdateDto.PRNumber);
                throw new PreConditionFailedCustomException("PR details not found.", $"PR details not found for number : {prUpdateDto.PRNumber}.");
            }

            var aribaConfig = _repositoryWrapper.DomainSetting
                            .FindFirstByCondition(t => t.DomainId == domainId &&
                                                       t.SettingsKey == Common.DOMAIN_SETTINGS_MATERIAL_PR_POSTING)
                            ?? throw new NotFoundCustomException("Domain settings not found", "Material PR posting configuration missing");

            var config = JsonConvert.DeserializeObject<AribaPurchaseOrderSettings>(aribaConfig.SettingsValue);

            if (string.Equals(prDetailsList.First().PRType, Common.PR_MATERIAL_ITEM_CATEGORY, StringComparison.OrdinalIgnoreCase))
            {

                var lineItems = prDetailsList.Select(dbLine =>
                {
                    // Try to find matching update line
                    var updateLine = prUpdateDto.LineItems
                        .FirstOrDefault(x => x.LineNumber == dbLine.LineNumber?.ToString());

                    return new MaterialPRLineItemDto
                    {
                        NumberInCollection = dbLine.LineNumber ?? 0,
                        Quantity = dbLine.Quantity ?? 0,
                        DeliverTo = dbLine.DeliverTo,
                        NeedBy = dbLine.NeedByDate ?? DateTime.UtcNow,
                        Description = dbLine.Description,
                        MaterialCode = dbLine.MaterialCode,
                        CommodityCode = dbLine.CommodityCode,
                        UOM = dbLine.UOM,
                        CompanyCode = dbLine.CompanyCode,
                        PurchaseOrg = dbLine.PurchaseOrg,
                        CostCenter = dbLine.CostCenter,
                        GLAccount = dbLine.GLAccount,
                        Currency = dbLine.Currency,
                        Vendor = dbLine.SupplierId,
                        Amount = updateLine != null ? Convert.ToDecimal(updateLine.Price) : (dbLine.Price ?? 0),
                        ShipToPlant = dbLine?.CompanyCode,
                        BillTo = dbLine?.CompanyCode
                    };

                }).ToList();


                // 🔹 Header (take any one row)
                var defaultDetails = prDetailsList.First();


                // 🔹 FINAL REQUEST (same structure as your working one)
                var aribaReq = new MaterialPRDto
                {
                    AribaEndPoint = config.aribaUrl,
                    Partition = config.Partition,
                    Variant = config.Variant,
                    Username = config.UserName,
                    Password = config.Password,
                    Server = config.Server,
                    PRNumber = prUpdateDto.PRNumber,
                    Name = defaultDetails.PRTitle,
                    UniqueName = prUpdateDto.PRNumber,
                    Operation = Common.OPERATION_MATERIAL_PR_UPDATE,
                    CompanyCode = defaultDetails.CompanyCode,
                    DeliverTo = defaultDetails.DeliverTo,
                    NeedBy = defaultDetails.NeedByDate ?? DateTime.UtcNow,
                    Requester = person.AribaUserName,
                    RequesterPasswordAdapter = person.AribaPassword,
                    Preparer = person.AribaUserName,
                    LineItems = lineItems,
                    PRType = prUpdateDto.PRType,
                };


                // 🔹 Call Ariba
                var response = await PostMaterialPRToAriba(aribaReq);

                return new MaterialPRResultDto
                {
                    PRNumber = response?.PRNumber,
                    Status = response?.Status
                };
            }
            else if (string.Equals(prDetailsList.First().PRType, Common.PR_SERVICE_ITEM_CATEGORY, StringComparison.OrdinalIgnoreCase))
            {
                var lineItems = prDetailsList.Select(dbLine =>
                {
                    // Try to find matching update line
                    var updateLine = prUpdateDto.LineItems
                        .FirstOrDefault(x => x.LineNumber == dbLine.LineNumber?.ToString());

                    return new ServicePRLineItemDto
                    {
                        NumberInCollection = dbLine.LineNumber ?? 0,
                        Quantity = dbLine.Quantity ?? 0,
                        DeliverTo = dbLine.DeliverTo,
                        NeedBy = dbLine.NeedByDate ?? DateTime.UtcNow,
                        Description = dbLine.Description,
                        MaterialCode = dbLine.MaterialCode,
                        CommodityCode = dbLine.CommodityCode,
                        UOM = dbLine.UOM,
                        CompanyCode = dbLine.CompanyCode,
                        PurchaseOrg = dbLine.PurchaseOrg,
                        CostCenter = dbLine.CostCenter,
                        GLAccount = dbLine.GLAccount,
                        Currency = dbLine.Currency,
                        Vendor = dbLine.SupplierId,
                        Amount = updateLine != null ? Convert.ToDecimal(updateLine.Price) : (dbLine.Price ?? 0),
                        ShipToPlant = dbLine?.CompanyCode,
                        BillTo = dbLine?.CompanyCode,
                        ServiceStartDate = (DateTime)dbLine.ServiceStartDate,
                        ServiceEndDate = (DateTime)dbLine.ServiceEndDate
                    };

                }).ToList();


                // 🔹 Header (take any one row)
                var defaultDetails = prDetailsList.First();


                // 🔹 FINAL REQUEST (same structure as your working one)
                var aribaReq = new ServicePRDto
                {
                    AribaEndPoint = config.aribaUrl,
                    Partition = config.Partition,
                    Variant = config.Variant,
                    Username = config.UserName,
                    Password = config.Password,
                    Server = config.Server,
                    PRNumber = prUpdateDto.PRNumber,
                    Name = defaultDetails.PRTitle,
                    CompanyCode = defaultDetails.CompanyCode,
                    DeliverTo = defaultDetails.DeliverTo,
                    UniqueName = prUpdateDto.PRNumber,
                    NeedBy = defaultDetails.NeedByDate ?? DateTime.UtcNow,
                    Operation = Common.OPERATION_SERVICE_PR_UPDATE,
                    ServiceVariant = config.ServiceVariant,
                    ServicePartition = config.ServicePartition,
                    Preparer = person.AribaUserName,
                    PasswordAdapter = person.AribaPassword,
                    LineItems = lineItems,
                    PRType = prUpdateDto.PRType,
                };


                // 🔹 Call Ariba
                var response = await PostServicesPRToAriba(aribaReq);

                return new MaterialPRResultDto
                {
                    PRNumber = response?.PRNumber,
                    Status = response?.Status
                };
            }
            else
            {
                _logger.LogError("Invalid PR type: " + prUpdateDto.PRType);
                throw new PreConditionFailedCustomException("Invalid PR type.", $"PR type {prUpdateDto.PRType} is not supported.");
            }
        }




        public async Task<MaterialPRSoapEnvelope> PostMaterialPRToAriba(MaterialPRDto aribaReq)
        {

            return await SendMaterialPRRequestAsync(aribaReq, aribaReq.AribaEndPoint, aribaReq.Username, aribaReq.Password);
        }

        public async Task<MaterialPRSoapEnvelope> SendMaterialPRRequestAsync(MaterialPRDto request, string? aribaUrl, string? username, string? password)
        {
            using (var client = new HttpClient())
            {
                var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

                // SOAPAction header
                client.DefaultRequestHeaders.Add("SOAPAction", "RequisitionImportPull");

                string soapXml = BuildMaterialPRRequest(request);


                var boundary = "----=_Part_" + Guid.NewGuid().ToString("N");

                var multipart = new MultipartContent("related", boundary);

                multipart.Headers.ContentType.Parameters.Add(
                    new NameValueHeaderValue("type", "\"application/xop+xml\"")
                );

                multipart.Headers.ContentType.Parameters.Add(
                    new NameValueHeaderValue("start", "\"<rootpart>\"")
                );

                multipart.Headers.ContentType.Parameters.Add(
                    new NameValueHeaderValue("start-info", "\"text/xml\"")
                );

                // SOAP PART
                var soapContent = new StringContent(soapXml, Encoding.UTF8);
                soapContent.Headers.ContentType =
                    new MediaTypeHeaderValue("application/xop+xml");
                soapContent.Headers.ContentType.Parameters.Add(
                    new NameValueHeaderValue("type", "\"application/soap+xml\"")
                );

                soapContent.Headers.ContentType.Parameters.Add(
                    new NameValueHeaderValue("charset", "UTF-8")
                );
                soapContent.Headers.Add("Content-ID", "<rootpart>");

                multipart.Add(soapContent);

                // ATTACHMENTS (CID mapping)
                if (request.Attachments != null)
                {
                    foreach (var att in request.Attachments)
                    {
                        byte[] fileBytes = att.FileBytes;


                        var fileContent = new ByteArrayContent(fileBytes);
                        fileContent.Headers.Add("Content-Transfer-Encoding", "binary");

                        fileContent.Headers.ContentType =
                            new MediaTypeHeaderValue(att.ContentType);

                        fileContent.Headers.Add(
                            "Content-ID",
                            $"<{att.FileName}>"
                        );

                        fileContent.Headers.ContentDisposition =
                            new ContentDispositionHeaderValue("attachment")
                            {
                                FileName = att.FileName
                            };

                        multipart.Add(fileContent);
                    }
                }



                try
                {
                    var response = await client.PostAsync(aribaUrl, multipart);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError($"Failed to post Material PR with status code {response.StatusCode}");
                        throw new Exception($"Failed to post Material PR with status code {response.StatusCode}");
                    }

                    var responseXml = await response.Content.ReadAsStringAsync();


                    var doc = XDocument.Parse(responseXml);


                    var xmlString = doc.ToString(SaveOptions.DisableFormatting);

                    MaterialPRSoapEnvelope envelope = null;

                    try
                    {
                        var serializer = new XmlSerializer(typeof(MaterialPRSoapEnvelope));
                        using (var reader = new StringReader(xmlString))
                        {
                            envelope = (MaterialPRSoapEnvelope)serializer.Deserialize(reader);
                        }

                        if (envelope?.Body?.RequisitionImportPullReply == null)
                        {
                            // Try parsing as a SOAP fault
                            var faultSerializer = new XmlSerializer(typeof(SoapFaultEnvelope));
                            using (var faultReader = new StringReader(xmlString))
                            {
                                var faultEnvelope = (SoapFaultEnvelope)faultSerializer.Deserialize(faultReader);
                                if (faultEnvelope?.Body?.Fault != null)
                                    throw new Exception($"Ariba SOAP Fault: {faultEnvelope.FaultString}");
                            }
                        }
                    }
                    catch (InvalidOperationException invEx)
                    {
                        // Optional: log inner exception for easier debugging
                        _logger.LogError(invEx.InnerException?.Message ?? invEx.Message);

                        // Fallback: try fault parsing again (fresh reader)
                        var faultSerializer = new XmlSerializer(typeof(SoapFaultEnvelope));
                        using (var faultReader = new StringReader(xmlString))
                        {
                            var faultEnvelope = (SoapFaultEnvelope)faultSerializer.Deserialize(faultReader);
                            if (faultEnvelope?.Body?.Fault != null)
                                throw new Exception($"Ariba SOAP Fault: {faultEnvelope.FaultString}");
                        }
                    }

                    _logger.LogInfo($"PR Number: {envelope?.PRNumber}");
                    _logger.LogInfo($"PR Status: {envelope?.Status}");


                    // Return the envelope after successful parsing
                    return envelope;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    throw new Exception("Failed to post the PR in ariba", ex);
                }
            }
        }
        private static string BuildLineItemAttachments(List<LineItemAttachmentDto> attachments)
        {

            if (attachments == null || !attachments.Any())
                return string.Empty;

            var sb = new StringBuilder();
            sb.Append("<urn:Attachments>");

            foreach (var att in attachments)
            {
                sb.Append($@"
                    <urn:item>
                        <urn:Attachment>
                            <urn:ContentType>{att.ContentType}</urn:ContentType>
                            <urn:Filename>{att.FileName}</urn:Filename>
                        </urn:Attachment>

                        <urn:Date>{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}</urn:Date>
                        <urn:ExternalAttachment>false</urn:ExternalAttachment>

                        <urn:MappedFilename><xop:Include href=""cid:{att.FileName}""/></urn:MappedFilename>

                        <urn:User>
                            <urn:PasswordAdapter>{att.PasswordAdapter}</urn:PasswordAdapter>
                            <urn:UniqueName>{att.UploadedBy}</urn:UniqueName>
                        </urn:User>
                    </urn:item>");
            }

            sb.Append("</urn:Attachments>");
            return sb.ToString();
        }



        private static string BuildMaterialPRRequest(MaterialPRDto prRequest)
        {
            var lineItemsXml = new StringBuilder();
            foreach (var lineItem in prRequest.LineItems)
            {

                lineItemsXml.Append($@"
            <urn:item>
                <urn:NumberInCollection>{lineItem.NumberInCollection}</urn:NumberInCollection>
                <urn:Quantity>{lineItem.Quantity}</urn:Quantity>
                <urn:ImportedDeliverToStaging>{(lineItem.DeliverTo != null ? XmlEscape(lineItem.DeliverTo) : lineItem.DeliverTo)}</urn:ImportedDeliverToStaging>
                <urn:ImportedNeedByStaging>{lineItem.NeedBy:yyyy-MM-ddTHH:mm:ssZ}</urn:ImportedNeedByStaging>

                <urn:Description>
                    <urn:Description>{(lineItem.Description != null ? XmlEscape(lineItem.Description) : lineItem.Description)}</urn:Description>
                    <urn:ManPartNumber>{lineItem.ManPartNumber}</urn:ManPartNumber>
                    <urn:SupplierPartNumber>{lineItem.Vendor}</urn:SupplierPartNumber>
                    <urn:SupplierPartAuxiliaryID>{lineItem.SupplierPartAuxiliaryID}</urn:SupplierPartAuxiliaryID>
                    <urn:UnitOfMeasure><urn:UniqueName>{lineItem.UOM}</urn:UniqueName></urn:UnitOfMeasure>
                    <urn:Price>
                        <urn:Amount>{lineItem.Amount}</urn:Amount>
                        <urn:Currency><urn:UniqueName>{lineItem.Currency}</urn:UniqueName></urn:Currency>
                    </urn:Price>
                    <urn:CommonCommodityCode>
                        <urn:Domain>custom</urn:Domain>
                        <urn:UniqueName>{lineItem.CommodityCode}</urn:UniqueName>
                    </urn:CommonCommodityCode>
                </urn:Description>

                <urn:custom>
                    <urn:CustomString name=""MaterialCode"">{lineItem.MaterialCode}</urn:CustomString>
                </urn:custom>

                <urn:CommodityCode><urn:UniqueName>{lineItem.CommodityCode}</urn:UniqueName></urn:CommodityCode>
                <urn:ItemCategory><urn:UniqueName>{(prRequest.PRType == Common.PR_MATERIAL_TYPE ? "M" : "D")}</urn:UniqueName></urn:ItemCategory>
                <urn:PurchaseOrg><urn:UniqueName>{lineItem.PurchaseOrg}</urn:UniqueName></urn:PurchaseOrg>
                <urn:PurchaseGroup><urn:UniqueName>{lineItem.PurchaseGroup}</urn:UniqueName></urn:PurchaseGroup>
                <urn:ShipTo><urn:UniqueName>{lineItem.ShipToPlant}</urn:UniqueName></urn:ShipTo>
                <urn:BillingAddress><urn:UniqueName>{lineItem.BillTo}</urn:UniqueName></urn:BillingAddress>
                <urn:Supplier><urn:UniqueName>{lineItem.Vendor}</urn:UniqueName></urn:Supplier>
                <urn:SupplierLocation><urn:UniqueName>{lineItem.SupplierLocation}</urn:UniqueName></urn:SupplierLocation>

                <urn:ImportedAccountingsStaging>
                    <urn:SplitAccountings>
                        <urn:item>
                            <urn:NumberInCollection>1</urn:NumberInCollection>
                            <urn:Percentage>100</urn:Percentage>

                            <urn:CostCenter>
                                <urn:UniqueName>{lineItem.CostCenter}</urn:UniqueName>
                                <urn:CompanyCode><urn:UniqueName>{lineItem.CompanyCode}</urn:UniqueName></urn:CompanyCode>
                            </urn:CostCenter>

                            <urn:GeneralLedger>
                                <urn:UniqueName>{lineItem.GLAccount}</urn:UniqueName>
                                <urn:CompanyCode><urn:UniqueName>{lineItem.CompanyCode}</urn:UniqueName></urn:CompanyCode>
                            </urn:GeneralLedger>

                            <urn:ProcurementUnit><urn:UniqueName>{lineItem.ProcurementUnit}</urn:UniqueName></urn:ProcurementUnit>
                        </urn:item>
                    </urn:SplitAccountings>
                    <urn:Type><urn:UniqueName>_Percentage</urn:UniqueName></urn:Type>
                </urn:ImportedAccountingsStaging>
            </urn:item>");
            }
            var attachmentXml = BuildLineItemAttachments(prRequest.Attachments);
            return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:urn=""urn:Ariba:Buyer:{prRequest.Server}"" xmlns:xop=""http://www.w3.org/2004/08/xop/include"">
                <soapenv:Header>
                    <urn:Headers>
                        <urn:variant>{prRequest.Variant}</urn:variant>
                        <urn:partition>{prRequest.Partition}</urn:partition>
                    </urn:Headers>
                </soapenv:Header>
    <soapenv:Body>
        <urn:RequisitionImportPullRequest partition=""{prRequest.Partition}"" variant=""{prRequest.Variant}"">
            <urn:Requisition_RequisitionImportPull_Item>
                <urn:item>
                <urn:Operation>{prRequest.Operation}</urn:Operation>
                    <urn:UniqueName>{prRequest.UniqueName}</urn:UniqueName>
                    <urn:Name>{(prRequest.Name != null ? XmlEscape(prRequest.Name) : prRequest.Name)}</urn:Name>

                    <urn:Preparer>
                        <urn:PasswordAdapter>{prRequest.RequesterPasswordAdapter}</urn:PasswordAdapter>
                        <urn:UniqueName>{prRequest.Preparer}</urn:UniqueName>
                    </urn:Preparer>

                    <urn:Requester>
                        <urn:PasswordAdapter>{prRequest.RequesterPasswordAdapter}</urn:PasswordAdapter>
                        <urn:UniqueName>{prRequest.Requester}</urn:UniqueName>
                    </urn:Requester>

                    <urn:CompanyCode><urn:UniqueName>{prRequest.CompanyCode}</urn:UniqueName></urn:CompanyCode>
                    <urn:OriginatingSystemReferenceID></urn:OriginatingSystemReferenceID>

                    <urn:DefaultLineItem>
                        <urn:DeliverTo>{prRequest.DeliverTo}</urn:DeliverTo>
                        <urn:NeedBy>{prRequest.NeedBy:yyyy-MM-ddTHH:mm:ssZ}</urn:NeedBy>
                    </urn:DefaultLineItem>
                     <urn:ImportedHeaderCommentStaging>
                        {(prRequest.HeaderComment != null ? XmlEscape(prRequest.HeaderComment) : prRequest.HeaderComment)}
                    </urn:ImportedHeaderCommentStaging>
                    <urn:ImportedHeaderExternalCommentStaging>true</urn:ImportedHeaderExternalCommentStaging>
                        {attachmentXml}
                        
                    <urn:LineItems>
                        {lineItemsXml}
                    </urn:LineItems>
                </urn:item>
            </urn:Requisition_RequisitionImportPull_Item>
        </urn:RequisitionImportPullRequest>
    </soapenv:Body>
</soapenv:Envelope>";
        }


        private static string XmlEscape(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value
                .Replace("&", "&amp;")   // Must be first!
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }

        public async Task<MaterialPRSoapEnvelope> PostServicesPRToAriba(ServicePRDto aribaReq)
        {

            return await SendMaterialPRRequestAsync(aribaReq, aribaReq.AribaEndPoint, aribaReq.Username, aribaReq.Password);
        }

        public async Task<MaterialPRSoapEnvelope> SendMaterialPRRequestAsync(ServicePRDto request, string? aribaUrl, string? username, string? password)
        {
            using (var client = new HttpClient())
            {
                var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

                // SOAPAction header
                client.DefaultRequestHeaders.Add("SOAPAction", "RequisitionImportPull");

                string soapXml = BuildServicePRRequest(request);
                Console.WriteLine("Constructed SOAP XML:");
                Console.WriteLine(soapXml);



                var boundary = "----=_Part_" + Guid.NewGuid().ToString("N");

                var multipart = new MultipartContent("related", boundary);

                multipart.Headers.ContentType.Parameters.Add(
                    new NameValueHeaderValue("type", "\"application/xop+xml\"")
                );

                multipart.Headers.ContentType.Parameters.Add(
                    new NameValueHeaderValue("start", "\"<rootpart>\"")
                );

                multipart.Headers.ContentType.Parameters.Add(
                    new NameValueHeaderValue("start-info", "\"text/xml\"")
                );

                // SOAP PART
                var soapContent = new StringContent(soapXml, Encoding.UTF8);
                soapContent.Headers.ContentType =
                    new MediaTypeHeaderValue("application/xop+xml");
                soapContent.Headers.ContentType.Parameters.Add(
                    new NameValueHeaderValue("type", "\"application/soap+xml\"")
                );

                soapContent.Headers.ContentType.Parameters.Add(
                    new NameValueHeaderValue("charset", "UTF-8")
                );
                soapContent.Headers.Add("Content-ID", "<rootpart>");

                multipart.Add(soapContent);

                // ATTACHMENTS (CID mapping)
                if (request.Attachments != null)
                {
                    foreach (var att in request.Attachments)
                    {
                        byte[] fileBytes = att.FileBytes;


                        var fileContent = new ByteArrayContent(fileBytes);
                        fileContent.Headers.Add("Content-Transfer-Encoding", "binary");

                        fileContent.Headers.ContentType =
                            new MediaTypeHeaderValue(att.ContentType);

                        fileContent.Headers.Add(
                            "Content-ID",
                            $"<{att.FileName}>"
                        );

                        fileContent.Headers.ContentDisposition =
                            new ContentDispositionHeaderValue("attachment")
                            {
                                FileName = att.FileName
                            };

                        multipart.Add(fileContent);
                    }
                }



                try
                {
                    var response = await client.PostAsync(aribaUrl, multipart);


                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError($"Failed to post Material PR with status code {response.StatusCode}");
                        throw new Exception($"Failed to post Material PR with status code {response.StatusCode}");
                    }

                    var responseXml = await response.Content.ReadAsStringAsync();


                    var doc = XDocument.Parse(responseXml);


                    var xmlString = doc.ToString(SaveOptions.DisableFormatting);

                    MaterialPRSoapEnvelope envelope = null;

                    try
                    {
                        var serializer = new XmlSerializer(typeof(MaterialPRSoapEnvelope));
                        using (var reader = new StringReader(xmlString))
                        {
                            envelope = (MaterialPRSoapEnvelope)serializer.Deserialize(reader);
                        }

                        if (envelope?.Body?.RequisitionImportPullReply == null)
                        {
                            // Try parsing as a SOAP fault
                            var faultSerializer = new XmlSerializer(typeof(SoapFaultEnvelope));
                            using (var faultReader = new StringReader(xmlString))
                            {
                                var faultEnvelope = (SoapFaultEnvelope)faultSerializer.Deserialize(faultReader);
                                if (faultEnvelope?.Body?.Fault != null)
                                    throw new Exception($"Ariba SOAP Fault: {faultEnvelope.FaultString}");
                            }
                        }
                    }
                    catch (InvalidOperationException invEx)
                    {
                        // Optional: log inner exception for easier debugging
                        _logger.LogError(invEx.InnerException?.Message ?? invEx.Message);

                        // Fallback: try fault parsing again (fresh reader)
                        var faultSerializer = new XmlSerializer(typeof(SoapFaultEnvelope));
                        using (var faultReader = new StringReader(xmlString))
                        {
                            var faultEnvelope = (SoapFaultEnvelope)faultSerializer.Deserialize(faultReader);
                            if (faultEnvelope?.Body?.Fault != null)
                                throw new Exception($"Ariba SOAP Fault: {faultEnvelope.FaultString}");
                        }
                    }

                    _logger.LogInfo($"PR Number: {envelope?.PRNumber}");
                    _logger.LogInfo($"PR Status: {envelope?.Status}");


                    // Return the envelope after successful parsing
                    return envelope;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    throw new Exception("Failed to post Service PR", ex);
                }
            }
        }

        private static string BuildServiceLineItemAttachments(List<LineItemAttachmentDto> attachments)
        {

            if (attachments == null || !attachments.Any())
                return string.Empty;

            var sb = new StringBuilder();
            sb.Append("<urn:Attachments>");

            foreach (var att in attachments)
            {
                sb.Append($@"
                    <urn:item>
                        <urn:Attachment>
                            <urn:ContentType>{att.ContentType}</urn:ContentType>
                            <urn:Filename>{att.FileName}</urn:Filename>
                        </urn:Attachment>

                        <urn:Date>{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}</urn:Date>
                        <urn:ExternalAttachment>false</urn:ExternalAttachment>

                        <urn:MappedFilename><xop:Include href=""cid:{att.FileName}""/></urn:MappedFilename>

                        <urn:User>
                            <urn:PasswordAdapter>{att.PasswordAdapter}</urn:PasswordAdapter>
                            <urn:UniqueName>{att.UploadedBy}</urn:UniqueName>
                        </urn:User>
                    </urn:item>");
            }

            sb.Append("</urn:Attachments>");
            return sb.ToString();
        }



        private static string BuildServicePRRequest(ServicePRDto prRequest)
        {
            var lineItemsXml = new StringBuilder();



            foreach (var lineItem in prRequest.LineItems)
            {
                lineItemsXml.Append($@"
                <urn:item>
                    <urn:NumberInCollection>{lineItem.NumberInCollection}</urn:NumberInCollection>
                    <urn:OriginatingSystemLineNumber>{lineItem.NumberInCollection}</urn:OriginatingSystemLineNumber>
                    <urn:BillingAddress>
                        <urn:UniqueName>{(lineItem.BillTo != null ? XmlEscape(lineItem.BillTo) : lineItem.BillTo)}</urn:UniqueName>
                    </urn:BillingAddress>
                    <urn:CommodityCode>
                        <urn:UniqueName>{lineItem.CommodityCode}</urn:UniqueName>
                    </urn:CommodityCode>
                    <urn:Description>
                        <urn:CommonCommodityCode>
                            <urn:Domain>custom</urn:Domain>
                            <urn:UniqueName>{lineItem.CommodityCode}</urn:UniqueName>
                        </urn:CommonCommodityCode>
                        <urn:ConversionFactor>1</urn:ConversionFactor>
                        <urn:Description>{(lineItem.Description != null ? XmlEscape(lineItem.Description) : lineItem.Description)}</urn:Description>
                        <urn:Price>
                            <urn:Amount>{lineItem.Amount}</urn:Amount>
                            <urn:Currency>
                                <urn:UniqueName>{lineItem.Currency}</urn:UniqueName>
                            </urn:Currency>
                        </urn:Price>
                        <urn:UnitOfMeasure>
                            <urn:UniqueName>{lineItem.UOM}</urn:UniqueName>
                        </urn:UnitOfMeasure>
                    </urn:Description>
                    <urn:Quantity>{lineItem.Quantity}</urn:Quantity>
                    <urn:PurchaseOrg>
                        <urn:UniqueName>{lineItem.PurchaseOrg}</urn:UniqueName>
                    </urn:PurchaseOrg>
                    <urn:PurchaseGroup>
                        <urn:UniqueName>{lineItem.PurchaseGroup}</urn:UniqueName>
                    </urn:PurchaseGroup>
                    <urn:ItemCategory>
                        <urn:UniqueName>D</urn:UniqueName>
                    </urn:ItemCategory>
                    <urn:ImportedItemCategoryStaging>
                        <urn:UniqueName>D</urn:UniqueName>
                    </urn:ImportedItemCategoryStaging>
                    <urn:ImportedAccountCategoryStaging>
                        <urn:UniqueName></urn:UniqueName>
                    </urn:ImportedAccountCategoryStaging>
                    <urn:ImportedNeedByStaging>{lineItem.NeedBy:yyyy-MM-ddTHH:mm:ss}</urn:ImportedNeedByStaging>
                    <urn:ImportedDeliverToStaging>{lineItem.DeliverTo}</urn:ImportedDeliverToStaging>
                    <urn:ShipTo>
                        <urn:UniqueName>{lineItem.ShipToPlant}</urn:UniqueName>
                    </urn:ShipTo>
                    <urn:IsServiceChild>false</urn:IsServiceChild>
                    <urn:ImportServiceDetailsStaging>
                        <urn:ExpectedAmount>
                            <urn:Amount>{lineItem.Amount}</urn:Amount>
                            <urn:Currency>
                                <urn:UniqueName>{lineItem.Currency}</urn:UniqueName>
                            </urn:Currency>
                        </urn:ExpectedAmount>
                        <urn:MaxAmount>
                            <urn:Amount>{lineItem.Amount}</urn:Amount>
                            <urn:Currency>
                                <urn:UniqueName>{lineItem.Currency}</urn:UniqueName>
                            </urn:Currency>
                        </urn:MaxAmount>
                        <urn:RequiresServiceEntry>true</urn:RequiresServiceEntry>
                        <urn:ServiceStartDate>{lineItem.ServiceStartDate:yyyy-MM-ddTHH:mm:ss}</urn:ServiceStartDate>
                        <urn:ServiceEndDate>{lineItem.ServiceEndDate:yyyy-MM-ddTHH:mm:ss}</urn:ServiceEndDate>
                    </urn:ImportServiceDetailsStaging>
                    <urn:ImportedAccountingsStaging>
                        <urn:SplitAccountings>
                            <urn:item>
                                <urn:NumberInCollection>{lineItem.NumberInCollection}</urn:NumberInCollection>
                                <urn:Percentage>100</urn:Percentage>
                                <urn:CostCenter>
                                    <urn:UniqueName>{lineItem.CostCenter}</urn:UniqueName>
                                    <urn:CompanyCode>
                                        <urn:UniqueName>{lineItem.CompanyCode}</urn:UniqueName>
                                    </urn:CompanyCode>
                                </urn:CostCenter>
                                <urn:GeneralLedger>
                                    <urn:UniqueName>{lineItem.GLAccount}</urn:UniqueName>
                                    <urn:CompanyCode>
                                        <urn:UniqueName>{lineItem.CompanyCode}</urn:UniqueName>
                                    </urn:CompanyCode>
                                </urn:GeneralLedger>
                                <urn:ProcurementUnit>
                                    <urn:UniqueName></urn:UniqueName>
                                </urn:ProcurementUnit>
                            </urn:item>
                        </urn:SplitAccountings>
                    </urn:ImportedAccountingsStaging>
                    <urn:Supplier>
                        <urn:UniqueName>{lineItem.Vendor}</urn:UniqueName>
                    </urn:Supplier>
                </urn:item>");
            }
            var attachmentXml = BuildServiceLineItemAttachments(prRequest.Attachments);
            return $@"
            <soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/""
                            xmlns:urn=""urn:Ariba:Buyer:{prRequest.Server}"" xmlns:xop=""http://www.w3.org/2004/08/xop/include"">
                <soapenv:Header>
                    <urn:Headers>
                        <urn:variant>{prRequest.Variant}</urn:variant>
                        <urn:partition>{prRequest.Partition}</urn:partition>
                    </urn:Headers>
                </soapenv:Header>

                <soapenv:Body>
                    <urn:RequisitionImportPullRequest partition=""{prRequest.ServicePartition}"" variant=""{prRequest.ServiceVariant}"">
                        <urn:Requisition_RequisitionImportPull_Item>
                            <urn:item>
                            <urn:Operation>{prRequest.Operation}</urn:Operation>
                            <urn:OriginatingSystem>{(prRequest.Name != null ? XmlEscape(prRequest.Name) : prRequest.Name)}</urn:OriginatingSystem>
                            <urn:OriginatingSystemReferenceID></urn:OriginatingSystemReferenceID>
                                <urn:UniqueName>{prRequest.UniqueName}</urn:UniqueName>
                                <urn:Name>{(prRequest.Name != null ? XmlEscape(prRequest.Name) : prRequest.Name)}</urn:Name>

                                <urn:Preparer>
                                    <urn:PasswordAdapter>{prRequest.PasswordAdapter}</urn:PasswordAdapter>
                                    <urn:UniqueName>{prRequest.Preparer}</urn:UniqueName>
                                </urn:Preparer>

                                <urn:CompanyCode>
                                    <urn:UniqueName>{prRequest.CompanyCode}</urn:UniqueName>
                                </urn:CompanyCode>
                                

                                <urn:DefaultLineItem>
                                    <urn:DeliverTo>{prRequest.DeliverTo}</urn:DeliverTo>
                                    <urn:NeedBy>{prRequest.NeedBy:yyyy-MM-ddTHH:mm:ss}</urn:NeedBy>
                                </urn:DefaultLineItem>
                                <urn:ImportedHeaderCommentStaging>
                                    {prRequest.HeaderComment}
                                </urn:ImportedHeaderCommentStaging>
                                <urn:ImportedHeaderExternalCommentStaging>true</urn:ImportedHeaderExternalCommentStaging>
                                {attachmentXml}

                                <urn:LineItems>
                                    {lineItemsXml}
                                </urn:LineItems>
                            </urn:item>
                        </urn:Requisition_RequisitionImportPull_Item>
                    </urn:RequisitionImportPullRequest>
                </soapenv:Body>
            </soapenv:Envelope>";
        }
    }
}
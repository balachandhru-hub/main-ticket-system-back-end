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
using DocumentFormat.OpenXml.Drawing;

namespace Services
{
    public class ServicesPRService : IServicesPRService
    {
        private readonly IRepositoryWrapper _repoWrapper;

        private readonly ILoggerManager _logger;
        private readonly IDomainService _domainService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private static AribaToken _cachedToken;
        private static readonly Dictionary<Guid, (AribaToken Token, DateTime Expiry)> _tokenCache = new();
        private static DateTime _tokenExpiry;
        /// <param name="repoWrapper">Repository wrapper.</param>
        /// <param name="logger">Logger manager.</param>

        public ServicesPRService(IRepositoryWrapper repoWrapper, ILoggerManager logger, IDomainService domainService, IHttpContextAccessor httpContextAccessor)
        {
            _repoWrapper = repoWrapper;
            _logger = logger;
            _domainService = domainService;
            _httpContextAccessor = httpContextAccessor;
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
                    throw new Exception("Failed to post Material PR", ex);
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
        <urn:UniqueName>{lineItem.BillTo}</urn:UniqueName>
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
        <urn:Description>{lineItem.Description}</urn:Description>
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
            string teamReceivedXml = string.Empty;

            if (prRequest.Type == Common.PR_ENTERTAINMENT_DEPARTMENT)
            {
                teamReceivedXml =
                    $"<urn:IsTeamReceived>{prRequest.TeamReceived.ToString().ToLower()}</urn:IsTeamReceived>";
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
                
                   <urn:OriginatingSystem>{prRequest.Name}</urn:OriginatingSystem>
                 <urn:OriginatingSystemReferenceID>{prRequest.SystemReferenceID}</urn:OriginatingSystemReferenceID>
                    <urn:UniqueName></urn:UniqueName>
                    <urn:Name>{prRequest.Name}</urn:Name>

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
                    {teamReceivedXml}
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

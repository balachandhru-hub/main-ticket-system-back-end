using Contracts;
using Contracts.IRepository;
using Contracts.IServices;
using Dto;
using Entities;
using Entities.Common;
using Entities.Models;
using ExceptionHandler;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;


namespace Services
{
    public class PublicRelationServicePRService : IPublicRelationServicePRService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerManager _logger;
        private readonly IPublicRelationServicePRSoapService _publicRelationServicePRService;
        private readonly IBulkInsertHelper _bulkInsertHelper;
        private readonly IAssetService _assetService;
        private readonly IEmailService _emailService;

        public PublicRelationServicePRService(
            IRepositoryWrapper repositoryWrapper,
            ILoggerManager logger,
            IPublicRelationServicePRSoapService publicRelationServicePRService,
            IAssetService assetService,
            IBulkInsertHelper bulkInsertHelper,
            IEmailService emailService, IServiceProvider serviceProvider)
        {
            _repositoryWrapper = repositoryWrapper;
            _publicRelationServicePRService = publicRelationServicePRService;
            _logger = logger;

            _bulkInsertHelper = bulkInsertHelper;
            _assetService = assetService;
            _emailService = emailService;

        }

        public async Task<List<MaterialPRResultDto>> PostPublicQuickPRToAriba(
      List<AribaServicePRDto> prRequests,
      Guid domainId,
      MyProfileDto dto,
      Guid userId,
      List<AssetUploadDto>? assetUploadDto,
      bool isUpdate)
        {
            _logger.LogInfo("Preparing to post Material PRs to Ariba");

            var profileDto = _repositoryWrapper.Person.GetMyProfileByUserId(userId);
            if (profileDto == null)
                throw new NotFoundCustomException("User profile missing", $"No profile found for userId {userId}");

            if (dto == null)
                throw new NotFoundCustomException("Profile data missing", "User profile is required to create PR");

            var defaultDetails = _repositoryWrapper.MaterialPR.GetPRDetails(dto.AribaUserName);
            List<MaterialPRResultDto> prResults = new();
            List<ServicePR> newRecords = new();
            List<ServicePR> updatedRecords = new();

            try
            {
                var groupedPRs = prRequests
                    .SelectMany(pr => pr.LineItems.Select(li => new { pr, li }))
                    .GroupBy(x => isUpdate ? x.pr.PRNumber : x.pr.UniqueName)
                    .Select(g => new ServicePRDto
                    {
                        PRNumber = g.First().pr.PRNumber,
                        UniqueName = g.Key,
                        Name = g.First().pr.Name,
                        IsHeader = g.First().pr.IsHeader,
                        NeedBy = g.First().pr.NeedBy,
                        HeaderComment = g.First().pr.HeaderComment,
                        LineItems = g.Select(x => new ServicePRLineItemDto
                        {
                            NumberInCollection = x.li.NumberInCollection,
                            LineItemNumber = x.li.LineItemNumber,
                            Description = x.li.Description,
                            Quantity = x.li.Quantity,
                            UOM = x.li.UOM,
                            Amount = x.li.Amount,
                            Currency = x.li.Currency,
                            CommodityCode = x.li.CommodityCode,
                            Vendor = x.li.Vendor,
                            Contract = x.li.Contract,
                            PurchaseOrg = x.li.PurchaseOrg,
                            GLAccount = x.li.GLAccount,
                            CostCenter = x.li.CostCenter,
                            MaterialCode = x.li.MaterialCode,
                            NeedBy = x.li.NeedBy,
                            DeliveryDate = x.li.DeliveryDate,
                            ShipToPlant = x.li.ShipToPlant,
                            PurchaseGroup = x.li.PurchaseGroup,
                            BillTo = x.li.BillTo,
                            TaxCode = x.li.TaxCode,
                            Title = x.li.Title,
                            Venue = x.li.Venue,
                            AccountType = x.li.AccountType,
                            AutoSES = x.li.AutoSES,
                            AutoInvoices = x.li.AutoInvoices,
                            TaxInclude = x.li.TaxInclude,
                            VendorName = x.li.VendorName,
                            InvoiceID = x.li.InvoiceID,
                            InvoiceDate = x.li.InvoiceDate,
                            InvoiceComment = x.li.InvoiceComment,
                            ServiceEndDate = x.li.ServiceEndDate,
                            ServiceStartDate = x.li.ServiceStartDate,
                            CategoryName = x.li.CategoryName,
                            ParentLineItemNumber = x.li.ParentLineItemNumber,
                            InvoiceAttachment = x.li.InvoiceAttachment != null
                                ? x.li.InvoiceAttachment.Select(att => new AssetUploadDto
                                {
                                    FileName = att.FileName,
                                    ContentType = att.ContentType,
                                    FileBytes = att.FileBytes,
                                    AssetType = att.AssetType,
                                    EntityType = att.EntityType
                                }).ToList()
                                : new List<AssetUploadDto>()
                        }).ToList()
                    })
                    .ToList();

                // Material Attachments
                if (assetUploadDto != null && assetUploadDto.Any())
                {
                    foreach (var pr in groupedPRs)
                    {
                        pr.Attachments = assetUploadDto.Select(att => new LineItemAttachmentDto
                        {
                            FileName = att.FileName,
                            Extension = Path.GetExtension(att.FileName)?.Replace(".", ""),
                            ContentType = att.ContentType,
                            UploadedBy = profileDto.AribaUserName,
                            PasswordAdapter = profileDto.AribaPassword,
                            type = Common.TYPE_SERVICE_PR_UPLOAD,
                            FileBytes = att.FileBytes
                        }).ToList();
                    }
                }

                foreach (var prRequest in groupedPRs)
                {
                    if (prRequest.NeedBy == default(DateTime))
                    {
                        prRequest.NeedBy = DateTime.UtcNow.Date;
                    }

                    prRequest.NeedBy = new DateTime(
                        prRequest.NeedBy.Year,
                        prRequest.NeedBy.Month,
                        prRequest.NeedBy.Day,
                        12, 0, 0,
                        DateTimeKind.Utc
                    );


                    string description = "";
                    string actualPRNumber = "";
                    string headerValue = "";
                    // Truncate headerValue to max 120 chars


                    headerValue = prRequest.LineItems?.FirstOrDefault() is var item && item != null ? string.IsNullOrWhiteSpace(item.Venue) ? $"{item.VendorName} {item.NeedBy:yyyy-MM-dd}" : $"{item.CategoryName} {item.Venue} {item.VendorName} {item.NeedBy:yyyy-MM-dd}" : string.Empty;
                    headerValue = headerValue?.Length > Common.PR_TITLE_LIMIT ? headerValue.Substring(0, Common.PR_TITLE_LIMIT) : headerValue;

                    foreach (var li in prRequest.LineItems)
                    {
                        if (!isUpdate)
                        {

                            if (li.ParentLineItemNumber != null)
                            {
                                var parentLine = prRequest.LineItems
                                   .FirstOrDefault(x => x.NumberInCollection == li.ParentLineItemNumber);

                                if (parentLine == null)
                                    throw new Exception($"Parent line item not found for {li.ParentLineItemNumber}");

                                li.CompanyCode = parentLine.CompanyCode;

                                var parentDescription = parentLine.Description;
                                var subPropertyName = li.Venue;
                                var CategoryName = parentLine.CategoryName;


                                var reInvoiceDescription =
                                    $"Re-invoicing to {subPropertyName} {parentDescription}";



                                // Set Line Item Description (only for child)
                                li.Description = reInvoiceDescription;

                                li.CostCenter = li.CostCenter;
                                li.GLAccount = li.GLAccount;
                                li.PurchaseGroup = parentLine.PurchaseGroup;
                                li.PurchaseOrg = parentLine.PurchaseOrg;
                                li.ShipToPlant = parentLine.ShipToPlant;
                                li.DeliverTo = parentLine.DeliverTo;
                                li.CompanyCode = parentLine.CompanyCode;
                                li.BillTo = parentLine.CompanyCode;
                                li.MaterialCode = parentLine.MaterialCode ?? Common.PUBLIC_RELATION_MATERIAL_CODE;


                            }
                            // 🔹 NORMAL LINE ITEM (Venue logic – existing behavior)
                            else
                            {
                                var venueDetails = await _repositoryWrapper.PublicProperty
                                    .GetPublicVenueDetailsByNameAsync(domainId, li.Venue);

                                li.PurchaseGroup = li.PurchaseGroup ?? venueDetails.PurchaseGroup;
                                li.CompanyCode = li.CompanyCode ?? venueDetails.CompanyCode;
                                li.PurchaseOrg = li.PurchaseOrg ?? venueDetails.PurchaseOrg;
                                li.CostCenter = venueDetails.CostCenter;
                                li.GLAccount = li.GLAccount ?? venueDetails.GLAccount;
                                li.ShipToPlant = li.ShipToPlant ?? venueDetails.ShipToPlant;

                                li.DeliverTo = li.DeliverTo ?? defaultDetails.DeliverTo;
                                li.BillTo = venueDetails.CompanyCode;

                            }


                            li.PurchaseGroup = li.PurchaseGroup ?? defaultDetails.PurchaseGroup;
                            if (li.TaxInclude)
                            {
                                li.CommodityCode = Common.PUBLIC_RELATION_SERVICE_COMMODITY_CODE_TAX;
                            }
                            else
                            {
                                li.CommodityCode = Common.PUBLIC_RELATION_SERVICE_COMMODITY_CODE_TAX;
                            }
                            li.NumberInCollection = li.NumberInCollection;
                            li.Currency = string.IsNullOrWhiteSpace(li.Currency)
                                ? Common.DEFAULT_CURRENCY
                                : li.Currency;
                            var today = DateTime.UtcNow.Date;
                            var endOfMonth = new DateTime(
                                today.Year,
                                today.Month,
                                DateTime.DaysInMonth(today.Year, today.Month)
                            );

                            if (li.ServiceStartDate == default(DateTime))
                            {
                                li.ServiceStartDate = today;
                            }

                            if (li.ServiceEndDate == default(DateTime))
                            {
                                li.ServiceEndDate = endOfMonth;
                            }

                            li.NeedBy = prRequest.NeedBy;
                            li.CategoryName = li.CategoryName;

                            description = string.IsNullOrWhiteSpace(li.Venue)
                                ? $"{li.VendorName} {li.ServiceStartDate:yyyy-MM-dd}"
                                : $"{li.CategoryName} {li.Venue} {li.VendorName}{li.ServiceStartDate:yyyy-MM-dd}";

                            if (string.IsNullOrWhiteSpace(li.Description))
                            {
                                li.Description = string.IsNullOrWhiteSpace(li.Venue)
                                    ? $"{li.VendorName} {li.ServiceStartDate:yyyy-MM-dd}"
                                    : $"{li.CategoryName} {li.Venue} {li.VendorName} {li.ServiceStartDate:yyyy-MM-dd}";
                            }
                            prRequest.Name = headerValue ?? description;

                            li.DeliveryDate = li.DeliveryDate ?? DateTime.Now.AddDays(1);
                            li.InvoiceAttachment = li.InvoiceAttachment;
                            li.AutoSES = li.AutoSES;
                            li.AutoInvoices = li.AutoInvoices;
                        }
                    }

                    if (isUpdate)
                    {
                        var storedReferenceId = _repositoryWrapper.ServicePR.GetOriginatingReferenceIdByPRNumber(domainId, prRequest.PRNumber);

                        var aribaConfig = _repositoryWrapper.DomainSetting
                            .FindFirstByCondition(t => t.DomainId == domainId &&
                                                       t.SettingsKey == Common.DOMAIN_SETTINGS_MATERIAL_PR_POSTING)
                            ?? throw new NotFoundCustomException("Domain settings not found", "Material PR posting configuration missing");

                        var config = JsonConvert.DeserializeObject<AribaPurchaseOrderSettings>(aribaConfig.SettingsValue);

                        var aribaReq = new ServicePRDto
                        {
                            AribaEndPoint = config.aribaUrl,
                            Partition = config.Partition,
                            Variant = config.Variant,
                            Username = config.UserName,
                            Password = config.Password,
                            Server = config.Server,
                            PRNumber = prRequest.PRNumber,
                            Name = prRequest.Name,
                            CompanyCode = prRequest.LineItems.FirstOrDefault()?.CompanyCode,
                            DeliverTo = defaultDetails.DeliverTo,
                            UniqueName = prRequest.UniqueName,
                            NeedBy = prRequest.NeedBy,
                            Operation = Common.OPERATION_SERVICE_PR_UPDATE,
                            HeaderComment = prRequest.HeaderComment,
                            Preparer = profileDto.AribaUserName,
                            PasswordAdapter = profileDto.AribaPassword,
                            ServiceVariant = config.ServiceVariant,
                            ServicePartition = config.ServicePartition,
                            LineItems = prRequest.LineItems,
                            Attachments = prRequest.Attachments,
                            SystemReferenceID = storedReferenceId,
                            InvoiceAttachment = prRequest.LineItems
                                .SelectMany(li => li.InvoiceAttachment ?? new List<AssetUploadDto>())
                                .ToList()
                        };

                        var response = await _publicRelationServicePRService.PostPublicServicesPRToAriba(aribaReq);
                        if (response == null || string.IsNullOrWhiteSpace(response.PRNumber))
                        {
                            _logger.LogError("Ariba response missing PRNumber.");

                            throw new PreConditionFailedCustomException(
                                "Ariba PR update failed",
                                "PRNumber is missing in Ariba response"
                            );
                        }
                        actualPRNumber = response?.PRNumber ?? prRequest.PRNumber;
                        List<ServicePR> dbRecords;
                        bool entitiesAlreadyTracked = false;
                        if (!string.Equals(prRequest.PRNumber, actualPRNumber, StringComparison.OrdinalIgnoreCase))
                        {
                            var oldPRs = _repositoryWrapper.ServicePR
                                .FindByCondition(x => x.DomainId == domainId && x.PRNumber == prRequest.PRNumber)
                                .ToList();

                            foreach (var pr in oldPRs)
                                pr.PRNumber = actualPRNumber;

                            _repositoryWrapper.ServicePR.UpdateRange(oldPRs);

                            var invoices = _repositoryWrapper.ServicePRInvoice
                                .FindByCondition(x => x.PRNumber == prRequest.PRNumber)
                                .ToList();

                            foreach (var inv in invoices)
                                inv.PRNumber = actualPRNumber;

                            _repositoryWrapper.ServicePRInvoice.UpdateRange(invoices);

                            var attachments = _repositoryWrapper.ServicePRAttachment
                                .FindByCondition(x => x.PRServiceUniqueName == prRequest.PRNumber)
                                .ToList();

                            foreach (var att in attachments)
                                att.PRServiceUniqueName = actualPRNumber;

                            _repositoryWrapper.ServicePRAttachment.UpdateRange(attachments);

                            // Save changes and clear tracker to avoid conflicts
                            await _repositoryWrapper.SaveAsync();
                            dbRecords = oldPRs.Where(x => x.IsActive).ToList();
                            entitiesAlreadyTracked = true;
                        }
                        else
                        {
                            // Query using actualPRNumber and without AsNoTracking
                            dbRecords = _repositoryWrapper.ServicePR
                                .FindByCondition(x => x.PRNumber == actualPRNumber
                                                   && x.DomainId == domainId
                                                   && x.IsActive)
                                .ToList();
                            entitiesAlreadyTracked = false;
                        }


                        if (!dbRecords.Any())
                            throw new NotFoundCustomException(
                                "PR not found",
                                $"No PR found with PR Number {actualPRNumber} for update");

                        if (prRequest.IsHeader)
                        {
                            foreach (var oldRecord in dbRecords)
                            {
                                oldRecord.IsActive = false;
                                updatedRecords.Add(oldRecord);
                            }
                            if (!entitiesAlreadyTracked)
                            {
                                _repositoryWrapper.ServicePR.UpdateRange(dbRecords);
                            }

                            var mergedInvoiceID = prRequest.LineItems
                                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.InvoiceID))?.InvoiceID;
                            var mergedInvoiceDate = prRequest.LineItems
                                .FirstOrDefault(x => x.InvoiceDate.HasValue)?.InvoiceDate;
                            var mergedInvoiceComment = prRequest.LineItems
                                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.InvoiceComment))?.InvoiceComment;

                            var hasInvoiceData = !string.IsNullOrWhiteSpace(mergedInvoiceID)
                                                 || mergedInvoiceDate.HasValue
                                                 || !string.IsNullOrWhiteSpace(mergedInvoiceComment);

                            var existingNonHeaderInvs = _repositoryWrapper.ServicePRInvoice
                                .FindByCondition(x => x.PRNumber == actualPRNumber && !x.IsHeader)
                                .ToList();
                            if (existingNonHeaderInvs.Any())
                            {
                                _repositoryWrapper.ServicePRInvoice.DeleteRange(existingNonHeaderInvs);
                            }

                            if (hasInvoiceData)
                            {
                                var existingHeaderInv = _repositoryWrapper.ServicePRInvoice
                                    .FindByCondition(x => x.PRNumber == actualPRNumber && x.IsHeader)
                                    .FirstOrDefault();

                                if (existingHeaderInv != null)
                                {
                                    existingHeaderInv.InvoiceID = mergedInvoiceID;
                                    existingHeaderInv.InvoiceDate = mergedInvoiceDate;
                                    existingHeaderInv.InvoiceComment = mergedInvoiceComment;
                                    _repositoryWrapper.ServicePRInvoice.Update(existingHeaderInv);
                                }
                                else
                                {
                                    _repositoryWrapper.ServicePRInvoice.Create(new Entities.Models.ServicePRInvoice
                                    {
                                        Id = Guid.NewGuid(),
                                        PRNumber = actualPRNumber,
                                        InvoiceID = mergedInvoiceID,
                                        InvoiceDate = mergedInvoiceDate,
                                        InvoiceComment = mergedInvoiceComment,
                                        LineItemNumber = 1,
                                        IsHeader = true
                                    });
                                }
                            }

                            int fallbackIndex = 1;
                            foreach (var li in prRequest.LineItems)
                            {
                                var lineNumber = li.NumberInCollection ?? li.LineItemNumber ?? fallbackIndex++;

                                var venueDetailsForLine = !string.IsNullOrWhiteSpace(li?.Venue)
                                    ? await _repositoryWrapper.AribaVenueListMapping.GetVenueDetailsByNameAsync(domainId, li.Venue)
                                    : null;

                                newRecords.Add(new ServicePR
                                {
                                    DomainId = domainId,
                                    PRNumber = actualPRNumber,
                                    PRServiceUniqueName = aribaReq.SystemReferenceID,
                                    LineItemNumber = lineNumber,
                                    Status = response?.Status,
                                    Description = li?.Description ?? prRequest.Name,
                                    Title = prRequest.Name,
                                    CommodityCode = li?.CommodityCode,
                                    Amount = li?.Amount ?? 0,
                                    Quantity = li?.Quantity ?? 0,
                                    DeliveryDate = li?.DeliveryDate ?? DateTime.Now.AddDays(1),
                                    NeedBy = aribaReq.NeedBy,
                                    UOM = li?.UOM,
                                    Vendor = li?.Vendor,
                                    Type = Common.SERVICE_TYPE,
                                    AccountType = Common.ACCOUNT_TYPE,
                                    GLAccount = li?.GLAccount,
                                    CostCenter = li?.CostCenter,
                                    MaterialCode = li?.MaterialCode,
                                    AutoSES = li?.AutoSES ?? false,
                                    AutoInvoices = li?.AutoInvoices ?? false,
                                    Currency = li?.Currency,
                                    PurchaseOrg = li?.PurchaseOrg,
                                    PurchaseGroup = li?.PurchaseGroup,
                                    CompanyCode = li.CompanyCode ?? venueDetailsForLine?.CompanyCode ?? defaultDetails.CompanyCode,
                                    ShipToPlant = li.ShipToPlant ?? venueDetailsForLine?.ShipToPlant ?? defaultDetails.ShipToPlant,
                                    DeliverTo = li?.DeliverTo ?? defaultDetails.DeliverTo,
                                    PreparerName = aribaReq.Preparer,
                                    BillTo = li?.BillTo ?? venueDetailsForLine?.CompanyCode ?? defaultDetails.CompanyCode,
                                    TaxCode = li?.TaxCode,
                                    Venue = li?.Venue,
                                    HeaderComment = aribaReq.HeaderComment,
                                    Contract = li?.Contract,
                                    TaxInclude = li?.TaxInclude ?? false,
                                    VendorName = li?.VendorName,
                                    InvoiceID = null,
                                    InvoiceDate = null,
                                    InvoiceComment = null,
                                    Category = li?.CategoryName,
                                    ParentLineItemNumber = li?.ParentLineItemNumber,
                                    ServiceStartDate = li.ServiceStartDate,
                                    ServiceEndDate = li.ServiceEndDate,
                                    MaxAmount = li.Amount,
                                    ExpectedAmount = li.Amount
                                });
                            }

                            prResults.Add(new MaterialPRResultDto
                            {
                                PRNumber = actualPRNumber,
                                Status = response?.Status
                            });

                            continue;
                        }
                        var invoicesToUpdate = new List<ServicePRInvoice>();
                        var invoicesToCreate = new List<ServicePRInvoice>();

                        foreach (var li in prRequest.LineItems)
                        {
                            var hasLineInvoice =
                                !string.IsNullOrWhiteSpace(li.InvoiceID) ||
                                li.InvoiceDate.HasValue ||
                                !string.IsNullOrWhiteSpace(li.InvoiceComment);
                            if (!hasLineInvoice)
                                continue;

                            var lineNumber = li.NumberInCollection ?? li.LineItemNumber ?? 1;

                            var existingInv = _repositoryWrapper.ServicePRInvoice
                                .FindByCondition(x =>
                                    x.PRNumber == actualPRNumber &&
                                    x.LineItemNumber == lineNumber &&
                                    !x.IsHeader)
                                .FirstOrDefault();

                            if (existingInv != null)
                            {
                                existingInv.InvoiceID = li.InvoiceID;
                                existingInv.InvoiceDate = li.InvoiceDate;
                                existingInv.InvoiceComment = li.InvoiceComment;
                                existingInv.DateUpdated = DateTime.UtcNow;
                                invoicesToUpdate.Add(existingInv);
                            }
                            else
                            {

                                invoicesToCreate.Add(new ServicePRInvoice
                                {
                                    Id = Guid.NewGuid(),
                                    PRNumber = actualPRNumber,
                                    LineItemNumber = lineNumber,
                                    InvoiceID = li.InvoiceID,
                                    InvoiceDate = li.InvoiceDate,
                                    InvoiceComment = li.InvoiceComment,
                                    IsHeader = false,
                                });
                            }
                        }
                        if (invoicesToUpdate.Any())
                        {
                            _repositoryWrapper.ServicePRInvoice.UpdateRange(invoicesToUpdate);
                        }

                        if (invoicesToCreate.Any())
                        {
                            _repositoryWrapper.ServicePRInvoice.CreateRange(invoicesToCreate);
                        }

                        int dbCount = dbRecords.Count;
                        int reqCount = prRequest.LineItems.Count;

                        if (dbCount != reqCount)
                        {
                            _repositoryWrapper.ServicePR.DeleteRange(dbRecords);

                            foreach (var li in prRequest.LineItems)
                            {
                                var liHasInvoice = !string.IsNullOrWhiteSpace(li.InvoiceID)
                                                   || li.InvoiceDate.HasValue
                                                   || !string.IsNullOrWhiteSpace(li.InvoiceComment);

                                newRecords.Add(new ServicePR
                                {
                                    DomainId = domainId,
                                    PRNumber = actualPRNumber,
                                    PRServiceUniqueName = aribaReq.SystemReferenceID,
                                    LineItemNumber = li.NumberInCollection,
                                    Status = response?.Status,
                                    Description = li.Description,
                                    Title = prRequest.Name,
                                    CommodityCode = li.CommodityCode,
                                    Amount = li.Amount,
                                    Quantity = li.Quantity,
                                    DeliveryDate = li.DeliveryDate ?? DateTime.Now.AddDays(1),
                                    NeedBy = aribaReq.NeedBy,
                                    UOM = li.UOM,
                                    Vendor = li.Vendor,
                                    Type = Common.SERVICE_TYPE,
                                    AccountType = Common.ACCOUNT_TYPE,
                                    GLAccount = li.GLAccount,
                                    CostCenter = li.CostCenter,
                                    MaterialCode = li.MaterialCode,
                                    AutoSES = li.AutoSES,
                                    AutoInvoices = li.AutoInvoices,
                                    Currency = li.Currency,
                                    PurchaseOrg = li.PurchaseOrg,
                                    PurchaseGroup = li.PurchaseGroup,
                                    CompanyCode = li.CompanyCode,
                                    ShipToPlant = li.ShipToPlant,
                                    DeliverTo = li.DeliverTo ?? defaultDetails.DeliverTo,
                                    PreparerName = aribaReq.Preparer,
                                    BillTo = li.BillTo ?? li.CompanyCode,
                                    TaxCode = li.TaxCode,
                                    Venue = li.Venue,
                                    HeaderComment = aribaReq.HeaderComment,
                                    Contract = li.Contract,
                                    TaxInclude = li.TaxInclude,
                                    VendorName = li.VendorName,
                                    InvoiceID = (prRequest.IsHeader || liHasInvoice) ? null : li.InvoiceID,
                                    InvoiceDate = (prRequest.IsHeader || liHasInvoice) ? null : li.InvoiceDate,
                                    ParentLineItemNumber = li.ParentLineItemNumber,
                                    Category = li.CategoryName,
                                    MaxAmount = li.Amount,
                                    ExpectedAmount = li.Amount
                                });
                            }
                        }
                        else
                        {
                            foreach (var existing in dbRecords)
                            {
                                var li = prRequest.LineItems.FirstOrDefault(x =>
                                    (x.LineItemNumber != null && x.LineItemNumber == existing.LineItemNumber) ||
                                    (x.NumberInCollection == existing.LineItemNumber));

                                if (li == null) continue;

                                if (li.TaxInclude)
                                {
                                    li.CommodityCode = Common.PUBLIC_RELATION_COMMODITY_CODE_WITH_TAX;
                                }
                                else
                                {
                                    li.CommodityCode = Common.PUBLIC_RELATION_COMMODITY_CODE_WITHOUT_TAX;
                                }

                                existing.Description = prRequest.Name ?? existing.Description;
                                existing.Title = prRequest.Name ?? existing.Title;
                                existing.CommodityCode = li.CommodityCode ?? existing.CommodityCode;
                                existing.Amount = li.Amount;
                                existing.Quantity = li.Quantity;
                                existing.DeliveryDate = li.DeliveryDate ?? existing.DeliveryDate;
                                existing.UOM = li.UOM ?? existing.UOM;
                                existing.Vendor = li.Vendor ?? existing.Vendor;
                                existing.GLAccount = li.GLAccount ?? existing.GLAccount;
                                existing.CostCenter = li.CostCenter ?? existing.CostCenter;
                                existing.MaterialCode = li.MaterialCode ?? existing.MaterialCode;
                                existing.PurchaseOrg = li.PurchaseOrg ?? existing.PurchaseOrg;
                                existing.PurchaseGroup = li.PurchaseGroup ?? existing.PurchaseGroup;
                                existing.ShipToPlant = li.ShipToPlant ?? existing.ShipToPlant;
                                existing.DeliverTo = li.DeliverTo ?? existing.DeliverTo;
                                existing.TaxCode = li.TaxCode ?? existing.TaxCode;
                                existing.HeaderComment = prRequest.HeaderComment ?? existing.HeaderComment;
                                existing.Contract = li.Contract ?? existing.Contract;
                                existing.BillTo = li.BillTo ?? existing.BillTo;
                                existing.Venue = li.Venue ?? existing.Venue;
                                existing.CompanyCode = prRequest.CompanyCode ?? existing.CompanyCode;
                                existing.Currency = li.Currency ?? existing.Currency;
                                existing.NeedBy = aribaReq.NeedBy;
                                existing.Status = response?.Status ?? existing.Status;
                                existing.VendorName = li.VendorName ?? existing.VendorName;
                                existing.AutoSES = li.AutoSES;
                                existing.AutoInvoices = li.AutoInvoices;
                                existing.ParentLineItemNumber = li.ParentLineItemNumber;
                                existing.Category = li.CategoryName ?? existing.Category;
                                existing.ServiceStartDate = li.ServiceStartDate;
                                existing.ServiceEndDate = li.ServiceEndDate;


                                if (li.AutoInvoices != existing.AutoInvoices)
                                {
                                    existing.AutoInvoices = li.AutoInvoices;
                                }

                                var liHasInvoice = !string.IsNullOrWhiteSpace(li.InvoiceID)
                                                   || li.InvoiceDate.HasValue
                                                   || !string.IsNullOrWhiteSpace(li.InvoiceComment);
                                if (prRequest.IsHeader || liHasInvoice)
                                {
                                    existing.InvoiceID = null;
                                    existing.InvoiceDate = null;
                                    existing.InvoiceComment = null;
                                }
                                else
                                {
                                    existing.InvoiceID = li.InvoiceID ?? existing.InvoiceID;
                                    existing.InvoiceDate = li.InvoiceDate ?? existing.InvoiceDate;
                                    existing.InvoiceComment = li.InvoiceComment ?? existing.InvoiceComment;
                                }

                                updatedRecords.Add(existing);
                            }
                            if (!entitiesAlreadyTracked)
                            {
                                _repositoryWrapper.ServicePR.UpdateRange(dbRecords);
                            }
                        }


                        prResults.Add(new MaterialPRResultDto
                        {
                            PRNumber = actualPRNumber,
                            Status = response?.Status
                        });
                    }
                    else
                    {
                        var aribaConfig = _repositoryWrapper.DomainSetting
                            .FindFirstByCondition(t => t.DomainId == domainId &&
                                                       t.SettingsKey == Common.DOMAIN_SETTINGS_MATERIAL_PR_POSTING)
                            ?? throw new NotFoundCustomException("Domain settings not found", "Material PR posting configuration missing");

                        var config = JsonConvert.DeserializeObject<AribaPurchaseOrderSettings>(aribaConfig.SettingsValue);
                        var generatedPRNumber = await GenerateServiceReferenceNumber();
                        prRequest.UniqueName = headerValue ?? description;
                        var aribaReq = new ServicePRDto
                        {
                            AribaEndPoint = config.aribaUrl,
                            Partition = config.Partition,
                            Variant = config.Variant,
                            Username = config.UserName,
                            Password = config.Password,
                            Server = config.Server,
                            PRNumber = prRequest.PRNumber,
                            Name = prRequest.UniqueName,
                            CompanyCode = prRequest.LineItems.FirstOrDefault()?.CompanyCode,
                            DeliverTo = defaultDetails.DeliverTo,
                            UniqueName = prRequest.UniqueName,
                            NeedBy = prRequest.NeedBy,
                            Operation = Common.OPERATION_MATERIAL_PR_CREATION,
                            HeaderComment = prRequest.HeaderComment,
                            Preparer = profileDto.AribaUserName,
                            PasswordAdapter = profileDto.AribaPassword,
                            ServiceVariant = config.ServiceVariant,
                            ServicePartition = config.ServicePartition,
                            LineItems = prRequest.LineItems,
                            Attachments = prRequest.Attachments,
                            SystemReferenceID = generatedPRNumber,
                            InvoiceAttachment = prRequest.LineItems.SelectMany(li => li.InvoiceAttachment ?? new List<AssetUploadDto>()).ToList()
                        };

                        var response = await _publicRelationServicePRService.PostPublicServicesPRToAriba(aribaReq);
                        if (response == null || string.IsNullOrWhiteSpace(response.PRNumber))
                        {
                            _logger.LogError("Ariba response missing PRNumber.");

                            throw new PreConditionFailedCustomException(
                                "Ariba PR creation failed",
                                "PRNumber is missing in Ariba response"
                            );
                        }
                        actualPRNumber = response?.PRNumber ?? aribaReq.PRNumber;

                        if (prRequest.IsHeader)
                        {
                            var mergedInvoiceID = prRequest.LineItems
                                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.InvoiceID))?.InvoiceID;
                            var mergedInvoiceDate = prRequest.LineItems
                                .FirstOrDefault(x => x.InvoiceDate.HasValue)?.InvoiceDate;
                            var mergedInvoiceComment = prRequest.LineItems
                                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.InvoiceComment))?.InvoiceComment;

                            var hasInvoiceData = !string.IsNullOrWhiteSpace(mergedInvoiceID)
                                                 || mergedInvoiceDate.HasValue
                                                 || !string.IsNullOrWhiteSpace(mergedInvoiceComment);

                            if (hasInvoiceData)
                            {
                                _repositoryWrapper.ServicePRInvoice.Create(new Entities.Models.ServicePRInvoice
                                {
                                    Id = Guid.NewGuid(),
                                    PRNumber = actualPRNumber,
                                    InvoiceID = mergedInvoiceID,
                                    InvoiceDate = mergedInvoiceDate,
                                    InvoiceComment = mergedInvoiceComment,
                                    LineItemNumber = 1,
                                    IsHeader = true
                                });
                            }
                        }
                        else
                        {
                            var createInvoices = new List<ServicePRInvoice>();
                            var updateInvoices = new List<ServicePRInvoice>();
                            foreach (var li in prRequest.LineItems)
                            {
                                var hasLineInvoice = !string.IsNullOrWhiteSpace(li.InvoiceID)
                                                     || li.InvoiceDate.HasValue
                                                     || !string.IsNullOrWhiteSpace(li.InvoiceComment);
                                if (!hasLineInvoice) continue;

                                var lineNumber = li.NumberInCollection ?? li.LineItemNumber ?? 1;

                                var existingInv = _repositoryWrapper.ServicePRInvoice
                                    .FindByCondition(x => x.PRNumber == actualPRNumber
                                                         && x.LineItemNumber == lineNumber
                                                         && !x.IsHeader)
                                    .FirstOrDefault();

                                if (existingInv != null)
                                {
                                    existingInv.InvoiceID = li.InvoiceID;
                                    existingInv.InvoiceDate = li.InvoiceDate;
                                    existingInv.InvoiceComment = li.InvoiceComment;
                                    updateInvoices.Add(existingInv);
                                }
                                else
                                {
                                    createInvoices.Add(new Entities.Models.ServicePRInvoice
                                    {
                                        Id = Guid.NewGuid(),
                                        PRNumber = actualPRNumber,
                                        InvoiceID = li.InvoiceID,
                                        InvoiceDate = li.InvoiceDate,
                                        InvoiceComment = li.InvoiceComment,
                                        LineItemNumber = lineNumber,
                                        IsHeader = false
                                    });
                                }
                            }
                            if (updateInvoices.Any())
                            {
                                _repositoryWrapper.ServicePRInvoice.UpdateRange(updateInvoices);
                            }
                            if (createInvoices.Any())
                            {
                                _repositoryWrapper.ServicePRInvoice.CreateRange(createInvoices);
                            }
                        }

                        prResults.Add(new MaterialPRResultDto
                        {
                            PRNumber = actualPRNumber,
                            Status = response?.Status
                        });

                        foreach (var li in prRequest.LineItems)
                        {
                            var liHasInvoice = !string.IsNullOrWhiteSpace(li.InvoiceID)
                                               || li.InvoiceDate.HasValue
                                               || !string.IsNullOrWhiteSpace(li.InvoiceComment);

                            newRecords.Add(new ServicePR
                            {
                                DomainId = domainId,
                                PRNumber = actualPRNumber,
                                PRServiceUniqueName = aribaReq.SystemReferenceID,
                                LineItemNumber = li.NumberInCollection,
                                Status = response?.Status,
                                Description = li.Description,
                                Title = prRequest.Name,
                                CommodityCode = li.CommodityCode,
                                Amount = li.Amount,
                                Quantity = li.Quantity,
                                DeliveryDate = li.DeliveryDate ?? DateTime.Now.AddDays(1),
                                NeedBy = aribaReq.NeedBy,
                                UOM = li.UOM,
                                Vendor = li.Vendor,
                                Type = Common.SERVICE_TYPE,
                                AccountType = Common.ACCOUNT_TYPE,
                                GLAccount = li.GLAccount,
                                CostCenter = li.CostCenter,
                                MaterialCode = li.MaterialCode,
                                AutoSES = li.AutoSES,
                                AutoInvoices = li.AutoInvoices,
                                Currency = li.Currency,
                                PurchaseOrg = li.PurchaseOrg,
                                PurchaseGroup = li.PurchaseGroup,
                                CompanyCode = li.CompanyCode,
                                ShipToPlant = li.ShipToPlant,
                                DeliverTo = li.DeliverTo ?? defaultDetails.DeliverTo,
                                PreparerName = aribaReq.Preparer,
                                BillTo = li.BillTo,
                                TaxCode = li.TaxCode,
                                Venue = li.Venue,
                                HeaderComment = aribaReq.HeaderComment,
                                Contract = li.Contract,
                                TaxInclude = li.TaxInclude,
                                VendorName = li.VendorName,
                                InvoiceID = (prRequest.IsHeader || liHasInvoice) ? null : li.InvoiceID,
                                InvoiceDate = (prRequest.IsHeader || liHasInvoice) ? null : li.InvoiceDate,
                                InvoiceComment = (prRequest.IsHeader || liHasInvoice) ? null : li.InvoiceComment,
                                ParentLineItemNumber = li.ParentLineItemNumber,
                                Category = li.CategoryName,
                                ServiceStartDate = li.ServiceStartDate,
                                ServiceEndDate = li.ServiceEndDate,
                                MaxAmount = li.Amount,
                                ExpectedAmount = li.Amount
                            });
                        }
                    }

                    // Save invoice attachments using actualPRNumber
                    foreach (var li in prRequest.LineItems)
                    {
                        if (li.InvoiceAttachment == null || !li.InvoiceAttachment.Any())
                            continue;

                        foreach (var attachment in li.InvoiceAttachment)
                        {
                            string ext = Path.GetExtension(attachment.FileName)?.ToLower();

                            var allowedExtensions = new List<string> { ".pdf", ".jpg", ".jpeg", ".png", ".jpe", ".jfif", ".gif", ".tiff" };

                            if (!allowedExtensions.Contains(ext))
                            {
                                _logger.LogError($"Invalid file upload attempted: {attachment.FileName}");

                                throw new BadRequestCustomException(
                                    "Invalid file format",
                                    "Only PDF, JPG, JPEG, PNG, JPE, JFIF, GIF, and TIFF files are allowed for invoice attachment"
                                );
                            }
                            attachment.AssetType = Common.INVOICE_TYPE_PR_SERVICE_ATTACHMENT;
                            attachment.EntityType = Common.INVOICE_TYPE_PR_SERVICE_ATTACHMENT;

                            var fileBytes = attachment.FileBytes ?? new byte[0];

                            var assetUpload = new AssetUploadDto
                            {
                                FileBytes = fileBytes,
                                FileName = attachment.FileName,
                                ContentType = attachment.ContentType,
                                AssetType = attachment.AssetType,
                                EntityType = attachment.EntityType
                            };

                            Guid assetId = _assetService.UploadDocument(assetUpload);

                            _repositoryWrapper.ServicePRAttachment.Create(new ServicePRAttachment
                            {
                                Id = Guid.NewGuid(),
                                PRServiceUniqueName = actualPRNumber,
                                AssetId = assetId,
                                LineItemNumber = li.NumberInCollection,
                                InvoiceID = li.InvoiceID,
                                Type = Common.INVOICE_TYPE_PR_SERVICE_ATTACHMENT
                            });
                        }
                    }
                }

                if (newRecords.Any())
                {
                    _repositoryWrapper.ServicePR.CreateRange(newRecords);
                }

                foreach (var pr in groupedPRs)
                {
                    if (pr.Attachments == null || !pr.Attachments.Any())
                        continue;

                    foreach (var attachment in pr.Attachments)
                    {
                        var assetUpload = new AssetUploadDto
                        {
                            FileBytes = attachment.FileBytes,
                            FileName = attachment.FileName,
                            ContentType = attachment.ContentType,
                            AssetType = Common.TYPE_SERVICE_PR_UPLOAD,
                            EntityType = Common.TYPE_SERVICE_PR_UPLOAD
                        };

                        Guid assetId = _assetService.UploadDocument(assetUpload);

                        _repositoryWrapper.ServicePRAttachment.Create(new ServicePRAttachment
                        {
                            Id = Guid.NewGuid(),
                            PRServiceUniqueName = pr.UniqueName,
                            AssetId = assetId,
                            Type = Common.TYPE_SERVICE_PR_UPLOAD
                        });
                    }
                }

                await _repositoryWrapper.SaveAsync();

                if (newRecords.Any())
                {
                    foreach (var servicePR in newRecords)
                    {
                        _emailService.CreateServicePREmailNotification(
                            servicePR,
                            Common.SERVICE_PR_CREATION_EMAIL_KEY,
                            userId
                        );
                    }
                }
                else if (updatedRecords.Any())
                {
                    foreach (var servicePR in updatedRecords)
                    {
                        _emailService.CreateServicePREmailNotification(
                            servicePR,
                            Common.SERVICE_PR_UPDATE_EMAIL_KEY,
                            userId
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Service PR creation/save error: {ex.InnerException?.Message ?? ex.Message}");
                throw;
            }

            return prResults;
        }
        public async Task<string> GenerateServiceReferenceNumber()
        {
            var lastPR = await _repositoryWrapper.ServicePR.GetLastPRUniqueNameAsync();
            int nextNumber = 1;
            if (!string.IsNullOrEmpty(lastPR))
            {
                var match = Regex.Match(lastPR, @"\d+");
                if (match.Success)
                {
                    nextNumber = int.Parse(match.Value) + 1;
                }
            }

            return $"PRSERP{nextNumber:D3}";
        }

    }
}
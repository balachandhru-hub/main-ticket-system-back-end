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
    public class PublicRelationService : IPublicRelationService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerManager _logger;
        private readonly IPublicRelationSoapService _publicRelationSoapService;
        private readonly IBulkInsertHelper _bulkInsertHelper;
        private readonly IAssetService _assetService;
        private readonly IEmailService _emailService;

        public PublicRelationService(
            IRepositoryWrapper repositoryWrapper,
            ILoggerManager logger,
            IPublicRelationSoapService publicRelationSoapService,
            IAssetService assetService,
            IBulkInsertHelper bulkInsertHelper,
            IEmailService emailService, IServiceProvider serviceProvider)
        {
            _repositoryWrapper = repositoryWrapper;
            _publicRelationSoapService = publicRelationSoapService;
            _logger = logger;

            _bulkInsertHelper = bulkInsertHelper;
            _assetService = assetService;
            _emailService = emailService;

        }

        public async Task<List<MaterialPRResultDto>> PostDigitalQuickPRToAriba(
      List<AribaMaterialPRDto> prRequests,
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
            List<MaterialPR> newRecords = new();
            List<MaterialPR> updatedRecords = new();

            try
            {
                var groupedPRs = prRequests
                    .SelectMany(pr => pr.LineItems.Select(li => new { pr, li }))
                    .GroupBy(x => isUpdate ? x.pr.PRNumber : x.pr.UniqueName)
                    .Select(g => new MaterialPRDto
                    {
                        PRNumber = g.First().pr.PRNumber,
                        UniqueName = g.Key,
                        Name = g.First().pr.Name,
                        IsHeader = g.First().pr.IsHeader,
                        NeedBy = g.First().pr.NeedBy,
                        HeaderComment = g.First().pr.HeaderComment,
                        LineItems = g.Select(x => new MaterialPRLineItemDto
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
                            AutoGRN = x.li.AutoGRN,
                            AutoInvoices = x.li.AutoInvoices,
                            TaxInclude = x.li.TaxInclude,
                            VendorName = x.li.VendorName,
                            InvoiceID = x.li.InvoiceID,
                            InvoiceDate = x.li.InvoiceDate,
                            InvoiceComment = x.li.InvoiceComment,
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
                            type = Common.TYPE_MATERIAL_PR_UPLOAD,
                            FileBytes = att.FileBytes
                        }).ToList();
                    }
                }

                foreach (var prRequest in groupedPRs)
                {
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
                                li.ProcurementUnit = li.ProcurementUnit ?? defaultDetails.ProcurementUnit;
                                li.DeliverTo = li.DeliverTo ?? defaultDetails.DeliverTo;
                                li.BillTo = venueDetails.CompanyCode;
                                li.MaterialCode = Common.PUBLIC_RELATION_MATERIAL_CODE ?? li.MaterialCode;
                            }
                        }
                        li.PurchaseGroup = li.PurchaseGroup ?? defaultDetails.PurchaseGroup;
                        if (li.TaxInclude)
                        {
                            li.CommodityCode = Common.PUBLIC_RELATION_COMMODITY_CODE_WITH_TAX;
                        }
                        else
                        {
                            li.CommodityCode = Common.PUBLIC_RELATION_COMMODITY_CODE_WITHOUT_TAX;
                        }
                        li.NumberInCollection = li.NumberInCollection;
                        li.Currency = string.IsNullOrWhiteSpace(li.Currency)
                            ? Common.DEFAULT_CURRENCY
                            : li.Currency;
                        li.NeedBy = prRequest.NeedBy;
                        li.CategoryName = li.CategoryName;
                        DateTime startDate = prRequest.NeedBy;
                        description = string.IsNullOrWhiteSpace(li.Venue)
                            ? $"{li.VendorName} {startDate:yyyy-MM-dd}"
                            : $"{li.CategoryName} {li.Venue} {li.VendorName}{startDate:yyyy-MM-dd}";

                        if (string.IsNullOrWhiteSpace(li.Description))
                        {
                            li.Description = string.IsNullOrWhiteSpace(li.Venue)
                                ? $"{li.VendorName} {startDate:yyyy-MM-dd}"
                                : $"{li.CategoryName} {li.Venue} {li.VendorName} {startDate:yyyy-MM-dd}";
                        }
                        prRequest.Name = headerValue ?? description;

                        li.DeliveryDate = li.DeliveryDate ?? DateTime.Now.AddDays(1);
                        li.InvoiceAttachment = li.InvoiceAttachment;
                        li.AutoGRN = li.AutoGRN;
                        li.AutoInvoices = li.AutoInvoices;
                    }

                    if (isUpdate)
                    {
                        var storedReferenceId = _repositoryWrapper.MaterialPR.GetOriginatingReferenceIdByPRNumber(domainId, prRequest.PRNumber);

                        var aribaConfig = _repositoryWrapper.DomainSetting
                            .FindFirstByCondition(t => t.DomainId == domainId &&
                                                       t.SettingsKey == Common.DOMAIN_SETTINGS_MATERIAL_PR_POSTING)
                            ?? throw new NotFoundCustomException("Domain settings not found", "Material PR posting configuration missing");

                        var config = JsonConvert.DeserializeObject<AribaPurchaseOrderSettings>(aribaConfig.SettingsValue);

                        var aribaReq = new MaterialPRDto
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
                            Operation = Common.OPERATION_MATERIAL_PR_UPDATE,
                            HeaderComment = prRequest.HeaderComment,
                            Requester = profileDto.AribaUserName,
                            RequesterPasswordAdapter = profileDto.AribaPassword,
                            Preparer = profileDto.AribaUserName,
                            LineItems = prRequest.LineItems,
                            Attachments = prRequest.Attachments,
                            SystemReferenceID = storedReferenceId,
                            InvoiceAttachment = prRequest.LineItems
                                .SelectMany(li => li.InvoiceAttachment ?? new List<AssetUploadDto>())
                                .ToList()
                        };

                        var response = await _publicRelationSoapService.PostDigitalQuickPRToAriba(aribaReq);
                        if (response == null || string.IsNullOrWhiteSpace(response.PRNumber))
                        {
                            _logger.LogError("Ariba response missing PRNumber.");

                            throw new PreConditionFailedCustomException(
                                "Ariba PR update failed",
                                "PRNumber is missing in Ariba response"
                            );
                        }

                        actualPRNumber = response?.PRNumber ?? prRequest.PRNumber;
                        List<MaterialPR> dbRecords;
                        bool entitiesAlreadyTracked = false;
                        if (!string.Equals(prRequest.PRNumber, actualPRNumber, StringComparison.OrdinalIgnoreCase))
                        {
                            var oldPRs = _repositoryWrapper.MaterialPR
                                .FindByCondition(x => x.DomainId == domainId && x.PRNumber == prRequest.PRNumber)
                                .ToList();

                            foreach (var pr in oldPRs)
                                pr.PRNumber = actualPRNumber;

                            _repositoryWrapper.MaterialPR.UpdateRange(oldPRs);

                            var invoices = _repositoryWrapper.MaterialInvoice
                                .FindByCondition(x => x.PRNumber == prRequest.PRNumber)
                                .ToList();

                            foreach (var inv in invoices)
                                inv.PRNumber = actualPRNumber;

                            _repositoryWrapper.MaterialInvoice.UpdateRange(invoices);

                            var attachments = _repositoryWrapper.MaterialPRAttachment
                                .FindByCondition(x => x.PRUniqueName == prRequest.PRNumber)
                                .ToList();

                            foreach (var att in attachments)
                                att.PRUniqueName = actualPRNumber;

                            _repositoryWrapper.MaterialPRAttachment.UpdateRange(attachments);

                            // Save changes and clear tracker to avoid conflicts
                            await _repositoryWrapper.SaveAsync();
                            _repositoryWrapper.MaterialInvoice.DetachAllEntities();
                            dbRecords = oldPRs.Where(x => x.IsActive).ToList();
                            entitiesAlreadyTracked = true;
                        }
                        else
                        {
                            // Query using actualPRNumber and without AsNoTracking
                            dbRecords = _repositoryWrapper.MaterialPR
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
                                _repositoryWrapper.MaterialPR.UpdateRange(dbRecords);
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

                            var existingNonHeaderInvs = _repositoryWrapper.MaterialInvoice
                                .FindByCondition(x => x.PRNumber == actualPRNumber && !x.IsHeader)
                                .ToList();
                            if (existingNonHeaderInvs.Any())
                            {
                                _repositoryWrapper.MaterialInvoice.DeleteRange(existingNonHeaderInvs);
                            }

                            if (hasInvoiceData)
                            {
                                var existingHeaderInv = _repositoryWrapper.MaterialInvoice
                                    .FindByCondition(x => x.PRNumber == actualPRNumber && x.IsHeader)
                                    .FirstOrDefault();

                                if (existingHeaderInv != null)
                                {
                                    existingHeaderInv.InvoiceID = mergedInvoiceID;
                                    existingHeaderInv.InvoiceDate = mergedInvoiceDate;
                                    existingHeaderInv.InvoiceComment = mergedInvoiceComment;
                                    _repositoryWrapper.MaterialInvoice.Update(existingHeaderInv);
                                }
                                else
                                {
                                    _repositoryWrapper.MaterialInvoice.Create(new Entities.Models.MaterialInvoice
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

                                newRecords.Add(new MaterialPR
                                {
                                    DomainId = domainId,
                                    PRNumber = actualPRNumber,
                                    PRUniqueName = aribaReq.SystemReferenceID,
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
                                    Type = Common.MATERIAL_TYPE,
                                    AccountType = Common.ACCOUNT_TYPE,
                                    GLAccount = li?.GLAccount,
                                    CostCenter = li?.CostCenter,
                                    MaterialCode = li?.MaterialCode,
                                    AutoGRN = li?.AutoGRN ?? false,
                                    AutoInvoices = li?.AutoInvoices ?? false,
                                    Currency = li?.Currency,
                                    PurchaseOrg = li?.PurchaseOrg,
                                    PurchaseGroup = li?.PurchaseGroup,
                                    CompanyCode = li.CompanyCode ?? venueDetailsForLine?.CompanyCode ?? defaultDetails.CompanyCode,
                                    ShipToPlant = li.ShipToPlant ?? venueDetailsForLine?.ShipToPlant ?? defaultDetails.ShipToPlant,
                                    DeliverTo = li?.DeliverTo ?? defaultDetails.DeliverTo,
                                    RequesterName = aribaReq.Requester,
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
                                    ParentLineItemNumber = li?.ParentLineItemNumber
                                });
                            }

                            prResults.Add(new MaterialPRResultDto
                            {
                                PRNumber = actualPRNumber,
                                Status = response?.Status
                            });

                            continue;
                        }
                        var invoicesToUpdate = new List<MaterialInvoice>();
                        var invoicesToCreate = new List<MaterialInvoice>();

                        foreach (var li in prRequest.LineItems)
                        {
                            var hasLineInvoice =
                                !string.IsNullOrWhiteSpace(li.InvoiceID) ||
                                li.InvoiceDate.HasValue ||
                                !string.IsNullOrWhiteSpace(li.InvoiceComment);
                            if (!hasLineInvoice)
                                continue;

                            var lineNumber = li.NumberInCollection ?? li.LineItemNumber ?? 1;

                            var existingInv = _repositoryWrapper.MaterialInvoice
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

                                invoicesToCreate.Add(new MaterialInvoice
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
                            _repositoryWrapper.MaterialInvoice.UpdateRange(invoicesToUpdate);
                        }

                        if (invoicesToCreate.Any())
                        {
                            _repositoryWrapper.MaterialInvoice.CreateRange(invoicesToCreate);
                        }

                        int dbCount = dbRecords.Count;
                        int reqCount = prRequest.LineItems.Count;

                        if (dbCount != reqCount)
                        {
                            _repositoryWrapper.MaterialPR.DeleteRange(dbRecords);

                            foreach (var li in prRequest.LineItems)
                            {
                                var liHasInvoice = !string.IsNullOrWhiteSpace(li.InvoiceID)
                                                   || li.InvoiceDate.HasValue
                                                   || !string.IsNullOrWhiteSpace(li.InvoiceComment);

                                newRecords.Add(new MaterialPR
                                {
                                    DomainId = domainId,
                                    PRNumber = actualPRNumber,
                                    PRUniqueName = aribaReq.SystemReferenceID,
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
                                    Type = Common.MATERIAL_TYPE,
                                    AccountType = Common.ACCOUNT_TYPE,
                                    GLAccount = li.GLAccount,
                                    CostCenter = li.CostCenter,
                                    MaterialCode = li.MaterialCode,
                                    AutoGRN = li.AutoGRN,
                                    AutoInvoices = li.AutoInvoices,
                                    Currency = li.Currency,
                                    PurchaseOrg = li.PurchaseOrg,
                                    PurchaseGroup = li.PurchaseGroup,
                                    CompanyCode = li.CompanyCode,
                                    ShipToPlant = li.ShipToPlant,
                                    DeliverTo = li.DeliverTo ?? defaultDetails.DeliverTo,
                                    RequesterName = aribaReq.Requester,
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
                                existing.AutoGRN = li.AutoGRN;
                                existing.AutoInvoices = li.AutoInvoices;
                                existing.ParentLineItemNumber = li.ParentLineItemNumber;
                                existing.Category = li.CategoryName ?? existing.Category;


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
                                _repositoryWrapper.MaterialPR.UpdateRange(dbRecords);
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
                        var generatedPRNumber = await GenerateMaterialReferenceNumber();

                        prRequest.UniqueName = headerValue ?? description;
                        var aribaReq = new MaterialPRDto
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
                            Requester = profileDto.AribaUserName,
                            RequesterPasswordAdapter = profileDto.AribaPassword,
                            Preparer = profileDto.AribaUserName,
                            LineItems = prRequest.LineItems,
                            Attachments = prRequest.Attachments,
                            SystemReferenceID = generatedPRNumber,
                            InvoiceAttachment = prRequest.LineItems.SelectMany(li => li.InvoiceAttachment ?? new List<AssetUploadDto>()).ToList()
                        };

                        var response = await _publicRelationSoapService.PostDigitalQuickPRToAriba(aribaReq);

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
                                _repositoryWrapper.MaterialInvoice.Create(new Entities.Models.MaterialInvoice
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
                            var createInvoices = new List<MaterialInvoice>();
                            var updateInvoices = new List<MaterialInvoice>();
                            foreach (var li in prRequest.LineItems)
                            {
                                var hasLineInvoice = !string.IsNullOrWhiteSpace(li.InvoiceID)
                                                     || li.InvoiceDate.HasValue
                                                     || !string.IsNullOrWhiteSpace(li.InvoiceComment);
                                if (!hasLineInvoice) continue;

                                var lineNumber = li.NumberInCollection ?? li.LineItemNumber ?? 1;

                                var existingInv = _repositoryWrapper.MaterialInvoice
                                    .FindByCondition(x => x.PRNumber == actualPRNumber
                                                         && x.LineItemNumber == lineNumber
                                                         && !x.IsHeader).AsNoTracking()
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
                                    createInvoices.Add(new Entities.Models.MaterialInvoice
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
                                _repositoryWrapper.MaterialInvoice.UpdateRange(updateInvoices);
                            }
                            if (createInvoices.Any())
                            {
                                _repositoryWrapper.MaterialInvoice.CreateRange(createInvoices);
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

                            newRecords.Add(new MaterialPR
                            {
                                DomainId = domainId,
                                PRNumber = actualPRNumber,
                                PRUniqueName = aribaReq.SystemReferenceID,
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
                                Type = Common.MATERIAL_TYPE,
                                AccountType = Common.ACCOUNT_TYPE,
                                GLAccount = li.GLAccount,
                                CostCenter = li.CostCenter,
                                MaterialCode = li.MaterialCode,
                                AutoGRN = li.AutoGRN,
                                AutoInvoices = li.AutoInvoices,
                                Currency = li.Currency,
                                PurchaseOrg = li.PurchaseOrg,
                                PurchaseGroup = li.PurchaseGroup,
                                CompanyCode = li.CompanyCode,
                                ShipToPlant = li.ShipToPlant,
                                DeliverTo = li.DeliverTo ?? defaultDetails.DeliverTo,
                                RequesterName = aribaReq.Requester,
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
                                Category = li.CategoryName
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
                            attachment.AssetType = Common.INVOICE_TYPE_PRATTACHMENT;
                            attachment.EntityType = Common.INVOICE_TYPE_PRATTACHMENT;

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

                            _repositoryWrapper.MaterialPRAttachment.Create(new MaterialPRAttachment
                            {
                                Id = Guid.NewGuid(),
                                PRUniqueName = actualPRNumber,
                                AssetId = assetId,
                                LineItemNumber = li.NumberInCollection,
                                InvoiceID = li.InvoiceID,
                                Type = Common.INVOICE_TYPE_PRATTACHMENT
                            });
                        }
                    }
                }

                if (newRecords.Any())
                {
                    _repositoryWrapper.MaterialPR.CreateRange(newRecords);
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
                            AssetType = Common.TYPE_MATERIAL_PR_UPLOAD,
                            EntityType = Common.TYPE_MATERIAL_PR_UPLOAD
                        };

                        Guid assetId = _assetService.UploadDocument(assetUpload);

                        _repositoryWrapper.MaterialPRAttachment.Create(new MaterialPRAttachment
                        {
                            Id = Guid.NewGuid(),
                            PRUniqueName = pr.UniqueName,
                            AssetId = assetId,
                            Type = Common.TYPE_MATERIAL_PR_UPLOAD
                        });
                    }
                }

                await _repositoryWrapper.SaveAsync();

                if (newRecords.Any())
                {
                    foreach (var materialPR in newRecords)
                    {
                        _emailService.CreateMaterialPREmailNotification(
                            materialPR,
                            Common.MATERIAL_PR_CREATION_EMAIL_KEY,
                            userId
                        );
                    }
                }
                else if (updatedRecords.Any())
                {
                    foreach (var materialPR in updatedRecords)
                    {
                        _emailService.CreateMaterialPREmailNotification(
                            materialPR,
                            Common.MATERIAL_PR_UPDATE_EMAIL_KEY,
                            userId
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Material PR creation/save error: {ex.InnerException?.Message ?? ex.Message}");
                throw;
            }

            return prResults;
        }

        public async Task<string> GenerateMaterialReferenceNumber()
        {
            var lastPR = await _repositoryWrapper.MaterialPR.GetLastPRUniqueNameAsync();
            int nextNumber = 1;
            if (!string.IsNullOrEmpty(lastPR))
            {
                var match = Regex.Match(lastPR, @"\d+");
                if (match.Success)
                {
                    nextNumber = int.Parse(match.Value) + 1;
                }
            }

            return $"PRMERP{nextNumber:D3}";
        }

        // Other service methods
        //Get Property Cross Charge Details by Name
        public async Task<List<PropertyCrossChargeDto>> GetPropertyCrossChargeForDropdownAsync(Guid domainId, string? companyCode, string? searchTerm, int index, int limit)
        {
            var query = _repositoryWrapper.PropertyCrossCharge
                .FindByCondition(s => s.IsActive)
                .Where(s =>
                    s.DomainId == domainId &&
                    (string.IsNullOrEmpty(companyCode) || s.CompanyCode == companyCode) &&
                    (string.IsNullOrEmpty(searchTerm) ||
                     s.PropertyCrossChargeName.Contains(searchTerm)));

            int total = await query.CountAsync();

            var suppliers = await query
                .OrderBy(s => s.PropertyCrossChargeName)
                .Skip(index)
                .Take(limit)

                .Select(s => new PropertyCrossChargeDto
                {

                    PropertyCrossChargeName = s.PropertyCrossChargeName,
                    CostCenter = s.CostCenter,

                })
                .ToListAsync();
            if (!suppliers.Any())
            {
                _logger.LogError("No Property Cross Charge data found");
                throw new NotFoundCustomException("No Data", "No Property Cross Charge data found");
            }

            return suppliers;
        }

        public async Task<PropertyCrossChargeDetailsDto?> GetCrossChargeDetailsByNameAsync(
            Guid domainId,
            string venueName, string? companyCode)
        {
            _logger.LogInfo($"Fetching venue details for Venue: {venueName} in DomainId: {domainId}");
            return await _repositoryWrapper.PropertyCrossCharge.GetDetailsByNameAsync(domainId, venueName, companyCode);

        }
        public async Task<VenueDetailsDto?> GetPublicVenueDetailsByNameAsync(
    Guid domainId,
    string venueName)
        {
            _logger.LogInfo($"Fetching venue details for Venue: {venueName} in DomainId: {domainId}");
            return await _repositoryWrapper.PublicProperty.GetPublicVenueDetailsByNameAsync(domainId, venueName);

        }

        public async Task<BatchProcessingResultDto> CreatePublicVenueBatchAsync(List<VenueExcelDto> dtos)
        {
            _logger.LogInfo($"Batch creating {dtos.Count} Contract Data records");

            BatchProcessingResultDto result = new BatchProcessingResultDto();
            List<PublicProperty> newCompanyDataList = new List<PublicProperty>();
            List<PublicProperty> updateCompanyDataList = new List<PublicProperty>();

            // Track processed contracts to avoid duplicates
            HashSet<string> processedCompanyKeys = new HashSet<string>();

            try
            {
                List<string> venueName = dtos.Select(k => k.VenueName).Distinct().ToList();

                Dictionary<string, PublicProperty> existingCompanys = await _repositoryWrapper.PublicProperty.FindByCondition(x => x.IsActive && venueName.Contains(x.VenueName!))
                        .ToDictionaryAsync(x => x.VenueName!);

                foreach (var dto in dtos)
                {
                    try
                    {
                        string companyKey = dto.VenueName;

                        if (!processedCompanyKeys.Contains(companyKey))
                        {
                            processedCompanyKeys.Add(companyKey);

                            if (existingCompanys.TryGetValue(companyKey, out PublicProperty existingCompany) && existingCompany.DomainId == dto.DomainId)
                            {
                                existingCompany.VenueName = dto.VenueName;
                                updateCompanyDataList.Add(existingCompany);
                            }
                            else
                            {
                                PublicProperty newCompany = new PublicProperty
                                {
                                    Id = Guid.NewGuid(),
                                    DomainId = dto.DomainId,
                                    VenueName = dto.VenueName,
                                    CompanyCode = dto.CompanyCode,
                                    CompanyName = dto.CompanyName,
                                    CostCenter = dto.CostCenter,
                                    GLAccount = dto.GLAccount,
                                    ShipToPlant = dto.ShipToPlant,
                                    PurchaseOrg = dto.PurchaseOrg,
                                    PurchaseGroup = dto.PurchaseGroup,
                                    BillTo = dto.BillTo
                                };

                                newCompanyDataList.Add(newCompany);
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
                            Message = "Error processing row",
                            Description = $"Error processing row {dto.RowNumber}: {ex.Message}"
                        });
                        _logger.LogError($"Error processing row {dto.RowNumber}: {ex.Message}");
                    }
                }

                // Step 4: Bulk insert all new orders in one transaction
                if (newCompanyDataList.Any())
                {
                    _logger.LogInfo($"Bulk inserting {newCompanyDataList.Count} new contracts");
                    await _repositoryWrapper.PublicProperty.CreateRangeAsync(newCompanyDataList);
                }

                if (updateCompanyDataList.Any())
                {
                    _logger.LogInfo($"Bulk updating {updateCompanyDataList.Count} contracts");
                    _repositoryWrapper.PublicProperty.UpdateRange(updateCompanyDataList);
                }

                await _repositoryWrapper.SaveAsync();

                _logger.LogInfo($"Batch processing completed. Success: {result.SuccessfulCount}, Failed: {result.FailedCount}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Critical error in batch processing: {ex.Message}");
                throw;
            }
        }
        public async Task<List<VenueListDto>> GetVenueForDropdownAsync(Guid domainId, string searchTerm, int index, int limit)
        {
            var query = _repositoryWrapper.PublicProperty
                .FindByCondition(s => s.IsActive)
                .Where(s =>
                    s.DomainId == domainId &&
                    (string.IsNullOrEmpty(searchTerm) ||
                     s.VenueName.Contains(searchTerm)));

            int total = await query.CountAsync();

            var suppliers = await query
                .OrderBy(s => s.VenueName)
                .Skip(index)
                .Take(limit)

                .Select(s => new VenueListDto
                {

                    VenueName = s.VenueName
                })
                .ToListAsync();

            return suppliers;
        }
        public async Task<BatchProcessingResultDto> CreatePublicCategoryBatchAsync(List<CategoryExcelDto> dtos)
        {
            _logger.LogInfo($"Batch creating {dtos.Count} Contract Data records");

            BatchProcessingResultDto result = new BatchProcessingResultDto();
            List<PublicRelationCategory> newCompanyDataList = new List<PublicRelationCategory>();
            List<PublicRelationCategory> updateCompanyDataList = new List<PublicRelationCategory>();

            // Track processed contracts to avoid duplicates
            HashSet<string> processedCompanyKeys = new HashSet<string>();

            try
            {
                List<string> venueName = dtos.Select(k => k.CategoryName).Distinct().ToList();

                Dictionary<string, PublicRelationCategory> existingCompanys = await _repositoryWrapper.PublicRelationCategory.FindByCondition(x => x.IsActive && venueName.Contains(x.Category!))
                        .ToDictionaryAsync(x => x.Category!);

                foreach (var dto in dtos)
                {
                    try
                    {
                        string companyKey = dto.CategoryName;

                        if (!processedCompanyKeys.Contains(companyKey))
                        {
                            processedCompanyKeys.Add(companyKey);

                            if (existingCompanys.TryGetValue(companyKey, out PublicRelationCategory existingCompany) && existingCompany.DomainId == dto.DomainId)
                            {
                                existingCompany.Category = dto.CategoryName;
                                updateCompanyDataList.Add(existingCompany);
                            }
                            else
                            {
                                PublicRelationCategory newCompany = new PublicRelationCategory
                                {
                                    Id = Guid.NewGuid(),
                                    DomainId = dto.DomainId,
                                    Category = dto.CategoryName,
                                };

                                newCompanyDataList.Add(newCompany);
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
                            Message = "Error processing row",
                            Description = $"Error processing row {dto.RowNumber}: {ex.Message}"
                        });
                        _logger.LogError($"Error processing row {dto.RowNumber}: {ex.Message}");
                    }
                }

                // Step 4: Bulk insert all new orders in one transaction
                if (newCompanyDataList.Any())
                {
                    _logger.LogInfo($"Bulk inserting {newCompanyDataList.Count} new contracts");
                    await _repositoryWrapper.PublicRelationCategory.CreateRangeAsync(newCompanyDataList);
                }

                if (updateCompanyDataList.Any())
                {
                    _logger.LogInfo($"Bulk updating {updateCompanyDataList.Count} contracts");
                    _repositoryWrapper.PublicRelationCategory.UpdateRange(updateCompanyDataList);
                }

                await _repositoryWrapper.SaveAsync();

                _logger.LogInfo($"Batch processing completed. Success: {result.SuccessfulCount}, Failed: {result.FailedCount}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Critical error in batch processing: {ex.Message}");
                throw;
            }
        }
        public async Task<List<PublicCategoryListDto>> GetPublicCategoryForDropdownAsync(Guid domainId, string searchTerm, int index, int limit)
        {
            var query = _repositoryWrapper.PublicRelationCategory
                .FindByCondition(s => s.IsActive)
                .Where(s =>
                    s.DomainId == domainId &&
                    (string.IsNullOrEmpty(searchTerm) ||
                     s.Category.Contains(searchTerm)));

            int total = await query.CountAsync();

            var suppliers = await query
                .OrderBy(s => s.Category)
                .Skip(index)
                .Take(limit)

                .Select(s => new PublicCategoryListDto
                {

                    CategoryName = s.Category
                })
                .ToListAsync();

            return suppliers;
        }


    }
}

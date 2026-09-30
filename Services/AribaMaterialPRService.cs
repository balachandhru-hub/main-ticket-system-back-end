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
    public class AribaMaterialPRService : IAribaMaterialPRService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerManager _logger;
        private readonly IMaterialPRService _materialPRService;
        private readonly IBulkInsertHelper _bulkInsertHelper;
        private readonly IAssetService _assetService;
        private readonly IEmailService _emailService;

        public AribaMaterialPRService(
            IRepositoryWrapper repositoryWrapper,
            ILoggerManager logger,
           IMaterialPRService materialPRService,
           IAssetService assetService,
            IBulkInsertHelper bulkInsertHelper,
            IEmailService emailService, IServiceProvider serviceProvider)
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
            _materialPRService = materialPRService;
            _bulkInsertHelper = bulkInsertHelper;
            _assetService = assetService;
            _emailService = emailService;

        }

        public async Task<List<MaterialPRResultDto>> PostMaterialPRToAriba(
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
                        TeamReceived = g.First().pr.TeamReceived,
                        Type = g.First().pr.Type,
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

                    foreach (var li in prRequest.LineItems)
                    {
                        if (!isUpdate)
                        {
                            var venueDetails = await _repositoryWrapper.AribaVenueListMapping
                                .GetVenueDetailsByNameAsync(domainId, li.Venue);
                            li.Venue = li.Venue;
                            li.PurchaseGroup = li.PurchaseGroup ?? venueDetails.PurchaseGroup;
                            li.CompanyCode = li.CompanyCode ?? venueDetails.CompanyCode;
                            li.PurchaseOrg = li.PurchaseOrg ?? venueDetails.PurchaseOrg;
                            li.CostCenter = venueDetails.CostCenter;
                            li.GLAccount = li.GLAccount ?? venueDetails.GLAccount;
                            li.ShipToPlant = li.ShipToPlant ?? venueDetails.ShipToPlant;
                            li.ProcurementUnit = li.ProcurementUnit ?? defaultDetails.ProcurementUnit;
                            li.UOM = li.UOM;
                            li.DeliverTo = li.DeliverTo ?? defaultDetails.DeliverTo;
                            li.BillTo = venueDetails.CompanyCode;
                            li.MaterialCode = Common.PR_ENTERTAINMENT_MATERIAL_CODE ?? li.MaterialCode;

                            var profileEmail = profileDto.Email;

                            // Container (list) of restricted emails from Common
                            var restrictedEmails = new List<string>
                            {
                                Common.PR_ENTERTAINMENT_USER_EMAIL_ONE_KEY,
                                Common.PR_ENTERTAINMENT_USER_EMAIL_TWO_KEY
                            };

                            // If profile email is NOT in the restricted list → take AribaUserId
                            if (!restrictedEmails.Any(e =>
                                    string.Equals(profileEmail, e, StringComparison.OrdinalIgnoreCase)))
                            {
                                li.AribaUserId = li.AribaUserId ?? venueDetails.AribaUserId;
                            }
                        }

                        li.PurchaseGroup = li.PurchaseGroup ?? defaultDetails.PurchaseGroup;
                        if (li.TaxInclude)
                        {
                            li.CommodityCode = Common.COMMODITY_CODES_WITH_TAX;
                        }
                        else
                        {
                            li.CommodityCode = Common.COMMODITY_CODES_WITHOUT_TAX;
                        }
                        li.NumberInCollection = li.NumberInCollection;
                        li.Currency = string.IsNullOrWhiteSpace(li.Currency)
                            ? Common.DEFAULT_CURRENCY
                            : li.Currency;
                        li.NeedBy = prRequest.NeedBy;
                        DateTime startDate = prRequest.NeedBy;

                        description = li.Description;
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
                            TeamReceived = prRequest.TeamReceived,
                            Type=Common.PR_ENTERTAINMENT_DEPARTMENT,
                            InvoiceAttachment = prRequest.LineItems
                                .SelectMany(li => li.InvoiceAttachment ?? new List<AssetUploadDto>())
                                .ToList()
                        };

                        var response = await _materialPRService.PostMaterialPRToAriba(aribaReq);

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
                                    Description = li?.Description,
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
                                    DeliverTo = li.DeliverTo ?? defaultDetails.DeliverTo,
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
                                    AribaUserId = li?.AribaUserId ?? venueDetailsForLine?.AribaUserId,
                                    TeamReceived = prRequest.TeamReceived
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
                                    InvoiceComment = (prRequest.IsHeader || liHasInvoice) ? null : li.InvoiceComment,
                                    AribaUserId = li.AribaUserId,
                                    TeamReceived = prRequest.TeamReceived
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
                                    li.CommodityCode = Common.COMMODITY_CODES_WITH_TAX;
                                }
                                else
                                {
                                    li.CommodityCode = Common.COMMODITY_CODES_WITHOUT_TAX;
                                }

                                existing.Description = li?.Description ?? existing.Description;
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
                                existing.AribaUserId = li.AribaUserId ?? existing.AribaUserId;
                                existing.TeamReceived = prRequest.TeamReceived ?? existing.TeamReceived;

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
                            UniqueName = prRequest.Name,
                            NeedBy = prRequest.NeedBy,
                            Operation = Common.OPERATION_MATERIAL_PR_CREATION,
                            HeaderComment = prRequest.HeaderComment,
                            Requester = profileDto.AribaUserName,
                            RequesterPasswordAdapter = profileDto.AribaPassword,
                            Preparer = profileDto.AribaUserName,
                            LineItems = prRequest.LineItems,
                            Attachments = prRequest.Attachments,
                            SystemReferenceID = generatedPRNumber,
                            TeamReceived = prRequest.TeamReceived,
                            Type=Common.PR_ENTERTAINMENT_DEPARTMENT,
                            InvoiceAttachment = prRequest.LineItems.SelectMany(li => li.InvoiceAttachment ?? new List<AssetUploadDto>()).ToList()
                        };

                        var response = await _materialPRService.PostMaterialPRToAriba(aribaReq);

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
                                AribaUserId = li.AribaUserId,
                                TeamReceived = prRequest.TeamReceived
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
                            PRUniqueName = pr.Name,
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



        public async Task UpdateMaterialPR(string prNumber, MaterialPRUpdateDto dto, Guid domainId)
        {
            _logger.LogInfo($"Fetching PR details for PR Number {prNumber}");

            var prItems = _repositoryWrapper.MaterialPR
                            .FindByCondition(x => x.PRNumber == prNumber && x.DomainId == domainId && x.IsActive)
                            .ToList();

            if (!prItems.Any())
            {
                _logger.LogError($"PR Number {prNumber} not found");
                throw new NotFoundCustomException("PR not found", $"No PR found with PR Number {prNumber}");
            }

            _logger.LogInfo($"Updating {prItems.Count} line items for PR Number {prNumber}");

            foreach (var item in prItems)
            {
                item.PurchaseOrg = dto.PurchaseOrg ?? item.PurchaseOrg;
                item.Vendor = dto.Vendor ?? item.Vendor;
                item.Description = dto.Description ?? item.Description;
                item.Title = dto.Description ?? item.Title;
                item.CommodityCode = dto.Commodity ?? item.CommodityCode;
                item.Contract = dto.Contract ?? item.Contract;
                item.PurchaseGroup = dto.PurchaseGroup ?? item.PurchaseGroup;
                item.MaterialCode = dto.MaterialCode ?? item.MaterialCode;

                if (dto.Quantity.HasValue)
                    item.Quantity = dto.Quantity.Value;

                if (dto.Amount.HasValue)
                    item.Amount = dto.Amount.Value;

                item.UOM = dto.UOM ?? item.UOM;
                if (dto.DeliveryDate.HasValue)
                    item.DeliveryDate = dto.DeliveryDate.Value;

                item.DeliverTo = dto.DeliverTo ?? item.DeliverTo;
                item.GLAccount = dto.GLAccount ?? item.GLAccount;
                item.CostCenter = dto.CostCenter ?? item.CostCenter;
                if (item.Status != null &&
                item.Status.Equals(Common.MATERIAL_PR_STATUS_COMPOSING, StringComparison.OrdinalIgnoreCase))
                {
                    item.Status = Common.MATERIAL_PR_STATUS_PENDING;

                }

            }

            if (prItems.Any())
            {
                _repositoryWrapper.MaterialPR.UpdateRange(prItems);
                await _repositoryWrapper.SaveAsync();
            }

            _logger.LogInfo($"Updated PR Number {prNumber} successfully");
        }
        // summary
        // Get all Material PRs by Domain Id
        public async Task<List<GetMaterialPRDto>> GetMaterialPRs(Guid userId, Guid domainId, MaterialPRFilterRequestDto filter)
        {
            _logger.LogInfo($"Filtering Material PR records for UserId: {userId} in DomainId: {domainId}");


            var profileDto = _repositoryWrapper.Person.GetMyProfileByUserId(userId);
            string? aribaUsername = profileDto?.AribaUserName;
            string? role = profileDto?.Role;


            var query = _repositoryWrapper.MaterialPR
                         .FindByCondition(t => t.DomainId == domainId);


            if (role != Common.BUSINESS_ADMIN_USER_KEY)
            {
                if (string.IsNullOrEmpty(aribaUsername))
                {
                    _logger.LogError($"No profile found for user: {userId}");
                    throw new NotFoundCustomException("Unable to find user profile", "User profile not found");
                }

                query = query.Where(t => t.DeliverTo == aribaUsername);
            }

            bool noFilter = (filter.GLAccount == null || !filter.GLAccount.Any()) &&
                            (filter.CostCenter == null || !filter.CostCenter.Any()) &&
                            (filter.MaterialCode == null || !filter.MaterialCode.Any()) &&
                            string.IsNullOrWhiteSpace(filter.Status) &&
                            !filter.FromDate.HasValue &&
                            !filter.ToDate.HasValue &&
                            string.IsNullOrWhiteSpace(filter.SearchTerm);

            if (!noFilter)
            {
                if (filter.GLAccount != null && filter.GLAccount.Any())
                    query = query.Where(t => filter.GLAccount.Contains(t.GLAccount));

                if (filter.CostCenter != null && filter.CostCenter.Any())
                    query = query.Where(t => filter.CostCenter.Contains(t.CostCenter));

                if (filter.MaterialCode != null && filter.MaterialCode.Any())
                    query = query.Where(t => filter.MaterialCode.Contains(t.MaterialCode));

                if (!string.IsNullOrWhiteSpace(filter.Status))
                    query = query.Where(t => t.Status == filter.Status);

                if (filter.FromDate.HasValue)
                    query = query.Where(t => t.DateCreated >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                    query = query.Where(t => t.DateCreated <= filter.ToDate.Value);

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    string s = filter.SearchTerm.ToLower();
                    query = query.Where(t =>
                        (t.Vendor != null && t.Vendor.ToLower().Contains(s)) ||
                        (t.PRNumber != null && t.PRNumber.ToLower().Contains(s)) ||
                        (t.MaterialCode != null && t.MaterialCode.ToLower().Contains(s))
                    );
                }
            }


            var result = await query
                .OrderByDescending(t => t.DateUpdated)
                .Skip(filter.Index)
                .Take(filter.Limit)
                .ToListAsync();


            var dtoList = result.Select(t => new GetMaterialPRDto
            {
                PRNumber = t.PRNumber,
                PurchaseOrg = t.PurchaseOrg,
                Description = t.Description,
                CommodityCode = t.CommodityCode,
                AccountType = t.AccountType,
                Type = t.Type,
                Amount = t.Amount,
                DeliveryDate = t.DeliveryDate,
                Quantity = t.Quantity,
                UOM = t.UOM,
                Vendor = t.Vendor,
                Contract = t.Contract,
                MaterialCode = t.MaterialCode,
                Title = t.Title,
                Currency = t.Currency,
                RequesterName = t.RequesterName,
                GLAccount = t.GLAccount,
                CostCenter = t.CostCenter,
                ShipToPlant = t.ShipToPlant,
                CompanyCode = t.CompanyCode,
                PurchaseGroup = t.PurchaseGroup,
                BillTo = t.BillTo,
                TaxCode = t.TaxCode,
                DeliverTo = t.DeliverTo,
                Status = t.Status,
                NeedBy = t.NeedBy,
                HeaderComment = t.HeaderComment,
                LineItemNumber = t.LineItemNumber,
                PRUniqueName = t.PRUniqueName,
                Counter = t.Counter,
                AutoGRN = t.AutoGRN,
                AutoInvoices = t.AutoInvoices,
                Venue = t.Venue,
                ReciptID = t.ReciptID,
                InvoiceID = t.InvoiceID,
                PONumber = t.PONumber,
                InvoiceDate = t.InvoiceDate,
                TaxInclude = t.TaxInclude,
                VendorName = t.VendorName
            }).ToList();

            return dtoList;
        }
        public Task<PagedResult<string>> GetMaterialCodesForDropdown(string? searchTerm, int page, int pageSize)
        {
            _logger.LogInfo($"Service call: Material dropdown search: {searchTerm}");
            return _repositoryWrapper.MaterialPR.GetMaterialCodesForDropdownAsync(searchTerm, page, pageSize);
        }
        public async Task<MaterialDetailsDto?> GetMaterialDetailsAsync(string materialCode)
        {
            if (string.IsNullOrWhiteSpace(materialCode))
                return null;

            return await _repositoryWrapper.MaterialPR.GetMaterialDetailsByMaterialCodeAsync(materialCode);
        }

        public async Task<MaterialDetailsDto?> GetCommodityDetailsAsync(string CommodityCode)
        {
            if (string.IsNullOrWhiteSpace(CommodityCode))
                return null;

            return await _repositoryWrapper.MaterialPR.GetMaterialDetailsByCommodityCodeAsync(CommodityCode);
        }

        public async Task<PRSmartDefaultsDto> GetPRSmartDefaultsAsync(string aribaUserName)
        {
            _logger.LogInfo($"[MaterialPRService] Fetching smart defaults for {aribaUserName}");

            var defaultDetails =
                _repositoryWrapper.MaterialPR.GetPRDetails(aribaUserName);

            if (defaultDetails == null)
                return null;

            return new PRSmartDefaultsDto
            {
                CompanyCode = defaultDetails.CompanyCode,
                ShipToPlant = defaultDetails.ShipToPlant,
                BillTo = _repositoryWrapper.AribaCompanyMapping.FindByCondition(x => x.CompanyCode == defaultDetails.CompanyCode).Select(x => x.CompanyName).FirstOrDefault() ?? defaultDetails.CompanyCode,
                PurchaseGroup = defaultDetails.PurchaseGroup,
                Currency = defaultDetails.Currency,
                PurchaseOrg = defaultDetails.PurchaseOrg,
                DeliverTo = defaultDetails.DeliverTo,
                CostCenter = defaultDetails.CostCenter
            };
        }
        public async Task<List<string>> GetPRNumbersAsync(Guid domainId, int index, int limit)
        {
            _logger.LogInfo("[MaterialPRService] Fetching PR Numbers");



            var prNumbers = await _repositoryWrapper.MaterialPR
                .FindByCondition(x => x.DomainId == domainId && x.PRNumber != null)
                .Select(x => x.PRNumber)
                .Distinct()
                .OrderBy(x => x)
                .Skip(index)
                .Take(limit)
                .ToListAsync();

            if (!prNumbers.Any())
            {
                _logger.LogError("[MaterialPRService] No PR numbers found");
                throw new NotFoundCustomException(
                    "PR Numbers Not Found",
                    "No Purchase Requests available for this domain"
                );
            }

            return prNumbers;
        }

        public async Task<List<SupplierListDto>> GetSuppliersForDropdownAsync(
      Guid domainId,
      string searchTerm,
      int index,
      int limit)
        {
            var suppliers = await _repositoryWrapper.AribaSupplierName
                .FindByCondition(s => s.IsActive && s.DomainId == domainId)
                .Where(s => string.IsNullOrEmpty(searchTerm) ||
                            s.SupplierName.Contains(searchTerm))
                .OrderBy(s => s.SupplierName)
                .Skip(index)
                .Take(limit)
                .Select(s => new SupplierListDto
                {
                    SupplierId = s.SupplierId,
                    SupplierName = s.SupplierName
                })
                .ToListAsync();

            return suppliers;
        }


        public async Task<List<VenueListDto>> GetVenueForDropdownAsync(Guid domainId, string searchTerm, int index, int limit)
        {
            var query = _repositoryWrapper.AribaVenueListMapping
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
        public async Task<VenueDetailsDto?> GetVenueDetailsByNameAsync(
            Guid domainId,
            string venueName)
        {
            _logger.LogInfo($"Fetching venue details for Venue: {venueName} in DomainId: {domainId}");
            return await _repositoryWrapper.AribaVenueListMapping.GetVenueDetailsByNameAsync(domainId, venueName);

        }
        public async Task<List<MaterialPRResponseDto>> GetMaterialPRsGroupedAsync(
        Guid userId,
        Guid domainId,
        MaterialPRFilterRequestDto filter)
        {
            _logger.LogInfo($"Fetching Material PRs grouped by PRNumber for UserId: {userId}, DomainId: {domainId}");

            var profile = _repositoryWrapper.Person.GetMyProfileByUserId(userId);

            if (profile == null)
            {
                _logger.LogError($"User profile not found for UserId: {userId}");
                throw new NotFoundCustomException(
                    "User profile not found",
                    $"UserId: {userId}"
                );
            }


            string? aribaUsername = profile.AribaUserName;
            string? role = profile.Role;

            IQueryable<MaterialPR> query =
                _repositoryWrapper.MaterialPR.FindByCondition(x => x.DomainId == domainId);

            if (role != Common.BUSINESS_ADMIN_USER_KEY)
            {
                if (string.IsNullOrEmpty(aribaUsername))
                    throw new NotFoundCustomException("Ariba username missing", $"UserId: {userId}");

                query = query.Where(x => x.DeliverTo == aribaUsername);
            }

            // ---------------- FILTERS ----------------
            if (filter != null)
            {
                if (filter.GLAccount?.Any() == true)
                    query = query.Where(x => filter.GLAccount.Contains(x.GLAccount));

                if (filter.CostCenter?.Any() == true)
                    query = query.Where(x => filter.CostCenter.Contains(x.CostCenter));

                if (filter.MaterialCode?.Any() == true)
                    query = query.Where(x => filter.MaterialCode.Contains(x.MaterialCode));

                if (!string.IsNullOrWhiteSpace(filter.Status))
                    query = query.Where(x => x.Status == filter.Status);

                if (filter.FromDate.HasValue)
                    query = query.Where(x => x.DateCreated >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                    query = query.Where(x => x.DateCreated <= filter.ToDate.Value);

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    string s = filter.SearchTerm.ToLower();
                    query = query.Where(x =>
                        (x.Vendor != null && x.Vendor.ToLower().Contains(s)) ||
                        (x.PRNumber != null && x.PRNumber.ToLower().Contains(s)) ||
                        (x.MaterialCode != null && x.MaterialCode.ToLower().Contains(s)));
                }
            }

            // ---------------- FLAT QUERY (Ticket-style JOIN) ----------------
            var flatResult = await (
                from pr in query

                join pra in _repositoryWrapper.MaterialPRAttachment
                    .FindByCondition(a => a.IsActive)
                    on pr.PRNumber equals pra.PRUniqueName into praJoin
                from pra in praJoin.DefaultIfEmpty()

                join asset in _repositoryWrapper.Asset
                    .FindByCondition(a => a.IsActive)
                    on pra.AssetId equals asset.Id into assetJoin
                from asset in assetJoin.DefaultIfEmpty()

                join assetType in _repositoryWrapper.Metadata
                    .FindByCondition(m => m.IsActive)
                    on asset.AssetType equals assetType.Id into assetTypeJoin
                from assetType in assetTypeJoin.DefaultIfEmpty()

                join fileType in _repositoryWrapper.Metadata
                    .FindByCondition(m => m.IsActive)
                    on asset.FileType equals fileType.Id into fileTypeJoin
                from fileType in fileTypeJoin.DefaultIfEmpty()
                join prat in _repositoryWrapper.MaterialPRAttachment
                  .FindByCondition(a => a.IsActive)
                  on pr.PRUniqueName equals prat.PRUniqueName into pratJoin
                from prat in pratJoin.DefaultIfEmpty()


                join assetpr in _repositoryWrapper.Asset
                    .FindByCondition(a => a.IsActive)
                    on prat.AssetId equals assetpr.Id into assetprJoin
                from assetpr in assetprJoin.DefaultIfEmpty()
                join assetTypePR in _repositoryWrapper.Metadata
                    .FindByCondition(m => m.IsActive)
                    on assetpr.AssetType equals assetTypePR.Id into assetTypePRJoin
                from assetTypePR in assetTypePRJoin.DefaultIfEmpty()
                join fileTypePR in _repositoryWrapper.Metadata
                    .FindByCondition(m => m.IsActive)
                    on assetpr.FileType equals fileTypePR.Id into fileTypePRJoin
                from fileTypePR in fileTypePRJoin.DefaultIfEmpty()

                orderby pr.DateUpdated descending

                select new
                {
                    PR = pr,
                    Attachment = prat,
                    InvoiceAttachment = pra,
                    Asset = asset,
                    AssetTypeKey = assetType != null ? assetType.Key : null,
                    FileTypeKey = fileType != null ? fileType.Key : null,
                    AssetPR = assetpr,
                    AssetPRTypeKey = assetTypePR != null ? assetTypePR.Key : null,
                    AssetPRFileTypeKey = fileTypePR != null ? fileTypePR.Key : null
                }
            ).ToListAsync();

            // ---------------- GROUPING ----------------
            var result = flatResult
    .GroupBy(x => x.PR.PRNumber)
    .Select(g =>
    {
        var prNumber = g.Key;

        // Get all invoices for this PR
        var prInvoices = _repositoryWrapper.MaterialInvoice
            .FindByCondition(i => i.PRNumber == prNumber)
            .ToList();
        bool isHeaderPR = prInvoices.Any(i => i.IsHeader);
        string dbStatus = g.First().PR.Status;


        string? externalStatus = null;

        if (!string.IsNullOrWhiteSpace(prNumber))
        {
            externalStatus = _repositoryWrapper.MaterialPR
                .GetExternalPRStatusByPRNumberAsync(prNumber)
                .GetAwaiter()
                .GetResult();
        }


        string finalStatus =
            !string.IsNullOrWhiteSpace(externalStatus) &&
            !string.Equals(dbStatus, externalStatus, StringComparison.OrdinalIgnoreCase)
                ? externalStatus
                : dbStatus;
        return new MaterialPRResponseDto
        {
            PRUniqueName = g.First().PR.PRUniqueName,
            PRNumber = prNumber,
            Name = g.First().PR.Title,
            NeedBy = g.First().PR.NeedBy,
            HeaderComment = g.First().PR.HeaderComment,
            TeamReceived = g.First().PR.TeamReceived,
            Status = finalStatus,
            IsHeader = isHeaderPR,
            LineItems = g
                .GroupBy(x => x.PR.LineItemNumber)
                .Select(li =>
                {
                    MaterialInvoice? invoice = null;

                    // Determine invoice based on IsHeader flag
                    if (prInvoices.Any(i => i.IsHeader))
                    {
                        // Take the first header invoice for all line items
                        invoice = prInvoices.First(i => i.IsHeader && i.PRNumber == prNumber);
                    }
                    else
                    {
                        // Take invoice for this specific line item
                        invoice = invoice = _repositoryWrapper.MaterialInvoice.FindByCondition(i => i.PRNumber == prNumber && i.LineItemNumber == li.Key).FirstOrDefault();
                    }

                    return new MaterialPRLineItemResponseDto
                    {
                        LineItemNumber = li.Key,
                        Description = li.First().PR.Description,
                        Amount = li.First().PR.Amount,
                        Quantity = li.First().PR.Quantity,
                        UOM = li.First().PR.UOM,
                        Vendor = li.First().PR.Vendor,
                        MaterialCode = li.First().PR.MaterialCode,
                        GLAccount = li.First().PR.GLAccount,
                        CostCenter = li.First().PR.CostCenter,
                        CompanyCode = li.First().PR.CompanyCode,
                        CommodityCode = li.First().PR.CommodityCode,
                        VendorName = li.First().PR.VendorName,
                        PurchaseGroup = li.First().PR.PurchaseGroup,
                        PurchaseOrg = li.First().PR.PurchaseOrg,
                        PONumber = li.First().PR.PONumber,
                        ShipToPlant = li.First().PR.ShipToPlant,
                        BillTo = li.First().PR.BillTo,
                        DeliverTo = li.First().PR.DeliverTo,
                        TaxCode = li.First().PR.TaxCode,
                        AccountType = li.First().PR.AccountType,
                        Type = li.First().PR.Type,
                        ReciptID = li.First().PR.ReciptID,
                        Contract = li.First().PR.Contract,
                        AutoGRN = li.First().PR.AutoGRN,
                        AutoInvoices = li.First().PR.AutoInvoices,
                        TaxInclude = li.First().PR.TaxInclude,
                        IsOCRAttachmentProcessed = invoice?.IsOCRAttachmentProcessed ?? false,
                        InvoiceComment = invoice?.InvoiceComment,
                        InvoiceDate = invoice?.InvoiceDate,
                        InvoiceID = invoice?.InvoiceID,
                        Venue = li.First().PR.Venue,
                        Currency = li.First().PR.Currency,
                        ExtractedInvoiceNumber = (invoice != null &&
                          li.First().PR.AutoInvoices == true &&
                          !string.IsNullOrWhiteSpace(invoice.InvoiceID))
                        ? _repositoryWrapper.MaterialInvoice.GetExtractedInvoiceByPRAsync(
                            invoice.PRNumber,
                            invoice.InvoiceID,
                            isHeaderPR ? null : li.Key
                        ).GetAwaiter().GetResult()?.ExtractedInvoiceNumber
                        : null,
                        Category = li.First().PR.Category,
                        AribaUserId = li.First().PR.AribaUserId,
                      

                        // Line-level attachments
                        InvoiceAttachments = li
                            .Where(x => x.Asset != null && x.InvoiceAttachment != null && x.InvoiceAttachment.Type == Common.INVOICE_TYPE_PRATTACHMENT && x.InvoiceAttachment.LineItemNumber == li.Key)

                            .GroupBy(x => x.Asset.Id)
                            .Select(x => new AssetDto
                            {
                                Id = x.First().Asset.Id,
                                AssetName = x.First().Asset.AssetName,
                                FileName = x.First().Asset.FileName,
                                AssetType = x.First().AssetTypeKey,
                                FileType = x.First().FileTypeKey
                            })
                            .ToList()
                    };
                })
                .ToList(),

            Attachments = g
    .Where(x => x.AssetPR != null
        && x.Attachment != null
        && x.Attachment.Type == Common.TYPE_MATERIAL_PR_UPLOAD)
    .GroupBy(x => x.AssetPR.Id)
    .Select(x => new AssetDto
    {
        Id = x.First().AssetPR.Id,
        AssetName = x.First().AssetPR.AssetName,
        FileName = x.First().AssetPR.FileName,
        AssetType = x.First().AssetPRTypeKey,
        FileType = x.First().AssetPRFileTypeKey
    })
                .Distinct()
                .ToList()
        };
    })
    .Skip(filter?.Index ?? 0)
    .Take(filter?.Limit ?? 10)
    .ToList();


            return result;
        }
    }
}
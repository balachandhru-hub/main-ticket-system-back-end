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
    public class LegalDepatmentMaterialService : ILegalDepartmentMaterialService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerManager _logger;
        private readonly IMaterialPRService _materialPRService;
        private readonly IBulkInsertHelper _bulkInsertHelper;
        private readonly IAssetService _assetService;
        private readonly IEmailService _emailService;

        public LegalDepatmentMaterialService(
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
                            var venueDetails = await _repositoryWrapper.LegalDepartmentMaterial
                                .GetVenueDetailsByNameAsync(domainId, li.Venue);
                            li.Venue = li.Venue;
                            li.PurchaseGroup = li.PurchaseGroup ?? venueDetails.PurchaseGroup;
                            li.CompanyCode = li.CompanyCode ?? venueDetails.CompanyCode;
                            li.PurchaseOrg = li.PurchaseOrg ?? venueDetails.PurchaseOrg;
                            li.CostCenter = venueDetails.CostCenter;
                            li.ShipToPlant = li.ShipToPlant ?? venueDetails.ShipToPlant;
                            li.ProcurementUnit = li.ProcurementUnit ?? defaultDetails.ProcurementUnit;
                            li.UOM = li.UOM ?? venueDetails.UOM;
                            li.DeliverTo = li.DeliverTo ?? defaultDetails.DeliverTo;
                            li.BillTo = venueDetails.CompanyCode;
                            var materialCode = await _repositoryWrapper.LegalMaterialCode
                               .GetLegalMaterialCodeDetailsByNameAsync(domainId, li.MaterialCode);
                            li.MaterialCode = li.MaterialCode;
                            li.GLAccount = li.GLAccount ?? materialCode.GLAccount;

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
                            li.CommodityCode = Common.LEGAL_DEPARTMENT_MATERIAL_COMMODITY_CODES_WITH_TAX;
                        }
                        else
                        {
                            li.CommodityCode = Common.LEGAL_DEPARTMENT_MATERIAL_COMMODITY_CODE_WITHOUT_TAX;
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
                                    ? await _repositoryWrapper.LegalDepartmentMaterial.GetVenueDetailsByNameAsync(domainId, li.Venue)
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
                                    AribaUserId = li?.AribaUserId ?? venueDetailsForLine?.AribaUserId
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
                                    AribaUserId = li.AribaUserId
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
                                    li.CommodityCode = Common.LEGAL_DEPARTMENT_MATERIAL_COMMODITY_CODES_WITH_TAX;
                                }
                                else
                                {
                                    li.CommodityCode = Common.LEGAL_DEPARTMENT_MATERIAL_COMMODITY_CODE_WITHOUT_TAX;
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
                                AribaUserId = li.AribaUserId
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




        public async Task<BatchProcessingResultDto> CreateVenueBatchAsync(List<VenueExcelDto> dtos)
        {
            _logger.LogInfo($"Batch creating {dtos.Count} Contract Data records");

            BatchProcessingResultDto result = new BatchProcessingResultDto();
            List<LegalVenueList> newCompanyDataList = new List<LegalVenueList>();
            List<LegalVenueList> updateCompanyDataList = new List<LegalVenueList>();

            // Track processed contracts to avoid duplicates
            HashSet<string> processedCompanyKeys = new HashSet<string>();

            try
            {
                List<string> venueName = dtos.Select(k => k.VenueName).Distinct().ToList();

                Dictionary<string, LegalVenueList> existingCompanys = await _repositoryWrapper.LegalDepartmentMaterial.FindByCondition(x => x.IsActive && venueName.Contains(x.VenueName!))
                        .ToDictionaryAsync(x => x.VenueName!);

                foreach (var dto in dtos)
                {
                    try
                    {
                        string companyKey = dto.VenueName;

                        if (!processedCompanyKeys.Contains(companyKey))
                        {
                            processedCompanyKeys.Add(companyKey);

                            if (existingCompanys.TryGetValue(companyKey, out LegalVenueList existingCompany) && existingCompany.DomainId == dto.DomainId)
                            {
                                existingCompany.VenueName = dto.VenueName;
                                updateCompanyDataList.Add(existingCompany);
                            }
                            else
                            {
                                LegalVenueList newCompany = new LegalVenueList
                                {
                                    Id = Guid.NewGuid(),
                                    DomainId = dto.DomainId,
                                    VenueName = dto.VenueName,
                                    CompanyCode = dto.CompanyCode,
                                    CompanyName = dto.CompanyName,
                                    CostCenter = dto.CostCenter,
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
                    await _repositoryWrapper.LegalDepartmentMaterial.CreateRangeAsync(newCompanyDataList);
                }

                if (updateCompanyDataList.Any())
                {
                    _logger.LogInfo($"Bulk updating {updateCompanyDataList.Count} contracts");
                    _repositoryWrapper.LegalDepartmentMaterial.UpdateRange(updateCompanyDataList);
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
            var query = _repositoryWrapper.LegalDepartmentMaterial
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
            return await _repositoryWrapper.LegalDepartmentMaterial.GetVenueDetailsByNameAsync(domainId, venueName);

        }

        public async Task<List<LegalMaterialCodeDetailsDto>> GetLegalMaterialCodesForDropdownAsync(Guid domainId, string searchTerm, int index, int limit)
        {
            var query = _repositoryWrapper.LegalMaterialCode
                .FindByCondition(s => s.IsActive)
                .Where(s =>
                    s.DomainId == domainId &&
                    (string.IsNullOrEmpty(searchTerm) ||
                     s.MaterialCode.Contains(searchTerm)));

            int total = await query.CountAsync();

            var suppliers = await query
                .OrderBy(s => s.MaterialCode)
                .Skip(index)
                .Take(limit)

                .Select(s => new LegalMaterialCodeDetailsDto
                {

                    MaterialCode = s.MaterialCode,
                    Description = s.Description
                })
                .ToListAsync();

            return suppliers;
        }
        public async Task<BatchProcessingResultDto> CreateLegalMaterialCodeBatchAsync(List<LegalMaterialCodeExcelDto> dtos)
        {
            _logger.LogInfo($"Batch creating {dtos.Count} Legal Material Code records");

            BatchProcessingResultDto result = new BatchProcessingResultDto();
            List<LegalMaterialCode> newMaterialCodeDataList = new List<LegalMaterialCode>();
            List<LegalMaterialCode> updateMaterialCodeDataList = new List<LegalMaterialCode>();

            // Track processed contracts to avoid duplicates
            HashSet<string> processedMaterialCodeKeys = new HashSet<string>();

            try
            {
                List<string> supplierId = dtos.Select(k => k.MaterialCode).Distinct().ToList();

                Dictionary<string, LegalMaterialCode> existingMaterialCodes = await _repositoryWrapper.LegalMaterialCode.FindByCondition(x => x.IsActive && supplierId.Contains(x.MaterialCode!))
                        .ToDictionaryAsync(x => x.MaterialCode!);

                foreach (var dto in dtos)
                {
                    try
                    {
                        string materialCodeKey = dto.MaterialCode;

                        if (!processedMaterialCodeKeys.Contains(materialCodeKey))
                        {
                            processedMaterialCodeKeys.Add(materialCodeKey);

                            if (existingMaterialCodes.TryGetValue(materialCodeKey, out LegalMaterialCode existingMaterialCode) && existingMaterialCode.DomainId == dto.DomainId)
                            {
                                existingMaterialCode.Description = dto.Description;
                                existingMaterialCode.GLAccount = dto.GLAccount;
                                updateMaterialCodeDataList.Add(existingMaterialCode);
                            }
                            else
                            {
                                LegalMaterialCode newMaterialCode = new LegalMaterialCode
                                {
                                    Id = Guid.NewGuid(),
                                    DomainId = dto.DomainId,
                                    MaterialCode = dto.MaterialCode,
                                    Description = dto.Description,
                                    GLAccount = dto.GLAccount
                                };

                                newMaterialCodeDataList.Add(newMaterialCode);
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
                if (newMaterialCodeDataList.Any())
                {
                    _logger.LogInfo($"Bulk inserting {newMaterialCodeDataList.Count} new legal material codes");
                    await _repositoryWrapper.LegalMaterialCode.CreateRangeAsync(newMaterialCodeDataList);
                }

                if (updateMaterialCodeDataList.Any())
                {
                    _logger.LogInfo($"Bulk updating {updateMaterialCodeDataList.Count} legal material codes");
                    _repositoryWrapper.LegalMaterialCode.UpdateRange(updateMaterialCodeDataList);
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
        public async Task<LegalMaterialCodeListDto?> GetMaterialCodeDetailsByNameAsync(
              Guid domainId,
              string materialcode)
        {
            _logger.LogInfo($"Fetching venue details for Venue: {materialcode} in DomainId: {domainId}");
            return await _repositoryWrapper.LegalMaterialCode.GetLegalMaterialCodeDetailsByNameAsync(domainId, materialcode);

        }

    }
}
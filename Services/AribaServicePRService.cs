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
    public class AribaServicePRService : IAribaServicePRService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerManager _logger;
        private readonly IServicesPRService _servicesPRService;
        private readonly IAssetService _assetService;
        private readonly IBulkInsertHelper _bulkInsertHelper;
        private readonly IEmailService _emailService;

        public AribaServicePRService(
            IRepositoryWrapper repositoryWrapper,
            ILoggerManager logger,
           IServicesPRService servicesPRService,
            IBulkInsertHelper bulkInsertHelper, IAssetService assetService, IEmailService emailService)
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
            _servicesPRService = servicesPRService;
            _assetService = assetService;
            _emailService = emailService;
            _bulkInsertHelper = bulkInsertHelper;
        }


        public async Task<List<MaterialPRResultDto>> PostMaterialPRToAriba(
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
                      
                         TeamReceived = g.First().pr.TeamReceived,
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

                            li.UOM = li.UOM;
                            li.DeliverTo = profileDto.AribaUserName;
                            li.BillTo = venueDetails.CompanyCode;

                        }

                        li.PurchaseGroup = li.PurchaseGroup ?? defaultDetails.PurchaseGroup;
                        if (li.TaxInclude)
                        {
                            li.CommodityCode = Common.PR_ENTERINMENT_SERVICE_COMMODITY_CODE_WITHOUT_TAX;
                        }
                        else
                        {
                            li.CommodityCode = Common.PR_ENTERINMENT_SERVICE_COMMODITY_CODE_WITHOUT_TAX;
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
                        description = li.Description;

                        li.DeliveryDate = li.DeliveryDate ?? DateTime.Now.AddDays(1);
                        li.InvoiceAttachment = li.InvoiceAttachment;
                        li.AutoSES = li.AutoSES;
                        li.AutoInvoices = li.AutoInvoices;
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
                            DeliverTo = profileDto.AribaUserName,
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
                            TeamReceived = prRequest.TeamReceived,
                            Type=Common.PR_ENTERTAINMENT_DEPARTMENT,
                            InvoiceAttachment = prRequest.LineItems
                                .SelectMany(li => li.InvoiceAttachment ?? new List<AssetUploadDto>())
                                .ToList()
                        };

                        var response = await _servicesPRService.PostServicesPRToAriba(aribaReq);
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
                                    DeliverTo = li?.DeliverTo ?? profileDto.AribaUserName,
                                    PreparerName = aribaReq.Preparer,
                                    BillTo = li?.BillTo ?? venueDetailsForLine?.CompanyCode ?? defaultDetails.CompanyCode,
                                    TaxCode = li?.TaxCode,
                                    Venue = li?.Venue,
                                    HeaderComment = aribaReq.HeaderComment,
                                    Contract = li?.Contract,
                                    TaxInclude = li?.TaxInclude ?? false,
                                    VendorName = li?.VendorName,
                                    ServiceEndDate = li.ServiceEndDate,
                                    ServiceStartDate = li.ServiceStartDate,
                                    MaxAmount = li.Amount,
                                    ExpectedAmount = li.Amount,
                                    TeamReceived = prRequest.TeamReceived,

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
                                    DeliverTo = li.DeliverTo ?? profileDto.AribaUserName,
                                    PreparerName = aribaReq.Preparer,
                                    BillTo = li.BillTo ?? li.CompanyCode,
                                    TaxCode = li.TaxCode,
                                    Venue = li.Venue,
                                    HeaderComment = aribaReq.HeaderComment,
                                    Contract = li.Contract,
                                    TaxInclude = li.TaxInclude,
                                    VendorName = li.VendorName,
                                    ServiceEndDate = li.ServiceEndDate,
                                    ServiceStartDate = li.ServiceStartDate,
                                    MaxAmount = li.Amount,
                                    ExpectedAmount = li.Amount,
                                    TeamReceived = prRequest.TeamReceived,

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
                                existing.AutoSES = li.AutoSES;
                                existing.AutoInvoices = li.AutoInvoices;
                                existing.ServiceStartDate = li.ServiceStartDate;
                                existing.ServiceEndDate = li.ServiceEndDate;
                                existing.MaxAmount = li.Amount;
                                existing.ExpectedAmount = li.Amount;
                                existing.TeamReceived = prRequest.TeamReceived;

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
                            UniqueName = prRequest.Name,
                            NeedBy = prRequest.NeedBy,
                            Operation = Common.OPERATION_SERVICE_PR_CREATION,
                            HeaderComment = prRequest.HeaderComment,
                            Preparer = profileDto.AribaUserName,
                            PasswordAdapter = profileDto.AribaPassword,
                            LineItems = prRequest.LineItems,
                            Attachments = prRequest.Attachments,
                            SystemReferenceID = generatedPRNumber,
                            InvoiceAttachment = prRequest.LineItems.SelectMany(li => li.InvoiceAttachment ?? new List<AssetUploadDto>()).ToList(),
                            ServiceVariant = config.ServiceVariant,
                            ServicePartition = config.ServicePartition,
                            TeamReceived = prRequest.TeamReceived,
                            Type=Common.PR_ENTERTAINMENT_DEPARTMENT,
                        };

                        var response = await _servicesPRService.PostServicesPRToAriba(aribaReq);

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
                                ServiceEndDate = li.ServiceEndDate,
                                ServiceStartDate = li.ServiceStartDate,
                                MaxAmount = li.Amount,
                                ExpectedAmount = li.Amount,
                                TeamReceived = prRequest.TeamReceived,
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
                            PRServiceUniqueName = pr.Name,
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
                _logger.LogError($"Material PR creation/save error: {ex.InnerException?.Message ?? ex.Message}");
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


        public async Task<List<ServicePRResponseDto>> GetServicePRsGroupedAsync(
      Guid userId,
      Guid domainId,
      ServicePRFilterRequestDto filter)
        {
            _logger.LogInfo($"Fetching Service PRs grouped by PRNumber for UserId: {userId}, DomainId: {domainId}");

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

            IQueryable<ServicePR> query =
                _repositoryWrapper.ServicePR.FindByCondition(x => x.DomainId == domainId);

            if (role != Common.BUSINESS_ADMIN_USER_KEY)
            {
                if (string.IsNullOrEmpty(aribaUsername))
                    throw new NotFoundCustomException("Ariba username missing", $"UserId: {userId}");

                query = query.Where(x => x.PreparerName == aribaUsername);
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

                join pra in _repositoryWrapper.ServicePRAttachment
                    .FindByCondition(a => a.IsActive)
                    on pr.PRNumber equals pra.PRServiceUniqueName into praJoin
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
                join prat in _repositoryWrapper.ServicePRAttachment
                  .FindByCondition(a => a.IsActive)
                  on pr.PRServiceUniqueName equals prat.PRServiceUniqueName into pratJoin
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
        var prInvoices = _repositoryWrapper.ServicePRInvoice
            .FindByCondition(i => i.PRNumber == prNumber)
            .ToList();
        bool isHeaderPR = prInvoices.Any(i => i.IsHeader);
        string dbStatus = g.First().PR.Status;


        string? externalStatus = null;

        if (!string.IsNullOrWhiteSpace(prNumber))
        {
            externalStatus = _repositoryWrapper.ServicePR
                .GetExternalPRStatusByPRNumberAsync(prNumber)
                .GetAwaiter()
                .GetResult();
        }


        string finalStatus =
            !string.IsNullOrWhiteSpace(externalStatus) &&
            !string.Equals(dbStatus, externalStatus, StringComparison.OrdinalIgnoreCase)
                ? externalStatus
                : dbStatus;
        return new ServicePRResponseDto
        {
            PRServiceUniqueName = g.First().PR.PRServiceUniqueName,
            PRNumber = prNumber,
            Name = g.First().PR.Title,
            NeedBy = g.First().PR.NeedBy,
            HeaderComment = g.First().PR.HeaderComment,
            Status = finalStatus,
            IsHeader = isHeaderPR,
            LineItems = g
                .GroupBy(x => x.PR.LineItemNumber)
                .Select(li =>
                {
                    ServicePRInvoice? invoice = null;

                    // Determine invoice based on IsHeader flag
                    if (prInvoices.Any(i => i.IsHeader))
                    {
                        // Take the first header invoice for all line items
                        invoice = prInvoices.First(i => i.IsHeader && i.PRNumber == prNumber);
                    }
                    else
                    {
                        // Take invoice for this specific line item
                        invoice = invoice = _repositoryWrapper.ServicePRInvoice.FindByCondition(i => i.PRNumber == prNumber && i.LineItemNumber == li.Key).FirstOrDefault();
                    }

                    return new ServicePRLineItemResponseDto
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

                        SSID = (li.First().PR.AutoSES == true &&
                                !string.IsNullOrWhiteSpace(li.First().PR.PONumber))
                            ? _repositoryWrapper.MaterialInvoice.GetExtractedSSIDByPRAsync(li.First().PR.PONumber, li.Key).GetAwaiter().GetResult()?.ExtractedSSID : null,
                        Contract = li.First().PR.Contract,
                        AutoSES = li.First().PR.AutoSES,
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
                        ? _repositoryWrapper.MaterialInvoice.GetExtractedInvoiceByServicePRAsync(
                            invoice.PRNumber,
                            invoice.InvoiceID,
                            isHeaderPR ? null : li.Key
                        ).GetAwaiter().GetResult()?.ExtractedInvoiceNumber
                        : null,
                        Category = li.First().PR.Category,
                        ServiceStartDate = li.First().PR.ServiceStartDate,
                        ServiceEndDate = li.First().PR.ServiceEndDate,
                        MaxAmount = li.First().PR.MaxAmount,
                        ExpectedAmount = li.First().PR.ExpectedAmount,


                        // Line-level attachments
                        InvoiceAttachments = li
                            .Where(x => x.Asset != null && x.InvoiceAttachment != null && x.InvoiceAttachment.Type == Common.INVOICE_TYPE_PR_SERVICE_ATTACHMENT && x.InvoiceAttachment.LineItemNumber == li.Key)

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
    .Skip(filter.Index)
    .Take(filter.Limit)
    .ToList();


            return result;
        }

        public async Task<BatchProcessingResultDto> CreateDraftServicePRBatchAsync(
           List<DraftServicePRExcelUploadDto> dtos,
           Guid userId, Guid domainId)
        {
            _logger.LogInfo($"Batch creating {dtos.Count} Draft Service PR records");

            BatchProcessingResultDto result = new();
            List<DraftServicePR> newPRList = new();

            try
            {
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

                int currentDraftNumber = _repositoryWrapper.DraftServicePR
               .FindByCondition(x =>
                   x.IsActive &&
                   x.UserId == userId &&
                   x.DomainId == domainId &&
                   x.DraftID != null &&
                   x.DraftID.StartsWith(Common.DRAFT_ID_START_WITH_PR))
               .Select(x => x.DraftID.Substring(Common.DRAFT_ID_START_WITH_PR.Length))
               .AsEnumerable()
               .Where(x => int.TryParse(x, out _))
               .Select(int.Parse)
               .DefaultIfEmpty(0)
               .Max();


                // 🔹 Track PRUniqueName → DraftID & LineItemNumber
                Dictionary<string, string> prDraftMap = new();
                Dictionary<string, int> prLineItemMap = new();

                foreach (var dto in dtos)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(dto.PRServiceUniqueName))
                            throw new Exception("PRServiceUniqueName is required");

                        if (!prDraftMap.ContainsKey(dto.PRServiceUniqueName))
                        {
                            currentDraftNumber++;
                            string newDraftId = $"{Common.DRAFT_ID_START_WITH_PR}{currentDraftNumber:D2}";

                            prDraftMap[dto.PRServiceUniqueName] = newDraftId;
                            prLineItemMap[dto.PRServiceUniqueName] = 1;
                        }
                        else
                        {
                            prLineItemMap[dto.PRServiceUniqueName]++;
                        }

                        DateTime startDate = DateTime.UtcNow.Date;
                        string description = string.IsNullOrWhiteSpace(dto.Venue)
                            ? $"{dto.Vendor} {startDate:yyyy-MM-dd}"
                            : $"{dto.Vendor} {dto.Venue} {startDate:yyyy-MM-dd}";

                        DraftServicePR pr = new DraftServicePR
                        {
                            Id = Guid.NewGuid(),
                            DomainId = domainId,
                            UserId = userId,

                            DraftID = prDraftMap[dto.PRServiceUniqueName],
                            LineItemNumber = prLineItemMap[dto.PRServiceUniqueName],
                            PRServiceUniqueName = dto.PRServiceUniqueName,
                            RequesterName = aribaUsername,
                            Venue = dto.Venue,
                            Vendor = dto.Vendor,
                            NeedBy = dto.ServiceStartDate,
                            Quantity = dto.Quantity,
                            Amount = dto.Amount,
                            TaxInclude = dto.TaxInclude,
                            Currency = dto.Currency,
                            AutoInvoices = dto.AutoInvoices,
                            InvoiceID = dto.InvoiceID,
                            InvoiceDate = dto.InvoiceDate,
                            AutoSES = dto.AutoSES?? false,
                            TeamReceived = dto.TeamReceived?? false,
                            IsOCRAttachmentProcessed = false,
                            Description = dto.Description ?? description,
                            Title = dto.Title ?? description
                        };

                        newPRList.Add(pr);
                        result.SuccessfulCount++;
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount++;
                        result.Errors.Add(new ErrorResponseDto
                        {
                            StatusCode = 400,
                            Message = "Draft Material PR row failed",
                            Description = $"Row {dto.RowNumber}: {ex.Message}"
                        });

                        _logger.LogError($"Row {dto.RowNumber} failed: {ex.Message}");
                    }
                }

                if (newPRList.Any())
                {
                    await _repositoryWrapper.DraftServicePR.CreateRangeAsync(newPRList);
                    await _repositoryWrapper.SaveAsync();
                }

                _logger.LogInfo($"Draft Service PR batch completed. Success: {result.SuccessfulCount}, Failed: {result.FailedCount}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Critical error in Draft Service PR batch: {ex.Message}");
                throw;
            }
        }


    }
}

using Contracts;
using Contracts.IRepository;
using Contracts.IServices;
using Dto;
using Entities.Common;
using System.IO;
using Entities.Models;
using ExceptionHandler;
using Microsoft.EntityFrameworkCore;


namespace Services
{
    /// <summary>
    /// Service for Draft Material PR creation
    /// Handles creation of draft material PRs without calling SOAP API
    /// </summary>
    public class DraftMaterialPRService : IDraftMaterialPRService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerManager _logger;
        private readonly IAssetService _assetService;

        public DraftMaterialPRService(
            IRepositoryWrapper repositoryWrapper,
            ILoggerManager logger,
            IAssetService assetService)
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
            _assetService = assetService;
        }

        /// <summary>
        /// Create a new draft Material PR with auto-increment DraftID
        /// Does not call SOAP API
        /// </summary>
        public async Task<List<DraftMaterialPRResultDto>> CreateDraftMaterialPRAsync(
        List<DraftAribaMaterialPRDto> prRequests,
        Guid domainId,
        Guid userId,
        List<AssetUploadDto>? assetUploadDto)
        {
            _logger.LogInfo("Preparing to create Draft Material PRs");

            if (prRequests == null || !prRequests.Any())
            {
                _logger.LogError("No PR requests provided for draft creation");
                throw new PreConditionFailedCustomException(
                    "Invalid request",
                    "At least one PR request is required");
            }

            _logger.LogDebug($"Fetching user profile for userId: {userId}");
            var profileDto = _repositoryWrapper.Person.GetMyProfileByUserId(userId);
            if (profileDto == null)
            {
                _logger.LogError($"User profile not found for userId: {userId}");
                throw new NotFoundCustomException("User profile missing", $"No profile found for userId {userId}");
            }
            _logger.LogDebug($"User profile retrieved successfully. AribaUserName: {profileDto.AribaUserName}");

            MaterialPRLineItemDto defaultDetails = null;
            try
            {
                defaultDetails = _repositoryWrapper.MaterialPR.GetPRDetails(profileDto.AribaUserName);
            }
            catch
            {
                _logger.LogInfo($"Could not retrieve default details for user {profileDto.AribaUserName}, will use line item values");
                defaultDetails = null;
            }

            var draftResults = new List<DraftMaterialPRResultDto>();

            try
            {
                // Group PRs by UniqueName
                var groupedPRs = prRequests
                    .SelectMany(pr => pr.LineItems.Select(li => new { pr, li }))
                    .GroupBy(x => x.pr.UniqueName)
                    .Select(g => new DraftAribaMaterialPRDto
                    {
                        UniqueName = g.Key,
                        Name = g.First().pr.Name,
                        NeedBy = g.First().pr.NeedBy,
                        HeaderComment = g.First().pr.HeaderComment,
                        DefaultDeliverTo = g.First().pr.DefaultDeliverTo,
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
                            ErrorComment = x.li.ErrorComment,
                            ParentLineItem = x.li.ParentLineItem,
                            Percentage = x.li.Percentage,
                            CategoryName= x.li.CategoryName,
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

                // Attach generic PR attachments if provided
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
                    var newDraftRecords = new List<DraftMaterialPR>();
                    _logger.LogDebug($"Processing draft PR: {prRequest.UniqueName}");

                    // Get next DraftID for this PR
                    string draftId = await GenerateNextDraftIdAsync(userId,domainId);


                    // Prepare NeedBy date with standard time
                    var needByDate = new DateTime(
                        prRequest.NeedBy.Year,
                        prRequest.NeedBy.Month,
                        prRequest.NeedBy.Day,
                        12, 0, 0,
                        DateTimeKind.Utc
                    );

                    // Create draft PR records
                    foreach (var li in prRequest.LineItems)
                    {
                        var draftMaterialPR = new DraftMaterialPR
                        {
                            Id = Guid.NewGuid(),
                            DomainId = domainId,
                            UserId = userId,
                            DraftID = draftId,
                            PRUniqueName = prRequest.UniqueName ?? string.Empty,
                            Description = li.Description,
                            Title = li.Description ?? string.Empty,
                            CommodityCode = li.CommodityCode,
                            Amount = li.Amount,
                            Quantity = li.Quantity,
                            DeliveryDate = li.DeliveryDate ?? DateTime.Now.AddDays(1),
                            NeedBy = needByDate,
                            UOM = li.UOM,
                            Vendor = li.Vendor ?? string.Empty,
                            Type = Common.MATERIAL_TYPE,
                            AccountType = Common.ACCOUNT_TYPE,
                            GLAccount = li.GLAccount,
                            CostCenter = li.CostCenter,
                            MaterialCode = li.MaterialCode ?? string.Empty,
                            AutoGRN = li.AutoGRN,
                            AutoInvoices = li.AutoInvoices,
                            Currency = string.IsNullOrWhiteSpace(li.Currency) ? Common.DEFAULT_CURRENCY : li.Currency,
                            PurchaseOrg = li.PurchaseOrg,
                            PurchaseGroup = li.PurchaseGroup ?? string.Empty,
                            CompanyCode = li.CompanyCode,
                            ShipToPlant = li.ShipToPlant,
                            DeliverTo = li.DeliverTo ?? defaultDetails?.DeliverTo ?? prRequest.DefaultDeliverTo ?? string.Empty,
                            RequesterName = profileDto.AribaUserName,
                            BillTo = li.BillTo,
                            TaxCode = li.TaxCode,
                            Venue = li.Venue,
                            HeaderComment = prRequest.HeaderComment,
                            Contract = li.Contract,
                            TaxInclude = li.TaxInclude,
                            VendorName = li.VendorName,
                            InvoiceID = li.InvoiceID,
                            InvoiceDate = li.InvoiceDate,
                            LineItemNumber = li.NumberInCollection,
                            Counter =draftId,
                            ErrorComment = li.ErrorComment,
                            ParentLineItem = li.ParentLineItem,
                            Category = li.CategoryName,
                            AribaUserId = li.AribaUserId
                           

                        };

                        newDraftRecords.Add(draftMaterialPR);
                    }

                    // Save draft PRs first
                    if (newDraftRecords.Any())
                    {
                        _repositoryWrapper.DraftMaterialPR.CreateRange(newDraftRecords);
                        await _repositoryWrapper.SaveAsync();
                        _logger.LogDebug($"Created {newDraftRecords.Count} draft material PR records");
                    }

                    // Save line-item attachments immediately
                    foreach (var li in prRequest.LineItems)
                    {
                        if (li.InvoiceAttachment == null || !li.InvoiceAttachment.Any())
                            continue;

                        foreach (var attachment in li.InvoiceAttachment)
                        {
                            var assetUpload = new AssetUploadDto
                            {
                                FileBytes = attachment.FileBytes,
                                FileName = attachment.FileName,
                                ContentType = attachment.ContentType,
                                AssetType = Common.INVOICE_TYPE_PRATTACHMENT,
                                EntityType = Common.INVOICE_TYPE_PRATTACHMENT
                            };

                            Guid assetId = _assetService.UploadDocument(assetUpload);

                            _repositoryWrapper.DraftMaterialPRAttachment.Create(new DraftMaterialPRAttachment
                            {
                                Id = Guid.NewGuid(),
                                DraftID = draftId,
                                DomainId = domainId,
                                UserId = userId,
                                AssetId = assetId,
                                LineItemNumber = li.NumberInCollection,
                                InvoiceID = li.InvoiceID,
                                Type = Common.INVOICE_TYPE_PRATTACHMENT,

                            });
                        }
                    }

                    // Save generic PR attachments immediately
                    if (prRequest.Attachments != null && prRequest.Attachments.Any())
                    {
                        foreach (var attachment in prRequest.Attachments)
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

                            _repositoryWrapper.DraftMaterialPRAttachment.Create(new DraftMaterialPRAttachment
                            {
                                Id = Guid.NewGuid(),
                                DraftID = draftId,
                                DomainId = domainId,
                                UserId = userId,
                                AssetId = assetId,
                                Type = Common.TYPE_MATERIAL_PR_UPLOAD,

                            });
                        }
                    }

                    // Save all attachments
                    await _repositoryWrapper.SaveAsync();
                    _logger.LogInfo($"Draft PR {prRequest.UniqueName} attachments saved successfully");

                    draftResults.Add(new DraftMaterialPRResultDto
                    {
                        DraftID = draftId,
                        Message = $"Draft PR created successfully"
                    });
                }

                _logger.LogInfo("All Draft Material PRs and attachments created successfully");
                return draftResults;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating draft Material PRs: {ex.Message}");
                throw;
            }
        }
          private async Task<string> GenerateNextDraftIdAsync(Guid userId, Guid domainId)
            {
                var draftIds = await _repositoryWrapper.DraftMaterialPR
                    .FindByCondition(x =>
                        x.IsActive &&
                        x.UserId == userId &&
                        x.DomainId == domainId &&
                        x.DraftID != null &&
                        x.DraftID.StartsWith(Common.DRAFT_ID_START_WITH_PR))
                    .Select(x => x.DraftID)
                    .ToListAsync();

                int maxNumber = draftIds
                    .Select(id =>
                    {
                        var numPart = id.Substring(Common.DRAFT_ID_START_WITH_PR.Length);
                        return int.TryParse(numPart, out int n) ? n : 0;
                    })
                    .DefaultIfEmpty(0)
                    .Max();

            
                return $"{Common.DRAFT_ID_START_WITH_PR}{(maxNumber + 1):D2}";
            }





        public async Task<List<DraftMaterialPRDetailsDto>> GetDraftMaterialPRsGroupedAsync(
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

            IQueryable<DraftMaterialPR> query =
                _repositoryWrapper.DraftMaterialPR.FindByCondition(x => x.DomainId == domainId);

            if (role != Common.BUSINESS_ADMIN_USER_KEY)
            {
                if (string.IsNullOrEmpty(aribaUsername))
                    throw new NotFoundCustomException("Ariba username missing", $"UserId: {userId}");

                query = query.Where(x => x.RequesterName == aribaUsername);
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
                     string term = filter.SearchTerm.Trim();
                        string upperTerm = term.ToUpper();

                        
                        if (int.TryParse(term, out int draftNumber))
                        {
                            string prDraftId = $"{Common.DRAFT_ID_START_WITH_PR}{draftNumber}";

                            query = query.Where(x =>
                                x.DraftID == draftNumber.ToString() ||   
                                x.DraftID == prDraftId                   
                            );
                        }
                    
                        else if (upperTerm.StartsWith(Common.DRAFT_ID_START_WITH_PR))
                        {
                            string numberPart = upperTerm
                                .Substring(Common.DRAFT_ID_START_WITH_PR.Length)
                                .TrimStart('0');

                            string normalizedPr = Common.DRAFT_ID_START_WITH_PR +
                                                (string.IsNullOrEmpty(numberPart) ? "0" : numberPart);

                            query = query.Where(x =>
                                x.DraftID.ToUpper() == upperTerm ||      
                                x.DraftID.ToUpper() == normalizedPr      
                            );
                        }

                    else
                    {
                        string s = term.ToLower();
                        query = query.Where(x =>
                            (x.Vendor != null && x.Vendor.ToLower().Contains(s)) ||
                            (x.MaterialCode != null && x.MaterialCode.ToLower().Contains(s))||(x.Description != null && x.Description.ToLower().Contains(s))||(x.Category != null && x.Category.ToLower().Contains(s))
                        );
                    }
                }
            }

            // ---------------- FLAT QUERY (Ticket-style JOIN) ----------------
            var flatResult = (
             from pr in query

             join pra in _repositoryWrapper.DraftMaterialPRAttachment
                 .FindByCondition(a => a.IsActive)
                 on new { pr.DraftID, pr.LineItemNumber }
                 equals new { pra.DraftID, pra.LineItemNumber } into praJoin
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

            join prat in _repositoryWrapper.DraftMaterialPRAttachment
                    .FindByCondition(a => a.IsActive)
                    on pr.DraftID equals prat.DraftID into pratJoin
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
         ).ToList();   

            // ---------------- GROUPING ----------------
            var result = flatResult
                .GroupBy(x => x.PR.DraftID)
                .Select(g => new DraftMaterialPRDetailsDto
                {
                    PRUniqueName = g.First().PR.PRUniqueName,
                    DraftID = g.Key,
                    NeedBy = g.First().PR.NeedBy,
                    HeaderComment = g.First().PR.HeaderComment,
                    Title = g.First().PR.Title,
                    


                    // 🔹 ONE LINE ITEM (like ticket view)
                    LineItems = g
                        .GroupBy(x => x.PR.LineItemNumber)
                        .OrderByDescending(li => li.Key) 
                        .Select(li => new DraftMaterialPRLineItemDto
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
                            CommdityCode = li.First().PR.CommodityCode,
                            VendorName = li.First().PR.VendorName,
                            PurchaseGroup = li.First().PR.PurchaseGroup,
                            PurchaseOrg = li.First().PR.PurchaseOrg,
                            ShipToPlant = li.First().PR.ShipToPlant,
                            BillTo = li.First().PR.BillTo,
                            DeliverTo = li.First().PR.DeliverTo,
                            TaxCode = li.First().PR.TaxCode,
                            AccountType = li.First().PR.AccountType,
                            Type = li.First().PR.Type,
                            Contract = li.First().PR.Contract,
                            AutoGRN = li.First().PR.AutoGRN,
                            AutoInvoices = li.First().PR.AutoInvoices,
                            TaxInclude = li.First().PR.TaxInclude,
                            IsOCRAttachmentProcessed = li.First().PR.IsOCRAttachmentProcessed,
                            InvoiceComment = li.First().PR.InvoiceComment,
                            InvoiceDate = li.First().PR.InvoiceDate,
                            InvoiceID = li.First().PR.InvoiceID,
                            ErrorComment = li.First().PR.ErrorComment,
                            Venue=li.First().PR.Venue,
                            ParentLineItem = li.First().PR.ParentLineItem,
                            Category = li.First().PR.Category,
                            AribaUserId = li.First().PR.AribaUserId,
                            TeamReceived = li.First().PR.TeamReceived,
                            Title = li.First().PR.Title,

                            
                           


                            // 🔹 Line-level attachments (AssetDto)
                            InvoiceAttachments = li
                                .Where(x =>
                                    x.Asset != null &&
                                    x.InvoiceAttachment != null &&
                                    x.InvoiceAttachment.Type == Common.INVOICE_TYPE_PRATTACHMENT)
                                .GroupBy(x => x.Asset.Id)
                                .Select(h => new AssetDto
                                {
                                    Id = h.Key,
                                    AssetName = h.First().Asset.AssetName,
                                    FileName = h.First().Asset.FileName,
                                    AssetType = h.First().AssetTypeKey,
                                    FileType = h.First().FileTypeKey
                                })
                                .ToList()
                        })
                        .ToList(),

                    // 🔹 Header-level attachments (AssetDto)
                    HeaderAttachments = g
                    .Where(x =>
                        x.AssetPR != null &&
                        x.Attachment != null &&
                        x.Attachment.Type == Common.TYPE_MATERIAL_PR_UPLOAD)
                    .GroupBy(x => x.AssetPR.Id)   
                    .Select(h => new AssetDto
                    {
                        Id = h.Key,
                        AssetName = h.First().AssetPR.AssetName,
                        FileName = h.First().AssetPR.FileName,
                        AssetType = h.First().AssetPRTypeKey,
                        FileType = h.First().AssetPRFileTypeKey
                    })
                    .ToList()

                })
                .OrderByDescending(x =>
                    int.TryParse(
                        x.DraftID.Replace(Common.DRAFT_ID_START_WITH_PR, ""),
                        out var num
                    ) ? num : 0
                )
                .Skip(filter.Index)
                .Take(filter.Limit)
                .ToList();

            return result;
        }

        


    }
}

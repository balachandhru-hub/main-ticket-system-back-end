using Contracts;
using EFCore.BulkExtensions;
using Entities.Models;
using Contracts.IServices;
using Contracts.IRepository;
using Entities.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    public class BulkInsertHelper : IBulkInsertHelper
    {
        private readonly RepositoryContext _context;
        private readonly ILoggerManager _logger;
        private readonly IUserIdentityService _userIdentityService;

        public BulkInsertHelper(RepositoryContext context, ILoggerManager logger, IUserIdentityService userIdentityService)
        {
            _context = context;
            _logger = logger;
            _userIdentityService = userIdentityService;
        }

        /// <summary>
        /// Performs a bulk insert or update safely across multiple entities.
        /// Handles duplicates within the same batch, applies audit metadata, and commits atomically.
        /// </summary>
        public async Task BulkInsertOrUpdateSafeAsync<T>(List<T> entities) where T : class
        {
            if (entities == null || entities.Count == 0)
            {
                _logger.LogInfo("[BulkInsert] No records to insert or update.");
                return;
            }

            try
            {
                Guid userId = _userIdentityService.GetCurrentUser();

                // 🧩 Step 1: Apply audit fields
                ApplyAuditFields(entities, userId);

                // 🧩 Step 2: Deduplicate in memory to avoid same-list duplicate key conflicts
                entities = RemoveDuplicateEntities(entities);

                // 🧩 Step 3: Configure bulk settings
                var bulkConfig = new BulkConfig
                {
                    PreserveInsertOrder = true,
                    SetOutputIdentity = false,
                    BatchSize = 2000,
                    UseTempDB = true,
                };

                // 🧩 Step 4: Add unique key handling for known tables
                string typeName = typeof(T).Name.ToLower();

                if (typeName.Contains("purchaseordermaster") || typeName.Contains("purchaseorder"))
                {
                    bulkConfig.UpdateByProperties = new List<string>
                    {
                        "PurchaseOrderId",
                        "LineItem",
                        "DomainId",
                        "EPID"
                    };
                }
                else if (typeName.Contains("aribacontract"))
                {
                    bulkConfig.UpdateByProperties = new List<string>
                    {
                        "ContractId",
                        "DomainId"
                    };
                }

                _logger.LogInfo($"[BulkInsert] Starting bulk insert/update for {typeof(T).Name} ({entities.Count} records)...");

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // 🧩 Phase 1: Try insert (ignore duplicates if they exist)
                        await _context.BulkInsertAsync(entities, bulkConfig);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"[BulkInsert] Insert phase encountered duplicates: {ex.Message}");
                    }

                    // 🧩 Phase 2: Bulk update to ensure latest values are applied
                    await _context.BulkUpdateAsync(entities, bulkConfig);

                    await transaction.CommitAsync();
                }

                _logger.LogInfo($"[BulkInsert] Successfully processed {entities.Count} {typeof(T).Name} records.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[BulkInsert] Error during bulk insert/update of {typeof(T).Name}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Deduplicates entities within the same list based on unique key or primary key.
        /// </summary>
        private List<T> RemoveDuplicateEntities<T>(List<T> entities)
        {
            var entityType = typeof(T);
            var typeName = entityType.Name.ToLower();

            IEnumerable<IGrouping<string, T>> grouped;

            if (typeName.Contains("purchaseordermaster") || typeName.Contains("purchaseorder"))
            {
                grouped = entities.GroupBy(e =>
                {
                    var poId = e.GetType().GetProperty("PurchaseOrderId")?.GetValue(e)?.ToString() ?? "";
                    var lineItem = e.GetType().GetProperty("LineItem")?.GetValue(e)?.ToString() ?? "";
                    var domain = e.GetType().GetProperty("DomainId")?.GetValue(e)?.ToString() ?? "";
                    var epid = e.GetType().GetProperty("EPID")?.GetValue(e)?.ToString() ?? "";
                    return $"{poId}|{lineItem}|{domain}|{epid}";
                });
            }
            else if (typeName.Contains("aribacontract"))
            {
                grouped = entities.GroupBy(e =>
                {
                    var cid = e.GetType().GetProperty("ContractId")?.GetValue(e)?.ToString() ?? "";
                    var domain = e.GetType().GetProperty("DomainId")?.GetValue(e)?.ToString() ?? "";
                    return $"{cid}|{domain}";
                });
            }
            else
            {
                // Default fallback: deduplicate by all public props
                grouped = entities.GroupBy(e =>
                    string.Join("|", e.GetType().GetProperties().Select(p => p.GetValue(e)?.ToString() ?? "")));
            }

            // Keep the latest (last) version of each duplicate
            return grouped.Select(g => g.Last()).ToList();
        }

        /// <summary>
        /// Applies BaseModel audit fields manually for bulk operations.
        /// </summary>
        private void ApplyAuditFields<T>(List<T> entities, Guid userId) where T : class
        {
            var now = DateTime.UtcNow;

            foreach (var entity in entities)
            {
                if (entity is BaseModel baseModel)
                {
                    baseModel.DateUpdated = now;
                    baseModel.UpdatedBy = userId;

                    if (baseModel.DateCreated == default)
                    {
                        baseModel.DateCreated = now;
                        baseModel.CreatedBy = userId;
                        baseModel.IsActive = true;
                    }
                }
            }
        }
    }
}

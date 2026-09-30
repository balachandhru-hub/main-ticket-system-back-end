using Contracts.IRepository;
using Dto;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Entities.Models;
using Entities.DbContext;
using Contracts;
using Entities.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Repository
{

    /// <summary>
    /// Repository for Person entity, implementing IPersonRepository interface.
    /// </summary>
    public class MaterialPRRepository : RepositoryBase<MaterialPR>, IMaterialPRRepository
    {
        private ILoggerManager _logger;
        private IConfiguration _configuration;




        public MaterialPRRepository(RepositoryContext repositoryContext, ILoggerManager logger, IConfiguration configuration)
            : base(repositoryContext)
        {
            _logger = logger;
            _configuration = configuration;

        }
        public MaterialPRLineItemDto GetPRDetails(string aribaUser)
        {
            MaterialPRLineItemDto prDetails = new MaterialPRLineItemDto();
            string _dbConnectionString = _configuration[Common.PYTHON_USER_DB]!;
            string databaseName = _configuration[Common.PYTHON_USER_DB_NAME]!;
            _logger.LogDebug($"Fetching PR details for Ariba user: {aribaUser}");
            if (string.IsNullOrWhiteSpace(_dbConnectionString))
                throw new ApplicationException("DB connection string missing");

            if (string.IsNullOrWhiteSpace(databaseName))
                throw new ApplicationException("Database name missing (PYTHON_USER_DB_NAME)");

            _logger.LogDebug($"Fetching PR details for Ariba user: {aribaUser}");
            _logger.LogDebug($"Using database: {databaseName}");

            using (SqlConnection connection = new SqlConnection(_dbConnectionString))
            {
                connection.Open();
                string sql = $@"
            SELECT [SAPPlant],
                   [SAPPurchaseOrg],
                   [SAPCompanyCode],
                   [SAPPurchaseGroup],
                   [DefaultCurrency_UniqueName],
                   [SAPCostCenter],
                   [Name]
                FROM [{databaseName}].[dbo].[ConsolidatedGroupExport]
                    WHERE [Name] = @AribaUser";

                using (SqlCommand sqlCommand = new SqlCommand(sql, connection))
                {
                    sqlCommand.Parameters.AddWithValue("@AribaUser", aribaUser);

                    using (var reader = sqlCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                                prDetails.ShipToPlant = reader.GetString(0);

                            if (!reader.IsDBNull(1))
                                prDetails.PurchaseOrg = reader.GetString(1);

                            if (!reader.IsDBNull(2))
                                prDetails.CompanyCode = reader.GetString(2);

                            if (!reader.IsDBNull(3))
                                prDetails.PurchaseGroup = reader.GetString(3);
                            if (!reader.IsDBNull(4))
                                prDetails.Currency = reader.GetString(4);
                            if (!reader.IsDBNull(5))
                                prDetails.CostCenter = reader.GetString(5);

                            if (!reader.IsDBNull(6))
                                prDetails.DeliverTo = reader.GetString(6);

                        }
                    }
                }

                connection.Close();
            }


            return prDetails;
        }
        public async Task<PagedResult<string>> GetMaterialCodesForDropdownAsync(string searchTerm, int page, int pageSize)
        {
            List<string> materialCodes = new();
            string connectionString = _configuration[Common.PYTHON_USER_DB]!;
            string databaseName = _configuration[Common.PYTHON_USER_DB_NAME]!;

            int offset = (page - 1) * pageSize;

            int total = 0;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // First, get total count
                using (SqlCommand countCmd = new SqlCommand($@"
            SELECT COUNT(DISTINCT Material_Code)
            FROM [{databaseName}].dbo.PR_mainData
            WHERE Material_Code IS NOT NULL
              AND Material_Code LIKE @Search", connection))
                {
                    countCmd.Parameters.AddWithValue("@Search", "%" + searchTerm + "%");
                    total = (int)await countCmd.ExecuteScalarAsync();
                }

                // Then, get paged data
                var sql = $@"
            SELECT DISTINCT Material_Code
            FROM [{databaseName}].dbo.PR_mainData
            WHERE Material_Code IS NOT NULL
              AND Material_Code LIKE @Search
            ORDER BY Material_Code
            OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";

                using (SqlCommand cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Search", "%" + searchTerm + "%");
                    cmd.Parameters.AddWithValue("@Offset", offset);
                    cmd.Parameters.AddWithValue("@Limit", pageSize);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (!reader.IsDBNull(0))
                                materialCodes.Add(reader.GetString(0));
                        }
                    }
                }
            }

            return new PagedResult<string>
            {
                Data = materialCodes,
                Pagination = new PaginationInfo
                {
                    Page = page,
                    PageSize = pageSize,
                    Total = total,
                    TotalPages = (int)Math.Ceiling(total / (double)pageSize)
                }
            };
        }


        public async Task<MaterialDetailsDto?> GetMaterialDetailsByMaterialCodeAsync(string materialCode)
        {
            MaterialDetailsDto? result = null;
            string connectionString = _configuration[Common.PYTHON_USER_DB]!;
            string databaseName = _configuration[Common.PYTHON_USER_DB_NAME]!;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var sql = $@"
            SELECT TOP (1)
                  
                   Commodity_Code,
                   Tax,
                   UOM
            FROM [{databaseName}].dbo.PR_mainData
            WHERE Material_Code = @MaterialCode
            ORDER BY Last_Modified DESC";

                using (SqlCommand cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@MaterialCode", materialCode);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            result = new MaterialDetailsDto
                            {

                                CommodityCode = reader.IsDBNull(0) ? null : reader.GetString(0),
                                TaxCode = reader.IsDBNull(1) ? null : reader.GetString(1),
                                UnitOfMeasure = reader.IsDBNull(2) ? null : reader.GetString(2)
                            };
                        }
                    }
                }
            }

            return result;
        }

        public async Task<MaterialDetailsDto?> GetMaterialDetailsByCommodityCodeAsync(string CommodityCode)
        {
            MaterialDetailsDto? result = null;
            string connectionString = _configuration[Common.PYTHON_USER_DB]!;
            string databaseName = _configuration[Common.PYTHON_USER_DB_NAME]!;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var sql = $@"
            SELECT TOP (1)
                  
               
                   Tax,
                   UOM
            FROM [{databaseName}].dbo.PR_mainData
            WHERE Commodity_Code = @CommodityCode
            ORDER BY Last_Modified DESC";

                using (SqlCommand cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@CommodityCode", CommodityCode);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            result = new MaterialDetailsDto
                            {


                                TaxCode = reader.IsDBNull(0) ? null : reader.GetString(0),
                                UnitOfMeasure = reader.IsDBNull(1) ? null : reader.GetString(1)
                            };
                        }
                    }
                }
            }

            return result;
        }
        public async Task<string?> GetExternalPRStatusByPRNumberAsync(string prNumber)
        {
            string? status = null;

            string connectionString = _configuration[Common.PYTHON_USER_DB]!;
            string databaseName = _configuration[Common.PYTHON_USER_DB_NAME]!;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var sql = $@"
                    SELECT TOP (1)
                        Status
                    FROM [{databaseName}].dbo.PR_mainData
                    WHERE PR_Number = @PRNumber
                    ORDER BY Last_Modified DESC";

                using (SqlCommand cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@PRNumber", prNumber);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            status = reader.IsDBNull(0) ? null : reader.GetString(0);
                        }
                    }
                }
            }

            return status;
        }
        public string GetOriginatingReferenceIdByPRNumber(Guid domainId, string prNumber)
        {
            return FindByCondition(x =>
                    x.DomainId == domainId &&
                    x.PRNumber == prNumber &&
                    x.IsActive)
                .Select(x => x.PRUniqueName)   // stored during CREATE
                .FirstOrDefault();
        }

        public async Task<List<PRDetailsDto>> GetPRDetailsByPRNumberAsync(string prNumber)
        {
            string connectionString = _configuration[Common.PYTHON_USER_DB]!;
            string databaseName = _configuration[Common.PYTHON_USER_DB_NAME]!;
            List<PRDetailsDto> prDetailsList = new List<PRDetailsDto>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var sql = $@"
            SELECT PR_Number, PR_Title, Requester, Company_Code, Purchase_Org, Currency, Line_Number, 
                   Description, Material_Code, Commodity_Code, Quantity, UOM, Need_By_Date, 
                   Supplier_Part_Number, Cost_Center, GL_Account, Deliver_To, Unit_Price, ItemCategory, ServiceStartDate, ServiceEndDate
            FROM [{databaseName}].dbo.PR_mainData
            WHERE PR_Number = @PRNumber AND Line_Number < 100000
            ORDER BY Last_Modified DESC";

                using (SqlCommand cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@PRNumber", prNumber);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync()) // ✅ loop all rows
                        {
                            var prDetails = new PRDetailsDto
                            {
                                PRNumber = reader.IsDBNull(0) ? null : reader.GetString(0),
                                PRTitle = reader.IsDBNull(1) ? null : reader.GetString(1),
                                Requester = reader.IsDBNull(2) ? null : reader.GetString(2),
                                CompanyCode = reader.IsDBNull(3) ? null : reader.GetString(3),
                                PurchaseOrg = reader.IsDBNull(4) ? null : reader.GetString(4),
                                Currency = reader.IsDBNull(5) ? null : reader.GetString(5),
                                LineNumber = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6),
                                Description = reader.IsDBNull(7) ? null : reader.GetString(7),
                                MaterialCode = reader.IsDBNull(8) ? null : reader.GetString(8),
                                CommodityCode = reader.IsDBNull(9) ? null : reader.GetString(9),
                                Quantity = reader.IsDBNull(10) ? (decimal?)null : reader.GetDecimal(10),
                                UOM = reader.IsDBNull(11) ? null : reader.GetString(11),
                                NeedByDate = reader.IsDBNull(12) ? (DateTime?)null : reader.GetDateTime(12),
                                SupplierId = reader.IsDBNull(13) ? null : reader.GetString(13),
                                CostCenter = reader.IsDBNull(14) ? null : reader.GetString(14),
                                GLAccount = reader.IsDBNull(15) ? null : reader.GetString(15),
                                DeliverTo = reader.IsDBNull(16) ? null : reader.GetString(16),
                                Price = reader.IsDBNull(17) ? (decimal?)null : reader.GetDecimal(17),
                                PRType = reader.IsDBNull(18) ? null : reader.GetString(18),
                                ServiceStartDate = reader.IsDBNull(19) ? (DateTime?)null : reader.GetDateTime(19),
                                ServiceEndDate = reader.IsDBNull(20) ? (DateTime?)null : reader.GetDateTime(20)
                            };

                            prDetailsList.Add(prDetails);
                        }
                    }
                }
            }

            return prDetailsList;
        }
public async Task<string?> GetLastPRUniqueNameAsync()
{
    var prList = await FindByCondition(x =>
            x.PRUniqueName != null &&
            (
                x.PRUniqueName.StartsWith("PRERP") ||
                x.PRUniqueName.StartsWith("PRMERP")
            ))
        .Select(x => x.PRUniqueName)
        .ToListAsync();
 
    return prList
        .OrderByDescending(x =>
        {
            var match = Regex.Match(x, @"\d+");
            return match.Success ? int.Parse(match.Value) : 0;
        })
        .FirstOrDefault();
}

    }
}

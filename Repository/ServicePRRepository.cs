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
    public class ServicePRRepository : RepositoryBase<ServicePR>, IServicePRRepository
    {
        private ILoggerManager _logger;
        private IConfiguration _configuration;




        public ServicePRRepository(RepositoryContext repositoryContext, ILoggerManager logger, IConfiguration configuration)
            : base(repositoryContext)
        {
            _logger = logger;
            _configuration = configuration;

        }
        public ServicePRLineItemDto GetServicePRDetails(string aribaUser)
        {
            ServicePRLineItemDto prDetails = new ServicePRLineItemDto();
            string _dbConnectionString = _configuration[Common.PYTHON_USER_DB]!;
            string databaseName = _configuration[Common.PYTHON_USER_DB_NAME]!;
            _logger.LogDebug($"Fetching Service  PR details for Ariba user: {aribaUser}");
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

         public string GetOriginatingReferenceIdByPRNumber(Guid domainId, string prNumber)
        {
            return FindByCondition(x =>
                    x.DomainId == domainId &&
                    x.PRNumber == prNumber &&
                    x.IsActive)
                .Select(x => x.PRServiceUniqueName)   // stored during CREATE
                .FirstOrDefault();
        }
         public async Task<string?> GetExternalPRStatusByPRNumberAsync(string? prNumber)
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
      public async Task<string?> GetLastPRUniqueNameAsync()
{
    var prList = await FindByCondition(x =>
            x.PRServiceUniqueName != null &&
            (
                x.PRServiceUniqueName.StartsWith("PRERP") ||
                x.PRServiceUniqueName.StartsWith("PRSERP")
            ))
        .Select(x => x.PRServiceUniqueName)
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
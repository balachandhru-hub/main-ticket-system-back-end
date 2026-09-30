using Contracts.IRepository;
using Entities.DbContext;
using Dto;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Entities.Models;
using Contracts;
using Entities.Common;
using Microsoft.Data.SqlClient;
namespace Repository
{
    public class AribaPRRepository : RepositoryBase<AribaPR>, IAribaPRRepository
    {
        private readonly RepositoryContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILoggerManager _logger;


        public AribaPRRepository(RepositoryContext context, ILoggerManager logger, IConfiguration configuration) : base(context)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        public PRDataDto GetCompanyTaxDetailByRequisition(string requisitionId)
        {
            var result = new PRDataDto();

            string _dbConnectionString = _configuration[Common.PYTHON_USER_DB]!;
            string databaseName = _configuration[Common.PYTHON_USER_DB_NAME]!;

            _logger.LogDebug($"Fetching CompanyCode, TaxAmount, ContractId for Requisition: {requisitionId}");

            using (SqlConnection connection = new SqlConnection(_dbConnectionString))
            {
                connection.Open();

                var sql = $@"
            SELECT [Company_Code], [Tax_Amount], [contract_id]
            FROM [{databaseName}].[dbo].[PR_mainData]
            WHERE [PR_Number] = @RequisitionId";

                using (SqlCommand sqlCommand = new SqlCommand(sql, connection))
                {
                    sqlCommand.Parameters.AddWithValue("@RequisitionId", requisitionId);

                    using (var reader = sqlCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                                result.CompanyCode = reader.GetString(0);

                            if (!reader.IsDBNull(1))
                                result.TaxAmount = Convert.ToDouble(reader.GetDecimal(1));

                            if (!reader.IsDBNull(2))
                                result.ContractId = reader.GetString(2);
                        }
                    }
                }
            }

            _logger.LogDebug($"Fetched CompanyCode: {result.CompanyCode}, TaxAmount: {result.TaxAmount}, ContractId: {result.ContractId}");

            return result;
        }

        public string GetDepartmentDetail(string requesterName)
        {
            string department = string.Empty;
            string _dbConnectionString = _configuration[Common.PYTHON_USER_DB]!;
            string databaseName = _configuration[Common.PYTHON_USER_DB_NAME]!;

            _logger.LogDebug($"Fetching department for requester: {requesterName} from ConsolidatedGroupExport table.");

            using (SqlConnection connection = new SqlConnection(_dbConnectionString))
            {
                connection.Open();

               string sql = $@"
            SELECT TOP 1 [cus_Department]
            FROM [{databaseName}].[dbo].[ConsolidatedGroupExport]
            WHERE [Name] = @RequesterName";

                using (SqlCommand cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@RequesterName", requesterName);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read() && !reader.IsDBNull(0))
                        {
                            department = reader.GetString(0);
                        }
                    }
                }

                connection.Close();
            }

            _logger.LogDebug($"Fetched department for requester '{requesterName}': {department}");
            return department;
        }


    }
}

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

namespace Repository
{
    /// <summary>
    /// Repository for MaterialInvoice entity, implementing IMaterialInvoiceRepository interface.
    /// Handles header-level invoice storage and queries.
    /// </summary>
    public class MaterialInvoiceRepository : RepositoryBase<MaterialInvoice>, IMaterialInvoiceRepository
    {
        private readonly ILoggerManager _logger;
        private IConfiguration _configuration;

        public MaterialInvoiceRepository(RepositoryContext repositoryContext, ILoggerManager logger,IConfiguration configuration)
            : base(repositoryContext)
        {
            _logger = logger;
            _configuration = configuration;
        }
         public async Task<ExtractedInvoiceDto?> GetExtractedInvoiceByPRAsync(
    string prNumber,
    string invoiceId,
    int? lineItemNumber = null) 
{
    ExtractedInvoiceDto? result = null;

    string connectionString = _configuration[Common.PYTHON_USER_DB]!;
    string databaseName = _configuration[Common.PYTHON_USER_DB_NAME]!;

    using (SqlConnection connection = new SqlConnection(connectionString))
    {
        await connection.OpenAsync();

        // Base SQL query
        var sql = $@"
            SELECT TOP (1)
                extracted_invoice_number
            FROM [{databaseName}].dbo.auto_invoice
            WHERE pr_id = @PrNumber
            AND invoice_id = @InvoiceId";

        // If line item number is provided, add it to WHERE
        if (lineItemNumber.HasValue)
        {
            sql += " AND Pr_line_item = @LineItemNumber";
        }

        sql += " ORDER BY updated_at DESC";

        using (SqlCommand cmd = new SqlCommand(sql, connection))
        {
            cmd.Parameters.AddWithValue("@PrNumber", prNumber);
            cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);

            if (lineItemNumber.HasValue)
            {
                cmd.Parameters.AddWithValue("@LineItemNumber", lineItemNumber.Value);
            }

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    result = new ExtractedInvoiceDto
                    {
                        ExtractedInvoiceNumber = reader.IsDBNull(0)
                            ? null
                            : reader.GetString(0)
                    };
                }
            }
        }
    }

    return result;
}


 public async Task<ExtractedSSIDDto?> GetExtractedSSIDByPRAsync(
    string ponumber,
    int? lineItemNumber) 
{
    ExtractedSSIDDto? result = null;

    string connectionString = _configuration[Common.PYTHON_USER_DB]!;
    string databaseName = _configuration[Common.PYTHON_USER_DB_NAME]!;

    using (SqlConnection connection = new SqlConnection(connectionString))
    {
        await connection.OpenAsync();

        // Base SQL query
        var sql = $@"
            SELECT TOP (1)
                generated_service_sheet_number
            FROM [{databaseName}].dbo.auto_ses
            WHERE po_number = @PONumber
            AND line_item = @LineItemNumber";

      


        using (SqlCommand cmd = new SqlCommand(sql, connection))
        {
            cmd.Parameters.AddWithValue("@PONumber", ponumber);
            cmd.Parameters.AddWithValue("@LineItemNumber", lineItemNumber);

         

            using (var reader = await cmd.ExecuteReaderAsync())
            {                if (await reader.ReadAsync())
                {
                    result = new ExtractedSSIDDto
                    {
                        ExtractedSSID = reader.IsDBNull(0)
                            ? null
                            : reader.GetString(0)
                    };
                }
            }
        }
    }

    return result;
}

   public async Task<ExtractedInvoiceDto?> GetExtractedInvoiceByServicePRAsync(
    string prNumber,
    string invoiceId,
    int? lineItemNumber = null) 
{
    ExtractedInvoiceDto? result = null;

    string connectionString = _configuration[Common.PYTHON_USER_DB]!;
    string databaseName = _configuration[Common.PYTHON_USER_DB_NAME]!;

    using (SqlConnection connection = new SqlConnection(connectionString))
    {
        await connection.OpenAsync();

        // Base SQL query
        var sql = $@"
            SELECT TOP (1)
                extracted_invoice_number
            FROM [{databaseName}].dbo.invoice
            WHERE pr_id = @PrNumber
            AND invoice_id = @InvoiceId";

        // If line item number is provided, add it to WHERE
        if (lineItemNumber.HasValue)
        {
            sql += " AND Pr_line_item = @LineItemNumber";
        }

        sql += " ORDER BY updated_at DESC";

        using (SqlCommand cmd = new SqlCommand(sql, connection))
        {
            cmd.Parameters.AddWithValue("@PrNumber", prNumber);
            cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);

            if (lineItemNumber.HasValue)
            {
                cmd.Parameters.AddWithValue("@LineItemNumber", lineItemNumber.Value);
            }

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    result = new ExtractedInvoiceDto
                    {
                        ExtractedInvoiceNumber = reader.IsDBNull(0)
                            ? null
                            : reader.GetString(0)
                    };
                }
            }
        }
    }

    return result;
}

    
    }
}

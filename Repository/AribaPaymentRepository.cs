using Contracts;
using Contracts.IRepository;
using Dto;
using Entities.Common;
using Entities.DbContext;
using Entities.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    /// <summary>
    /// Class <c>AribaPaymentRepository</c> used to implement the methods related for Ariba Payment.
    /// </summary>
    public class AribaPaymentRepository : RepositoryBase<AribaPayment>, IAribaPaymentRepository
    {
        private readonly ILoggerManager _logger;
        private readonly IConfiguration _config;
        /// <summary>
        /// Constructor for injecting DbContext and ILogger
        /// </summary>
        /// <param name="repositoryContext"></param>
        /// <returns></returns>
        public AribaPaymentRepository(RepositoryContext repositoryContext, ILoggerManager logger, IConfiguration config) : base(repositoryContext)
        {
            _logger = logger;
            _config = config;
        }
        /// <summary>
        /// GetAllAttachmentsByInvoiceId
        /// </summary>
        /// <param name="invoiceId"></param>
        /// <returns></returns>
        public SplitAttachmentResponseDto GetAllAttachmentsByInvoiceId(string invoiceId)
        {
            _logger.LogInfo($"GetAllAttachmentsByInvoiceId for invoiceId: {invoiceId}");
            string dbString = _config.GetConnectionString("DefaultConnection")!;
            string databaseName = _config[Common.TICKET_SYSTEM_DB_NAME]!;
            string schema = _config[Common.APPLICATION_SCHEMA]!;
            SplitAttachmentResponseDto result = new SplitAttachmentResponseDto();
            using (SqlConnection connection = new SqlConnection(dbString))
            {
                connection.Open();
                string sql = $@"
                    SELECT fa.attachment, fa.type FROM [{databaseName}].[{schema}].final_attachment fa
                    WHERE fa.ariba_id = @InvoiceId AND fa.is_active = 1

                    UNION ALL

                    SELECT pra.attachment, pra.type
                    FROM [{databaseName}].[{schema}].final_prattachment pra
                    WHERE pra.ariba_id = (
                        SELECT ap.prnumber
                        FROM [{databaseName}].[{schema}].ariba_payment ap
                        WHERE ap.invoice_id = @InvoiceId AND ap.is_active = 1)
                    AND pra.is_active = 1";
                SqlCommand sqlCommand = new SqlCommand(sql, connection);
                sqlCommand.Parameters.AddWithValue("@InvoiceId", invoiceId);
                sqlCommand.CommandTimeout = 180;

                using (var reader = sqlCommand.ExecuteReader(CommandBehavior.SequentialAccess))
                {
                    while (reader.Read())
                    {
                        if (reader.IsDBNull(0))
                            continue;

                        using MemoryStream ms = new();
                        long offset = 0;
                        const int chunkSize = 8192;
                        byte[] buffer = new byte[chunkSize];

                        long bytesRead;
                        while ((bytesRead = reader.GetBytes(0, offset, buffer, 0, buffer.Length)) > 0)
                        {
                            ms.Write(buffer, 0, (int)bytesRead);
                            offset += bytesRead;
                        }

                        string base64 = Convert.ToBase64String(ms.ToArray());
                        string type = reader.GetString(1);

                        // ✅ SEGREGATION LOGIC (FINAL)
                        if (type.Equals("ARIBA_INVOICE", StringComparison.OrdinalIgnoreCase))
                        {
                            result.InvoiceAttachments.Add(base64);
                        }
                        else if (type.Equals("ARIBA_CONTRACT", StringComparison.OrdinalIgnoreCase))
                        {
                            result.ContractAttachments.Add(base64);
                        }
                        else if (type.Equals("ARIBA_PR", StringComparison.OrdinalIgnoreCase))
                        {
                            result.PRAttachments.Add(base64);
                        }

                    }
                }
                connection.Close();
            }
            _logger.LogInfo($"Fetched Attachments for invoiceId: {invoiceId}");
            return result;
        }
    }
}

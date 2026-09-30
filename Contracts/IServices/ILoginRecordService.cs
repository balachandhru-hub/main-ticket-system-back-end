using Entities.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dto;
namespace Contracts.IServices
{
    public interface ILoginRecordService
    {
         Task<ExportExcelResultDto> ExportLoginRecordsExcelAsync(int limit);
    }
}

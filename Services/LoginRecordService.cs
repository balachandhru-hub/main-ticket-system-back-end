using System;
using System.Threading.Tasks;
using Contracts.IServices;
using Entities.DbContext;
using Entities.Models;
using Contracts.IRepository;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Entities.Common;
using Dto;
namespace Services
{
    public class LoginRecordService : ILoginRecordService
    {
        private readonly IRepositoryWrapper _repoWrapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoginRecordService(IRepositoryWrapper repo, IHttpContextAccessor httpContextAccessor)
        {
            _repoWrapper = repo;
            _httpContextAccessor = httpContextAccessor;
        }

   public async Task<ExportExcelResultDto> ExportLoginRecordsExcelAsync(int limit)
{
   

    var loginRecords = _repoWrapper.LoginRecord
        .FindByCondition(x => x.IsActive)
        .OrderByDescending(x => x.LoginTime)
        .Take(limit)
        .ToList();

    using var workbook = new XLWorkbook();
    var ws = workbook.Worksheets.Add("Login Records");

    if (!loginRecords.Any())
    {
        ws.Cell(1, 1).Value = "No Records Found";

        using var emptyStream = new MemoryStream();
        workbook.SaveAs(emptyStream);

        return new ExportExcelResultDto
        {
            FileContent = emptyStream.ToArray(),
            FileName = $"LoginRecords_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
        };
    }

    // Prepare export model converting UserId -> UserName
    var exportData = new List<object>();

    foreach (var item in loginRecords)
    {
        var user = await _repoWrapper.User.FindFirstByConditionAsync(u => u.Id == item.UserId);

        exportData.Add(new
        {
            UserName = user?.UserName ,
            item.DomainId,
            item.EntityType,
            item.AttributeId,
            item.IpAddress,
            LoginTime = item.LoginTime
        });
    }

    ws.Cell(1, 1).InsertTable(exportData);
    ws.Columns().AdjustToContents();

    using var stream = new MemoryStream();
    workbook.SaveAs(stream);

    return new ExportExcelResultDto
    {
        FileContent = stream.ToArray(),
        FileName = $"LoginRecords_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
    };
    }
}
}
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dto;


namespace Contracts.IRepository
{
    public interface IAribaPRRepository : IRepositoryBase<AribaPR>
    {
        PRDataDto GetCompanyTaxDetailByRequisition(string requisitionId);
        string GetDepartmentDetail(string requesterName);
    }
}
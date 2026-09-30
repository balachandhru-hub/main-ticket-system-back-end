using Entities.Models;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Contracts.IRepository
{
    public interface IAdditionalQueryRepository : IRepositoryBase<AdditionalQuery>
    {
    }
}

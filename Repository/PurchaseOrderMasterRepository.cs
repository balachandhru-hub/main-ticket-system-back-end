using Contracts;
using Contracts.IRepository;
using Dto;
using Entities;
using Entities.Common;
using Entities.DbContext;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class PurchaseOrderRepositoryMaster : RepositoryBase<PurchaseOrderMaster>, IPurchaseOrderMasterRepository
    {

        public PurchaseOrderRepositoryMaster(RepositoryContext repositoryContext)
            : base(repositoryContext)
        {

        }

    }
}

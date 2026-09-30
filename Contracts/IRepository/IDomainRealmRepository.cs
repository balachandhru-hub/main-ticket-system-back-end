using Contracts.IRepository;
using Dto;
using System;
using System.Collections.Generic;
using Entities.Models;


namespace Contracts.IRepository
{
    /// <summary>
    /// Interface for Person repository, extending IRepositoryBase.
    /// </summary>
    public interface IDomainRealmRepository : IRepositoryBase<DomainRealm>
    {
       
    }
 
}
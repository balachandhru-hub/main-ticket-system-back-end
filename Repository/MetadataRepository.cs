using Entities.Models;
using Contracts.IRepository;
using Entities.DbContext;

namespace Repository
{
    /// <summary>
    /// Class <c>MetadataRepository</c> used to implement the methods related for RefTerm
    /// </summary>
    public class MetadataRepository : RepositoryBase<Metadata>, IMetadataRepository
    {
        public MetadataRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
        }
    }
}
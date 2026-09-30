using Contracts.IRepository;
using Entities.Models;
using Entities.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    /// <summary>
    /// Class <c>RoleRepository</c> used to implement the methods related for User.
    /// </summary>
    public class RequesterNameRepository : RepositoryBase<RequesterName>, IRequesterNameRepository
    {
        /// <summary>
        /// Constructor for injecting DbContext
        /// </summary>
        /// <param name="repositoryContext"></param>
        /// <returns></returns>
         private readonly RepositoryContext _context;
        public RequesterNameRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
            _context = repositoryContext;
 
        }
        public async Task<string?> GetRequesterByTypeAsync(string firstType, Guid userId)
{
    return await _context.RequesterName
        .Where(x => x.IsActive
                    && x.Department == firstType
                    && x.UserId == userId)
        .Select(x => x.Name) // or x.RequesterName based on your column
        .FirstOrDefaultAsync();
}

  public List<RequesterNameDto> GetRequesterNamesByUserId(Guid userId)
{
    return (from mapping in RepositoryContext.User
            join model in RepositoryContext.RequesterName
                on mapping.Id equals model.UserId
            where mapping.IsActive && model.IsActive && mapping.Id == userId
            select new RequesterNameDto
            {
              Name= model.Name,
                Department = model.Department
            })
           .ToList();
}
    }
}
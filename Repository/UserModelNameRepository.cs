using Contracts.IRepository;
using Entities.Models;
using Entities.DbContext;
using Dto;
 
namespace Repository
{
public class UserModelNameRepository : RepositoryBase<UserModelName>, IUserModelNameRepository
{
    public UserModelNameRepository(RepositoryContext repositoryContext)
        : base(repositoryContext)
    {
    }
      /// <summary>
        /// Get all distinct model names
        /// </summary>
        /// <returns>List of model names</returns>
        public List<UserModelNameDto> GetAllModelNames()
{
    return FindByCondition(x => x.IsActive)
        .Select(x => new UserModelNameDto
        {
            Id = x.Id,
            ModelName = x.ModelName
        })
        .OrderBy(x => x.ModelName)
        .ToList();
}


  public List<UserModelNameDto> GetModelNamesByUserId(Guid userId)
{
    return (from mapping in RepositoryContext.UserModelMapping
            join model in RepositoryContext.UserModelName
                on mapping.ModelId equals model.Id
            where mapping.IsActive && model.IsActive && mapping.UserId == userId
            select new UserModelNameDto
            {
                Id = model.Id,
                ModelName = model.ModelName
            })
           .ToList();
}
 public List<UserAppNameDto> GetAppNamesByUserId(Guid userId)
        {
            return (from mapping in RepositoryContext.AppUserMapping
                    join App in RepositoryContext.UserAppMapping
                        on mapping.AppId equals App.Id
                    where mapping.IsActive && App.IsActive && mapping.UserId == userId
                    select new UserAppNameDto
                    {
                        Id = App.Id,
                        Key = App.Key,
                        Name = App.Name,
                        Description = App.Description,
                        Link = App.Link
                    })
                   .ToList();
        }


}
}
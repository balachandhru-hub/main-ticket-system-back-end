using Contracts.IRepository;
using Entities.Models;
using Entities.DbContext;
using Dto;
using Contracts;
using Entities.Common;
using NetTopologySuite.Utilities;

namespace Repository
{
    /// <summary>
    /// Repository for Ticket entity, implementing ITicketRepository interface.
    /// </summary>
    public class TicketRepository : RepositoryBase<Ticket>, ITicketRepository
    {
        private readonly RepositoryContext _repositoryContext;
        private readonly ILoggerManager _logger;
        public TicketRepository(RepositoryContext repositoryContext, ILoggerManager logger)
            : base(repositoryContext)
        {
            _repositoryContext = repositoryContext;
            _logger = logger;
        }

        public TicketDetailDto? GetTicketById(Guid id)
        {
            _logger.LogInfo($"Fetching ticket with ID: {id}");
            return (
         from ticket in RepositoryContext.Ticket
        .Where(x => x.IsActive && x.Id == id)

         join user in RepositoryContext.User
             on ticket.UserId equals user.Id into userJoin
         from user in userJoin.DefaultIfEmpty()

         join person in RepositoryContext.Person
             on user.PersonId equals person.Id into personJoin
         from person in personJoin.DefaultIfEmpty()

         let asset = RepositoryContext.Asset
             .Where(a => a.IsActive &&
                         a.AssetType == Common.UPLOADED_FILE_ID &&
                         a.EntityId == ticket.Id)
             .OrderBy(a => a.DateCreated)
             .ToList()
       
         select new TicketDetailDto
         {
             Id = ticket.Id,
             UserId = ticket.UserId,
             Title = ticket.Title,
             Description = ticket.Description,
             Status = ticket.Status,
             Priority = ticket.Priority,
             Category = ticket.Category,
             AssignedTo = ticket.AssignedTo,
             AssignedBy = ticket.AssignedBy,
             DateCreated = ticket.DateCreated,
             AdminDescription = ticket.AdminDescription,
             Name = person != null ? person.Name : null,
             TotalHours = ticket.TotalHours,
             TicketCode = ticket.TicketCode,
             Asset = asset.Select(a => new AssetDto
             {
                 Id = a.Id,
                 AssetName = a.AssetName,
                 FileName = a.FileName,
                 AssetType = RepositoryContext.Metadata
                    .Where(m => m.IsActive && m.Id == a.AssetType)
                    .Select(m => m.Key)
                    .FirstOrDefault(),
                 FileType = RepositoryContext.Metadata
                    .Where(m => m.IsActive && m.Id == a.FileType)
                    .Select(m => m.Key)
                    .FirstOrDefault()
             }).ToList(),
             PRDetail = RepositoryContext.TicketPR
                .Where(p => p.IsActive && p.TicketId == ticket.Id)
                .Select(x => new TicketPRDetailDto
                {
                    Id = x.Id,
                    TaskName = x.TaskName,
                    Hrs = x.Hrs
                })
                .ToList()
         }
    ).FirstOrDefault();
        }
    }
}
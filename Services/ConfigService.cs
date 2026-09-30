using Dto;
using Contracts.IRepository;
using Contracts.IServices;
using Contracts;
using Entities.Models; // or the correct namespace where ILoggerManager is defined


public class PlanConfigService : IPlanConfigService
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerManager _logger;

    public PlanConfigService(IRepositoryWrapper repositoryWrapper, ILoggerManager logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _logger = logger;
    }

    public PlanConfigDto? GetPlanConfig(string plan, string engagementType)
    {
        _logger.LogInfo($"Start: Fetching plan config for plan: {plan}, engagementType: {engagementType}");
        var configs = _repositoryWrapper.PlanConfig.GetByPlanAndEngagementType(plan, engagementType);

        if (configs == null || !configs.Any())
        {
            _logger.LogError($"No plan config found for plan: {plan}, engagementType: {engagementType}");
            throw new ExceptionHandler.NotFoundCustomException(
                "Plan config not found",
                $"No config found for plan: {plan}, engagementType: {engagementType}"
            );
        }

        var result = configs.FirstOrDefault();
        _logger.LogInfo($"End: Fetched plan config for plan: {plan}, engagementType: {engagementType}");
        return result;
    }
        /// <summary>
        /// getAllPlanConfig  
        /// </summary>
   


    public List<GetPlanConfigDto> GetAllPlanConfigs()
    {
        var query = _repositoryWrapper.PlanConfig
            .FindByCondition(pc => pc.IsActive)
            .Select(pc => new GetPlanConfigDto
            {
                Id = pc.Id,
                Plan = pc.Plan,
                EngagementType = pc.EngagementType,
                Key = pc.Key,
                Value = pc.Value
            });

        return query.ToList();
    }
      /// <summary>
      /// update PlanConfig
      /// </summary>





    public bool UpdatePlanConfig(UpdatePlanConfigDto dto)
    {
        _logger.LogInfo($"Start: Updating plan config with Id: {dto.Id}");

        var entity = _repositoryWrapper.PlanConfig.FindByCondition(x => x.Id == dto.Id && x.IsActive).FirstOrDefault();
        if (entity == null)
        {
            _logger.LogError($"PlanConfig not found for Id: {dto.Id}");
            throw new ExceptionHandler.NotFoundCustomException(
                "Plan config not found",
                $"No config found with Id: {dto.Id}"
            );
        }


        entity.Value = dto.Value;


        _repositoryWrapper.PlanConfig.Update(entity);
        _repositoryWrapper.Save();

        _logger.LogInfo($"End: Updated plan config with Id: {dto.Id}");
        return true;
    }

        
        
}

using Dto;

public interface IPlanConfigService
{
    PlanConfigDto? GetPlanConfig(string plan, string engagementType);
    public List<GetPlanConfigDto> GetAllPlanConfigs();
    bool UpdatePlanConfig(UpdatePlanConfigDto dto);
}

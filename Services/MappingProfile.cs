using AutoMapper;
using Dto;
using Entities.Models;

namespace Services
{
    /// <summary>
    /// Class for managing MappingProfile
    /// </summary>
    public class MappingProfile : Profile
    {
        /// <summary>
        /// 
        /// </summary>
        public MappingProfile()
        {
            _ = CreateMap<Metadata, RefTermDto>();
            _ = CreateMap<Role, RoleDetailDto>();
            _ = CreateMap<Domain, DomainDto>();
        }
    }
}

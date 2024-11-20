using AutoMapper;
using LaptopStore.Application.Requests.Identity;
using LaptopStore.Application.Responses.Identity;
using LaptopStore.Infrastructure.Models.Identity;

namespace LaptopStore.Infrastructure.Mappings
{
    public class RoleClaimProfile : Profile
    {
        public RoleClaimProfile()
        {
            CreateMap<RoleClaimResponse, RoleClaim>()
                .ForMember(nameof(RoleClaim.ClaimType), opt => opt.MapFrom(c => c.Type))
                .ForMember(nameof(RoleClaim.ClaimValue), opt => opt.MapFrom(c => c.Value))
                .ReverseMap();

            CreateMap<RoleClaimRequest, RoleClaim>()
                .ForMember(nameof(RoleClaim.ClaimType), opt => opt.MapFrom(c => c.Type))
                .ForMember(nameof(RoleClaim.ClaimValue), opt => opt.MapFrom(c => c.Value))
                .ReverseMap();
        }
    }
}
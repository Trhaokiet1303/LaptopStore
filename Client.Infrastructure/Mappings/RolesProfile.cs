using AutoMapper;
using LaptopStore.Application.Requests.Identity;
using LaptopStore.Application.Responses.Identity;

namespace LaptopStore.Client.Infrastructure.Mappings
{
    public class RoleProfile : Profile
    {
        public RoleProfile()
        {
            CreateMap<PermissionResponse, PermissionRequest>().ReverseMap();
            CreateMap<RoleClaimResponse, RoleClaimRequest>().ReverseMap();
        }
    }
}
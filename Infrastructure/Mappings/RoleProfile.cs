using AutoMapper;
using LaptopStore.Infrastructure.Models.Identity;
using LaptopStore.Application.Responses.Identity;

namespace LaptopStore.Infrastructure.Mappings
{
    public class RoleProfile : Profile
    {
        public RoleProfile()
        {
            CreateMap<RoleResponse, BlazorHeroRole>().ReverseMap();
        }
    }
}
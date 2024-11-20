using AutoMapper;
using LaptopStore.Infrastructure.Models.Identity;
using LaptopStore.Application.Responses.Identity;

namespace LaptopStore.Infrastructure.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserResponse, BlazorHeroUser>().ReverseMap();
        }
    }
}
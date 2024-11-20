using AutoMapper;
using LaptopStore.Application.Features.Brands.Commands.AddEdit;
using LaptopStore.Application.Features.Brands.Queries.GetAll;
using LaptopStore.Application.Features.Brands.Queries.GetById;
using LaptopStore.Domain.Entities.Catalog;

namespace LaptopStore.Application.Mappings
{
    public class BrandProfile : Profile
    {
        public BrandProfile()
        {
            CreateMap<AddEditBrandCommand, Brand>().ReverseMap();
            CreateMap<GetBrandByIdResponse, Brand>().ReverseMap();
            CreateMap<GetAllBrandsResponse, Brand>().ReverseMap();
        }
    }
}
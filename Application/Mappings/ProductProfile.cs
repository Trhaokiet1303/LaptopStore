using AutoMapper;
using LaptopStore.Application.Features.Products.Commands.AddEdit;
using LaptopStore.Application.Features.Products.Queries.GetProductById;
using LaptopStore.Domain.Entities.Catalog;

namespace LaptopStore.Application.Mappings
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<AddEditProductCommand, Product>().ReverseMap();

            CreateMap<Product, GetProductByIdResponse>();
        }
    }
}
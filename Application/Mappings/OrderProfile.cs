using AutoMapper;
using LaptopStore.Application.Features.Orders.Commands.AddEdit;
using LaptopStore.Application.Features.Orders.Queries.GetAll;
using LaptopStore.Application.Features.Orders.Queries.GetById;
using LaptopStore.Domain.Entities.Catalog;
using System.Linq;

namespace LaptopStore.Application.Mappings
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<AddEditOrderCommand, Order>().ReverseMap();

            CreateMap<GetOrderByIdResponse, AddEditOrderCommand>().ReverseMap();

            CreateMap<Order, GetAllOrdersResponse>()
                .ForMember(dest => dest.OrderItem, opt => opt.MapFrom(src => src.OrderItem));

            CreateMap<OrderItem, GetAllOrderItemsResponse>().ReverseMap();

            CreateMap<Order, GetOrderByIdResponse>()
                .ForMember(dest => dest.OrderItem, opt => opt.MapFrom(src => src.OrderItem));

            CreateMap<OrderItem, GetOrderItemByIdResponse>().ReverseMap();

        }
    }
}

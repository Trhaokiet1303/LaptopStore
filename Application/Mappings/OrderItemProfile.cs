using AutoMapper;
using LaptopStore.Application.Features.OrderItems.Commands.AddEdit;
using LaptopStore.Application.Features.Orders.Queries.GetAll;
using LaptopStore.Application.Features.Orders.Queries.GetById;
using LaptopStore.Domain.Entities.Catalog;
using System.Linq;

namespace LaptopStore.Application.Mappings
{
    public class OrderItemProfile : Profile
    {
        public OrderItemProfile()
        {
            CreateMap<AddEditOrderItemCommand, OrderItem>().ReverseMap();

            CreateMap<OrderItem, GetAllOrderItemsResponse>().ReverseMap();

            CreateMap<OrderItem, GetOrderItemByIdResponse>().ReverseMap();

        }
    }
}

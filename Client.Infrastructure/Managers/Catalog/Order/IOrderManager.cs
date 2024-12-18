using LaptopStore.Application.Features.Orders.Queries.GetAll;
using LaptopStore.Shared.Wrapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using LaptopStore.Application.Features.Orders.Commands.AddEdit;
using LaptopStore.Domain.Entities.Catalog;
using LaptopStore.Application.Requests.Catalog;
using LaptopStore.Application.Features.Orders.Queries.GetById;
using LaptopStore.Application.Features.Orders.Commands.Update;

namespace LaptopStore.Client.Infrastructure.Managers.Catalog.Order
{
    public interface IOrderManager : IManager
    {
        Task<IResult<List<GetAllOrdersResponse>>> GetAllAsync();
        Task<IResult<List<GetAllOrdersResponse>>> GetAllForUserAsync();
        Task<IResult<GetOrderByIdResponse>> GetOrderByIdAsync(int orderId);
        Task<IResult<GetOrderByIdResponse>> GetOrderByIdForUserAsync(int orderId);
        Task<IResult> SaveAsync(AddEditOrderCommand request);
        Task<IResult<int>> DeleteAsync(int id);
        Task<IResult> CreateOrderAsync(Domain.Entities.Catalog.Order orderRequest);
                Task<IResult> UpdateOrderStatusAsync(UpdateOrderStatusCommand command);
        Task<IResult> UpdateOrderTotalPriceAsync(UpdateOrderTotalPriceCommand command);

    }
}
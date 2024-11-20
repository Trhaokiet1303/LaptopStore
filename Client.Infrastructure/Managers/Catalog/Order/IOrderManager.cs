using LaptopStore.Application.Features.Orders.Queries.GetAll;
using LaptopStore.Shared.Wrapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using LaptopStore.Application.Features.Orders.Commands.AddEdit;
using LaptopStore.Domain.Entities.Catalog;
using LaptopStore.Application.Requests.Catalog;
using LaptopStore.Application.Features.Orders.Queries.GetById;

namespace LaptopStore.Client.Infrastructure.Managers.Catalog.Order
{
    public interface IOrderManager : IManager
    {
        Task<IResult<List<GetAllOrdersResponse>>> GetAllAsync();
        Task<IResult<GetOrderByIdResponse>> GetOrderByIdAsync(int orderId);

        Task<IResult> SaveAsync(AddEditOrderCommand request);

        Task<IResult<int>> DeleteAsync(int id);

        Task<IResult> CreateOrderAsync(Domain.Entities.Catalog.Order orderRequest);
    }
}
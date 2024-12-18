using LaptopStore.Application.Features.OrderItems.Commands.AddEdit;
using LaptopStore.Application.Features.OrderItems.Commands.Update;
using LaptopStore.Application.Features.Orders.Queries.GetAll;
using LaptopStore.Application.Features.Orders.Queries.GetById;
using LaptopStore.Shared.Wrapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LaptopStore.Client.Infrastructure.Managers.Catalog.OrderItem
{
    public interface IOrderItemManager : IManager
    {
        Task<IResult<List<GetAllOrderItemsResponse>>> GetAllAsync();
        Task<IResult<List<GetAllOrderItemsResponse>>> GetAllForUserAsync();
        Task<IResult<GetOrderItemByIdResponse>> GetOrderItemByIdAsync(int orderItemId);
        Task<IResult<GetOrderItemByIdResponse>> GetOrderItemByIdForUserAsync(int orderItemId);
        Task<IResult<int>> SaveAsync(AddEditOrderItemCommand request);
        Task<IResult<int>> DeleteAsync(int id);
        Task<IResult> UpdateIsRatedAsync(UpdateIsRatedCommand command);

    }
}

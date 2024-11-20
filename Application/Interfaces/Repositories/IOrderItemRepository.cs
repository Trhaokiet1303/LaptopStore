using LaptopStore.Domain.Entities.Catalog;
using System.Threading.Tasks;

namespace LaptopStore.Application.Interfaces.Repositories
{
    public interface IOrderItemRepository
    {
        Task<Order> GetOrderItemByIdAsync(int orderId);

    }
}

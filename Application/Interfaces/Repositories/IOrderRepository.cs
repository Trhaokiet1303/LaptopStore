using LaptopStore.Domain.Entities.Catalog;
using System.Threading.Tasks;

namespace LaptopStore.Application.Interfaces.Repositories
{
    public interface IOrderRepository
    {
        Task<Order> GetOrderByIdAsync(int orderId);

    }
}

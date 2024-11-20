using LaptopStore.Application.Interfaces.Repositories;
using LaptopStore.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace LaptopStore.Infrastructure.Repositories
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly IRepositoryAsync<Order, int> _repository;

        public OrderItemRepository(IRepositoryAsync<Order, int> repository)
        {
            _repository = repository;
        }

        public async Task<Order> GetOrderItemByIdAsync(int orderId)
        {
            return await _repository.Entities
                .Include(o => o.OrderItem)
                .FirstOrDefaultAsync(p => p.Id == orderId);
        }
    }
}

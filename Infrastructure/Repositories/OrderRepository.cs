using LaptopStore.Application.Interfaces.Repositories;
using LaptopStore.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace LaptopStore.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IRepositoryAsync<Order, int> _repository;

        public OrderRepository(IRepositoryAsync<Order, int> repository)
        {
            _repository = repository;
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            return await _repository.Entities
                .Include(o => o.OrderItem)
                .FirstOrDefaultAsync(p => p.Id == orderId);
        }
    }
}

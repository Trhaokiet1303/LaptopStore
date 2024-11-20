using LaptopStore.Domain.Entities.Catalog;
using System.Threading.Tasks;

namespace LaptopStore.Application.Interfaces.Repositories
{
    public interface IProductRepository
    {
        Task<bool> IsBrandUsed(int brandId);
        Task<Product> GetProductByIdAsync(int productId); // Thêm phương thức này


    }
}
using LaptopStore.Application.Features.Products.Commands.AddEdit;
using LaptopStore.Application.Features.Products.Queries.GetAllPaged;
using LaptopStore.Application.Features.Products.Queries.GetProductById;
using LaptopStore.Application.Requests.Catalog;
using LaptopStore.Shared.Wrapper;
using System.Threading.Tasks;

namespace LaptopStore.Client.Infrastructure.Managers.Catalog.Product
{
    public interface IProductManager : IManager
    {
        Task<PaginatedResult<GetAllPagedProductsResponse>> GetProductsAsync(GetAllPagedProductsRequest request);
        Task<IResult<GetProductByIdResponse>> GetProductByIdAsync(int productId);

        Task<IResult<string>> GetProductImageAsync(int id);

        Task<IResult<int>> SaveAsync(AddEditProductCommand request);

        Task<IResult<int>> DeleteAsync(int id);

    }
}
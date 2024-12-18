using LaptopStore.Application.Features.Products.Commands.AddEdit;
using LaptopStore.Application.Features.Products.Queries.GetAllPaged;
using LaptopStore.Application.Features.Products.Queries.GetProductById;
using LaptopStore.Application.Requests.Catalog;
using LaptopStore.Client.Infrastructure.Extensions;
using LaptopStore.Shared.Wrapper;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace LaptopStore.Client.Infrastructure.Managers.Catalog.Product
{
    public class ProductManager : IProductManager
    {
        private readonly HttpClient _httpClient;

        public ProductManager(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IResult<GetProductByIdResponse>> GetProductByIdAsync(int productId)
        {
            var response = await _httpClient.GetAsync(Routes.ProductsEndpoints.GetProductById(productId));
            return await response.ToResult<GetProductByIdResponse>();
        }

        public async Task<IResult<int>> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{Routes.ProductsEndpoints.Delete}/{id}");
            return await response.ToResult<int>();
        }

        public async Task<IResult<string>> GetProductImageAsync(int id)
        {
            var response = await _httpClient.GetAsync(Routes.ProductsEndpoints.GetProductImage(id));
            return await response.ToResult<string>();
        }

        public async Task<PaginatedResult<GetAllPagedProductsResponse>> GetProductsAsync(GetAllPagedProductsRequest request)
        {
            var response = await _httpClient.GetAsync(
                Routes.ProductsEndpoints.GetAllPaged(
                    request.PageNumber,
                    request.PageSize,
                    request.SearchString,
                    request.Orderby
                )
            );
            return await response.ToPaginatedResult<GetAllPagedProductsResponse>();
        }

        public async Task<IResult> UpdateFeaturedStatusAsync(int productId)
        {
            var response = await _httpClient.PutAsJsonAsync(Routes.ProductsEndpoints.UpdateFeaturedStatus, new { ProductId = productId });
            return await response.ToResult();
        }

        public async Task<IResult<int>> SaveAsync(AddEditProductCommand request)
        {
            var response = await _httpClient.PostAsJsonAsync(Routes.ProductsEndpoints.Save, request);
            return await response.ToResult<int>();
        }
        public async Task<IResult<int>> UpdateRateAsync(int productId, decimal newRate)
        {
            var response = await _httpClient.PutAsJsonAsync(Routes.ProductsEndpoints.UpdateRate, new { ProductId = productId, NewRate = newRate });
            return await response.ToResult<int>();
        }
        public async Task<IResult<int>> UpdateProductQuantityAsync(int productId, int newQuantity)
        {
            var response = await _httpClient.PutAsJsonAsync(Routes.ProductsEndpoints.UpdateProductQuantity, new { ProductId = productId, NewQuantity = newQuantity });
            return await response.ToResult<int>();
        }
    }
}
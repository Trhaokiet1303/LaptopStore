using LaptopStore.Application.Features.Orders.Queries.GetAll;
using LaptopStore.Client.Infrastructure.Extensions;
using LaptopStore.Shared.Wrapper;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LaptopStore.Application.Features.Orders.Commands.AddEdit;
using LaptopStore.Application.Requests.Catalog;
using LaptopStore.Application.Features.Products.Queries.GetProductById;
using LaptopStore.Application.Features.Orders.Queries.GetById;

namespace LaptopStore.Client.Infrastructure.Managers.Catalog.Order
{
    public class OrderManager : IOrderManager
    {
        private readonly HttpClient _httpClient;

        public OrderManager(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IResult> CreateOrderAsync(Domain.Entities.Catalog.Order orderRequest)
        {
            var response = await _httpClient.PostAsJsonAsync(Routes.OrdersEndpoints.CreateOrder, orderRequest);
            return await response.ToResult<int>();
        }

        public async Task<IResult<GetOrderByIdResponse>> GetOrderByIdAsync(int orderId)
        {
            var response = await _httpClient.GetAsync(Routes.OrdersEndpoints.GetOrderById(orderId));
            return await response.ToResult<GetOrderByIdResponse>();
        }

        public async Task<IResult<int>> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{Routes.OrdersEndpoints.Delete}/{id}");
            return await response.ToResult<int>();
        }

        public async Task<IResult<List<GetAllOrdersResponse>>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync(Routes.OrdersEndpoints.GetAll);
            return await response.ToResult<List<GetAllOrdersResponse>>();
        }

        public async Task<IResult> SaveAsync(AddEditOrderCommand request)
        {
            var response = await _httpClient.PostAsJsonAsync(Routes.OrdersEndpoints.Save, request);
            return await response.ToResult();
        }
    }
}
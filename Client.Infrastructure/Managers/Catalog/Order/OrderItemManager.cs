using LaptopStore.Application.Features.Orders.Queries.GetAll;
using LaptopStore.Client.Infrastructure.Extensions;
using LaptopStore.Shared.Wrapper;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LaptopStore.Application.Features.OrderItems.Commands.AddEdit;
using LaptopStore.Application.Requests.Catalog;
using LaptopStore.Application.Features.Products.Queries.GetProductById;
using LaptopStore.Application.Features.Orders.Queries.GetById;

namespace LaptopStore.Client.Infrastructure.Managers.Catalog.OrderItem
{
    public class OrderItemManager : IOrderItemManager
    {
        private readonly HttpClient _httpClient;

        public OrderItemManager(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IResult<GetOrderItemByIdResponse>> GetOrderItemByIdAsync(int orderItemId)
        {
            var response = await _httpClient.GetAsync(Routes.OrderItemsEndpoints.GetOrderItemById(orderItemId));
            return await response.ToResult<GetOrderItemByIdResponse>();
        }

        public async Task<IResult<int>> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{Routes.OrderItemsEndpoints.Delete}/{id}");
            return await response.ToResult<int>();
        }

        public async Task<IResult<List<GetAllOrderItemsResponse>>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync(Routes.OrderItemsEndpoints.GetAll);
            return await response.ToResult<List<GetAllOrderItemsResponse>>();
        }

        //public async Task<IResult> SaveAsync(AddEditOrderItemCommand request)
        //{
        //    var response = await _httpClient.PostAsJsonAsync(Routes.OrderItemsEndpoints.Save, request);
        //    return await response.ToResult();
        //}
        public async Task<IResult<int>> SaveAsync(AddEditOrderItemCommand request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/orderitems/add-edit", request);
            return await response.ToResult<int>();
        }

    }
}
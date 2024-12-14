using LaptopStore.Domain.Contracts;
using LaptopStore.Domain.Entities.Catalog;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LaptopStore.Application.Features.Orders.Queries.GetAll
{
    public class GetAllOrdersResponse
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserPhone { get; set; }
        public string UserAddress { get; set; }
        public long TotalPrice { get; set; }
        public string MethodPayment { get; set; }
        public string StatusOrder { get; set; }
        public bool IsPayment { get; set; }
        public List<GetAllOrderItemsResponse> OrderItem { get; set; }

    }

    public class GetAllOrderItemsResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int ProductPrice { get; set; }
        public string ProductImage { get; set; }
        public int Quantity { get; set; }
        public long TotalPrice { get; set; }
        public int OrderId { get; set; }
        public int Instock { get; set; }
        public bool IsRated { get; set; }
        public decimal Rate { get; set; }

        [JsonIgnore]
        public Order Order { get; set; }

    }
}
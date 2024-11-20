using LaptopStore.Domain.Contracts;
using LaptopStore.Domain.Entities.Catalog;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LaptopStore.Application.Features.Orders.Queries.GetById
{
    public class GetOrderByIdResponse
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserPhone { get; set; }
        public string UserAddress { get; set; }
        public int TotalPrice { get; set; }
        public string MethodPayment { get; set; }
        public string StatusOrder { get; set; }
        public bool IsPayment { get; set; }
        public List<GetOrderItemByIdResponse> OrderItem { get; set; }

    }

    public class GetOrderItemByIdResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int ProductPrice { get; set; }
        public string ProductImage { get; set; }
        public int Quantity { get; set; }
        public int QuantityOrdered { get; set; }
        public int OrderId { get; set; }

        [JsonIgnore]
        public Order Order { get; set; }
    }
}
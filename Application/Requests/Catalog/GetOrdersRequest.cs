using LaptopStore.Domain.Contracts;
using System.Collections.Generic;

namespace LaptopStore.Application.Requests.Catalog
{
    public class GetOrdersRequest
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserPhone { get; set; }
        public string UserAddress { get; set; }
        public int TotalPrice { get; set; }
        public string MethodPayment { get; set; }
        public string StatusOrder { get; set; }
        public bool IsPayment { get; set; }
        public List<OrderItem> Items { get; set; }

    }

    public class OrderItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int ProductPrice { get; set; }
        public string ProductImage { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

}
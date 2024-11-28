using LaptopStore.Domain.Contracts;
using System.Collections.Generic;

namespace LaptopStore.Application.Requests.Catalog
{
    public class GetOrderItemsRequest
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int ProductPrice { get; set; }
        public string ProductImage { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int TotalPrice { get; set; }
    }

}
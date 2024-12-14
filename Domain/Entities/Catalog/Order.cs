using LaptopStore.Domain.Contracts;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;

namespace LaptopStore.Domain.Entities.Catalog
{
    public class Order : AuditableEntity<int>
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserPhone { get; set; }
        public string UserAddress { get; set; }
        public long TotalPrice { get; set; }
        public string MethodPayment { get; set; }
        public string StatusOrder { get; set; }
        public bool IsPayment { get; set; }
        public List<OrderItem> OrderItem { get; set; }
    
    }

    public class OrderItem : AuditableEntity<int>
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int ProductPrice { get; set; }
        public string ProductImage { get; set; }
        public int Quantity { get; set; }
        public long TotalPrice { get; set; }
        public bool IsRated { get; set; }
        public decimal Rate { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
    }
}

using LaptopStore.Application.Specifications.Base;
using LaptopStore.Domain.Entities.Catalog;

namespace LaptopStore.Application.Specifications.Catalog
{
    public class OrderItemFilterSpecification : HeroSpecification<OrderItem>
    {
        public OrderItemFilterSpecification(string searchString)
        {
            if (!string.IsNullOrEmpty(searchString))
            {
                Criteria = p => p.Id.ToString().Contains(searchString) || p.ProductId.ToString().Contains(searchString) ||
                p.ProductName.Contains(searchString) || p.ProductPrice.ToString().Contains(searchString) ||
                p.Quantity.ToString().Contains(searchString) || p.OrderId.ToString().Contains(searchString);
            }
            else
            {
                Criteria = p => true;
            }
        }
    }
}

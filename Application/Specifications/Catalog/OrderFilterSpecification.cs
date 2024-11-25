using LaptopStore.Application.Specifications.Base;
using LaptopStore.Domain.Entities.Catalog;

namespace LaptopStore.Application.Specifications.Catalog
{
    public class OrderFilterSpecification : HeroSpecification<Order>
    {
        public OrderFilterSpecification(string searchString)
        {
            if (!string.IsNullOrEmpty(searchString))
            {
                Criteria = p => p.UserId.Contains(searchString) || p.UserAddress != null && (p.UserAddress.Contains(searchString) ||
                p.UserName.Contains(searchString) || p.UserPhone.Contains(searchString) ||
                p.MethodPayment.Contains(searchString) || p.StatusOrder.Contains(searchString) ||
                p.IsPayment.ToString().Contains(searchString));
            }
            else
            {
                Criteria = p => true;
            }
        }
    }
}

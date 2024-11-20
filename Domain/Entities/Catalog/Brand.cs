using LaptopStore.Domain.Contracts;

namespace LaptopStore.Domain.Entities.Catalog
{
    public class Brand : AuditableEntity<int>
    {
        public string Name { get; set; }
    }
}
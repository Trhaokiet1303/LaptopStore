using LaptopStore.Application.Specifications.Catalog;
using System.Collections.Generic;

namespace LaptopStore.Application.Requests.Catalog
{
    public class GetAllPagedProductsRequest : PagedRequest
    {
        public string SearchString { get; set; }
        public List<string> DescriptionFilter { get; set; } = new();
        public List<string> BrandFilter { get; set; } = new();
        public List<string> PriceRangeFilter { get; set; } = new();
        public List<string> RateRangeFilter { get; set; } = new();
    }
}

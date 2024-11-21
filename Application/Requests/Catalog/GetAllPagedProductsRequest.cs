using System.Collections.Generic;

namespace LaptopStore.Application.Requests.Catalog
{
    public class GetAllPagedProductsRequest : PagedRequest
    {
        public string SearchString { get; set; }
        public string PriceRange { get; set; }
        public string RateRange { get; set; }
        public List<string> DescriptionFilter { get; set; }
        public List<string> BrandFilter { get; set; }
    }
}
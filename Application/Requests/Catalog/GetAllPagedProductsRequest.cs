using System.Collections.Generic;

namespace LaptopStore.Application.Requests.Catalog
{
    public class GetAllPagedProductsRequest : PagedRequest
    {
        public string SearchString { get; set; }
        public List<string> Descriptions { get; set; }
        public string PriceRange { get; set; }
        public List<string> Brands { get; set; }
        public string RateRange { get; set; }
    }
}
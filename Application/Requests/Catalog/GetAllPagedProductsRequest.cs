using System.Collections.Generic;

namespace LaptopStore.Application.Requests.Catalog
{
    public class GetAllPagedProductsRequest : PagedRequest
    {
        public string SearchString { get; set; }

    }
}
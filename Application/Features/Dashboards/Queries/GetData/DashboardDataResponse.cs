using System.Collections.Generic;

namespace LaptopStore.Application.Features.Dashboards.Queries.GetData
{
    public class DashboardDataResponse
    {
        public int ProductCount { get; set; }
        public int BrandCount { get; set; }
        public int OrderCount { get; set; }
        public int UserCount { get; set; }
        public int RoleCount { get; set; }
    }

}
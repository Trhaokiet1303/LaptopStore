using LaptopStore.Application.Features.Products.Queries.GetProductById;
using System;
using System.Linq;

namespace LaptopStore.Client.Infrastructure.Routes
{
    public static class ProductsEndpoints
    {
        public static string GetAllPaged(int pageNumber, int pageSize, string searchString, string[] orderBy, string[] selectedBrands = null, string[] selectedDescriptions = null, string priceRange = null, string rateRange = null)
        {
            var url = $"api/v1/products?pageNumber={pageNumber}&pageSize={pageSize}&searchString={searchString}&orderBy=";

            if (orderBy?.Any() == true)
            {
                foreach (var orderByPart in orderBy)
                {
                    url += $"{orderByPart},";
                }
                url = url[..^1]; // Xóa dấu phẩy cuối cùng
            }

            // Thêm tham số cho thương hiệu
            if (selectedBrands?.Any() == true)
            {
                url += $"&brands={string.Join(",", selectedBrands)}";
            }

            // Thêm tham số cho mô tả
            if (selectedDescriptions?.Any() == true)
            {
                url += $"&descriptions={string.Join(",", selectedDescriptions)}";
            }

            // Thêm khoảng giá
            if (!string.IsNullOrEmpty(priceRange))
            {
                url += $"&priceRange={priceRange}";
            }

            // Thêm khoảng đánh giá
            if (!string.IsNullOrEmpty(rateRange))
            {
                url += $"&rateRange={rateRange}";
            }

            return url;
        }

        public static string GetCount = "api/v1/products/count";

        public static string GetProductById(int productId)
        {
            return $"api/v1/products/{productId}";
        }

        public static string GetProductImage(int productId)
        {
            return $"api/v1/products/image/{productId}";
        }

        public static string Save = "api/v1/products";
        public static string Delete = "api/v1/products";
    }

}

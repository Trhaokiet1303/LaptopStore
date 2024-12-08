using LaptopStore.Application.Features.Products.Queries.GetProductById;
using System;
using System.Linq;

namespace LaptopStore.Client.Infrastructure.Routes
{
    public static class ProductsEndpoints
    {
        public static string GetAllPaged(int pageNumber, int pageSize, string searchString, string[] orderBy)
        {
            var url = $"api/v1/products?pageNumber={pageNumber}&pageSize={pageSize}&searchString={searchString}&orderBy=";

            if (orderBy?.Any() == true)
            {
                foreach (var orderByPart in orderBy)
                {
                    url += $"{orderByPart},";
                }
                url = url[..^1]; 
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
        public static string UpdateRate = "api/v1/products/update-rate";
        public static string UpdateFeaturedStatus = "api/v1/products/UpdateFeaturedStatus";
        public static string UpdateProductQuantity = "api/v1/products/update-quantity";
    }
}

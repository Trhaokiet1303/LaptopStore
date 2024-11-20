namespace LaptopStore.Client.Infrastructure.Routes
{
    public static class OrderItemsEndpoints
    {
        public static string GetOrderItemById(int productId)
        {
            return $"api/v1/orderitems/{productId}";
        }

        public static string GetAll = "api/v1/orderitems";
        public static string Delete = "api/v1/orderitems";
        public static string Save = "api/v1/orderitems/add-edit";
        public static string GetCount = "api/v1/orderitems/count";
    }
}
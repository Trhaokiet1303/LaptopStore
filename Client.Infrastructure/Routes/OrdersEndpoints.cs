namespace LaptopStore.Client.Infrastructure.Routes
{
    public static class OrdersEndpoints
    {
        public static string GetOrderById(int orderId)
        {
            return $"api/v1/orders/{orderId}";
        }

        public static string GetAll = "api/v1/orders";
        public static string CreateOrder = "api/v1/orders/create-order";
        public static string Delete = "api/v1/orders";
        public static string Save = "api/v1/orders/add-edit";
        public static string GetCount = "api/v1/orders/count";
        public static string UpdateStatus = "api/v1/orders/update-status";
        public static string UpdateTotalPrice = "api/v1/orders/update-totalprice";
    }
}
namespace LaptopStore.Client.Infrastructure.Routes
{
    public static class OrderItemsEndpoints
    {
        public static string GetOrderItemById(int productId)
        {
            return $"api/v1/orderitems/admin/{productId}";
        }
        public static string GetOrderItemByIdForUser(int productId)
        {
            return $"api/v1/orderitems/user/{productId}";
        }
        public static string GetAll = "api/v1/orderitems/admin/all";
        public static string GetAllForUser = "api/v1/orderitems/user/all";
        public static string Delete = "api/v1/orderitems";
        public static string Save = "api/v1/orderitems/add-edit";
        public static string GetCount = "api/v1/orderitems/count";
        public static string UpdateIsRated = "api/v1/orderitems/update-is-rated";
    }
}
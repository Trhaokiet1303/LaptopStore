using Microsoft.AspNetCore.Components;

namespace LaptopStore.Client.Pages.Shop.Layout
{
    public partial class ShopLayout : LayoutComponentBase
    {
        [Inject] private NavigationManager NavigationManager { get; set; }

        private string PageTitle { get; set; }

        protected override void OnInitialized()
        {
            // Lấy thông tin cơ bản về trang hiện tại (ví dụ: URL để tùy chỉnh tiêu đề)
            var currentUri = NavigationManager.Uri;
            PageTitle = currentUri.Contains("home") ? "Home Page" : "Shop Page";
        }
    }
}

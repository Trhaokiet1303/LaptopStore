using LaptopStore.Application.Features.Products.Queries.GetProductById;
using LaptopStore.Domain.Entities.Catalog;
using LaptopStore.Client.Infrastructure.Managers.Catalog.Product;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LaptopStore.Application.Features.Products.Queries.GetAllPaged;
using MediatR;


namespace LaptopStore.Client.Pages.Shop
{
    public partial class ProductDetail : ComponentBase
    {
        [Inject] private IProductManager ProductManager { get; set; }
        [Inject] private NavigationManager NavigationManager { get; set; }
        [Inject] private ISnackbar Snackbar { get; set; }
        private int quantity = 1; 
        [Parameter] public int productId { get; set; }
        public GetProductByIdResponse Product { get; set; } = new();
        private List<GetAllPagedProductsResponse> AllProducts { get; set; } = new();
        protected override async Task OnInitializedAsync()
        {
            await LoadAllProducts();
        }
        protected override async Task OnParametersSetAsync()
        {
            Console.WriteLine("Đang tải sản phẩm với ID: " + productId); // Ghi log giá trị productId
            if (productId > 0)
            {
                await LoadProduct(productId);
            }
            else
            {
                Snackbar.Add("ID sản phẩm không hợp lệ.", Severity.Error);
                NavigationManager.NavigateTo("/not-found");
            }
        }

        private async Task LoadProduct(int id)
        {
            try
            {
                Console.WriteLine("Bắt đầu gọi ProductManager.GetProductByIdAsync với ID: " + id);

                var result = await ProductManager.GetProductByIdAsync(id);

                // Kiểm tra nếu `result` không thành công hoặc `Data` là null
                if (result == null)
                {
                    Console.WriteLine("Kết quả trả về từ ProductManager.GetProductByIdAsync là null.");
                    Snackbar.Add("Không thể kết nối tới dịch vụ lấy dữ liệu sản phẩm.", Severity.Error);
                    return;
                }

                if (result.Succeeded && result.Data != null)
                {
                    Product = result.Data;
                    Console.WriteLine($"Dữ liệu sản phẩm đã tải thành công: Tên sản phẩm - {Product.Name}, Giá - {Product.Price}");
                    Snackbar.Add("Dữ liệu sản phẩm đã tải thành công!", Severity.Success);

                    // Cập nhật giao diện để hiển thị dữ liệu
                    StateHasChanged();
                }
                else
                {
                    Console.WriteLine("Không thể tải dữ liệu sản phẩm hoặc sản phẩm không tồn tại.");
                    Snackbar.Add("Không thể tải dữ liệu sản phẩm hoặc sản phẩm không tồn tại.", Severity.Error);
                    NavigationManager.NavigateTo("/not-found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Đã xảy ra lỗi khi tải dữ liệu sản phẩm: " + ex.Message);
                Snackbar.Add($"Đã xảy ra lỗi khi tải dữ liệu sản phẩm: {ex.Message}", Severity.Error);
            }
        }
        private async Task AddToCart()
        {
            var cartItem = new OrderItem
            {
                ProductId = Product.Id,
                ProductName = Product.Name,
                ProductPrice = Product.Price,
                ProductImage = Product.ImageDataURL,
                Quantity = quantity
            };

            // Lấy giỏ hàng từ localStorage
            var existingCartJson = await JS.InvokeAsync<string>("localStorage.getItem", "cartItems");
            var cartItems = existingCartJson != null ? JsonSerializer.Deserialize<List<OrderItem>>(existingCartJson) : new List<OrderItem>();

            // Kiểm tra xem sản phẩm đã có trong giỏ hàng chưa
            var existingItem = cartItems.FirstOrDefault(i => i.ProductId == cartItem.ProductId);

            if (existingItem != null)
            {
                // Nếu sản phẩm đã có, tăng số lượng lên
                existingItem.Quantity += cartItem.Quantity;
            }
            else
            {
                // Nếu sản phẩm chưa có, thêm sản phẩm vào giỏ hàng
                cartItems.Add(cartItem);
            }

            // Lưu giỏ hàng vào localStorage
            await JS.InvokeVoidAsync("localStorage.setItem", "cartItems", JsonSerializer.Serialize(cartItems));

            Snackbar.Add("Sản phẩm đã được thêm vào giỏ hàng!", Severity.Success);
        }
        private async Task InvokeModal(int id = 0)
        {
            var parameters = new DialogParameters();

            if (id != 0)
            {
                // Lấy dữ liệu sản phẩm từ API
                var result = await ProductManager.GetProductByIdAsync(id);

                if (result != null && result.Succeeded && result.Data != null)
                {
                    var product = result.Data;

                    // Thêm dữ liệu sản phẩm vào parameters
                    parameters.Add(nameof(ViewProduct.Product), new GetProductByIdResponse
                    {
                        ImageDataURL = product.ImageDataURL,
                        Id = product.Id,
                        Name = product.Name,
                        Price = product.Price,
                        CPU = product.CPU,
                        Screen = product.Screen,
                        Card = product.Card,
                        Ram = product.Ram,
                        Rom = product.Rom,
                        Battery = product.Battery,
                        Weight = product.Weight,
                        Description = product.Description,
                        Rate = product.Rate,
                        Barcode = product.Barcode,
                    });
                }
                else
                {
                    Snackbar.Add("Không thể tải dữ liệu sản phẩm.", Severity.Error);
                    return;
                }
            }

            // Cấu hình modal
            var options = new DialogOptions
            {
                CloseButton = true,
                MaxWidth = MaxWidth.Medium,
                FullWidth = true,
                DisableBackdropClick = true
            };

            // Hiển thị modal
            var dialog = _dialogService.Show<ViewProduct>("Thông tin sản phẩm", parameters, options);
            var resultDialog = await dialog.Result;

            if (!resultDialog.Cancelled)
            {
                Snackbar.Add("Đã đóng modal.", Severity.Info);
            }
        }


        private async Task LoadAllProducts()
        {
            try
            {
                Console.WriteLine("Đang tải tất cả sản phẩm...");
                var request = new Application.Requests.Catalog.GetAllPagedProductsRequest
                {
                    PageNumber = 1, // Bạn có thể thay đổi theo yêu cầu
                    PageSize = 6,  // Số lượng sản phẩm cần hiển thị
                    SearchString = string.Empty,
                    Orderby = new[] { "Name ascending" }
                };

                var result = await ProductManager.GetProductsAsync(request);

                if (result != null && result.Succeeded)
                {
                    AllProducts = result.Data.ToList();
                    Console.WriteLine($"Đã tải {AllProducts.Count} sản phẩm.");
                    StateHasChanged();
                }
                else
                {
                    Console.WriteLine("Không thể tải danh sách sản phẩm.");
                    Snackbar.Add("Không thể tải danh sách sản phẩm.", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Đã xảy ra lỗi khi tải danh sách sản phẩm: {ex.Message}");
                Snackbar.Add($"Đã xảy ra lỗi khi tải danh sách sản phẩm: {ex.Message}", Severity.Error);
            }
        }
        private async Task HandleBuyNow()
        {
            await AddToCart();

            // Chờ 0.5 giây trước khi chuyển sang trang "order"
            await Task.Delay(500);

            // Chuyển hướng sang trang "order"
            NavigationManager.NavigateTo("/order");
        }

        private void NavigateToProductDetail(int productId)
        {
            NavigationManager.NavigateTo($"/product/{productId}");
        }

        //Hàm tạo ngôi sao
        private List<string> GetStars(decimal rate)
        {
            // Lấy phần nguyên của rate để xác định số sao đầy
            var fullStars = (int)Math.Floor(rate);

            // Kiểm tra xem có nửa sao không
            var halfStar = (rate - fullStars) >= 0.5m; // 'm' để chỉ định là kiểu decimal

            // Tính số sao trống còn lại
            var emptyStars = 5 - fullStars - (halfStar ? 1 : 0);

            // Tạo danh sách chứa các icon sao
            var stars = new List<string>();

            // Thêm icon sao đầy
            stars.AddRange(Enumerable.Repeat(Icons.Material.Filled.Star, fullStars));

            // Thêm icon nửa sao nếu có
            if (halfStar)
            {
                stars.Add(Icons.Material.Filled.StarHalf);
            }

            // Thêm icon sao trống
            stars.AddRange(Enumerable.Repeat(Icons.Material.Outlined.StarOutline, emptyStars));

            return stars;
        }

    }
}

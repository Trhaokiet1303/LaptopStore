using LaptopStore.Application.Features.Products.Queries.GetProductById;
using LaptopStore.Application.Requests;
using LaptopStore.Client.Extensions;
using LaptopStore.Shared.Constants.Application;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Blazored.FluentValidation;
using LaptopStore.Client.Infrastructure.Managers.Catalog.Brand;
using LaptopStore.Client.Infrastructure.Managers.Catalog.Product;
using LaptopStore.Client.Pages.Admin.Products;
using System.Text.Json;
using LaptopStore.Domain.Entities.Catalog;
using Microsoft.JSInterop;

namespace LaptopStore.Client.Pages.Shop
{
    public partial class ViewProduct
    {
        [Inject] private IProductManager ProductManager { get; set; }
        [Inject] private IBrandManager BrandManager { get; set; }
        [Inject] private ISnackbar Snackbar { get; set; }
        [Inject] private IJSRuntime JSRuntime { get; set; }
        private int quantity = 1;
        private int MaxQuantity = 10;

        [Parameter] public GetProductByIdResponse Product { get; set; } = new();
        [CascadingParameter] private HubConnection HubConnection { get; set; }
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        private FluentValidationValidator _fluentValidationValidator;
        private bool Validated => _fluentValidationValidator.Validate(options => { options.IncludeAllRuleSets(); });

        public void Cancel()
        {
            MudDialog.Cancel();
        }

        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();
            HubConnection = HubConnection.TryInitialize(_navigationManager);
            if (HubConnection.State == HubConnectionState.Disconnected)
            {
                await HubConnection.StartAsync();
            }
        }

        private async Task LoadDataAsync()
        {
            await LoadImageAsync();
            await LoadMaxQuantityAsync();
        }

        private async Task LoadMaxQuantityAsync()
        {
            var productResponse = await ProductManager.GetProductByIdAsync(Product.Id);
            if (productResponse.Succeeded)
            {
                MaxQuantity = productResponse.Data.Quantity;
            }
            else
            {
                Snackbar.Add("Không thể tải thông tin sản phẩm.", Severity.Error);
            }
        }

        private async Task LoadImageAsync()
        {
            var data = await ProductManager.GetProductImageAsync(Product.Id);
            if (data.Succeeded)
            {
                var imageData = data.Data;
                if (!string.IsNullOrEmpty(imageData))
                {
                    Product.ImageDataURL = imageData;
                }
            }
        }
        private async Task AddToCart()
        {
            // Lấy thông tin sản phẩm từ cơ sở dữ liệu
            var productResponse = await ProductManager.GetProductByIdAsync(Product.Id);
            if (!productResponse.Succeeded)
            {
                Snackbar.Add("Không thể tải thông tin sản phẩm.", Severity.Error);
                return;
            }

            var product = productResponse.Data;

            // Kiểm tra nếu sản phẩm đã hết hàng
            if (product.Quantity <= 0)
            {
                Snackbar.Add("Sản phẩm đã hết hàng.", Severity.Error);
                return;
            }

            // Kiểm tra số lượng muốn thêm vào giỏ hàng
            if (quantity > product.Quantity)
            {
                Snackbar.Add($"Chỉ có {product.Quantity} sản phẩm trong kho.", Severity.Error);
                return;
            }

            var cartItem = new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ProductPrice = product.Price,
                ProductImage = product.ImageDataURL,
                Quantity = quantity
            };

            // Lấy giỏ hàng từ localStorage
            var existingCartJson = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "cartItems");
            var cartItems = existingCartJson != null ? JsonSerializer.Deserialize<List<OrderItem>>(existingCartJson) : new List<OrderItem>();

            // Kiểm tra xem sản phẩm đã có trong giỏ hàng chưa
            var existingItem = cartItems.FirstOrDefault(i => i.ProductId == cartItem.ProductId);

            if (existingItem != null)
            {
                // Nếu sản phẩm đã có, tăng số lượng lên
                if (existingItem.Quantity + cartItem.Quantity <= product.Quantity)
                {
                    existingItem.Quantity += cartItem.Quantity;
                }
                else
                {
                    Snackbar.Add($"Không thể thêm quá {product.Quantity} sản phẩm vào giỏ hàng.", Severity.Error);
                    return;
                }
            }
            else
            {
                // Nếu sản phẩm chưa có, thêm sản phẩm vào giỏ hàng
                cartItems.Add(cartItem);
            }

            // Lưu giỏ hàng vào localStorage
            await JSRuntime.InvokeVoidAsync("localStorage.setItem", "cartItems", JsonSerializer.Serialize(cartItems));

            Snackbar.Add("Sản phẩm đã được thêm vào giỏ hàng!", Severity.Success);
        }
    }
}

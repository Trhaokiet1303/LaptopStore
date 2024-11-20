using AutoMapper;
using LaptopStore.Application.Features.Orders.Commands.AddEdit;
using LaptopStore.Application.Interfaces.Repositories;
using LaptopStore.Application.Interfaces.Services;
using LaptopStore.Application.Requests.Catalog;
using LaptopStore.Client.Extensions;
using LaptopStore.Client.Infrastructure.Managers.Catalog.Order;
using LaptopStore.Client.Infrastructure.Managers.Catalog.Product;
using LaptopStore.Shared.Constants.Application;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LaptopStore.Client.Pages.Shop
{
    public partial class Cart
    {
        [Inject] private IProductManager ProductManager { get; set; }
        [Inject] private IOrderManager OrderManager { get; set; }
        [Inject] private NavigationManager NavigationManager { get; set; }

        private List<OrderItem> cartItems = new();

        protected override async Task OnInitializedAsync()
        {
            var cartJson = await JS.InvokeAsync<string>("localStorage.getItem", "cartItems");
            if (!string.IsNullOrEmpty(cartJson))
            {
                cartItems = JsonSerializer.Deserialize<List<OrderItem>>(cartJson);

                // Tải hình ảnh cho từng sản phẩm
                foreach (var item in cartItems)
                {
                    await LoadImageAsync(item);
                }
            }
        }

        private bool IsQuantityLessThanOrEqualToOne(OrderItem item) => item.Quantity <= 1;

        private async Task UpdateQuantity(OrderItem item, int newQuantity)
        {
            item.Quantity = newQuantity;
            await SaveCartToLocalStorage();
        }

        private async Task RemoveFromCart(int productId)
        {
            cartItems = cartItems.Where(i => i.ProductId != productId).ToList();
            await SaveCartToLocalStorage();
        }

        private async Task SaveCartToLocalStorage()
        {
            await JS.InvokeVoidAsync("localStorage.setItem", "cartItems", System.Text.Json.JsonSerializer.Serialize(cartItems));
        }
        private decimal GetTotalPrice()
        {
            return cartItems.Sum(item => item.ProductPrice * item.Quantity);
        }


        private async Task Checkout()
        {
            if (cartItems == null || !cartItems.Any())
            {
                await JS.InvokeVoidAsync("alert", "Giỏ hàng của bạn đang trống!");
                return;
            }
            else
            {
                NavigateToProductDetail();

            }
        }

        private async Task LoadImageAsync(OrderItem item)
        {
            var data = await ProductManager.GetProductImageAsync(item.ProductId);
            if (data.Succeeded)
            {
                var imageData = data.Data;
                if (!string.IsNullOrEmpty(imageData))
                {
                    item.ProductImage = imageData;
                }
            }
        }

        private void NavigateToProductDetail()
        {
            NavigationManager.NavigateTo($"/order");
        }

    }
}

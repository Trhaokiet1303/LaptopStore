﻿using LaptopStore.Application.Responses.Identity;
using LaptopStore.Client.Extensions;
using LaptopStore.Client.Infrastructure.Managers.Catalog.Order;
using LaptopStore.Client.Infrastructure.Managers.Identity.Users;
using LaptopStore.Shared.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using LaptopStore.Domain.Entities.Catalog;
using LaptopStore.Client.Infrastructure.Managers.Catalog.Product;
using System.Text.Json;
using Microsoft.JSInterop;
using LaptopStore.Application.Features.Products.Commands.Update;
using MudBlazor;

namespace LaptopStore.Client.Pages.Shop
{
    public partial class Order
    {
        [Inject] private IUserManager UserManager { get; set; }
        [Inject] private IProductManager ProductManager { get; set; }
        [Inject] private HttpClient Http { get; set; }
        [Inject] private IOrderManager OrderManager { get; set; }
        [Inject] private IJSRuntime JS { get; set; }
        [Inject] NavigationManager NavigationManager { get; set; }


        private List<OrderItem> cartItems = new();

        private string firstName;
        private string lastName;
        private string phoneNumber;
        private string street;
        private string selectedCity;
        private string selectedDistrict;
        private string selectedWard;
        private bool isPayment;

        public string FirstName { get => firstName; set => firstName = value; }
        public string LastName { get => lastName; set => lastName = value; }
        public string PhoneNumber { get => phoneNumber; set => phoneNumber = value; }
        public string Street { get => street; set => street = value; }

        public string SelectedCity
        {
            get => selectedCity;
            set
            {
                if (selectedCity != value)
                {
                    selectedCity = value;
                    OnCityChanged();
                }
            }
        }

        public string SelectedDistrict
        {
            get => selectedDistrict;
            set
            {
                if (selectedDistrict != value)
                {
                    selectedDistrict = value;
                    OnDistrictChanged();
                }
            }
        }

        public string SelectedWard
        {
            get => selectedWard;
            set => selectedWard = value;
        }
        public string SelectedPaymentMethod { get; set; }


        private List<City> Cities { get; set; } = new();
        private List<District> Districts { get; set; } = new();
        private List<Ward> Wards { get; set; } = new();


        public void SetUserDetails(ClaimsPrincipal user)
        {
            FirstName = user.GetFirstName();
            LastName = user.GetLastName();
            PhoneNumber = user.GetPhoneNumber();
        }
        protected override async Task OnInitializedAsync()
        {
            // Lấy dữ liệu từ localStorage
            var cartJson = await JS.InvokeAsync<string>("localStorage.getItem", "cartItems");
            if (!string.IsNullOrEmpty(cartJson))
            {
                // Deserialize dữ liệu cartItems từ JSON
                cartItems = JsonSerializer.Deserialize<List<OrderItem>>(cartJson);
            }

            try
            {
                // Lấy thông tin user
                var state = await _stateProvider.GetAuthenticationStateAsync();
                var user = state.User;
                if (user != null)
                {
                    SetUserDetails(user);
                }

                // Lấy danh sách tỉnh/thành phố
                var data = await Http.GetFromJsonAsync<List<City>>("json/data.json");
                Cities = data ?? new List<City>();
                Console.WriteLine($"Số tỉnh thành tải về: {Cities.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tải dữ liệu: {ex.Message}");
            }
        }



        private void OnCityChanged()
        {
            var selectedCity = Cities.FirstOrDefault(c => c.Name == SelectedCity);
            Districts = selectedCity?.Districts ?? new List<District>();
            Wards.Clear();
            SelectedDistrict = ""; // Reset district selection
            SelectedWard = ""; // Reset ward selection
        }

        private void OnDistrictChanged()
        {
            var selectedDistrict = Districts.FirstOrDefault(d => d.Name == SelectedDistrict);
            Wards = selectedDistrict?.Wards ?? new List<Ward>();
            SelectedWard = ""; // Reset ward selection
        }

        private void OnCitySelect(ChangeEventArgs e)
        {
            SelectedCity = e.Value.ToString();
        }

        private void OnDistrictSelect(ChangeEventArgs e)
        {
            SelectedDistrict = e.Value.ToString();
        }

        private void OnWardSelect(ChangeEventArgs e)
        {
            SelectedWard = e.Value.ToString();
        }

        public class City
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public List<District> Districts { get; set; }
        }

        public class District
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public List<Ward> Wards { get; set; }
        }

        public class Ward
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
        private List<PaymentMethod> _paymentMethods = new List<PaymentMethod>
        {
            new PaymentMethod { Name = "VNPAY", Description = "Thanh toán online qua cổng VNPAY" },
            new PaymentMethod { Name = "COD", Description = "Thanh toán khi giao hàng (COD)" },
            new PaymentMethod { Name = "Chuyển khoản ngân hàng", Description = "Chuyển khoản ngân hàng" },
            new PaymentMethod { Name = "Thẻ Nội địa", Description = "Thanh toán bằng thẻ nội địa" }
        };

        public class PaymentMethod
        {
            public string Name { get; set; }
            public string Description { get; set; }
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
            if (string.IsNullOrEmpty(FirstName) || string.IsNullOrEmpty(LastName))
            {
                await JS.InvokeVoidAsync("alert", "Vui lòng nhập họ và tên!");
                return;
            }

            if (string.IsNullOrEmpty(PhoneNumber) || !System.Text.RegularExpressions.Regex.IsMatch(PhoneNumber, @"^\d{10}$"))
            {
                await JS.InvokeVoidAsync("alert", "Vui lòng nhập số điện thoại!");
                return;
            }
            if (string.IsNullOrEmpty(SelectedCity) || string.IsNullOrEmpty(SelectedDistrict) || string.IsNullOrEmpty(SelectedWard))
            {
                await JS.InvokeVoidAsync("alert", "Vui lòng chọn địa chỉ giao hàng!");
                return;
            }
            if (string.IsNullOrEmpty(SelectedPaymentMethod))
            {
                await JS.InvokeVoidAsync("alert", "Vui lòng chọn phương thức thanh toán!");
                return;
            }

            var state = await _stateProvider.GetAuthenticationStateAsync();
            var user = state.User;
            var userId = user.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                await JS.InvokeVoidAsync("alert", "Bạn phải đăng nhập để đặt hàng!");
                return;
            }

            var userAddress = $"{street}  {selectedWard}, {selectedDistrict}, {selectedCity}";
            var fullName = $"{FirstName} {LastName}";

            var order = new Domain.Entities.Catalog.Order
            {
                UserId = userId,
                UserName = fullName,
                UserPhone = phoneNumber,
                UserAddress = userAddress,
                TotalPrice = (int)GetTotalPrice(),
                MethodPayment = SelectedPaymentMethod,
                StatusOrder = "Đặt Thành Công",
                IsPayment = SelectedPaymentMethod != "COD",
                OrderItem = cartItems.Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    ProductImage = item.ProductImage,
                    ProductPrice = item.ProductPrice,
                    Quantity = item.Quantity,
                    TotalPrice = item.TotalPrice,
                }).ToList()
            };

            var createdOrder = await OrderManager.CreateOrderAsync(order);

            if (createdOrder != null)
            {
                foreach (var item in cartItems)
                {
                    // Get the current product details to retrieve the current stock (Instock)
                    var productResponse = await ProductManager.GetProductByIdAsync(item.ProductId);

                    if (productResponse != null && productResponse.Succeeded)
                    {
                        // Get the current stock (Instock)
                        var currentStock = productResponse.Data.Quantity;

                        // Check if there's enough stock
                        if (currentStock < item.Quantity)
                        {
                            await JS.InvokeVoidAsync("alert", $"Không đủ hàng cho sản phẩm {item.ProductName}");
                            return;
                        }

                        // Update the stock (decrease it based on the order quantity)
                        var newQuantity = currentStock - item.Quantity;

                        // Call the API to update the product quantity
                        var updateResult = await ProductManager.UpdateProductQuantityAsync(item.ProductId, newQuantity);

                        if (!updateResult.Succeeded)
                        {
                            await JS.InvokeVoidAsync("alert", "Có lỗi khi cập nhật số lượng sản phẩm.");
                            return;
                        }
                        await UpdateFeaturedStatusForProduct(item.ProductId);
                    }
                    else
                    {
                        await JS.InvokeVoidAsync("alert", "Lỗi khi lấy thông tin sản phẩm.");
                        return;
                    }
                }

                await JS.InvokeVoidAsync("alert", "Đặt hàng thành công!");
                await JS.InvokeVoidAsync("localStorage.removeItem", "cartItems");
                NavigationManager.NavigateTo($"/orderdetail");
            }
            else
            {
                await JS.InvokeVoidAsync("alert", "Đặt hàng không thành công, vui lòng thử lại!");
            }
        }

        private async Task UpdateFeaturedStatusForProduct(int productId)
        {
            try
            {
                // Gửi yêu cầu cập nhật trạng thái featured cho sản phẩm
                var response = await ProductManager.UpdateFeaturedStatusAsync(productId);

                if (response.Succeeded)
                {
                    _snackBar.Add("Trạng thái nổi bật của sản phẩm đã được cập nhật!", Severity.Success);
                }
                else
                {
                    foreach (var message in response.Messages)
                    {
                        _snackBar.Add(message, Severity.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                _snackBar.Add($"Có lỗi xảy ra khi cập nhật trạng thái nổi bật: {ex.Message}", Severity.Error);
            }
        }

        private void HandlePaymentMethodChange(ChangeEventArgs e)
        {
            SelectedPaymentMethod = e.Value?.ToString();
            Console.WriteLine($"Selected Payment Method: {SelectedPaymentMethod}");
        }

    }
}

using LaptopStore.Application.Responses.Identity;
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

namespace LaptopStore.Client.Pages.Shop
{
    public partial class Order
    {
        [Inject] private IUserManager UserManager { get; set; }
        [Inject] private IProductManager ProductManager { get; set; }
        [Inject] private HttpClient Http { get; set; }
        [Inject] private IOrderManager OrderManager { get; set; }
        [Inject] private IJSRuntime JS { get; set; }

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

            var userAddress = $"{street}, {selectedWard}, {selectedDistrict}, {selectedCity}";
            var fullName = $"{FirstName} {LastName}";

            var order = new Domain.Entities.Catalog.Order
            {
                UserId = userId,
                UserName = fullName,
                UserPhone = phoneNumber,
                UserAddress = userAddress,
                TotalPrice = (int)GetTotalPrice(),
                MethodPayment = SelectedPaymentMethod,
                StatusOrder = "Đang xử lý",
                IsPayment = SelectedPaymentMethod != "COD", 
                OrderItem = cartItems.Select(item => new Domain.Entities.Catalog.OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    ProductImage = item.ProductImage,
                    ProductPrice = item.ProductPrice,
                    Quantity = item.Quantity
                }).ToList()
            };

            await OrderManager.CreateOrderAsync(order);
            await JS.InvokeVoidAsync("alert", "Đặt hàng thành công!");

            await JS.InvokeVoidAsync("localStorage.removeItem", "cartItems");
        }

        private void HandlePaymentMethodChange(ChangeEventArgs e)
        {
            SelectedPaymentMethod = e.Value?.ToString();
            Console.WriteLine($"Selected Payment Method: {SelectedPaymentMethod}");
        }

    }
}

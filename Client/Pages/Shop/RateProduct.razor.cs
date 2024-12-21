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
using LaptopStore.Application.Features.OrderItems.Commands.Update;
using LaptopStore.Client.Infrastructure.Managers.Catalog.OrderItem;

namespace LaptopStore.Client.Pages.Shop
{
    public partial class RateProduct
    {
        [Inject] private IProductManager ProductManager { get; set; }
        [Inject] private IBrandManager BrandManager { get; set; }
        [Inject] private IOrderItemManager OrderItemManager { get; set; }
        [Inject] private ISnackbar Snackbar { get; set; }
        [Inject] private NavigationManager NavigationManager { get; set; }
        [Inject] private IJSRuntime JSRuntime { get; set; }
        private decimal _selectedRating = 0;
        [Parameter] public int OrderItemId { get; set; }


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
            await LoadOrderItemAsync();
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

        private async Task LoadProduct(int id)
        {
            try
            {
                Console.WriteLine("Bắt đầu gọi ProductManager.GetProductByIdAsync với ID: " + id);

                var result = await ProductManager.GetProductByIdAsync(id);

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
        private async Task LoadOrderItemAsync()
        {
            try
            {
                var result = await OrderItemManager.GetOrderItemByIdForUserAsync(OrderItemId);
                if (result.Succeeded && result.Data != null)
                {
                    _selectedRating = result.Data.Rate;
                    StateHasChanged();
                }
                else
                {
                    Snackbar.Add("Không thể tải thông tin đánh giá.", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Đã xảy ra lỗi: {ex.Message}", Severity.Error);
            }
        }


        private void HandleRatingChange(ChangeEventArgs e)
        {
            if (int.TryParse(e.Value.ToString(), out int rating))
            {
                _selectedRating = rating;
                Console.WriteLine($"Rating selected: {_selectedRating}");
            }
        }
        private async Task SubmitRating()
        {
            if (_selectedRating == 0)
            {
                Snackbar.Add("Vui lòng chọn số sao để đánh giá!", Severity.Warning);
                return;
            }

            var response = await ProductManager.UpdateRateAsync(Product.Id, _selectedRating);

            if (response.Succeeded)
            {
                Snackbar.Add("Đánh giá sản phẩm thành công!", Severity.Success);

                // Update the OrderItem rating status
                var updateResponse = await OrderItemManager.UpdateIsRatedAsync(new UpdateIsRatedCommand
                {
                    OrderItemId = OrderItemId,
                    Rate = _selectedRating,
                    IsRated = true,
                });

                if (updateResponse.Succeeded)
                {
                    Snackbar.Add("Cập nhật trạng thái đánh giá thành công!", Severity.Success);
                }
                else
                {
                    Snackbar.Add("Không thể cập nhật trạng thái đã đánh giá. Vui lòng thử lại sau.", Severity.Error);
                }

                await UpdateFeaturedStatusForProduct(Product.Id);
                await LoadProduct(Product.Id);
            }
            else
            {
                Snackbar.Add("Không thể cập nhật đánh giá. Vui lòng thử lại sau.", Severity.Error);
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
                    Snackbar.Add("Trạng thái nổi bật của sản phẩm đã được cập nhật!", Severity.Success);
                }
                else
                {
                    foreach (var message in response.Messages)
                    {
                        Snackbar.Add(message, Severity.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Có lỗi xảy ra khi cập nhật trạng thái nổi bật: {ex.Message}", Severity.Error);
            }
        }
    }
}

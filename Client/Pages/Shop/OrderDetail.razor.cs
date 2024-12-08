using LaptopStore.Application.Features.Orders.Commands.Update;
using LaptopStore.Application.Features.Orders.Queries.GetAll;
using LaptopStore.Client.Extensions;
using LaptopStore.Client.Infrastructure.Managers.Catalog.Order;
using LaptopStore.Client.Infrastructure.Managers.Catalog.Product;
using LaptopStore.Client.Shared.Dialogs;
using LaptopStore.Domain.Entities.Catalog;
using LaptopStore.Shared.Constants.Application;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace LaptopStore.Client.Pages.Shop
{
    public partial class OrderDetail : ComponentBase
    {
        [Inject] private IOrderManager OrderManager { get; set; }
        [Inject] private AuthenticationStateProvider AuthenticationProvider { get; set; }
        [Inject] private IJSRuntime JSRuntime { get; set; }
        [Inject] private IProductManager ProductManager { get; set; }
        [Inject] private IJSRuntime JS { get; set; }
        [Inject] private IDialogService DialogService { get; set; }

        private bool IsLoading { get; set; }
        private List<GetAllOrdersResponse> Orders { get; set; } = new();
        private string SelectedStatus { get; set; }

        protected override async Task OnInitializedAsync()
        {
            IsLoading = true;
            SelectedStatus = ""; 
            await LoadOrdersAsync(SelectedStatus);
            IsLoading = false;
        }

        private async Task FilterProcessingOrders() => await FilterOrdersByStatusAsync("Đặt thành công");
        private async Task FilterShippingOrders() => await FilterOrdersByStatusAsync("Đang giao");
        private async Task FilterDeliveredOrders() => await FilterOrdersByStatusAsync("Đã giao");
        private async Task FilterCanceledOrders() => await FilterOrdersByStatusAsync("Đã hủy");


        private async Task FilterOrdersByStatusAsync(string status)
        {
            SelectedStatus = status; // Cập nhật trạng thái đã chọn
            IsLoading = true;

            // Lấy trạng thái đăng nhập và ID người dùng
            var state = await AuthenticationProvider.GetAuthenticationStateAsync();
            var user = state.User;
            var userId = user.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                await JSRuntime.InvokeVoidAsync("alert", "Bạn phải đăng nhập để xem đơn hàng!");
                IsLoading = false;
                return;
            }

            // Lấy danh sách đơn hàng từ server
            var result = await OrderManager.GetAllAsync();
            if (result.Succeeded)
            {
                Orders = result.Data
                               .Where(o => o.UserId == userId &&
                                           (string.IsNullOrEmpty(status) ||
                                            string.Equals(o.StatusOrder, status, StringComparison.OrdinalIgnoreCase)))
                               .ToList();

                if (!Orders.Any())
                {
                    _snackBar.Add("Không có đơn hàng phù hợp với trạng thái đã chọn.", Severity.Warning);
                }
            }
            else
            {
                Orders.Clear(); // Nếu không thành công, xóa danh sách hiện tại
                await JSRuntime.InvokeVoidAsync("alert", "Không thể tải danh sách đơn hàng!");
            }

            IsLoading = false;
        }


        private async Task LoadOrdersAsync(string status = "")
        {
            var state = await AuthenticationProvider.GetAuthenticationStateAsync();
            var user = state.User;
            var userId = user.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                await JSRuntime.InvokeVoidAsync("alert", "Bạn phải đăng nhập để xem đơn hàng!");
                return;
            }

            var result = await OrderManager.GetAllAsync();
            if (result.Succeeded)
            {
                // Hiển thị tất cả đơn hàng khi không có status hoặc lọc theo trạng thái
                Orders = result.Data
                               .Where(o => o.UserId == userId &&
                                           (string.IsNullOrEmpty(status) || o.StatusOrder == status))
                               .ToList();
            }
            else
            {
                await JSRuntime.InvokeVoidAsync("alert", "Không thể tải danh sách đơn hàng!");
            }
        }


        private async Task ConfirmCancelOrder(int orderId, string currentStatus)
        {
            // Prevent canceling if the order is already "Đã Hủy", "Đang giao" or "Đã giao"
            if (currentStatus == "Đã Hủy")
            {
                await JSRuntime.InvokeVoidAsync("alert", "Bạn đã hủy đơn hàng này rồi!");
                return;
            }

            if (currentStatus == "Đang Giao" || currentStatus == "Đã Giao")
            {
                await JSRuntime.InvokeVoidAsync("alert", $"Đơn hàng này không thể vì: {currentStatus}!");
                return;
            }

            // Show a confirmation dialog for cancellation
            var parameters = new DialogParameters
    {
        { "ContentText", "Bạn có chắc chắn muốn hủy đơn hàng này không?" },
        { "ButtonText", "Xác nhận" },
        { "Color", Color.Error }
    };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true };
            var dialog = DialogService.Show<ConfirmationDialog>("Xác nhận hủy đơn hàng", parameters, options);
            var result = await dialog.Result;

            if (!result.Cancelled)
            {
                await UpdateOrderStatus(orderId, "Đã Hủy");
            }
        }

        private bool IsCancelDisabled(string status)
        {
            return status == "Đã Hủy";
        }

        private async Task UpdateOrderStatus(int orderId, string newStatus)
        {
            IsLoading = true;

            var response = await OrderManager.UpdateOrderStatusAsync(new UpdateOrderStatusCommand
            {
                OrderId = orderId,
                NewStatus = newStatus
            });

            if (response.Succeeded)
            {
                if (newStatus == "Đã Hủy")
                {
                    var orderResponse = await OrderManager.GetOrderByIdAsync(orderId);
                    if (orderResponse.Succeeded && orderResponse.Data != null)
                    {
                        foreach (var item in orderResponse.Data.OrderItem)
                        {
                            var updateResult = await ProductManager.UpdateProductQuantityAsync(item.ProductId, item.Instock + item.Quantity);

                            if (!updateResult.Succeeded)
                            {
                                await JS.InvokeVoidAsync("alert", $"Lỗi khi cập nhật lại số lượng sản phẩm {item.ProductName}. Vui lòng thử lại!");
                                break;
                            }
                        }
                    }
                }

                _snackBar.Add("Trạng thái đơn hàng đã được cập nhật thành công!", Severity.Success);

                await LoadOrdersAsync();
            }
            else
            {
                _snackBar.Add($"Không thể cập nhật trạng thái đơn hàng. Lỗi: {response.Messages[0]}", Severity.Error);
            }

            IsLoading = false;
        }

    }
}

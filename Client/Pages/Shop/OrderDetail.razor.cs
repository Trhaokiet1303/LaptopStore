using LaptopStore.Application.Features.Orders.Commands.Update;
using LaptopStore.Application.Features.Orders.Queries.GetAll;
using LaptopStore.Client.Extensions;
using LaptopStore.Client.Infrastructure.Managers.Catalog.Order;
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
using System.Threading.Tasks;

namespace LaptopStore.Client.Pages.Shop
{
    public partial class OrderDetail : ComponentBase
    {
        [Inject] private IOrderManager OrderManager { get; set; }
        [Inject] private AuthenticationStateProvider AuthenticationProvider { get; set; }
        [Inject] private IJSRuntime JSRuntime { get; set; }
        [Inject] private IDialogService DialogService { get; set; }

        private bool IsLoading { get; set; }
        private List<GetAllOrdersResponse> Orders { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            IsLoading = true;
            await LoadOrdersAsync();
            IsLoading = false;
        }

        private async Task LoadOrdersAsync()
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
                Console.WriteLine($"Orders loaded from server: {result.Data.Count} items.");
                Orders = result.Data.Where(o => o.UserId == userId).ToList();
            }
            else
            {
                await JSRuntime.InvokeVoidAsync("alert", "Không thể tải danh sách đơn hàng!");
            }
        }


        private async Task ConfirmCancelOrder(int orderId, string currentStatus)
        {
            if (currentStatus == "Đã Hủy")
            {
                await JSRuntime.InvokeVoidAsync("alert", "Bạn đã hủy đơn hàng này rồi!");
                return;
            }

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
            // Hiển thị trạng thái đang xử lý
            IsLoading = true;

            // Gửi yêu cầu cập nhật trạng thái tới server
            var response = await OrderManager.UpdateOrderStatusAsync(new UpdateOrderStatusCommand
            {
                OrderId = orderId,
                NewStatus = newStatus
            });

            if (response.Succeeded)
            {
                _snackBar.Add("Hủy đơn hàng đã được cập nhật thành công!", Severity.Success);

                // Tải lại toàn bộ danh sách đơn hàng từ server
                await LoadOrdersAsync();
            }
            else
            {
                _snackBar.Add($"Không thể cập nhật trạng thái đơn hàng. Lỗi: {response.Messages[0]}", Severity.Error);
            }

            // Kết thúc trạng thái đang xử lý
            IsLoading = false;
        }
    }
}

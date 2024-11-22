using LaptopStore.Application.Features.Orders.Commands.AddEdit;
using LaptopStore.Application.Features.Orders.Queries.GetAll;
using LaptopStore.Application.Features.Orders.Queries.GetById;
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
    public partial class OrderDetail : ComponentBase // Đảm bảo lớp kế thừa từ ComponentBase
    {
        [Inject] private IOrderManager OrderManager { get; set; }
        [Inject] private AuthenticationStateProvider AuthenticationProvider { get; set; }
        [Inject] private IJSRuntime JSRuntime { get; set; }

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
                Orders = result.Data.Where(o => o.UserId == userId).ToList();
            }
            else
            {
                await JSRuntime.InvokeVoidAsync("alert", "Không thể tải danh sách đơn hàng!");
            }

        }

        private async Task ConfirmCancelOrder(int orderId)
        {
            var orderToCancel = Orders.FirstOrDefault(o => o.Id == orderId);
            if (orderToCancel == null)
            {
                _snackBar.Add("Đơn hàng không tồn tại!", Severity.Error);
                return;
            }

            // Tạo đối tượng AddEditOrderCommand với đầy đủ thông tin
            var command = new AddEditOrderCommand
            {
                Id = orderToCancel.Id,
                UserId = orderToCancel.UserId,
                UserName = orderToCancel.UserName,
                UserPhone = orderToCancel.UserPhone,
                UserAddress = orderToCancel.UserAddress,
                TotalPrice = orderToCancel.TotalPrice,
                MethodPayment = orderToCancel.MethodPayment,
                StatusOrder = "Đã Hủy", // Cập nhật trạng thái
                IsPayment = orderToCancel.IsPayment,
                OrderItem = orderToCancel.OrderItem.Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    ProductImage = item.ProductImage,
                    ProductPrice = item.ProductPrice,
                    Quantity = item.Quantity
                }).ToList()
            };

            // Gửi yêu cầu cập nhật tới API
            var response = await OrderManager.SaveAsync(command);

            if (response.Succeeded)
            {
                _snackBar.Add("Đơn hàng đã được hủy thành công!", Severity.Success);

                // Cập nhật danh sách hiển thị
                await LoadOrdersAsync();
            }
            else
            {
                _snackBar.Add("Không thể hủy đơn hàng. Vui lòng thử lại!", Severity.Error);
            }
        }





    }
}

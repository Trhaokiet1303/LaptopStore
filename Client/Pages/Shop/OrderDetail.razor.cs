using LaptopStore.Application.Features.Orders.Commands.Update;
using LaptopStore.Application.Features.Orders.Queries.GetAll;
using LaptopStore.Application.Features.Products.Queries.GetAllPaged;
using LaptopStore.Application.Features.Products.Queries.GetProductById;
using LaptopStore.Application.Requests.Catalog;
using LaptopStore.Client.Extensions;
using LaptopStore.Client.Infrastructure.Managers.Catalog.Order;
using LaptopStore.Client.Infrastructure.Managers.Catalog.Product;
using LaptopStore.Client.Shared.Dialogs;
using LaptopStore.Domain.Entities.Catalog;
using LaptopStore.Shared.Constants.Application;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
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
        [Inject] private IProductManager ProductManager { get; set; }
        [Inject] private NavigationManager Navigation { get; set; }
        [Inject] private IDialogService DialogService { get; set; }
        private IEnumerable<GetAllPagedProductsResponse> _pagedData;

        private bool IsLoading { get; set; }
        private List<GetAllOrdersResponse> Orders { get; set; } = new();
        private string SelectedStatus { get; set; }
        private bool IsRatedFilterApplied = false;
        protected override async Task OnInitializedAsync()
        {
            IsLoading = true;
            SelectedStatus = "";
            await LoadOrdersAsync(SelectedStatus);
            await LoadData(0, int.MaxValue, new TableState());
            IsLoading = false;
        }
        private async Task FilterAllOrders()
        {
            SelectedStatus = string.Empty;
            await FilterOrdersByStatusAsync(string.Empty);
        }
        private async Task FilterProcessingOrders() => await FilterOrdersByStatusAsync("Đặt thành công");
        private async Task FilterShippingOrders() => await FilterOrdersByStatusAsync("Đang giao");
        private async Task FilterDeliveredOrders() => await FilterOrdersByStatusAsync("Đã giao");
        private async Task FilterCanceledOrders() => await FilterOrdersByStatusAsync("Đã hủy");

        private async Task FilterOrdersByStatusAsync(string status)
        {
            SelectedStatus = status;
            IsLoading = true;

            var state = await AuthenticationProvider.GetAuthenticationStateAsync();
            var user = state.User;
            var userId = user.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                await JSRuntime.InvokeVoidAsync("alert", "Bạn phải đăng nhập để xem đơn hàng!");
                IsLoading = false;
                return;
            }

            var result = await OrderManager.GetAllForUserAsync();
            if (result.Succeeded)
            {
                Orders = result.Data
                            .Where(o => o.UserId == userId &&
                                        (string.IsNullOrEmpty(status) ||
                                        string.Equals(o.StatusOrder, status, StringComparison.OrdinalIgnoreCase)))
                            .ToList();
            }
            else
            {
                Orders.Clear();
                await JSRuntime.InvokeVoidAsync("alert", "Không thể tải danh sách đơn hàng!");
            }
            IsRatedFilterApplied = false;
            IsLoading = false;
        }

        private void FilterRatedProducts()
        {
            Orders = Orders
                .Where(order => order.OrderItem.Any(item => item.IsRated))
                .ToList();
            foreach (var order in Orders)
            {
                order.OrderItem = order.OrderItem.Where(item => item.IsRated).ToList();
            }
            IsRatedFilterApplied = true;
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

            var result = await OrderManager.GetAllForUserAsync();
            if (result.Succeeded)
            {
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
            return status == "Đã Hủy" || status == "Đang Giao" || status == "Đã Giao";
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
                _snackBar.Add("Trạng thái đơn hàng đã được cập nhật thành công!", Severity.Success);
                await LoadOrdersAsync();
            }
            else
            {
                _snackBar.Add($"Không thể cập nhật trạng thái đơn hàng. Lỗi: {response.Messages[0]}", Severity.Error);
            }

            IsLoading = false;
        }

        private async Task LoadData(int pageNumber, int pageSize, TableState state)
        {
            var request = new GetAllPagedProductsRequest
            {
                PageNumber = pageNumber + 1,
                PageSize = pageSize,
                SearchString = _searchString
            };
            var response = await ProductManager.GetProductsAsync(request);
            if (response.Succeeded)
            {
                _pagedData = response.Data;
                _totalItems = response.TotalCount;
            }
        }
        private int _totalItems;
        private int _currentPage;
        private string _searchString = "";

        private async Task InvokeModal(int id = 0, int orderitemid = 0)
        {
            var parameters = new DialogParameters();
            if (id != 0)
            {
                var product = _pagedData.FirstOrDefault(c => c.Id == id);
                if (product != null)
                {
                    parameters.Add(nameof(RateProduct.Product), new GetProductByIdResponse
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
            }
            parameters.Add("OrderItemId", orderitemid);

            var options = new DialogOptions
            {
                CloseButton = true,
                MaxWidth = MaxWidth.Medium,
                FullWidth = true,
                DisableBackdropClick = true

            };

            var dialog = _dialogService.Show<RateProduct>("Thông tin sản phẩm", parameters, options);
            var result = await dialog.Result;
        }
    }
}

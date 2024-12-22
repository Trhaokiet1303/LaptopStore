using LaptopStore.Application.Features.OrderItems.Commands.AddEdit;
using LaptopStore.Application.Features.Orders.Commands.AddEdit;
using LaptopStore.Application.Features.Orders.Commands.Update;
using LaptopStore.Application.Features.Orders.Queries.GetAll;
using LaptopStore.Application.Features.Orders.Queries.GetById;
using LaptopStore.Client.Infrastructure.Managers.Catalog.Order;
using LaptopStore.Client.Infrastructure.Managers.Catalog.OrderItem;
using LaptopStore.Client.Infrastructure.Managers.Catalog.Product;
using LaptopStore.Shared.Constants.Application;
using LaptopStore.Shared.Constants.Permission;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LaptopStore.Client.Pages.Admin.Orders
{
    public partial class OrderItems
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; }
        [CascadingParameter] private HubConnection HubConnection { get; set; }
        [Parameter] public int OrderId { get; set; } 
        [Inject] private IOrderManager OrderManager { get; set; } 
        [Inject] private IOrderItemManager OrderItemManager { get; set; } 
        [Inject] private IProductManager ProductManager { get; set; } 
        private List<GetOrderItemByIdResponse> orderItems { get; set; } = new();
        private List<GetAllOrderItemsResponse> _orderItemList = new();
        private GetAllOrderItemsResponse _orderItem = new();
        private ClaimsPrincipal _currentUser;
        private bool _canCreateOrderItems;
        private bool _canEditOrderItems;
        private bool _canDeleteOrderItems;
        private async Task LoadOrderItemsAsync()
        {
            _currentUser = await _authenticationManager.CurrentUser();
            _canCreateOrderItems = (await _authorizationService.AuthorizeAsync(_currentUser, Permissions.OrderItems.Create)).Succeeded;
            _canEditOrderItems = (await _authorizationService.AuthorizeAsync(_currentUser, Permissions.OrderItems.Edit)).Succeeded;
            _canDeleteOrderItems = (await _authorizationService.AuthorizeAsync(_currentUser, Permissions.OrderItems.Delete)).Succeeded;
            var response = await OrderManager.GetOrderByIdAsync(OrderId); 
            if (response.Succeeded)
            {
                orderItems = response.Data.OrderItem;
            }
            else
            {
                _snackBar.Add("Không thể tải dữ liệu.", Severity.Error);
            }
        }

        private void CloseDialog()
        {
            MudDialog.Close();
        }

        protected override async Task OnInitializedAsync()
        {
            await LoadOrderItemsAsync();
        }

        private async Task GetOrderItemsAsync()
        {
            var response = await OrderItemManager.GetAllAsync();
            if (response.Succeeded)
            {
                _orderItemList = response.Data.ToList();
            }
            else
            {
                foreach (var message in response.Messages)
                {
                    _snackBar.Add(message, Severity.Error);
                }
            }
        }
        private async Task InvokeAddModal()
        {
            var parameters = new DialogParameters
    {
        { nameof(AddEditOrderItemModal.AddEditOrderItemModel), new AddEditOrderItemCommand { OrderId = OrderId } }
    };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
            var dialog = _dialogService.Show<AddEditOrderItemModal>(_localizer["Add Order Item"], parameters, options);
            var result = await dialog.Result;

            if (!result.Cancelled)
            {
                await LoadOrderItemsAsync();

                var lastAddedOrderItem = orderItems.OrderByDescending(o => o.Id).FirstOrDefault();
                if (lastAddedOrderItem != null)
                {
                    await UpdateFeaturedStatusForProduct(lastAddedOrderItem.ProductId);
                }
            }
        }


        private async Task InvokeEditModal(int id)
        {
            var orderItemToEdit = orderItems.FirstOrDefault(x => x.Id == id);
            if (orderItemToEdit != null)
            {
                var parameters = new DialogParameters
        {
            { nameof(AddEditOrderItemModal.AddEditOrderItemModel), new AddEditOrderItemCommand
                {
                    Id = orderItemToEdit.Id,
                    OrderId = orderItemToEdit.OrderId,
                    ProductId = orderItemToEdit.ProductId,
                    Quantity = orderItemToEdit.Quantity,
                    TotalPrice = orderItemToEdit.TotalPrice
                }
            }
        };
                var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
                var dialog = _dialogService.Show<AddEditOrderItemModal>(_localizer["Edit"], parameters, options);
                var result = await dialog.Result;
                if (!result.Cancelled)
                {
                    await LoadOrderItemsAsync();

                    await UpdateFeaturedStatusForProduct(orderItemToEdit.ProductId);
                }
            }
        }

        private async Task Delete(int id, int orderId)
        {
            string deleteContent = _localizer["Delete Content"];
            var parameters = new DialogParameters
    {
        { nameof(Shared.Dialogs.DeleteConfirmation.ContentText), string.Format(deleteContent, id) }
    };
            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
            var dialog = _dialogService.Show<Shared.Dialogs.DeleteConfirmation>(_localizer["Delete"], parameters, options);
            var result = await dialog.Result;

            if (!result.Cancelled)
            {
                var response = await OrderItemManager.DeleteAsync(id);
                if (response.Succeeded)
                {
                    var orderItemToDelete = orderItems.FirstOrDefault(x => x.Id == id);
                    if (orderItemToDelete != null)
                    {
                        var productId = orderItemToDelete.ProductId;

                        // Cập nhật lại số lượng sản phẩm trong kho
                        var productResponse = await ProductManager.GetProductByIdAsync(productId);
                        if (productResponse.Succeeded)
                        {
                            var product = productResponse.Data;
                            var newQuantity = product.Quantity + orderItemToDelete.Quantity;

                            var updateProductQuantityResponse = await ProductManager.UpdateProductQuantityAsync(product.Id, newQuantity);
                            if (!updateProductQuantityResponse.Succeeded)
                            {
                                _snackBar.Add("Cập nhật số lượng sản phẩm thất bại.", Severity.Error);
                            }
                        }
                        await UpdateFeaturedStatusForProduct(productId);
                    }

                    var existingOrderItemResponse = await OrderManager.GetOrderByIdAsync(orderId);
                    if (existingOrderItemResponse.Succeeded && existingOrderItemResponse.Data.OrderItem != null)
                    {
                        var updatedTotalPrice = existingOrderItemResponse.Data.OrderItem.Sum(item =>
                            item.Quantity * item.ProductPrice);

                        var updateOrderCommand = new UpdateOrderTotalPriceCommand
                        {
                            OrderId = orderId,
                            TotalPrice = updatedTotalPrice
                        };

                        var orderUpdateResponse = await OrderManager.UpdateOrderTotalPriceAsync(updateOrderCommand);
                        if (orderUpdateResponse.Succeeded)
                        {
                            _snackBar.Add("Đã xóa sản phẩm và cập nhật tổng giá thành công.", Severity.Success);
                            await HubConnection.SendAsync("Cập nhật thành công.", orderId);
                            await HubConnection.SendAsync(ApplicationConstants.SignalR.SendUpdateDashboard);
                        }
                        else
                        {
                            _snackBar.Add("Cập nhật tổng giá thất bại.", Severity.Error);
                        }
                    }

                    await Reset();
                }
                else
                {
                    foreach (var message in response.Messages)
                    {
                        _snackBar.Add(message, Severity.Error);
                    }
                }
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
        private async Task Reset()
        {
            _orderItem = new GetAllOrderItemsResponse();
            await LoadOrderItemsAsync();
        }

    }
}

using LaptopStore.Application.Features.OrderItems.Commands.AddEdit;
using LaptopStore.Client.Infrastructure.Managers.Catalog.OrderItem;
using LaptopStore.Client.Infrastructure.Managers.Catalog.Product;
using LaptopStore.Client.Infrastructure.Managers.Catalog.Order;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.IO;
using System.Threading.Tasks;
using Blazored.FluentValidation;
using LaptopStore.Application.Requests;
using LaptopStore.Shared.Constants.Application;
using Microsoft.AspNetCore.SignalR.Client;
using LaptopStore.Client.Extensions;
using Microsoft.AspNetCore.Components.Forms;
using System.Linq;
using LaptopStore.Application.Features.Orders.Commands.Update;

namespace LaptopStore.Client.Pages.Admin.Orders
{
    public partial class AddEditOrderItemModal
    {
        [Inject] private IOrderItemManager OrderItemManager { get; set; }
        [Inject] private IOrderManager OrderManager { get; set; }
        [Inject] private IProductManager ProductManager { get; set; }
        [Parameter] public int OrderId { get; set; }

        [Parameter] public AddEditOrderItemCommand AddEditOrderItemModel { get; set; } = new();
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }
        [CascadingParameter] private HubConnection HubConnection { get; set; }

        private FluentValidationValidator _fluentValidationValidator;
        private bool Validated => _fluentValidationValidator.Validate(options => { options.IncludeAllRuleSets(); });

        protected override async Task OnInitializedAsync()
        {
            HubConnection = HubConnection.TryInitialize(_navigationManager);
            if (HubConnection.State == HubConnectionState.Disconnected)
            {
                await HubConnection.StartAsync();
            }

            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            if (OrderId != 0)
            {
                var orderItemResponse = await OrderManager.GetOrderByIdAsync(OrderId);
                if (orderItemResponse.Succeeded && orderItemResponse.Data.OrderItem != null && orderItemResponse.Data.OrderItem.Any())
                {
                    var orderItem = orderItemResponse.Data.OrderItem.First();
                    AddEditOrderItemModel = new AddEditOrderItemCommand
                    {
                        Id = orderItem.Id,
                        OrderId = orderItem.OrderId,
                        ProductId = orderItem.ProductId,
                        ProductName = orderItem.ProductName,
                        ProductPrice = orderItem.ProductPrice,
                        Quantity = orderItem.Quantity,
                        TotalPrice = orderItem.TotalPrice,
                        IsRated = orderItem.IsRated,
                    };
                    await LoadImageAsync();
                }
                else
                {
                    AddEditOrderItemModel = new AddEditOrderItemCommand { OrderId = OrderId };
                }
            }
        }

        private async Task LoadImageAsync()
        {
            var imageResponse = await ProductManager.GetProductImageAsync(AddEditOrderItemModel.ProductId);
            if (imageResponse.Succeeded)
            {
                AddEditOrderItemModel.ProductImage = imageResponse.Data;
            }
        }

        private async Task SaveAsync()
        {
            var productResponse = await ProductManager.GetProductByIdAsync(AddEditOrderItemModel.ProductId);
            if (productResponse.Succeeded)
            {
                AddEditOrderItemModel.ProductName = productResponse.Data.Name;
                AddEditOrderItemModel.ProductPrice = productResponse.Data.Price;
            }
            else
            {
                _snackBar.Add("Không tìm thấy sản phẩm.", Severity.Error);
                return;
            }

            var existingOrderItemResponse = await OrderManager.GetOrderByIdAsync(AddEditOrderItemModel.OrderId);
            if (existingOrderItemResponse.Succeeded && existingOrderItemResponse.Data.OrderItem != null)
            {
                var existingOrderItem = existingOrderItemResponse.Data.OrderItem
                    .FirstOrDefault(item => item.ProductId == AddEditOrderItemModel.ProductId);

                if (existingOrderItem != null)
                {
                    var quantityDifference = AddEditOrderItemModel.Quantity - existingOrderItem.Quantity;

                    if (quantityDifference <= productResponse.Data.Quantity || quantityDifference <= 0)
                    {
                        var updateCommand = new AddEditOrderItemCommand
                        {
                            Id = existingOrderItem.Id,
                            OrderId = existingOrderItem.OrderId,
                            ProductId = existingOrderItem.ProductId,
                            ProductName = existingOrderItem.ProductName,
                            ProductPrice = existingOrderItem.ProductPrice,
                            Quantity = AddEditOrderItemModel.Quantity,
                            TotalPrice = AddEditOrderItemModel.Quantity * existingOrderItem.ProductPrice,
                            IsRated = existingOrderItem.IsRated,
                        };

                        var updateResponse = await OrderItemManager.SaveAsync(updateCommand);
                        if (updateResponse.Succeeded)
                        {
                            var updatedTotalPrice = existingOrderItemResponse.Data.OrderItem.Sum(item =>
                                item.ProductId == AddEditOrderItemModel.ProductId
                                    ? AddEditOrderItemModel.Quantity * AddEditOrderItemModel.ProductPrice
                                    : item.Quantity * item.ProductPrice);

                            var updateOrderCommand = new UpdateOrderTotalPriceCommand
                            {
                                OrderId = AddEditOrderItemModel.OrderId,
                                TotalPrice = updatedTotalPrice
                            };

                            var orderUpdateResponse = await OrderManager.UpdateOrderTotalPriceAsync(updateOrderCommand);
                            if (orderUpdateResponse.Succeeded)
                            {
                                // Update product stock
                                var newStockQuantity = productResponse.Data.Quantity - quantityDifference;
                                var reduceStockResponse = await ProductManager.UpdateProductQuantityAsync(
                                    AddEditOrderItemModel.ProductId,
                                    newStockQuantity);

                                if (reduceStockResponse.Succeeded)
                                {
                                    _snackBar.Add("Cập nhật số lượng và tổng giá thành công.", Severity.Success);
                                    await HubConnection.SendAsync("Cập nhật thành công.", AddEditOrderItemModel.OrderId);
                                    await HubConnection.SendAsync(ApplicationConstants.SignalR.SendUpdateDashboard);
                                }
                                else
                                {
                                    _snackBar.Add("Cập nhật số lượng sản phẩm thất bại.", Severity.Error);
                                }
                            }
                            else
                            {
                                _snackBar.Add("Cập nhật tổng giá thất bại.", Severity.Error);
                            }
                            MudDialog.Close();
                        }
                        else
                        {
                            foreach (var message in updateResponse.Messages)
                            {
                                _snackBar.Add(message, Severity.Error);
                            }
                        }
                    }
                    else
                    {
                        _snackBar.Add($"Chỉ còn {productResponse.Data.Quantity} sản phẩm trong kho.", Severity.Error);
                    }
                }
                else
                {
                    var response = await OrderItemManager.SaveAsync(AddEditOrderItemModel);
                    if (response.Succeeded)
                    {
                        _snackBar.Add(response.Messages[0], Severity.Success);

                        var existingOrderResponse = await OrderManager.GetOrderByIdAsync(AddEditOrderItemModel.OrderId);
                        if (existingOrderResponse.Succeeded)
                        {
                            var updatedTotalPrice = existingOrderResponse.Data.OrderItem.Sum(item =>
                                item.ProductId == AddEditOrderItemModel.ProductId
                                    ? AddEditOrderItemModel.Quantity * AddEditOrderItemModel.ProductPrice
                                    : item.Quantity * item.ProductPrice);

                            var updateOrderCommand = new UpdateOrderTotalPriceCommand
                            {
                                OrderId = AddEditOrderItemModel.OrderId,
                                TotalPrice = updatedTotalPrice
                            };

                            var orderUpdateResponse = await OrderManager.UpdateOrderTotalPriceAsync(updateOrderCommand);
                            if (orderUpdateResponse.Succeeded)
                            {
                                // Reduce the stock of the product
                                var reduceStockResponse = await ProductManager.UpdateProductQuantityAsync(
                                    AddEditOrderItemModel.ProductId,
                                    productResponse.Data.Quantity - AddEditOrderItemModel.Quantity);

                                if (reduceStockResponse.Succeeded)
                                {
                                    _snackBar.Add("Thêm sản phẩm và cập nhật tổng giá thành công.", Severity.Success);
                                    await HubConnection.SendAsync("Cập nhật thành công.", AddEditOrderItemModel.OrderId);
                                    await HubConnection.SendAsync(ApplicationConstants.SignalR.SendUpdateDashboard);
                                }
                                else
                                {
                                    _snackBar.Add("Cập nhật số lượng sản phẩm thất bại.", Severity.Error);
                                }
                            }
                            else
                            {
                                _snackBar.Add("Cập nhật tổng giá thất bại.", Severity.Error);
                            }
                        }
                        else
                        {
                            _snackBar.Add("Không thể tải thông tin đơn hàng.", Severity.Error);
                        }

                        MudDialog.Close();
                        await HubConnection.SendAsync(ApplicationConstants.SignalR.SendUpdateDashboard);
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
            else
            {
                _snackBar.Add("Không thể tải thông tin sản phẩm.", Severity.Error);
            }
        }


        public void Cancel()
        {
            MudDialog.Cancel();
        }
    }
}

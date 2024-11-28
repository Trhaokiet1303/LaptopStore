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
<<<<<<< HEAD
using LaptopStore.Application.Features.Orders.Commands.Update;
=======
using LaptopStore.Application.Features.Orders.Commands.AddEdit;
>>>>>>> 3140a2f (add totalprice in orderitem)

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
            // Get the product details based on the ProductId
            var productResponse = await ProductManager.GetProductByIdAsync(AddEditOrderItemModel.ProductId);
            if (productResponse.Succeeded)
            {
                AddEditOrderItemModel.ProductName = productResponse.Data.Name;
                AddEditOrderItemModel.ProductPrice = productResponse.Data.Price;

<<<<<<< HEAD
=======
                // Check if the requested quantity exceeds the available stock
>>>>>>> 3140a2f (add totalprice in orderitem)
                if (AddEditOrderItemModel.Quantity > productResponse.Data.Quantity)
                {
                    _snackBar.Add($"Chỉ còn {productResponse.Data.Quantity} sản phẩm trong kho.", Severity.Error);
                    return;
                }
            }
            else
            {
                _snackBar.Add("Không tìm thấy sản phẩm.", Severity.Error);
                return;
            }

            // Get the order details
            var existingOrderItemResponse = await OrderManager.GetOrderByIdAsync(AddEditOrderItemModel.OrderId);
            if (existingOrderItemResponse.Succeeded && existingOrderItemResponse.Data.OrderItem != null)
            {
                var existingOrderItem = existingOrderItemResponse.Data.OrderItem
                    .FirstOrDefault(item => item.ProductId == AddEditOrderItemModel.ProductId);

                if (existingOrderItem != null)
                {
                    if (AddEditOrderItemModel.Quantity <= productResponse.Data.Quantity)
                    {
                        // Cập nhật số lượng sản phẩm
                        var updateCommand = new AddEditOrderItemCommand
                        {
                            Id = existingOrderItem.Id,
                            OrderId = existingOrderItem.OrderId,
                            ProductId = existingOrderItem.ProductId,
                            ProductName = existingOrderItem.ProductName,
                            ProductPrice = existingOrderItem.ProductPrice,
<<<<<<< HEAD
                            Quantity = AddEditOrderItemModel.Quantity
=======
                            Quantity = AddEditOrderItemModel.Quantity,
                            TotalPrice = AddEditOrderItemModel.Quantity * existingOrderItem.ProductPrice
>>>>>>> 3140a2f (add totalprice in orderitem)
                        };

                        var updateResponse = await OrderItemManager.SaveAsync(updateCommand);
                        if (updateResponse.Succeeded)
                        {
<<<<<<< HEAD
                            // Tính lại tổng giá và cập nhật ngay lập tức
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
                                _snackBar.Add("Cập nhật số lượng và tổng giá thành công.", Severity.Success);
                                await HubConnection.SendAsync("UpdateOrder", AddEditOrderItemModel.OrderId);
                                await HubConnection.SendAsync(ApplicationConstants.SignalR.SendUpdateDashboard);
                            }
                            else
                            {
                                _snackBar.Add("Cập nhật tổng giá thất bại.", Severity.Error);
                            }
=======
                            _snackBar.Add("Cập nhật số lượng sản phẩm thành công.", Severity.Success);
>>>>>>> 3140a2f (add totalprice in orderitem)

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
                        _snackBar.Add("Số lượng mới không thể lớn hơn số lượng hiện tại.", Severity.Error);
                    }
                }
<<<<<<< HEAD
=======
                else
                {
                    var response = await OrderItemManager.SaveAsync(AddEditOrderItemModel);
                    if (response.Succeeded)
                    {
                        _snackBar.Add(response.Messages[0], Severity.Success);


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
>>>>>>> 3140a2f (add totalprice in orderitem)
            }
            else
            {
                _snackBar.Add("Không thể tải thông tin đơn hàng.", Severity.Error);
            }
        }

<<<<<<< HEAD

=======
>>>>>>> 3140a2f (add totalprice in orderitem)
        public void Cancel()
        {
            MudDialog.Cancel();
        }
    }
}

﻿using LaptopStore.Application.Features.OrderItems.Commands.AddEdit;
using LaptopStore.Application.Features.Orders.Commands.AddEdit;
using LaptopStore.Application.Features.Orders.Queries.GetAll;
using LaptopStore.Application.Features.Orders.Queries.GetById;
using LaptopStore.Client.Infrastructure.Managers.Catalog.Order;
using LaptopStore.Client.Infrastructure.Managers.Catalog.OrderItem;
using LaptopStore.Shared.Constants.Application;
using LaptopStore.Shared.Constants.Permission;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;
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
                _snackBar.Add("Failed to load order items.", Severity.Error);
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
            }
        }

        private async Task InvokeEditModal(int id)
        {
            // Fetch the item to edit and pass it to the modal
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
                    Quantity = orderItemToEdit.Quantity
                }
            }
        };
                var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
                var dialog = _dialogService.Show<AddEditOrderItemModal>(_localizer["Edit"], parameters, options);
                var result = await dialog.Result;
                if (!result.Cancelled)
                {
                    await LoadOrderItemsAsync();
                }
            }
        }

        private async Task Delete(int id)
        {
            string deleteContent = _localizer["Delete Content"];
            var parameters = new DialogParameters
            {
                {nameof(Shared.Dialogs.DeleteConfirmation.ContentText), string.Format(deleteContent, id)}
            };
            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
            var dialog = _dialogService.Show<Shared.Dialogs.DeleteConfirmation>(_localizer["Delete"], parameters, options);
            var result = await dialog.Result;
            if (!result.Cancelled)
            {
                var response = await OrderItemManager.DeleteAsync(id);
                if (response.Succeeded)
                {
                    await Reset();
                    await HubConnection.SendAsync(ApplicationConstants.SignalR.SendUpdateDashboard);
                    _snackBar.Add(response.Messages[0], Severity.Success);
                }
                else
                {
                    await Reset();
                    foreach (var message in response.Messages)
                    {
                        _snackBar.Add(message, Severity.Error);
                    }
                }
            }
        }
        private async Task Reset()
        {
            _orderItem = new GetAllOrderItemsResponse();
            await LoadOrderItemsAsync();
        }

    }
}

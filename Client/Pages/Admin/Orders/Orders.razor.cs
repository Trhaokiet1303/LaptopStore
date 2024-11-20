using LaptopStore.Application.Features.Orders.Queries.GetAll;
using LaptopStore.Client.Extensions;
using LaptopStore.Shared.Constants.Application;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LaptopStore.Application.Features.Orders.Commands.AddEdit; 
using LaptopStore.Client.Infrastructure.Managers.Catalog.Order;
using LaptopStore.Shared.Constants.Permission;
using Microsoft.AspNetCore.Authorization;
using Microsoft.JSInterop;

namespace LaptopStore.Client.Pages.Admin.Orders
{
    public partial class Orders 
    {
        [Inject] private IOrderManager OrderManager { get; set; }

        [CascadingParameter] private HubConnection HubConnection { get; set; }

        private List<GetAllOrdersResponse> _orderList = new(); 
        private GetAllOrdersResponse _order = new();
        private string _searchString = "";
       
        private ClaimsPrincipal _currentUser;
        private bool _canCreateOrders;
        private bool _canEditOrders; 
        private bool _canDeleteOrders;
        private bool _canSearchOrders; 
        private bool _loaded;

        protected override async Task OnInitializedAsync()
        {
            _currentUser = await _authenticationManager.CurrentUser();
            _canCreateOrders = (await _authorizationService.AuthorizeAsync(_currentUser, Permissions.Orders.Create)).Succeeded; 
            _canEditOrders = (await _authorizationService.AuthorizeAsync(_currentUser, Permissions.Orders.Edit)).Succeeded; 
            _canDeleteOrders = (await _authorizationService.AuthorizeAsync(_currentUser, Permissions.Orders.Delete)).Succeeded; 
            _canSearchOrders = (await _authorizationService.AuthorizeAsync(_currentUser, Permissions.Orders.Search)).Succeeded; 

            await GetOrdersAsync(); 
            _loaded = true;
            HubConnection = HubConnection.TryInitialize(_navigationManager);
            if (HubConnection.State == HubConnectionState.Disconnected)
            {
                await HubConnection.StartAsync();
            }
        }

        private async Task GetOrdersAsync()
        {
            var response = await OrderManager.GetAllAsync();
            if (response.Succeeded)
            {
                _orderList = response.Data.ToList();
            }
            else
            {
                foreach (var message in response.Messages)
                {
                    _snackBar.Add(message, Severity.Error);
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
                var response = await OrderManager.DeleteAsync(id);
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

        private async Task InvokeModal(int id = 0)
        {
            var parameters = new DialogParameters();
            if (id != 0)
            {
                _order = _orderList.FirstOrDefault(c => c.Id == id);
                if (_order != null)
                {
                    parameters.Add(nameof(AddEditOrderModal.AddEditOrderModel), new AddEditOrderCommand
                    {
                        Id = _order.Id,
                        UserId = _order.UserId,
                    });
                }
            }
            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
            var dialog = _dialogService.Show<AddEditOrderModal>(id == 0 ? _localizer["Create"] : _localizer["Edit"], parameters, options);
            var result = await dialog.Result;
            if (!result.Cancelled)
            {
                await Reset();
            }
        }

        private async Task Reset()
        {
            _order = new GetAllOrdersResponse();
            await GetOrdersAsync();
        }

        private bool Search(GetAllOrdersResponse order)
        {
            if (string.IsNullOrWhiteSpace(_searchString)) return true;
            if (order.UserId?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
            {
                return true;
            }

            return false;
        }
        private async Task ViewProducts(int orderId)
        {
            var parameters = new DialogParameters
            {
                { "OrderId", orderId }
            };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
            _dialogService.Show<OrderItems>("Order Products", parameters, options);
        }
    }
}
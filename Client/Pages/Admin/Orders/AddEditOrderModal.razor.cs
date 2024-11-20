using LaptopStore.Client.Extensions;
using LaptopStore.Shared.Constants.Application;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;
using System.Threading.Tasks;
using Blazored.FluentValidation;
using LaptopStore.Application.Features.Orders.Commands.AddEdit; 
using LaptopStore.Client.Infrastructure.Managers.Catalog.Order;
using LaptopStore.Client.Infrastructure.Managers.Identity.Users;
using System.Collections.Generic;
using LaptopStore.Application.Responses.Identity;
using System.Linq;
using System;
using LaptopStore.Domain.Entities.Catalog;
using LaptopStore.Application.Features.Orders.Queries.GetById;

namespace LaptopStore.Client.Pages.Admin.Orders
{
    public partial class AddEditOrderModal 
    {
        [Inject] private IOrderManager OrderManager { get; set; }
        [Inject] private IUserManager UserManager { get; set; }

        [Parameter] public AddEditOrderCommand AddEditOrderModel { get; set; } = new();
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }
        [CascadingParameter] private HubConnection HubConnection { get; set; }

        private FluentValidationValidator _fluentValidationValidator;
        private List<UserResponse> _users = new();

        private bool Validated => _fluentValidationValidator.Validate(options => { options.IncludeAllRuleSets(); });

        public void Cancel()
        {
            MudDialog.Cancel();
        }

        private async Task SaveAsync()
        {
            var response = await OrderManager.SaveAsync(AddEditOrderModel);
            if (response.Succeeded)
            {
                _snackBar.Add(response.Messages[0], Severity.Success);
                MudDialog.Close();
            }
            else
            {
                foreach (var message in response.Messages)
                {
                    _snackBar.Add(message, Severity.Error);
                }
            }
            await HubConnection.SendAsync(ApplicationConstants.SignalR.SendUpdateDashboard);
        }

        private void MapOrder(GetOrderByIdResponse orderResponse)
        {
            AddEditOrderModel.UserId = orderResponse.UserId;
            AddEditOrderModel.UserName = orderResponse.UserName;
            AddEditOrderModel.UserPhone = orderResponse.UserPhone;
            AddEditOrderModel.UserAddress = orderResponse.UserAddress;
            AddEditOrderModel.TotalPrice = orderResponse.TotalPrice;
            AddEditOrderModel.MethodPayment = orderResponse.MethodPayment;
            AddEditOrderModel.StatusOrder = orderResponse.StatusOrder;
            AddEditOrderModel.IsPayment = orderResponse.IsPayment;
        }

        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();
            await LoadUsersAsync();

            if (AddEditOrderModel.Id != 0)
            {
                // Load the existing order data when editing
                var orderResponse = await OrderManager.GetOrderByIdAsync(AddEditOrderModel.Id); // Assuming you have this method.

                if (orderResponse.Succeeded)
                {
                    MapOrder(orderResponse.Data);
                }
                else
                {
                    _snackBar.Add(orderResponse.Messages.FirstOrDefault(), Severity.Error);
                }
            }

            HubConnection = HubConnection.TryInitialize(_navigationManager);
            if (HubConnection.State == HubConnectionState.Disconnected)
            {
                await HubConnection.StartAsync();
            }
        }



        private async Task LoadDataAsync()
        {
            await Task.CompletedTask;
        }

        private async Task LoadUsersAsync()
        {
            var data = await UserManager.GetAllAsync();
            if (data.Succeeded)
            {
                _users = data.Data;
            }
        }


        private async Task<IEnumerable<string>> SearchUsers(string value)
        {
            await Task.Delay(5);

            if (string.IsNullOrEmpty(value))
                return _users.Select(x => x.Id);

            return _users.Where(x => x.Email.Contains(value, StringComparison.InvariantCultureIgnoreCase))
                .Select(x => x.Id);
        }

    }
}
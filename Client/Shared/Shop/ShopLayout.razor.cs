using LaptopStore.Client.Extensions;
using LaptopStore.Client.Infrastructure.Managers.Identity.Roles;
using LaptopStore.Shared.Constants.Application;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;
using MudBlazor;
using System.Linq;
using System.Net.Http.Headers;
using System;
using LaptopStore.Client.Infrastructure.Authentication;
using LaptopStore.Client.Infrastructure.Managers.Identity.Account;
using LaptopStore.Client.Infrastructure.Managers.Identity.Authentication;
using LaptopStore.Client.Infrastructure.Managers.Identity.Users;
using System.Net.Http;

namespace LaptopStore.Client.Shared.Shop
{
    public partial class ShopLayout : LayoutComponentBase
    {
        [Inject] private NavigationManager NavigationManager { get; set; }
        [Inject] private IRoleManager RoleManager { get; set; }

        private string CurrentUserId { get; set; }
        private string ImageDataUrl { get; set; }
        private string FirstName { get; set; }
        private string SecondName { get; set; }
        private string Email { get; set; }
        private char FirstLetterOfName { get; set; }
        private string PageTitle { get; set; }
        private HubConnection hubConnection;

        public bool IsConnected => hubConnection.State == HubConnectionState.Connected;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                // Lấy thông tin cơ bản về trang hiện tại (ví dụ: URL để tùy chỉnh tiêu đề)
                var currentUri = NavigationManager.Uri;
                PageTitle = currentUri.Contains("home") ? "Home Page" : "Shop Page";

                // Register for interceptors and set up HubConnection
                _interceptor.RegisterEvent();
                hubConnection = hubConnection.TryInitialize(_navigationManager);

                // Start the connection to SignalR
                await hubConnection.StartAsync();

                // Load data about the user
                await LoadDataAsync();

                // Handle token regeneration and logout events
                hubConnection.On(ApplicationConstants.SignalR.ReceiveRegenerateTokens, async () =>
                {
                    try
                    {
                        var token = await _authenticationManager.TryForceRefreshToken();
                        if (!string.IsNullOrEmpty(token))
                        {
                            _snackBar.Add(localizer["Refreshed Token."], Severity.Success);
                            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        _snackBar.Add(localizer["You are Logged Out."], Severity.Error);
                        await _authenticationManager.Logout();
                        _navigationManager.NavigateTo("/");
                    }
                });

                hubConnection.On<string, string>(ApplicationConstants.SignalR.LogoutUsersByRole, async (userId, roleId) =>
                {
                    if (CurrentUserId != userId)
                    {
                        var rolesResponse = await RoleManager.GetRolesAsync();
                        if (rolesResponse.Succeeded)
                        {
                            var role = rolesResponse.Data.FirstOrDefault(x => x.Id == roleId);
                            if (role != null)
                            {
                                var currentUserRolesResponse = await _userManager.GetRolesAsync(CurrentUserId);
                                if (currentUserRolesResponse.Succeeded && currentUserRolesResponse.Data.UserRoles.Any(x => x.RoleName == role.Name))
                                {
                                    _snackBar.Add(localizer["You are logged out because the Permissions of one of your Roles have been updated."], Severity.Error);
                                    await hubConnection.SendAsync(ApplicationConstants.SignalR.OnDisconnect, CurrentUserId);
                                    await _authenticationManager.Logout();
                                    _navigationManager.NavigateTo("/login");
                                }
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _snackBar.Add(localizer["An error occurred while initializing the shop layout."], Severity.Error);
                Console.WriteLine(ex.Message);
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var state = await _stateProvider.GetAuthenticationStateAsync();
                var user = state.User;
                if (user == null || !user.Identity?.IsAuthenticated == true)
                {
                    _snackBar.Add(localizer["User is not authenticated."], Severity.Error);
                    return;
                }

                CurrentUserId = user.GetUserId();
                FirstName = user.GetFirstName();
                if (FirstName.Length > 0)
                {
                    FirstLetterOfName = FirstName[0];
                }
                SecondName = user.GetLastName();
                Email = user.GetEmail();

                // Fetch profile picture
                var imageResponse = await _accountManager.GetProfilePictureAsync(CurrentUserId);
                if (imageResponse.Succeeded)
                {
                    ImageDataUrl = imageResponse.Data;
                }

                // Validate user exists
                var currentUserResult = await _userManager.GetAsync(CurrentUserId);
                if (!currentUserResult.Succeeded || currentUserResult.Data == null)
                {
                    _snackBar.Add(localizer["You are logged out because the user with your Token has been deleted."], Severity.Error);
                    await _authenticationManager.Logout();
                    _navigationManager.NavigateTo("/login");
                    return;
                }

                await hubConnection.SendAsync(ApplicationConstants.SignalR.OnConnect, CurrentUserId);
            }
            catch (Exception ex)
            {
                _snackBar.Add(localizer["An error occurred while loading user data."], Severity.Error);
                Console.WriteLine(ex.Message);
            }
        }

        public void Dispose()
        {
            try
            {
                _interceptor.DisposeEvent();
                // Optionally, dispose of the SignalR connection here
                // await hubConnection.DisposeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error disposing resources: " + ex.Message);
            }
        }
    }
}

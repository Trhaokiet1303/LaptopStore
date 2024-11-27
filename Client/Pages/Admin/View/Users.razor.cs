using LaptopStore.Application.Responses.Identity;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LaptopStore.Shared.Constants.Application;
using LaptopStore.Shared.Constants.Permission;
using Microsoft.AspNetCore.Authorization;
using Microsoft.JSInterop;
using LaptopStore.Client.Pages.Identity;

namespace LaptopStore.Client.Pages.Admin.View
{
    public partial class Users
    {
        private List<UserResponse> _userList = new();
        private UserResponse _user = new();
        private string _searchString = "";

        private ClaimsPrincipal _currentUser;
        private bool _canCreateUsers;
        private bool _canSearchUsers;
        private bool _canDeleteUsers;
        private bool _canViewRoles;
        private bool _loaded;

        protected override async Task OnInitializedAsync()
        {
            _currentUser = await _authenticationManager.CurrentUser();
            _canCreateUsers = (await _authorizationService.AuthorizeAsync(_currentUser, Permissions.Users.Create)).Succeeded;
            _canSearchUsers = (await _authorizationService.AuthorizeAsync(_currentUser, Permissions.Users.Search)).Succeeded;
            _canDeleteUsers = (await _authorizationService.AuthorizeAsync(_currentUser, Permissions.Users.Delete)).Succeeded;
            _canViewRoles = (await _authorizationService.AuthorizeAsync(_currentUser, Permissions.Roles.View)).Succeeded;

            await GetUsersAsync();
            _loaded = true;
        }

        private async Task GetUsersAsync()
        {
            var response = await _userManager.GetAllAsync();
            if (response.Succeeded)
            {
                _userList = response.Data.ToList();
            }
            else
            {
                foreach (var message in response.Messages)
                {
                    _snackBar.Add(message, Severity.Error);
                }
            }
        }

        private bool Search(UserResponse user)
        {
            if (string.IsNullOrWhiteSpace(_searchString)) return true;
            if (user.FirstName?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
            {
                return true;
            }
            if (user.LastName?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
            {
                return true;
            }
            if (user.Email?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
            {
                return true;
            }
            if (user.PhoneNumber.ToString().Contains(_searchString))
            {
                return true;
            }
            if (user.UserName?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
            {
                return true;
            }
            return false;
        }

        private async Task InvokeModal()
        {
            var parameters = new DialogParameters();
            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
            var dialog = _dialogService.Show<RegisterUserModal>(_localizer["Register New User"], parameters, options);
            var result = await dialog.Result;
            if (!result.Cancelled)
            {
                await GetUsersAsync();
            }
        }

        private void ViewProfile(string userId)
        {
            _navigationManager.NavigateTo($"/user-profile/{userId}");
        }

        private async void ManageRoles(string userId)
        {
            // Kiểm tra nếu người dùng hiện tại có quyền quản lý vai trò
            var canManageRoles = await _authorizationService.AuthorizeAsync(_currentUser, Permissions.Roles.Edit);

            if (!canManageRoles.Succeeded)
            {
                // Nếu không có quyền, hiển thị thông báo lỗi
                _snackBar.Add(_localizer["You do not have permission to manage roles."], Severity.Error);
                return;
            }

            // Lấy email của người dùng hiện tại từ ClaimsPrincipal
            var currentUserEmail = _currentUser?.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(currentUserEmail))
            {
                _snackBar.Add(_localizer["Could not retrieve your email."], Severity.Error);
                return;
            }

            // Kiểm tra nếu người dùng có email không cho phép quản lý vai trò
            if (currentUserEmail == "mukesh@blazorhero.com")
            {
                _snackBar.Add(_localizer["Not Allowed."], Severity.Error);
                return;
            }

            // Nếu có quyền và không phải admin, chuyển hướng đến trang quản lý vai trò
            _navigationManager.NavigateTo($"/admin/user-roles/{userId}");
        }


        private async Task DeleteUserAsync(string userId)
        {
            var confirmDelete = await _dialogService.ShowMessageBox(
                _localizer["Confirm Deletion"],
                _localizer["Are you sure you want to delete this user?"],
                yesText: _localizer["Yes"],
                cancelText: _localizer["No"]
            );

            if (confirmDelete == true)
            {
                var response = await _userManager.DeleteUserAsync(userId);
                if (response.Succeeded)
                {
                    _snackBar.Add(_localizer["User deleted successfully"], Severity.Success);
                    await GetUsersAsync();
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

    }
}
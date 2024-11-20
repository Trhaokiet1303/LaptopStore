using LaptopStore.Application.Requests.Identity;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LaptopStore.Client.Pages.Admin.View
{
    public partial class UserProfile
    {
        [Parameter] public string Id { get; set; }
        [Parameter] public string Title { get; set; }
        [Parameter] public string Description { get; set; }

        private bool _active;
        private bool _confirm;
        private char _firstLetterOfName;
        private string _firstName;
        private string _lastName;
        private string _phoneNumber;
        private string _email;
        private bool _loaded;

        private async Task ToggleUserStatus()
        {
            var request = new ToggleUserStatusRequest { ActivateUser = _active, EmailConfirm = _confirm, UserId = Id };
            var result = await _userManager.ToggleUserStatusAsync(request);
            if (result.Succeeded)
            {
                _snackBar.Add(_localizer["Updated User Status."], Severity.Success);
                _navigationManager.NavigateTo("/admin/users");
            }
            else
            {
                foreach (var error in result.Messages)
                {
                    _snackBar.Add(error, Severity.Error);
                }
            }
        }

        [Parameter] public string ImageDataUrl { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var userId = Id;
            var result = await _userManager.GetAsync(userId);
            if (result.Succeeded)
            {
                var user = result.Data;
                if (user != null)
                {
                    _firstName = user.FirstName;
                    _lastName = user.LastName;
                    _email = user.Email;
                    _phoneNumber = user.PhoneNumber.ToString();
                    _active = user.IsActive;
                    _confirm = user.EmailConfirmed;
                    var data = await _accountManager.GetProfilePictureAsync(userId);
                    if (data.Succeeded)
                    {
                        ImageDataUrl = data.Data;
                    }
                }
                Title = $"{_firstName} {_lastName}'s {_localizer["Profile"]}";
                Description = _email;
                if (_firstName.Length > 0)
                {
                    _firstLetterOfName = _firstName[0];
                }
            }
            _loaded = true;
        }

    }
}
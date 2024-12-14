using LaptopStore.Application.Requests.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using System.Security.Claims;
using System.Threading.Tasks;
using Blazored.FluentValidation;
using System.Linq;

namespace LaptopStore.Client.Pages.Authentication
{
    public partial class Login
    {
        private FluentValidationValidator _fluentValidationValidator;
        private bool Validated => _fluentValidationValidator.Validate(options => { options.IncludeAllRuleSets(); });
        private TokenRequest _tokenModel = new();
        private ClaimsPrincipal _user;
        private bool _isAdmin;

        protected override async Task OnInitializedAsync()
        {
            var state = await _stateProvider.GetAuthenticationStateAsync();
            _user = state?.User;
            _isAdmin = !(_user?.IsInRole("Basic") ?? false);
            if (_user?.Identity?.IsAuthenticated == true)
            {
                if (!_isAdmin)
                {
                    _navigationManager.NavigateTo("/");
                }
                else
                {
                    _navigationManager.NavigateTo("/admin/dashboard"); 
                }
            }
            else
            {
                _navigationManager.NavigateTo("/login");
            }
        }

        private async Task SubmitAsync()
        {
            var result = await _authenticationManager.Login(_tokenModel);
            if (result.Succeeded)
            {
                _snackBar.Add(string.Format(_localizer["Welcome {0}"], _tokenModel.Email), Severity.Success);
                var state = await _stateProvider.GetAuthenticationStateAsync();
                _user = state?.User;
                _isAdmin = !(_user?.IsInRole("Basic") ?? false);

                if (!_isAdmin)
                {
                    _navigationManager.NavigateTo("/"); 
                }
                else
                {
                    _navigationManager.NavigateTo("/admin/dashboard");
                }
                _navigationManager.NavigateTo(_navigationManager.Uri, true);
            }
            else
            {
                foreach (var message in result.Messages)
                {
                    _snackBar.Add(message, Severity.Error);
                }
            }
        }

        private bool _passwordVisibility;
        private InputType _passwordInput = InputType.Password;
        private string _passwordInputIcon = Icons.Material.Filled.VisibilityOff;

        void TogglePasswordVisibility()
        {
            if (_passwordVisibility)
            {
                _passwordVisibility = false;
                _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
                _passwordInput = InputType.Password;
            }
            else
            {
                _passwordVisibility = true;
                _passwordInputIcon = Icons.Material.Filled.Visibility;
                _passwordInput = InputType.Text;
            }
        }
    }
}

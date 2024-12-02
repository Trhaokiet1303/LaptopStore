using LaptopStore.Application.Requests.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using System.Security.Claims;
using System.Threading.Tasks;
using Blazored.FluentValidation;

namespace LaptopStore.Client.Pages.Authentication
{
    public partial class Login
    {
        private FluentValidationValidator _fluentValidationValidator;
        private bool Validated => _fluentValidationValidator.Validate(options => { options.IncludeAllRuleSets(); });
        private TokenRequest _tokenModel = new();

        protected override async Task OnInitializedAsync()
        {
            var state = await _stateProvider.GetAuthenticationStateAsync();
            if (state?.User?.Identity?.IsAuthenticated != true)
            {
                _navigationManager.NavigateTo("/login");
            }
        }


        private async Task SubmitAsync()
        {
            var result = await _authenticationManager.Login(_tokenModel);
            if (result.Succeeded)
            {
                var state = await _stateProvider.GetAuthenticationStateAsync();
                var user = state?.User;

                if (user != null && user.IsInRole("basic"))
                {
                    _snackBar.Add(string.Format(_localizer["Welcome {0}"], _tokenModel.Email), Severity.Success);
                    _navigationManager.NavigateTo("/", true); 
                }
                else
                {
                    _snackBar.Add(string.Format(_localizer["Welcome {0}"], _tokenModel.Email), Severity.Success);
                    _navigationManager.NavigateTo("/admin/dashboard", true);
                }
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
            if(_passwordVisibility)
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
using LaptopStore.Application.Requests.Identity;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace LaptopStore.Application.Validators.Requests.Identity
{
    public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordRequestValidator(IStringLocalizer<ChangePasswordRequestValidator> localizer)
        {
            RuleFor(request => request.Password)
                .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage(x => localizer["Mật khẩu hiện tại không được trống!"]);
            RuleFor(request => request.NewPassword)
                .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage(x => localizer["Mật khẩu không được trống!"])
                .MinimumLength(8).WithMessage(localizer["Mật khẩu phải có ít nhất 8 ký tự"])
                .Matches(@"[A-Z]").WithMessage(localizer["Mật khẩu phải chứa ít nhất một chữ cái viết hoa"])
                .Matches(@"[a-z]").WithMessage(localizer["Mật khẩu phải chứa ít nhất một chữ cái viết thường"])
                .Matches(@"[0-9]").WithMessage(localizer["Mật khẩu phải chứa ít nhất một chữ số"]);
            RuleFor(request => request.ConfirmNewPassword)
                .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage(x => localizer["Xác nhận mật khẩu không được trống!"])
                .Equal(request => request.NewPassword).WithMessage(x => localizer["Mật khẩu không khớp"]);
        }
    }
}

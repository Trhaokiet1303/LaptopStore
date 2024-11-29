using LaptopStore.Application.Requests.Identity;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace LaptopStore.Application.Validators.Requests.Identity
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator(IStringLocalizer<RegisterRequestValidator> localizer)
        {
            RuleFor(request => request.FirstName)
                .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage(x => localizer["Tên không được trống"]);
            RuleFor(request => request.LastName)
                .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage(x => localizer["Họ không được trống"]);
            RuleFor(request => request.Email)
                .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage(x => localizer["Email không được trống"])
                .EmailAddress().WithMessage(x => localizer["Email không đúng định dạng"]);
            RuleFor(request => request.UserName)
                .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage(x => localizer["Tên đăng nhập không được trống"])
                .MinimumLength(6).WithMessage(localizer["Tên đăng nhập phải có ít nhất 6 ký tự"]);
            RuleFor(request => request.PhoneNumber)
                .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage(x => localizer["Số điện thoại không được trống"])
                .Length(10).WithMessage(localizer["Số điện thoại phải có đúng 10 ký tự"])
                .Matches(@"^\d{10}$").WithMessage(localizer["Số điện thoại phải là số"]);
            RuleFor(request => request.Password)
                .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage(x => localizer["Mật khẩu không được trống!"])
                .MinimumLength(8).WithMessage(localizer["Mật khẩu phải có ít nhất 8 ký tự"])
                .Matches(@"[A-Z]").WithMessage(localizer["Mật khẩu phải chứa ít nhất một chữ cái viết hoa"])
                .Matches(@"[a-z]").WithMessage(localizer["Mật khẩu phải chứa ít nhất một chữ cái viết thường"])
                .Matches(@"[0-9]").WithMessage(localizer["Mật khẩu phải chứa ít nhất một chữ số"]);
            RuleFor(request => request.ConfirmPassword)
                .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage(x => localizer["Xác nhận mật khẩu không được trống!"])
                .Equal(request => request.Password).WithMessage(x => localizer["Mật khẩu không khớp"]);
        }
    }
}

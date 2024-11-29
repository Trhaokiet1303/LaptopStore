using LaptopStore.Application.Features.Orders.Commands.AddEdit;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace LaptopStore.Application.Validators.Features.Order.Commands.AddEdit
{
    public class AddEditOrderCommandValidator : AbstractValidator<AddEditOrderCommand>
    {
        public AddEditOrderCommandValidator(IStringLocalizer<AddEditOrderCommandValidator> localizer)
        {
            RuleFor(request => request.UserAddress)
                .NotEmpty().WithMessage(localizer["Địa chỉ không được trống!"]);
            RuleFor(request => request.UserName)
                .NotEmpty().WithMessage(localizer["Tên người đặt không được trống!"]);
            RuleFor(request => request.UserPhone)
                .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage(x => localizer["Số điện thoại không được trống!"])
                .Length(10).WithMessage(localizer["số điện thoại phải đủ 10 kí tự!"])
                .Matches(@"^\d{10}$").WithMessage(localizer["Số điện thoại phải là số!"]);
            RuleFor(request => request.MethodPayment)
                .NotEmpty().WithMessage(localizer["Phương thức thanh toán không được trống!"]);
            RuleFor(request => request.StatusOrder)
                .NotEmpty().WithMessage(localizer["Trạng thái đơn hàng không được trống!"]);
        }
    }
}

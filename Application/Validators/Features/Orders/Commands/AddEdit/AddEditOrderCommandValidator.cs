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
                .NotEmpty().WithMessage(localizer["UserAddress is required!"]);
            RuleFor(request => request.UserName)
                .NotEmpty().WithMessage(localizer["UserName is required!"]);
            RuleFor(request => request.UserPhone)
                .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage(x => localizer["Phone Number is required"])
                .Length(10).WithMessage(localizer["Phone Number must be exactly 10 characters long"])
                .Matches(@"^\d{10}$").WithMessage(localizer["Phone Number must be number"]);
            RuleFor(request => request.MethodPayment)
                .NotEmpty().WithMessage(localizer["MethodPayment is required!"]);
            RuleFor(request => request.StatusOrder)
                .NotEmpty().WithMessage(localizer["StatusOrder is required!"]);
        }
    }
}

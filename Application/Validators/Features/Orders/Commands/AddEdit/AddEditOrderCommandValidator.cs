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
                .NotEmpty().WithMessage(localizer["UserPhone is required!"]);


            //RuleFor(request => request.ProductId)
            //    .GreaterThan(0).WithMessage(localizer["ProductId must be greater than 0!"]);

            //RuleFor(request => request.ProductName)
            //    .NotEmpty().WithMessage(localizer["ProductName is required!"]);

            //RuleFor(request => request.ProductQuanity)
            //    .GreaterThan(0).WithMessage(localizer["ProductQuanity must be greater than 0!"]);

            RuleFor(request => request.TotalPrice)
                .GreaterThan(0).WithMessage(localizer["TotalPrice must be greater than 0!"]);

            RuleFor(request => request.MethodPayment)
                .NotEmpty().WithMessage(localizer["MethodPayment is required!"]);

            RuleFor(request => request.StatusOrder)
                .NotEmpty().WithMessage(localizer["StatusOrder is required!"]);
        }
    }
}

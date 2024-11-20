using LaptopStore.Application.Features.Products.Commands.AddEdit;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace LaptopStore.Application.Validators.Features.Products.Commands.AddEdit
{
    public class AddEditProductCommandValidator : AbstractValidator<AddEditProductCommand>
    {
        public AddEditProductCommandValidator(IStringLocalizer<AddEditProductCommandValidator> localizer)
        {
            RuleFor(request => request.Name)
                .NotEmpty().WithMessage(localizer["Name is required!"]);

            RuleFor(request => request.Price)
                .GreaterThan(0).WithMessage(localizer["Price must be greater than 0!"]);

            RuleFor(request => request.Barcode)
                .NotEmpty().WithMessage(localizer["Barcode is required!"]);

            RuleFor(request => request.CPU)
                .NotEmpty().WithMessage(localizer["CPU is required!"]);

            RuleFor(request => request.Screen)
                .NotEmpty().WithMessage(localizer["Screen is required!"]);

            RuleFor(request => request.Card)
                .NotEmpty().WithMessage(localizer["Card is required!"]);

            RuleFor(request => request.Ram)
                .NotEmpty().WithMessage(localizer["Ram is required!"]);

            RuleFor(request => request.Rom)
                .NotEmpty().WithMessage(localizer["Rom is required!"]);

            RuleFor(request => request.Battery)
                .NotEmpty().WithMessage(localizer["Battery is required!"]);

            RuleFor(request => request.Weight)
                .NotEmpty().WithMessage(localizer["Weight is required!"]);

            RuleFor(request => request.Description)
                .NotEmpty().WithMessage(localizer["Description is required!"]);

            RuleFor(request => request.BrandId)
                .GreaterThan(0).WithMessage(localizer["Brand selection is required!"]);

            RuleFor(request => request.ProductLine)
                .NotEmpty().WithMessage(localizer["ProductLine is required!"]);

            RuleFor(request => request.Quantity)
                .GreaterThan(0).WithMessage(localizer["Quantity must be greater than 0!"]);

            RuleFor(request => request.Rate)
                .InclusiveBetween(1, 5).WithMessage(localizer["Rate must be between 1 and 5!"]);
        }
    }
}

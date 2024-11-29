using LaptopStore.Application.Features.OrderItems.Commands.AddEdit;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace LaptopStore.Application.Validators.Features.Order.Commands.AddEdit
{
    public class AddEditOrderItemCommandValidator : AbstractValidator<AddEditOrderItemCommand>
    {
        public AddEditOrderItemCommandValidator(IStringLocalizer<AddEditOrderItemCommandValidator> localizer)
        {
            RuleFor(request => request.ProductId)
                .GreaterThan(0).WithMessage(localizer["ID của sản phẩm phải lớn hơn 0!"]);
            RuleFor(request => request.Quantity)
                .GreaterThan(0).WithMessage(localizer["Số lượng phải lớn hơn 0!"]);
        }
    }
}

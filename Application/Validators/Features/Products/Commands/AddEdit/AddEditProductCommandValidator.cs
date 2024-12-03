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
                .NotEmpty().WithMessage(localizer["Tên không được trống!"]);

            RuleFor(request => request.Price)
                .GreaterThan(0).WithMessage(localizer["Giá phải lớn hơn 0!"]);

            RuleFor(request => request.Barcode)
                .NotEmpty().WithMessage(localizer["Mã vạch không được trống!"]);

            RuleFor(request => request.CPU)
                .NotEmpty().WithMessage(localizer["CPU không được trống!"]);

            RuleFor(request => request.Screen)
                .NotEmpty().WithMessage(localizer["Màn hình không được trống!"]);

            RuleFor(request => request.Card)
                .NotEmpty().WithMessage(localizer["Card không được trống!"]);

            RuleFor(request => request.Ram)
                .NotEmpty().WithMessage(localizer["Ram không được trống!"]);

            RuleFor(request => request.Rom)
                .NotEmpty().WithMessage(localizer["Rom không được trống!"]);

            RuleFor(request => request.Battery)
                .NotEmpty().WithMessage(localizer["Pin không được trống!"]);

            RuleFor(request => request.Weight)
                .NotEmpty().WithMessage(localizer["Cân nặng không được trống!"]);

            RuleFor(request => request.Description)
                .NotEmpty().WithMessage(localizer["Mô tả không được trống!"]);

            RuleFor(request => request.BrandId)
                .GreaterThan(0).WithMessage(localizer["Nhãn hàng không được trống!"]);

            RuleFor(request => request.ProductLine)
                .NotEmpty().WithMessage(localizer["Dòng sản phẩm không được trống!"]);

            RuleFor(request => request.Quantity)
                .GreaterThan(0).WithMessage(localizer["Số lượng phải lớn hơn 0!"]);

            RuleFor(request => request.Rate)
                .Must(rate => rate == 0 || (rate >= 1 && rate <= 5))
                .WithMessage(localizer["Đánh giá phải trong khoảng 1 đến 5 hoặc bằng 0!"]);
        }
    }
}

using LaptopStore.Application.Interfaces.Repositories;
using LaptopStore.Domain.Entities.Catalog;
using LaptopStore.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LaptopStore.Application.Features.Products.Commands.Update
{
    public class UpdateRateProductCommand : IRequest<Result<int>>
    {
        public int ProductId { get; set; }
        public decimal NewRate { get; set; }

        public class UpdateRateProductCommandHandler : IRequestHandler<UpdateRateProductCommand, Result<int>>
        {
            private readonly IUnitOfWork<int> _unitOfWork;
            private readonly IStringLocalizer<UpdateRateProductCommandHandler> _localizer;

            public UpdateRateProductCommandHandler(IUnitOfWork<int> unitOfWork, IStringLocalizer<UpdateRateProductCommandHandler> localizer)
            {
                _unitOfWork = unitOfWork;
                _localizer = localizer;
            }

            public async Task<Result<int>> Handle(UpdateRateProductCommand command, CancellationToken cancellationToken)
            {
                try
                {
                    var product = await _unitOfWork.Repository<Product>().GetByIdAsync(command.ProductId);

                    if (product == null)
                    {
                        return await Result<int>.FailAsync(_localizer["Không tìm thấy sản phẩm!"]);
                    }

                    if (product.Rate == 0)
                    {
                        product.Rate = command.NewRate;
                    }
                    else
                    {
                        product.Rate = (command.NewRate + product.Rate) / 2;
                    }
                    await _unitOfWork.Repository<Product>().UpdateAsync(product);
                    await _unitOfWork.Commit(cancellationToken);

                    return await Result<int>.SuccessAsync(product.Id, _localizer["Cập nhật đánh giá thành công."]);
                }
                catch (Exception ex)
                {
                    return await Result<int>.FailAsync($"Error: {ex.Message}");
                }
            }
        }
    }
}

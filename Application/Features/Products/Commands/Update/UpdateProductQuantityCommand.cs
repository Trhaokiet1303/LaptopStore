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
    public class UpdateProductQuantityCommand : IRequest<Result<int>>
    {
        public int ProductId { get; set; }
        public int NewQuantity { get; set; }

        public class UpdateProductQuantityCommandHandler : IRequestHandler<UpdateProductQuantityCommand, Result<int>>
        {
            private readonly IUnitOfWork<int> _unitOfWork;
            private readonly IStringLocalizer<UpdateProductQuantityCommandHandler> _localizer;

            public UpdateProductQuantityCommandHandler(IUnitOfWork<int> unitOfWork, IStringLocalizer<UpdateProductQuantityCommandHandler> localizer)
            {
                _unitOfWork = unitOfWork;
                _localizer = localizer;
            }

            public async Task<Result<int>> Handle(UpdateProductQuantityCommand command, CancellationToken cancellationToken)
            {
                try
                {
                    var product = await _unitOfWork.Repository<Product>().GetByIdAsync(command.ProductId);

                    if (product == null)
                    {
                        return await Result<int>.FailAsync(_localizer["Không tìm thấy sản phẩm!"]);
                    }

                    // Ensure the new quantity is not negative
                    if (command.NewQuantity < 0)
                    {
                        return await Result<int>.FailAsync(_localizer["Số lượng sản phẩm không hợp lệ!"]);
                    }

                    // Update the product quantity
                    product.Quantity = command.NewQuantity;

                    await _unitOfWork.Repository<Product>().UpdateAsync(product);
                    await _unitOfWork.Commit(cancellationToken);

                    return await Result<int>.SuccessAsync(product.Id, _localizer["Cập nhật số lượng sản phẩm thành công."]);
                }
                catch (Exception ex)
                {
                    return await Result<int>.FailAsync($"Error: {ex.Message}");
                }
            }
        }
    }
}

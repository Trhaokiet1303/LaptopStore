using LaptopStore.Application.Interfaces.Repositories;
using LaptopStore.Domain.Entities.Catalog;
using LaptopStore.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;

namespace LaptopStore.Application.Features.Products.Commands.Update
{
    public class UpdateProductFeaturedStatusCommand : IRequest<Result>
    {
        public int ProductId { get; set; }
    }

    public class UpdateProductFeaturedStatusCommandHandler : IRequestHandler<UpdateProductFeaturedStatusCommand, Result>
    {
        private readonly IUnitOfWork<int> _unitOfWork;
        private readonly IStringLocalizer<UpdateProductFeaturedStatusCommandHandler> _localizer;

        public UpdateProductFeaturedStatusCommandHandler(IUnitOfWork<int> unitOfWork, IStringLocalizer<UpdateProductFeaturedStatusCommandHandler> localizer)
        {
            _unitOfWork = unitOfWork;
            _localizer = localizer;
        }

        public async Task<Result> Handle(UpdateProductFeaturedStatusCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var product = await _unitOfWork.Repository<Product>().GetByIdAsync(command.ProductId);

                if (product == null)
                {
                    return Result<int>.Fail(_localizer["Không tìm thấy sản phẩm!"]); // Đảm bảo trả về kiểu Result, không phải IResult
                }

                // Tính tổng số lượng của sản phẩm trong tất cả các đơn hàng
                var totalQuantity = await _unitOfWork.Repository<Order>()
                    .Entities
                    .Where(order => order.OrderItem.Any(oi => oi.ProductId == product.Id))
                    .SelectMany(order => order.OrderItem)
                    .Where(oi => oi.ProductId == product.Id)
                    .SumAsync(oi => oi.Quantity);

                var currentRate = product.Rate;

                // Kiểm tra và cập nhật trạng thái Featured
                if (totalQuantity >= 10 && currentRate >= 4)
                {
                    product.Featured = true;
                }
                else
                {
                    product.Featured = false;
                }

                // Cập nhật trạng thái Featured trong cơ sở dữ liệu
                await _unitOfWork.Repository<Product>().UpdateAsync(product);
                await _unitOfWork.Commit(cancellationToken);

                return Result<int>.Success(_localizer["Cập nhật trạng thái Featured thành công."]); // Đảm bảo trả về kiểu Result
            }
            catch (Exception ex)
            {
                return Result<int>.Fail(_localizer[$"Đã xảy ra lỗi: {ex.Message}"]); // Đảm bảo trả về kiểu Result
            }
        }
    }
}

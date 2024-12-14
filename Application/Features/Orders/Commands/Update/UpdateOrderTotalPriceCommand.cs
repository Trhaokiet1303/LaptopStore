using LaptopStore.Application.Interfaces.Repositories;
using LaptopStore.Domain.Entities.Catalog;
using LaptopStore.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LaptopStore.Application.Features.Orders.Commands.Update
{
    public class UpdateOrderTotalPriceCommand : IRequest<Result<int>>
    {
        public int OrderId { get; set; }
        public long TotalPrice { get; set; }

        public class UpdateOrderTotalPriceCommandHandler : IRequestHandler<UpdateOrderTotalPriceCommand, Result<int>>
        {
            private readonly IUnitOfWork<int> _unitOfWork;
            private readonly IStringLocalizer<UpdateOrderTotalPriceCommandHandler> _localizer;

            public UpdateOrderTotalPriceCommandHandler(IUnitOfWork<int> unitOfWork, IStringLocalizer<UpdateOrderTotalPriceCommandHandler> localizer)
            {
                _unitOfWork = unitOfWork;
                _localizer = localizer;
            }

            public async Task<Result<int>> Handle(UpdateOrderTotalPriceCommand command, CancellationToken cancellationToken)
            {
                try
                {
                    var order = await _unitOfWork.Repository<Order>().GetByIdAsync(command.OrderId);

                    if (order == null)
                    {
                        return await Result<int>.FailAsync(_localizer["Không tìm thấy danh sách sản phẩm trong Đơn hàng!"]);
                    }

                    order.TotalPrice = command.TotalPrice;

                    await _unitOfWork.Repository<Order>().UpdateAsync(order);
                    await _unitOfWork.Commit(cancellationToken);

                    return await Result<int>.SuccessAsync(order.Id, _localizer["Cập nhật thành công."]);
                }
                catch (Exception ex)
                {
                    return await Result<int>.FailAsync($"Error: {ex.Message}");
                }
            }
        }
    }
}

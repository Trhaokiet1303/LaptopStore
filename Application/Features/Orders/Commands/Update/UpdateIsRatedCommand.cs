using LaptopStore.Application.Interfaces.Repositories;
using LaptopStore.Domain.Entities.Catalog;
using LaptopStore.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LaptopStore.Application.Features.OrderItems.Commands.Update
{
    public class UpdateIsRatedCommand : IRequest<Result<int>>
    {
        public int OrderItemId { get; set; }
        public bool IsRated { get; set; }
        public decimal Rate { get; set; }

        public class UpdateIsRatedCommandHandler : IRequestHandler<UpdateIsRatedCommand, Result<int>>
        {
            private readonly IUnitOfWork<int> _unitOfWork;
            private readonly IStringLocalizer<UpdateIsRatedCommandHandler> _localizer;

            public UpdateIsRatedCommandHandler(IUnitOfWork<int> unitOfWork, IStringLocalizer<UpdateIsRatedCommandHandler> localizer)
            {
                _unitOfWork = unitOfWork;
                _localizer = localizer;
            }

            public async Task<Result<int>> Handle(UpdateIsRatedCommand command, CancellationToken cancellationToken)
            {
                try
                {
                    // Tìm OrderItem theo ID
                    var orderItem = await _unitOfWork.Repository<OrderItem>().GetByIdAsync(command.OrderItemId);

                    if (orderItem == null)
                    {
                        return await Result<int>.FailAsync(_localizer["Không tìm thấy sản phẩm trong Đơn hàng!"]);
                    }

                    orderItem.IsRated = command.IsRated;
                    orderItem.Rate = command.Rate;

                    // Lưu thay đổi
                    await _unitOfWork.Repository<OrderItem>().UpdateAsync(orderItem);
                    await _unitOfWork.Commit(cancellationToken);

                    return await Result<int>.SuccessAsync(orderItem.Id, _localizer["Cập nhật trạng thái đánh giá thành công."]);
                }
                catch (Exception ex)
                {
                    return await Result<int>.FailAsync($"Error: {ex.Message}");
                }
            }
        }
    }
}

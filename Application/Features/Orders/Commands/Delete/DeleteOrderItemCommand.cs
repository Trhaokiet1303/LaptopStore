using LaptopStore.Application.Interfaces.Repositories;
using LaptopStore.Domain.Entities.Catalog;
using LaptopStore.Shared.Wrapper;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using LaptopStore.Shared.Constants.Application;

namespace LaptopStore.Application.Features.OrderItems.Commands.Delete
{
    public class DeleteOrderItemCommand : IRequest<Result<int>>
    {
        public int Id { get; set; }
    }

    internal class DeleteOrderItemCommandHandler : IRequestHandler<DeleteOrderItemCommand, Result<int>>
    {
        private readonly IStringLocalizer<DeleteOrderItemCommandHandler> _localizer;
        private readonly IUnitOfWork<int> _unitOfWork;

        public DeleteOrderItemCommandHandler(IUnitOfWork<int> unitOfWork, IStringLocalizer<DeleteOrderItemCommandHandler> localizer)
        {
            _unitOfWork = unitOfWork;
            _localizer = localizer;
        }

        public async Task<Result<int>> Handle(DeleteOrderItemCommand command, CancellationToken cancellationToken)
        {
            var orderItem = await _unitOfWork.Repository<OrderItem>().GetByIdAsync(command.Id);
            if (orderItem != null)
            {
                var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderItem.OrderId);

                if (order != null && (order.StatusOrder == "Đang Giao" || order.StatusOrder == "Đã Giao" || order.StatusOrder == "Đã Hủy"))
                {
                    return await Result<int>.FailAsync(_localizer["Không thể xóa sản phẩm trong đơn hàng {0}!", order.StatusOrder]);
                }
                await _unitOfWork.Repository<OrderItem>().DeleteAsync(orderItem);
                await _unitOfWork.CommitAndRemoveCache(cancellationToken, ApplicationConstants.Cache.GetAllOrderItemsCacheKey);
                return await Result<int>.SuccessAsync(orderItem.Id, _localizer["Xóa thành công"]);
            }
            else
            {
                return await Result<int>.FailAsync(_localizer["Không tìm thấy danh sách sản phẩm trong Đơn hàng!"]);
            }
        }
    }
}
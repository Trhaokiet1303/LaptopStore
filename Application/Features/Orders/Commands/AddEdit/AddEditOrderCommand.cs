using System.ComponentModel.DataAnnotations;
using AutoMapper;
using LaptopStore.Application.Interfaces.Repositories;
using LaptopStore.Domain.Entities.Catalog;
using LaptopStore.Shared.Wrapper;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using LaptopStore.Shared.Constants.Application;
using System.Collections.Generic;
using System.Linq;

namespace LaptopStore.Application.Features.Orders.Commands.AddEdit
{
    public partial class AddEditOrderCommand : IRequest<Result<int>>
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string UserName { get; set; }
        [Required]
        public string UserPhone { get; set; }
        [Required]
        public string UserAddress { get; set; }

        public List<OrderItem> OrderItem { get; set; } = new List<OrderItem>();

        [Required]
        public long TotalPrice { get; set; }

        [Required]
        public string MethodPayment { get; set; }

        [Required]
        public string StatusOrder { get; set; }

        [Required]
        public bool IsPayment { get; set; }
    }

    internal class AddEditOrderCommandHandler : IRequestHandler<AddEditOrderCommand, Result<int>>
    {
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<AddEditOrderCommandHandler> _localizer;
        private readonly IUnitOfWork<int> _unitOfWork;

        public AddEditOrderCommandHandler(IUnitOfWork<int> unitOfWork, IMapper mapper, IStringLocalizer<AddEditOrderCommandHandler> localizer)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizer = localizer;
        }

        public async Task<Result<int>> Handle(AddEditOrderCommand command, CancellationToken cancellationToken)
        {
            Order order = null;

            if (command.Id != 0)
            {
                order = await _unitOfWork.Repository<Order>().GetByIdAsync(command.Id);

                if (order == null)
                {
                    return await Result<int>.FailAsync(_localizer["Không tìm thấy đơn hàng!"]);
                }

                else if (order.StatusOrder == "Đã Giao" || order.StatusOrder == "Đã Hủy")
                {
                    return await Result<int>.FailAsync(_localizer["Không thể chỉnh sửa đơn hàng {0}!", order.StatusOrder]);
                }
                else if (order.StatusOrder == "Đang Giao")
                {
                    if ((command.UserId != order.UserId) ||
                    (command.UserName != order.UserName) ||
                    (command.UserPhone != order.UserPhone) ||
                    (command.UserAddress != order.UserAddress) ||
                    (command.MethodPayment != order.MethodPayment) ||
                    (command.IsPayment != order.IsPayment) ||
                    (command.OrderItem != null && command.OrderItem.Any()))
                    {
                        return await Result<int>.FailAsync(_localizer["Không thể chỉnh sửa thông tin khác ngoài trạng thái đơn hàng khi đơn hàng đang giao!"]);
                    }
                    if (!string.IsNullOrWhiteSpace(command.StatusOrder))
                    {
                        order.StatusOrder = command.StatusOrder;
                        await _unitOfWork.Repository<Order>().UpdateAsync(order);
                        await _unitOfWork.CommitAndRemoveCache(cancellationToken, ApplicationConstants.Cache.GetAllOrdersCacheKey);
                        return await Result<int>.SuccessAsync(order.Id, _localizer["Cập nhật trạng thái thành công"]);
                    }

                    return await Result<int>.FailAsync(_localizer["Không có trạng thái mới để cập nhật!"]);
                }
            }

            // Ensure OrderItem is not null
            command.OrderItem = command.OrderItem ?? new List<OrderItem>();

            // Calculate total price
            int totalPrice = 0;
            if (command.OrderItem.Any())
            {
                totalPrice = command.OrderItem.Sum(item => item.ProductPrice * item.Quantity);
            }

            if (command.Id == 0)
            {
                var newOrder = new Order
                {
                    UserId = command.UserId,
                    UserName = command.UserName,
                    UserPhone = command.UserPhone,
                    UserAddress = command.UserAddress,
                    TotalPrice = totalPrice,
                    MethodPayment = command.MethodPayment,
                    StatusOrder = command.StatusOrder,
                    IsPayment = command.IsPayment,
                    OrderItem = command.OrderItem.Select(item => new OrderItem
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        ProductImage = item.ProductImage,
                        ProductPrice = item.ProductPrice,
                        Quantity = item.Quantity,
                        IsRated = item.IsRated,
                        TotalPrice = item.ProductPrice * item.Quantity
                    }).ToList()
                };

                await _unitOfWork.Repository<Order>().AddAsync(newOrder);
                await _unitOfWork.CommitAndRemoveCache(cancellationToken, ApplicationConstants.Cache.GetAllOrdersCacheKey);
                return await Result<int>.SuccessAsync(newOrder.Id, _localizer["Lưu thành công"]);
            }
            else // Edit existing order
            {
                if (order != null)
                {
                    order.OrderItem = order.OrderItem ?? new List<OrderItem>();

                    order.UserId = command.UserId ?? order.UserId;
                    order.UserAddress = command.UserAddress ?? order.UserAddress;
                    order.UserName = command.UserName ?? order.UserName;
                    order.UserPhone = command.UserPhone ?? order.UserPhone;
                    order.MethodPayment = command.MethodPayment ?? order.MethodPayment;
                    order.StatusOrder = command.StatusOrder ?? order.StatusOrder;
                    order.IsPayment = command.IsPayment;

                    totalPrice = command.OrderItem.Any() ? command.OrderItem.Sum(item => item.ProductPrice * item.Quantity) : 0;

                    order.TotalPrice = totalPrice;

                    if (command.OrderItem != null)
                    {
                        foreach (var item in command.OrderItem)
                        {
                            var existingItem = order.OrderItem.FirstOrDefault(i => i.ProductId == item.ProductId);
                            if (existingItem != null)
                            {
                                existingItem.Quantity = item.Quantity;
                            }
                            else
                            {
                                order.OrderItem.Add(new OrderItem
                                {
                                    ProductId = item.ProductId,
                                    ProductName = item.ProductName,
                                    ProductImage = item.ProductImage,
                                    ProductPrice = item.ProductPrice,
                                    Quantity = item.Quantity,
                                    IsRated = item.IsRated,
                                    TotalPrice = item.ProductPrice * item.Quantity
                                });
                            }
                        }

                        // Remove items not in the new order
                        var itemsToRemove = order.OrderItem
                            .Where(existing => !command.OrderItem.Any(newItem => newItem.ProductId == existing.ProductId))
                            .ToList();
                        foreach (var itemToRemove in itemsToRemove)
                        {
                            order.OrderItem.Remove(itemToRemove);
                        }
                    }

                    await _unitOfWork.Repository<Order>().UpdateAsync(order);
                    await _unitOfWork.CommitAndRemoveCache(cancellationToken, ApplicationConstants.Cache.GetAllOrdersCacheKey);
                    return await Result<int>.SuccessAsync(order.Id, _localizer["Cập nhật thành công"]);
                }
                else
                {
                    return await Result<int>.FailAsync(_localizer["Không tìm thấy đơn hàng!"]);
                }
            }
        }
    }
}

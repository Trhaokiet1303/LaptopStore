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
        public int TotalPrice { get; set; }

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
            if (command.Id == 0)
            {

                var order = new Order
                {
                    UserId = command.UserId,
                    UserName = command.UserName,
                    UserPhone = command.UserPhone,
                    UserAddress = command.UserAddress,
                    TotalPrice = command.TotalPrice,
                    MethodPayment = command.MethodPayment,
                    StatusOrder = command.StatusOrder,
                    IsPayment = command.IsPayment,
                    OrderItem = command.OrderItem.Select(item => new OrderItem
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        ProductImage = item.ProductImage,
                        ProductPrice = item.ProductPrice,
                        Quantity = item.Quantity
                    }).ToList()
                };
                await _unitOfWork.Repository<Order>().AddAsync(order);
                await _unitOfWork.CommitAndRemoveCache(cancellationToken, ApplicationConstants.Cache.GetAllOrdersCacheKey);
                return await Result<int>.SuccessAsync(order.Id, _localizer["Order Saved"]);
            }
            else
            {
                var order = await _unitOfWork.Repository<Order>().GetByIdAsync(command.Id);
                if (order != null)
                {
                    order.UserId = command.UserId ?? order.UserId;
                    order.UserAddress = command.UserAddress ?? order.UserAddress;
                    order.UserName = command.UserName ?? order.UserName;
                    order.UserPhone = command.UserPhone ?? order.UserPhone;
                    order.TotalPrice = command.TotalPrice != 0 ? command.TotalPrice : order.TotalPrice;
                    order.MethodPayment = command.MethodPayment ?? order.MethodPayment;
                    order.StatusOrder = command.StatusOrder ?? order.StatusOrder;
                    order.IsPayment = command.IsPayment;

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
                                Quantity = item.Quantity,
                            });
                        }
                    }

                    var itemsToRemove = order.OrderItem.Where(existing => !command.OrderItem.Any(newItem => newItem.ProductId == existing.ProductId)).ToList();
                    foreach (var itemToRemove in itemsToRemove)
                    {
                        order.OrderItem.Remove(itemToRemove);
                    }

                    await _unitOfWork.Repository<Order>().UpdateAsync(order);
                    await _unitOfWork.CommitAndRemoveCache(cancellationToken, ApplicationConstants.Cache.GetAllOrdersCacheKey);
                    return await Result<int>.SuccessAsync(order.Id, _localizer["Order Updated"]);
                }
                else
                {
                    return await Result<int>.FailAsync(_localizer["Order Not Found!"]);
                }
            }
        }
    }
}

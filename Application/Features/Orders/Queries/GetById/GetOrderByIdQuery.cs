using AutoMapper;
using LaptopStore.Application.Interfaces.Repositories;
using LaptopStore.Domain.Entities.Catalog;
using LaptopStore.Shared.Wrapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LaptopStore.Application.Features.Orders.Queries.GetById
{
    public class GetOrderByIdQuery : IRequest<Result<GetOrderByIdResponse>>
    {
        public int Id { get; set; }
        public string UserId { get; set; }
    }

    internal class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<GetOrderByIdResponse>>
    {
        private readonly IUnitOfWork<int> _unitOfWork;
        private readonly IMapper _mapper;

        public GetOrderByIdQueryHandler(IUnitOfWork<int> unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<GetOrderByIdResponse>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var userId = request.UserId;
            var order = await _unitOfWork.Repository<Order>().Entities
                .Where(o => o.Id == request.Id && (userId == null || o.UserId == userId))
                .FirstOrDefaultAsync();
            if (order == null)
            {
                return await Result<GetOrderByIdResponse>.FailAsync("Order not found or you do not have permission to access it.");
            }
            var mappedOrder = _mapper.Map<GetOrderByIdResponse>(order);
            var orderItems = await _unitOfWork.Repository<OrderItem>().Entities
                .Where(c => c.OrderId == order.Id)
                .ToListAsync();
            long totalOrderPrice = 0;
            if (orderItems.Any())
            {
                mappedOrder.OrderItem = _mapper.Map<List<GetOrderItemByIdResponse>>(orderItems);
                foreach (var item in mappedOrder.OrderItem)
                {
                    var product = await _unitOfWork.Repository<Product>().GetByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        item.ProductName = product.Name;
                        item.ProductPrice = product.Price;
                        item.Quantity = orderItems.FirstOrDefault(c => c.ProductId == item.ProductId)?.Quantity ?? 0;
                        item.Instock = product.Quantity;
                        item.IsRated = item.IsRated;
                        item.Rate = item.Rate;
                        item.TotalPrice = item.Quantity * item.ProductPrice;
                        item.ProductImage = product.ImageDataURL;

                        totalOrderPrice += item.TotalPrice;
                    }
                    else
                    {
                        item.ProductName = "Unknown Product";
                        item.ProductPrice = 0;
                        item.Quantity = 0;
                        item.Instock = 0;
                        item.TotalPrice = 0;
                        item.IsRated = false;
                        item.Rate = 0;
                    }
                }
            }
            else
            {
                totalOrderPrice = 0;
            }
            mappedOrder.TotalPrice = totalOrderPrice;
            return await Result<GetOrderByIdResponse>.SuccessAsync(mappedOrder);
        }
    }
}
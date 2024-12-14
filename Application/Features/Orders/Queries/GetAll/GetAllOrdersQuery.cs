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

namespace LaptopStore.Application.Features.Orders.Queries.GetAll
{
    public class GetAllOrdersQuery : IRequest<Result<List<GetAllOrdersResponse>>>
    {
        public string UserId { get; set; }
    }

    internal class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, Result<List<GetAllOrdersResponse>>>
    {
        private readonly IUnitOfWork<int> _unitOfWork;
        private readonly IMapper _mapper;

        public GetAllOrdersQueryHandler(IUnitOfWork<int> unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<List<GetAllOrdersResponse>>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
        {
            var ordersQuery = _unitOfWork.Repository<Order>().Entities.AsQueryable();

            if (!string.IsNullOrEmpty(request.UserId))
            {
                ordersQuery = ordersQuery.Where(o => o.UserId == request.UserId);
            }

            var orders = await ordersQuery.ToListAsync(cancellationToken);

            var mappedOrders = _mapper.Map<List<GetAllOrdersResponse>>(orders);

            var orderItems = await _unitOfWork.Repository<OrderItem>().Entities
                .Where(oi => orders.Select(o => o.Id).Contains(oi.OrderId))
                .ToListAsync(cancellationToken);

            var products = await _unitOfWork.Repository<Product>().Entities
                .Where(p => orderItems.Select(oi => oi.ProductId).Contains(p.Id))
                .ToListAsync(cancellationToken);

            foreach (var order in mappedOrders)
            {
                var orderItemsForOrder = orderItems.Where(oi => oi.OrderId == order.Id).ToList();
                long totalOrderPrice = 0;

                if (orderItemsForOrder.Any())
                {
                    var mappedOrderItems = _mapper.Map<List<GetAllOrderItemsResponse>>(orderItemsForOrder);

                    foreach (var item in mappedOrderItems)
                    {
                        var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                        if (product != null)
                        {
                            item.ProductName = product.Name;
                            item.ProductPrice = product.Price;
                            item.Quantity = item.Quantity;
                            item.TotalPrice = item.Quantity * item.ProductPrice;
                            item.Instock = product.Quantity;
                            item.IsRated = item.IsRated;
                            item.Rate = item.Rate;
                            item.ProductImage = product.ImageDataURL;

                            totalOrderPrice += item.TotalPrice;
                        }
                        else
                        {
                            item.ProductName = "Unknown Product";
                            item.ProductPrice = 0;
                            item.Quantity = 0;
                            item.TotalPrice = 0;
                            item.Instock = 0;
                            item.IsRated = false;
                            item.Rate = 0;
                            item.ProductImage = null;
                        }
                    }

                    order.OrderItem = mappedOrderItems;
                }

                order.TotalPrice = totalOrderPrice;
            }

            return await Result<List<GetAllOrdersResponse>>.SuccessAsync(mappedOrders);
        }
    }
}

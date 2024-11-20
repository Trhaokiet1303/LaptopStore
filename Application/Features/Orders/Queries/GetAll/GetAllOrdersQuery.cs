using AutoMapper;
using LaptopStore.Application.Interfaces.Repositories;
using LaptopStore.Domain.Entities.Catalog;
using LaptopStore.Shared.Constants.Application;
using LaptopStore.Shared.Wrapper;
using LazyCache;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LaptopStore.Application.Features.Orders.Queries.GetAll
{
    public class GetAllOrdersQuery : IRequest<Result<List<GetAllOrdersResponse>>>
    {
        public GetAllOrdersQuery()
        {

        }
    }

    internal class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, Result<List<GetAllOrdersResponse>>>
    {
        private readonly IUnitOfWork<int> _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAppCache _cache;

        public GetAllOrdersQueryHandler(IUnitOfWork<int> unitOfWork, IMapper mapper, IAppCache cache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<Result<List<GetAllOrdersResponse>>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
        {
            Func<Task<List<Order>>> getAllOrders = () => _unitOfWork.Repository<Order>().GetAllAsync();
            var orderList = await _cache.GetOrAddAsync(ApplicationConstants.Cache.GetAllOrdersCacheKey, getAllOrders);

            var mappedOrders = _mapper.Map<List<GetAllOrdersResponse>>(orderList);

            foreach (var order in mappedOrders)
            {
                var cartItems = await _unitOfWork.Repository<OrderItem>().Entities
                    .Where(c => c.OrderId == order.Id) 
                    .ToListAsync(); 

                if (cartItems.Any()) 
                {
                    order.OrderItem = _mapper.Map<List<GetAllOrderItemsResponse>>(cartItems);

                    foreach (var item in order.OrderItem)
                    {
                        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(item.ProductId);
                        if (product != null)
                        {
                            item.ProductName = product.Name;
                            item.ProductPrice = product.Price;
                            item.Quantity = product.Quantity;
                            item.ProductImage=product.ImageDataURL;
                        }
                        else
                        {
                            item.ProductName = "Unknown Product";
                            item.ProductPrice = 0;
                            item.Quantity = 0;
                        }
                    }
                }
            }

            return await Result<List<GetAllOrdersResponse>>.SuccessAsync(mappedOrders);
        }
    }


}
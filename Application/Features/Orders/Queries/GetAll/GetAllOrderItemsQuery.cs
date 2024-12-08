using AutoMapper;
using LaptopStore.Application.Features.Orders.Queries.GetAll;
using LaptopStore.Application.Interfaces.Repositories;
using LaptopStore.Domain.Entities.Catalog;
using LaptopStore.Shared.Constants.Application;
using LaptopStore.Shared.Wrapper;
using LazyCache;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LaptopStore.Application.Features.Orders.Queries.GetAll
{
    public class GetAllOrderItemsQuery : IRequest<Result<List<GetAllOrderItemsResponse>>>
    {
        public GetAllOrderItemsQuery()
        {
        }
        public string UserId { get; set; }
    }

    internal class GetAllOrderItemsCachedQueryHandler : IRequestHandler<GetAllOrderItemsQuery, Result<List<GetAllOrderItemsResponse>>>
    {
        private readonly IUnitOfWork<int> _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAppCache _cache;

        public GetAllOrderItemsCachedQueryHandler(IUnitOfWork<int> unitOfWork, IMapper mapper, IAppCache cache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<Result<List<GetAllOrderItemsResponse>>> Handle(GetAllOrderItemsQuery request, CancellationToken cancellationToken)
        {
            Func<Task<List<OrderItem>>> getAllOrderItems = () => _unitOfWork.Repository<OrderItem>().GetAllAsync();
            var orderList = await _cache.GetOrAddAsync(ApplicationConstants.Cache.GetAllOrderItemsCacheKey, getAllOrderItems);
            var mappedOrders = _mapper.Map<List<GetAllOrderItemsResponse>>(orderList);
            if (!string.IsNullOrEmpty(request.UserId))
            {
                mappedOrders = mappedOrders.Where(order => order.Order.UserId == request.UserId).ToList();
            }
            return await Result<List<GetAllOrderItemsResponse>>.SuccessAsync(mappedOrders);
        }
    }

}
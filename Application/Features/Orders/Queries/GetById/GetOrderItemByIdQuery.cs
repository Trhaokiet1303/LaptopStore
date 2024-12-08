using AutoMapper;
using LaptopStore.Application.Features.Orders.Queries.GetById;
using LaptopStore.Application.Interfaces.Repositories;
using LaptopStore.Domain.Entities.Catalog;
using LaptopStore.Shared.Wrapper;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace LaptopStore.Application.Features.OrderItems.Queries.GetById
{
    public class GetOrderItemByIdQuery : IRequest<Result<GetOrderItemByIdResponse>>
    {
        public int Id { get; set; }
        public string UserId { get; set; }
    }

    internal class GetOrderItemByIdQueryHandler : IRequestHandler<GetOrderItemByIdQuery, Result<GetOrderItemByIdResponse>>
    {
        private readonly IUnitOfWork<int> _unitOfWork;
        private readonly IMapper _mapper;

        public GetOrderItemByIdQueryHandler(IUnitOfWork<int> unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<GetOrderItemByIdResponse>> Handle(GetOrderItemByIdQuery query, CancellationToken cancellationToken)
        {
            var repository = _unitOfWork.Repository<OrderItem>();

            var order = string.IsNullOrEmpty(query.UserId)
                ? await repository.GetByIdAsync(query.Id)
                : await repository.GetFirstAsync(o => o.Id == query.Id && o.Order.UserId == query.UserId);

            if (order == null)
            {
                return await Result<GetOrderItemByIdResponse>.FailAsync("Order not found or you do not have permission to access it.");
            }

            var mappedOrder = _mapper.Map<GetOrderItemByIdResponse>(order);
            return await Result<GetOrderItemByIdResponse>.SuccessAsync(mappedOrder);
        }

    }
}
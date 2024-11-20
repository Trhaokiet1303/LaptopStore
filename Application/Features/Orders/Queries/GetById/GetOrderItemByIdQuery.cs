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
            var order = await _unitOfWork.Repository<Domain.Entities.Catalog.OrderItem>().GetByIdAsync(query.Id);
            var mappedOrder = _mapper.Map<GetOrderItemByIdResponse>(order);
            return await Result<GetOrderItemByIdResponse>.SuccessAsync(mappedOrder);
        }
    }
}
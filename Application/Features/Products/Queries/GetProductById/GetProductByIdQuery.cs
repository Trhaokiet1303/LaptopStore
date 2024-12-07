using AutoMapper;
using LaptopStore.Application.Interfaces.Repositories;
using LaptopStore.Domain.Entities.Catalog;
using LaptopStore.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LaptopStore.Application.Features.Products.Queries.GetProductById
{
    public class GetProductByIdQuery : IRequest<Result<GetProductByIdResponse>>
    {
        public int Id { get; set; }

        public GetProductByIdQuery(int id)
        {
            Id = id;
        }

        public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<GetProductByIdResponse>>
        {
            private readonly IProductRepository _productRepository;
            private readonly IMapper _mapper;
            private readonly IStringLocalizer<GetProductByIdQueryHandler> _localizer;

            public GetProductByIdQueryHandler(
                IProductRepository productRepository,
                IMapper mapper,
                IStringLocalizer<GetProductByIdQueryHandler> localizer)
            {
                _productRepository = productRepository;
                _mapper = mapper;
                _localizer = localizer;
            }

            public async Task<Result<GetProductByIdResponse>> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
            {
                Console.WriteLine("Đang xử lý truy vấn với ID sản phẩm: " + query.Id);

                var product = await _productRepository.GetProductByIdAsync(query.Id);

                if (product != null)
                {
                    Console.WriteLine("Sản phẩm được tìm thấy: " + product.Name);

                    var productResponse = _mapper.Map<GetProductByIdResponse>(product);

                    Console.WriteLine("Tên sản phẩm sau khi ánh xạ: " + productResponse.Name);

                    return await Result<GetProductByIdResponse>.SuccessAsync(productResponse);
                }
                else
                {
                    Console.WriteLine("Sản phẩm không tồn tại.");
                    return await Result<GetProductByIdResponse>.FailAsync(_localizer["Product Not Found!"]);
                }
            }
        }

    }
}

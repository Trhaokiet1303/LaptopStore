using LaptopStore.Application.Interfaces.Repositories;
using LaptopStore.Domain.Entities.Catalog;
using LaptopStore.Shared.Wrapper;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using LaptopStore.Shared.Constants.Application;

namespace LaptopStore.Application.Features.Brands.Commands.Delete
{
    public class DeleteBrandCommand : IRequest<Result<int>>
    {
        public int Id { get; set; }
    }

    internal class DeleteBrandCommandHandler : IRequestHandler<DeleteBrandCommand, Result<int>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IStringLocalizer<DeleteBrandCommandHandler> _localizer;
        private readonly IUnitOfWork<int> _unitOfWork;

        public DeleteBrandCommandHandler(IUnitOfWork<int> unitOfWork, IProductRepository productRepository, IStringLocalizer<DeleteBrandCommandHandler> localizer)
        {
            _unitOfWork = unitOfWork;
            _productRepository = productRepository;
            _localizer = localizer;
        }

        public async Task<Result<int>> Handle(DeleteBrandCommand command, CancellationToken cancellationToken)
        {
            var isBrandUsed = await _productRepository.IsBrandUsed(command.Id);
            if (!isBrandUsed)
            {
                var brand = await _unitOfWork.Repository<Brand>().GetByIdAsync(command.Id);
                if (brand != null)
                {
                    await _unitOfWork.Repository<Brand>().DeleteAsync(brand);
                    await _unitOfWork.CommitAndRemoveCache(cancellationToken, ApplicationConstants.Cache.GetAllBrandsCacheKey);
                    return await Result<int>.SuccessAsync(brand.Id, _localizer["Xóa thành công"]);
                }
                else
                {
                    return await Result<int>.FailAsync(_localizer["Không tìm thấy nhãn hàng!"]);
                }
            }
            else
            {
                return await Result<int>.FailAsync(_localizer["Có sản phẩm của hãng này!"]);
            }
        }
    }
}
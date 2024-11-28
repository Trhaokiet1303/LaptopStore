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
using LaptopStore.Application.Requests;
using LaptopStore.Application.Interfaces.Services;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LaptopStore.Application.Features.Products.Commands.AddEdit;

namespace LaptopStore.Application.Features.OrderItems.Commands.AddEdit
{
    public partial class AddEditOrderItemCommand : IRequest<Result<int>>
    {
        public int Id { get; set; }
        [Required]
        public int ProductId { get; set; }
        [Required]
        public string ProductName { get; set; }
        [Required]
        public int ProductPrice { get; set; }
        public string ProductImage { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        public int TotalPrice { get; set; }
        [Required]
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public UploadRequest UploadRequest { get; set; }

    }

    internal class AddEditOrderItemCommandHandler : IRequestHandler<AddEditOrderItemCommand, Result<int>>
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork<int> _unitOfWork;
        private readonly IUploadService _uploadService;
        private readonly IStringLocalizer<AddEditOrderItemCommandHandler> _localizer;

        public AddEditOrderItemCommandHandler(IUnitOfWork<int> unitOfWork, IMapper mapper, IUploadService uploadService, IStringLocalizer<AddEditOrderItemCommandHandler> localizer)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _uploadService = uploadService;
            _localizer = localizer;
        }

        public async Task<Result<int>> Handle(AddEditOrderItemCommand command, CancellationToken cancellationToken)
        {
            var uploadRequest = command.UploadRequest;
            if (uploadRequest != null)
            {
                uploadRequest.FileName = $"P-{command.ProductId}{uploadRequest.Extension}";
            }

            if (command.Id == 0)
            {
                var orderItem = _mapper.Map<OrderItem>(command);
                if (uploadRequest != null)
                {
                    orderItem.ProductImage = _uploadService.UploadAsync(uploadRequest);
                }
                await _unitOfWork.Repository<OrderItem>().AddAsync(orderItem);
                await _unitOfWork.Commit(cancellationToken);
                return await Result<int>.SuccessAsync(orderItem.Id, _localizer["OrderItem Saved"]);
            }
            else
            {
                var orderItem = await _unitOfWork.Repository<OrderItem>().GetByIdAsync(command.Id);
                if (orderItem != null)
                {
                    orderItem.ProductId = (command.ProductId == 0) ? orderItem.ProductId : command.ProductId;
                    orderItem.ProductName = command.ProductName ?? orderItem.ProductName;
                    orderItem.ProductPrice = (command.ProductPrice == 0) ? orderItem.ProductPrice : command.ProductPrice;
                    orderItem.Quantity = (command.Quantity == 0) ? orderItem.Quantity : command.Quantity;
                    orderItem.OrderId = (command.OrderId == 0) ? orderItem.OrderId : command.OrderId;
                    orderItem.TotalPrice = command.Quantity * command.ProductPrice;
                   
                    if (uploadRequest != null)
                    {
                        orderItem.ProductImage = _uploadService.UploadAsync(uploadRequest);
                    }
                   
                    await _unitOfWork.Repository<OrderItem>().UpdateAsync(orderItem);
                    await _unitOfWork.Commit(cancellationToken);
                    return await Result<int>.SuccessAsync(orderItem.Id, _localizer["OrderItem Updated"]);
                }
                else
                {
                    return await Result<int>.FailAsync(_localizer["OrderItem Not Found!"]);
                }
            }
        }
    }
}
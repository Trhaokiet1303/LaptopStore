using LaptopStore.Application.Features.Orders.Queries.GetAll;
using LaptopStore.Application.Features.Orders.Queries.GetById;
using LaptopStore.Shared.Constants.Permission;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using LaptopStore.Application.Features.Orders.Commands.AddEdit;
using LaptopStore.Application.Features.Orders.Commands.Delete;
using LaptopStore.Client.Pages.Shop;
using LaptopStore.Domain.Entities.Catalog;
using System;
using System.Text.Json;
using System.Linq;
using LaptopStore.Application.Features.Products.Queries.GetProductById;
using LaptopStore.Application.Features.Brands.Queries.GetById;

namespace LaptopStore.Server.Controllers.v1.Catalog
{
    public class OrdersController : BaseApiController<OrdersController>
    {

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _mediator.Send(new GetAllOrdersQuery());
            return Ok(orders);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _mediator.Send(new GetOrderByIdQuery { Id = id });
            return Ok(order);
        }

        [Authorize(Policy = Permissions.Orders.Create)]
        [HttpPost("add-edit")]
        public async Task<IActionResult> Post(AddEditOrderCommand command)
        {    
            return Ok(await _mediator.Send(command));
        }

        [AllowAnonymous]
        [HttpPost("create-order")]
        public async Task<IActionResult> Post(Domain.Entities.Catalog.Order command)
        {
            var addEditOrderCommand = new AddEditOrderCommand
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
                    ProductPrice = item.ProductPrice,
                    ProductImage = item.ProductImage,
                    Quantity = item.Quantity
                }).ToList()
            };
            return Ok(await _mediator.Send(addEditOrderCommand));
        }



        [Authorize(Policy = Permissions.Orders.Delete)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return Ok(await _mediator.Send(new DeleteOrderCommand { Id = id }));
        }
    }
}
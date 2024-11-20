using LaptopStore.Application.Features.Orders.Queries.GetAll;
using LaptopStore.Application.Features.OrderItems.Queries.GetById;
using LaptopStore.Shared.Constants.Permission;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using LaptopStore.Application.Features.OrderItems.Commands.AddEdit;
using LaptopStore.Application.Features.OrderItems.Commands.Delete;
using LaptopStore.Client.Pages.Shop;
using LaptopStore.Domain.Entities.Catalog;
using System;
using System.Text.Json;
using System.Linq;
using LaptopStore.Application.Features.Orders.Commands.AddEdit;

namespace LaptopStore.Server.Controllers.v1.Catalog
{
    public class OrderItemsController : BaseApiController<OrderItemsController>
    {

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orderitems = await _mediator.Send(new GetAllOrderItemsQuery());
            return Ok(orderitems);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var orderitems = await _mediator.Send(new GetOrderItemByIdQuery { Id = id });
            return Ok(orderitems);
        }

        [Authorize(Policy = Permissions.OrderItems.Create)]
        [HttpPost("add-edit")]
        public async Task<IActionResult> Post(AddEditOrderItemCommand command)
        {
            
            return Ok(await _mediator.Send(command));
        }

        [Authorize(Policy = Permissions.OrderItems.Delete)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return Ok(await _mediator.Send(new DeleteOrderItemCommand { Id = id }));
        }
    }
}
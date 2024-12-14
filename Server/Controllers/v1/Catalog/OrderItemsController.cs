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
using MediatR;
using Microsoft.AspNetCore.Identity;
using LaptopStore.Infrastructure.Models.Identity;
using LaptopStore.Application.Features.Orders.Commands.Update;
using LaptopStore.Application.Features.OrderItems.Commands.Update;

namespace LaptopStore.Server.Controllers.v1.Catalog
{
    public class OrderItemsController : BaseApiController<OrderItemsController>
    {
        private readonly UserManager<User> _userManager;

        public OrderItemsController(UserManager<User> userManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var userId = user.Id;
            var hasPermission = User.HasClaim(c => c.Type == "Permission" && c.Value == Permissions.OrderItems.View);

            if (!hasPermission)
            {
                return Forbid();
            }

            var ordersQuery = new GetAllOrdersQuery
            {
                UserId = userId
            };

            return Ok(await _mediator.Send(ordersQuery));
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var userId = user.Id;
            var hasPermission = User.HasClaim(c => c.Type == "Permission" && c.Value == Permissions.OrderItems.View);

            if (!hasPermission)
            {
                return Forbid();
            }

            var orderItemQuery = new GetOrderItemByIdQuery
            {
                Id = id,
                UserId = userId
            };

            return Ok(await _mediator.Send(orderItemQuery));
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
        [HttpPost("update-is-rated")]
        public async Task<IActionResult> UpdateIsRated(UpdateIsRatedCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}

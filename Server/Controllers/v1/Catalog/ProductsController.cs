using LaptopStore.Application.Features.Products.Commands.AddEdit;
using LaptopStore.Application.Features.Products.Commands.Delete;
using LaptopStore.Application.Features.Products.Commands.Update;
using LaptopStore.Application.Features.Products.Queries.GetAllPaged;
using LaptopStore.Application.Features.Products.Queries.GetProductById;
using LaptopStore.Application.Features.Products.Queries.GetProductImage;
using LaptopStore.Shared.Constants.Permission;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LaptopStore.Server.Controllers.v1.Catalog
{
    public class ProductsController : BaseApiController<ProductsController>
    {

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll(int pageNumber, int pageSize, string searchString, string orderBy = null)
        {
            var products = await _mediator.Send(new GetAllProductsQuery(pageNumber, pageSize, searchString, orderBy));
            return Ok(products);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductByIdAsync(int id)
        {
            var productDetail = await _mediator.Send(new GetProductByIdQuery(id) { Id = id });
            return Ok(productDetail);
        }

        [AllowAnonymous]
        [HttpGet("image/{id}")]
        public async Task<IActionResult> GetProductImageAsync(int id)
        {
            var result = await _mediator.Send(new GetProductImageQuery(id));
            return Ok(result);
        }

        [Authorize(Policy = Permissions.Products.Create)]
        [HttpPost]
        public async Task<IActionResult> Post(AddEditProductCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [Authorize(Policy = Permissions.Products.Delete)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return Ok(await _mediator.Send(new DeleteProductCommand { Id = id }));
        }

        [HttpPost("GetAllPaged")]
        public async Task<IActionResult> GetAllPaged([FromBody] GetAllProductsQuery request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [Authorize]
        [HttpPut("update-rate")]
        public async Task<IActionResult> UpdateRate([FromBody] UpdateRateProductCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpPut("UpdateFeaturedStatus")]
        public async Task<IActionResult> UpdateFeaturedStatus([FromBody] UpdateProductFeaturedStatusCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpPut("update-quantity")]
        public async Task<IActionResult> UpdateProductQuantityAsync([FromBody] UpdateProductQuantityCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
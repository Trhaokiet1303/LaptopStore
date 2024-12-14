using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using LaptopStore.Infrastructure.Models.Identity;
using LaptopStore.Application.Features.Dashboards.Queries.GetData;

namespace LaptopStore.Server.Controllers.v1
{
    [ApiController]
    public class DashboardController : BaseApiController<DashboardController>
    {
        private readonly UserManager<User> _userManager;

        public DashboardController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetDataAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Basic"))
            {
                return Forbid();
            }
            var result = await _mediator.Send(new GetDashboardDataQuery());
            return Ok(result);
        }
    }
}

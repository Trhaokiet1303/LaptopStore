using LaptopStore.Application.Interfaces.Services.Identity;
using LaptopStore.Application.Requests.Identity;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Threading.Tasks;

[Route("api/identity/token")]
[ApiController]
public class TokenController : ControllerBase
{
    private readonly ITokenService _identityService;

    public TokenController(ITokenService identityService)
    {
        _identityService = identityService;
    }

    [HttpPost]
    public async Task<ActionResult> Get(TokenRequest model)
    {
        var origin = Request.Headers["Origin"];

        if (string.IsNullOrEmpty(origin))
        {
            origin = "http://default-origin.com"; 
        }

        var response = await _identityService.LoginAsync(model, origin);
        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult> Refresh([FromBody] RefreshTokenRequest model)
    {
        var response = await _identityService.GetRefreshTokenAsync(model);
        return Ok(response);
    }
}

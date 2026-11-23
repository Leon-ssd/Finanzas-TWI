using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanzasPersonales.Api.Controllers;

[ApiController]
[Route("api/auth/spotify")]
public class SpotifyAuthController : ControllerBase
{
    private const string ProviderScheme = "Spotify";

    [HttpGet("login")]
    public IActionResult Login([FromQuery] string returnUrl = "/")
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = returnUrl
        };
        return Challenge(properties, ProviderScheme);
    }

    [Authorize]
    [HttpGet("user")]
    public IActionResult GetSpotifyUser()
    {
        var provider = User.FindFirst("provider")?.Value;
        if (provider != ProviderScheme)
        {
            return BadRequest(new { mensaje = "La sesión activa no pertenece al proveedor Spotify." });
        }

        return Ok(new
        {
            proveedor = ProviderScheme,
            usuarioId = User.FindFirst("usuario_id")?.Value,
            nombre = User.Identity?.Name,
            email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
        });
    }
}

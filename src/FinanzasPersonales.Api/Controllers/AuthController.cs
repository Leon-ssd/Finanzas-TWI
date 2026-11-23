using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanzasPersonales.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        if (User.Identity == null || !User.Identity.IsAuthenticated)
        {
            return Ok(new
            {
                isAuthenticated = false,
                mensaje = "No hay una sesión activa."
            });
        }

        var usuarioId = User.FindFirst("usuario_id")?.Value
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        var nombre = User.Identity.Name
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? string.Empty;

        var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var proveedor = User.FindFirst("provider")?.Value ?? "Local";

        return Ok(new
        {
            isAuthenticated = true,
            usuarioId,
            nombre,
            email,
            proveedor
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromQuery] string returnUrl = "/")
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (HttpContext.Features.Get<Microsoft.AspNetCore.Http.Features.ISessionFeature>() != null)
        {
            HttpContext.Session.Clear();
        }

        if (Request.Headers.Accept.ToString().Contains("application/json"))
        {
            return Ok(new { mensaje = "Sesión cerrada correctamente." });
        }

        return Redirect(returnUrl);
    }
}
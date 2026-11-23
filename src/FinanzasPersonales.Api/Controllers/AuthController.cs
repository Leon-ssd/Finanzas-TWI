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

        return Ok(new
        {
            isAuthenticated = true,
            usuarioId = User.FindFirst("usuario_id")?.Value,
            nombre = User.Identity.Name,
            email = User.FindFirst(ClaimTypes.Email)?.Value,
            proveedor = User.FindFirst("provider")?.Value ?? "Local"
        });
    }

    [HttpPost("logout")]
    [HttpGet("logout")]
    public async Task<IActionResult> Logout([FromQuery] string returnUrl = "/")
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (Request.Headers.Accept.ToString().Contains("application/json"))
        {
            return Ok(new { mensaje = "Sesión cerrada correctamente." });
        }
        return Redirect(returnUrl);
    }
}

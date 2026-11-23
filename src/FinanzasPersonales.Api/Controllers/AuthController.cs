using System.Security.Claims;
using FinanzasPersonales.Api;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MyApp.Namespace
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [Authorize]
        [HttpGet("me")]
        public ActionResult<UsuarioSesionDto> Me()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid.TryParse(idClaim, out var usuarioId);

            var dto = new UsuarioSesionDto
            {
                Autenticado = User.Identity?.IsAuthenticated ?? false,
                UsuarioId = usuarioId,
                Nombre = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
                Email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            };

            return Ok(dto);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();

            return Ok(new { message = "Sesión cerrada correctamente." });
        }
    }
}

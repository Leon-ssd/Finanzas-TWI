using System.Security.Claims;
using FinanzasPersonales.Api.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanzasPersonales.Api.Controllers;

[AllowAnonymous]
[Route("auth/dev3")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class Dev3SessionController(IAuthenticationSchemeProvider schemes) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? error)
    {
        ViewData["DropboxConfigured"] = await schemes.GetSchemeAsync(Dev3Authentication.DropboxScheme) is not null;
        ViewData["MicrosoftConfigured"] = await schemes.GetSchemeAsync(Dev3Authentication.MicrosoftScheme) is not null;
        // Lista cerrada: nunca mostrar texto no confiable devuelto por un proveedor.
        ViewData["Error"] = error switch
        {
            "dropbox-not-configured" => "Dropbox aún no tiene ClientId y ClientSecret configurados.",
            "microsoft-not-configured" => "Microsoft aún no tiene ClientId y ClientSecret configurados.",
            "dropbox-cancelled" or "microsoft-cancelled" => "Inicio de sesión cancelado.",
            "dropbox-failed" or "microsoft-failed" => "No se pudo iniciar sesión. Vuelve a intentarlo o revisa la configuración del proveedor.",
            _ => null
        };
        return View("~/Views/Dev3Session/Index.cshtml");
    }

    [HttpGet("session")]
    public IActionResult Session() => Ok(new
    {
        authenticated = User.Identity?.IsAuthenticated == true,
        provider = User.FindFirstValue(Dev3Authentication.ProviderClaim),
        id = User.FindFirstValue(ClaimTypes.NameIdentifier),
        name = User.Identity?.Name,
        email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email")
    });

    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(Dev3Authentication.LocalScheme);
        return Redirect(Dev3Authentication.LoginPath);
    }
}

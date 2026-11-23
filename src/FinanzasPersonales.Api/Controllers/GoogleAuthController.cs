using FinanzasPersonales.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MyApp.Namespace
{
    [Route("api/auth/google")]
    [ApiController]
    public class GoogleAuthController : ControllerBase
    {
        private readonly GoogleAuthService _googleAuthService;

    public GoogleAuthController(GoogleAuthService googleAuthService)
    {
        _googleAuthService = googleAuthService;
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        var url = _googleAuthService.ConstruirUrlAutorizacion(HttpContext);
        if (url is null)
            return BadRequest(new { message = "Falta configurar Google en appsettings.json." });

        return Redirect(url);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string? code, [FromQuery] string? state)
    {
        var resultado = await _googleAuthService.ProcesarCallbackAsync(HttpContext, code, state);
        if (!resultado.Ok)
            return StatusCode(resultado.StatusCode, new { message = resultado.Mensaje });

        return Redirect("/api/auth/me");
    }
    }
}

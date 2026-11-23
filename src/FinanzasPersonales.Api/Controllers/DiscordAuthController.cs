using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanzasPersonales.Api
{
    [Route("api/auth/discord")]
    [ApiController]
    public class DiscordAuthController : ControllerBase
    {
        private readonly DiscordAuthService _discordAuthService;

    public DiscordAuthController(DiscordAuthService discordAuthService)
    {
        _discordAuthService = discordAuthService;
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        var url = _discordAuthService.ConstruirUrlAutorizacion(HttpContext);
        if (url is null)
            return BadRequest(new { message = "Falta configurar Discord en appsettings.json." });

        return Redirect(url);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string? code, [FromQuery] string? state)
    {
        var resultado = await _discordAuthService.ProcesarCallbackAsync(HttpContext, code, state);
        if (!resultado.Ok)
            return StatusCode(resultado.StatusCode, new { message = resultado.Mensaje });

        return Redirect("/api/auth/me");
    }
    }
}

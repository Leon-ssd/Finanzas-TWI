using FinanzasPersonales.Api.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanzasPersonales.Api.Controllers;

[AllowAnonymous]
[Route("auth/dropbox")]
public class DropboxAuthController(IAuthenticationSchemeProvider schemes) : Controller
{
    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login()
    {
        if (await schemes.GetSchemeAsync(Dev3Authentication.DropboxScheme) is null)
            return Redirect(Dev3Authentication.LoginPath + "?error=dropbox-not-configured");

        // Callback atendido por OAuthHandler, no por una acción que omita state/correlación.
        return Challenge(new AuthenticationProperties { RedirectUri = Dev3Authentication.LoginPath },
            Dev3Authentication.DropboxScheme);
    }
}

using FinanzasPersonales.Api.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanzasPersonales.Api.Controllers;

[AllowAnonymous]
[Route("auth/microsoft")]
public class MicrosoftAuthController(IAuthenticationSchemeProvider schemes) : Controller
{
    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login()
    {
        if (await schemes.GetSchemeAsync(Dev3Authentication.MicrosoftScheme) is null)
            return Redirect(Dev3Authentication.LoginPath + "?error=microsoft-not-configured");

        // Callback y validación OIDC a cargo del middleware oficial.
        return Challenge(new AuthenticationProperties { RedirectUri = Dev3Authentication.LoginPath },
            Dev3Authentication.MicrosoftScheme);
    }
}

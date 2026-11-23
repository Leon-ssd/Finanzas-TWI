using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace FinanzasPersonales.Api.Controllers;

public abstract class BaseApiController : ControllerBase
{
    protected Guid? ObtenerUsuarioId()
    {
        var claimValue = User.FindFirst("usuario_id")?.Value 
                         ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (Guid.TryParse(claimValue, out var usuarioId))
        {
            return usuarioId;
        }

        return null;
    }
}

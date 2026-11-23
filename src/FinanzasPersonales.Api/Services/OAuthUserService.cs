using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Models;

namespace FinanzasPersonales.Api.Services;

public interface IOAuthUserService
{
    Task ProcessOAuthUserAsync(TicketReceivedContext context);
}

public class OAuthUserService : IOAuthUserService
{
    private readonly FinanzasDbContext _dbContext;
    private readonly ILogger<OAuthUserService> _logger;

    public OAuthUserService(FinanzasDbContext dbContext, ILogger<OAuthUserService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task ProcessOAuthUserAsync(TicketReceivedContext context)
    {
        var principal = context.Principal;
        if (principal == null)
        {
            context.Fail("No se encontró información del usuario autenticado por el proveedor externo.");
            return;
        }

        var provider = context.Scheme.Name;
        var email = principal.FindFirst(ClaimTypes.Email)?.Value;
        var name = principal.FindFirst(ClaimTypes.Name)?.Value 
                   ?? principal.FindFirst("name")?.Value 
                   ?? principal.FindFirst("urn:twitch:display_name")?.Value
                   ?? principal.FindFirst("urn:spotify:display_name")?.Value;
        var providerUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.NewGuid().ToString();

        if (string.IsNullOrWhiteSpace(email))
        {
            // Si el proveedor no devuelve email, generamos un identificador unívoco para el proveedor
            email = $"{provider.ToLower()}_{providerUserId}@oauth.local";
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            name = $"{provider} User ({providerUserId[..Math.Min(6, providerUserId.Length)]})";
        }

        _logger.LogInformation("Procesando autenticación OAuth [{Provider}] para email: {Email}", provider, email);

        var usuario = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        if (usuario == null)
        {
            usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Nombre = name,
                Email = email,
                FechaCreacion = DateTimeOffset.UtcNow,
                Activo = true
            };

            _dbContext.Usuarios.Add(usuario);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Usuario registrado exitosamente desde {Provider} con ID: {UsuarioId}", provider, usuario.Id);
        }
        else
        {
            if (!usuario.Activo)
            {
                _logger.LogWarning("Intento de acceso bloqueado para usuario inactivo ID: {UsuarioId}", usuario.Id);
                context.Fail("El usuario se encuentra inactivo en el sistema.");
                return;
            }

            _logger.LogInformation("Usuario existente vinculado exitosamente desde {Provider} ID: {UsuarioId}", provider, usuario.Id);
        }

        // Crear los claims de la sesión de la aplicación
        var appClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new("usuario_id", usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nombre),
            new(ClaimTypes.Email, usuario.Email),
            new("provider", provider)
        };

        var identity = new ClaimsIdentity(appClaims, CookieAuthenticationDefaults.AuthenticationScheme);
        context.Principal = new ClaimsPrincipal(identity);
    }
}

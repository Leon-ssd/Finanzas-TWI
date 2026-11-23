using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using FinanzasPersonales.Api.Data;

namespace FinanzasPersonales.Api;

public class DiscordAuthService
{

    private readonly FinanzasDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;

    public DiscordAuthService(
        FinanzasDbContext db,
        IHttpClientFactory httpClientFactory,
        IConfiguration config)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
        _config = config;
    }

    public string? ConstruirUrlAutorizacion(HttpContext httpContext)
    {
        var clientId = _config["Authentication:Discord:ClientId"];
        var redirectUri = _config["Authentication:Discord:RedirectUri"];

        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(redirectUri))
            return null;

        var state = Guid.NewGuid().ToString();
        httpContext.Session.SetString("stateDiscord", state);

        var scope = Uri.EscapeDataString("identify email");

        return "https://discord.com/oauth2/authorize" +
               $"?client_id={Uri.EscapeDataString(clientId)}" +
               $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
               "&response_type=code" +
               $"&scope={scope}" +
               $"&state={Uri.EscapeDataString(state)}";
    }

    public async Task<(bool Ok, int StatusCode, string Mensaje)> ProcesarCallbackAsync(
        HttpContext httpContext,
        string? code,
        string? state)
    {
        if (string.IsNullOrWhiteSpace(code) || !ValidarYConsumirState(httpContext, state))
            return (false, 400, "Código de autorización o state inválido en Discord.");

        var clientId = _config["Authentication:Discord:ClientId"];
        var clientSecret = _config["Authentication:Discord:ClientSecret"];
        var redirectUri = _config["Authentication:Discord:RedirectUri"];

        if (string.IsNullOrWhiteSpace(clientId) ||
            string.IsNullOrWhiteSpace(clientSecret) ||
            string.IsNullOrWhiteSpace(redirectUri))
        {
            return (false, 400, "Configuración de Discord incompleta en appsettings.json.");
        }

        var httpClient = _httpClientFactory.CreateClient();

        var tokenPayload = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = redirectUri
        });

        var tokenResponse = await httpClient.PostAsync("https://discord.com/api/oauth2/token", tokenPayload);
        if (!tokenResponse.IsSuccessStatusCode)
            return (false, 400, "No se pudo intercambiar el código por token con Discord.");

        var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
        using var tokenDoc = JsonDocument.Parse(tokenJson);

        if (!tokenDoc.RootElement.TryGetProperty("access_token", out var accessTokenProp))
            return (false, 400, "La respuesta de Discord no incluyó access_token.");

        var accessToken = accessTokenProp.GetString();
        if (string.IsNullOrWhiteSpace(accessToken))
            return (false, 400, "El access_token devuelto por Discord está vacío.");

        var userRequest = new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/users/@me");
        userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var userResponse = await httpClient.SendAsync(userRequest);
        if (!userResponse.IsSuccessStatusCode)
            return (false, 400, "No se pudo consultar el perfil del usuario en Discord.");

        var userJson = await userResponse.Content.ReadAsStringAsync();
        using var userDoc = JsonDocument.Parse(userJson);

        var email = userDoc.RootElement.TryGetProperty("email", out var emailProp)
            ? emailProp.GetString()
            : null;

        if (string.IsNullOrWhiteSpace(email))
            return (false, 400, "Discord no devolvió un correo electrónico válido.");

        var nombre = userDoc.RootElement.TryGetProperty("global_name", out var globalNameProp) &&
                     !string.IsNullOrWhiteSpace(globalNameProp.GetString())
            ? globalNameProp.GetString()!
            : (userDoc.RootElement.TryGetProperty("username", out var usernameProp) &&
               !string.IsNullOrWhiteSpace(usernameProp.GetString())
                ? usernameProp.GetString()!
                : email);

        var emailNormalizado = email.Trim().ToLowerInvariant();
        var usuario = await _db.Usuarios
            .FirstOrDefaultAsync(u => u.Email.ToLower() == emailNormalizado);

        if (usuario is null)
        {
            usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Nombre = nombre.Trim(),
                Email = emailNormalizado,
                PasswordHash = null,
                FechaCreacion = DateTimeOffset.UtcNow,
                Activo = true
            };

            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync();
        }
        else if (!usuario.Activo)
        {
            return (false, 403, "El usuario se encuentra inactivo.");
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nombre),
            new Claim(ClaimTypes.Email, usuario.Email)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return (true, 200, "Autenticación con Discord completada.");
    }

    private bool ValidarYConsumirState(HttpContext httpContext, string? stateRecibido)
    {
        var stateGuardado = httpContext.Session.GetString("stateDiscord");
        httpContext.Session.Remove("stateDiscord");

        return !string.IsNullOrWhiteSpace(stateRecibido) &&
               !string.IsNullOrWhiteSpace(stateGuardado) &&
               string.Equals(stateRecibido, stateGuardado, StringComparison.Ordinal);
    }
}

using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace FinanzasPersonales.Api;

public class GoogleAuthService
{
    private readonly ApiDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;

    public GoogleAuthService(
        ApiDbContext db,
        IHttpClientFactory httpClientFactory,
        IConfiguration config)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
        _config = config;
    }

    public string? ConstruirUrlAutorizacion(HttpContext httpContext)
    {
        var clientId = _config["Authentication:Google:ClientId"];
        var redirectUri = _config["Authentication:Google:RedirectUri"];

        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(redirectUri))
            return null;

        var state = Guid.NewGuid().ToString();
        httpContext.Session.SetString("stateGoogle", state);

        var scope = Uri.EscapeDataString("openid profile email");

        return "https://accounts.google.com/o/oauth2/v2/auth" +
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
            return (false, 400, "Código de autorización o state inválido en Google.");

        var clientId = _config["Authentication:Google:ClientId"];
        var clientSecret = _config["Authentication:Google:ClientSecret"];
        var redirectUri = _config["Authentication:Google:RedirectUri"];

        if (string.IsNullOrWhiteSpace(clientId) ||
            string.IsNullOrWhiteSpace(clientSecret) ||
            string.IsNullOrWhiteSpace(redirectUri))
        {
            return (false, 400, "Configuración de Google incompleta en appsettings.json.");
        }

        var httpClient = _httpClientFactory.CreateClient();

        var tokenPayload = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["redirect_uri"] = redirectUri,
            ["grant_type"] = "authorization_code"
        });

        var tokenResponse = await httpClient.PostAsync("https://oauth2.googleapis.com/token", tokenPayload);
        if (!tokenResponse.IsSuccessStatusCode)
        {
            var errorBody = await tokenResponse.Content.ReadAsStringAsync();
            return (false, 400, $"Error de Google ({(int)tokenResponse.StatusCode}): {errorBody}");
        }

        var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
        using var tokenDoc = JsonDocument.Parse(tokenJson);

        if (!tokenDoc.RootElement.TryGetProperty("access_token", out var accessTokenProp))
            return (false, 400, "La respuesta de Google no incluyó access_token.");

        var accessToken = accessTokenProp.GetString();
        if (string.IsNullOrWhiteSpace(accessToken))
            return (false, 400, "El access_token devuelto por Google está vacío.");

        var userRequest = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v3/userinfo");
        userRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var userResponse = await httpClient.SendAsync(userRequest);
        if (!userResponse.IsSuccessStatusCode)
            return (false, 400, "No se pudo consultar el perfil del usuario en Google.");

        var userJson = await userResponse.Content.ReadAsStringAsync();
        using var userDoc = JsonDocument.Parse(userJson);

        var email = userDoc.RootElement.TryGetProperty("email", out var emailProp)
            ? emailProp.GetString()
            : null;

        if (string.IsNullOrWhiteSpace(email))
            return (false, 400, "Google no devolvió un correo electrónico válido.");

        var nombre = userDoc.RootElement.TryGetProperty("name", out var nameProp) &&
                     !string.IsNullOrWhiteSpace(nameProp.GetString())
            ? nameProp.GetString()!
            : email;

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

        return (true, 200, "Autenticación con Google completada.");
    }

    private bool ValidarYConsumirState(HttpContext httpContext, string? stateRecibido)
    {
        var stateGuardado = httpContext.Session.GetString("stateGoogle");
        httpContext.Session.Remove("stateGoogle");

        return !string.IsNullOrWhiteSpace(stateRecibido) &&
               !string.IsNullOrWhiteSpace(stateGuardado) &&
               string.Equals(stateRecibido, stateGuardado, StringComparison.Ordinal);
    }
}

using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using DotNetEnv;
using FinanzasPersonales.Api;
using FinanzasPersonales.Api.Authentication;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=finanzas_db;Username=postgres;Password=postgres";

builder.Services.AddScoped<IOAuthUserService, OAuthUserService>();

var twitchClientId = builder.Configuration["AUTH__TWITCH__CLIENTID"]
    ?? builder.Configuration["Auth:Twitch:ClientId"]
    ?? "placeholder_twitch_client_id";
var twitchClientSecret = builder.Configuration["AUTH__TWITCH__CLIENTSECRET"]
    ?? builder.Configuration["Auth:Twitch:ClientSecret"]
    ?? "placeholder_twitch_client_secret";
var twitchCallbackPath = builder.Configuration["AUTH__TWITCH__CALLBACKPATH"]
    ?? builder.Configuration["Auth:Twitch:CallbackPath"]
    ?? "/signin-twitch";

var spotifyClientId = builder.Configuration["AUTH__SPOTIFY__CLIENTID"]
    ?? builder.Configuration["Auth:Spotify:ClientId"]
    ?? "placeholder_spotify_client_id";
var spotifyClientSecret = builder.Configuration["AUTH__SPOTIFY__CLIENTSECRET"]
    ?? builder.Configuration["Auth:Spotify:ClientSecret"]
    ?? "placeholder_spotify_client_secret";
var spotifyCallbackPath = builder.Configuration["AUTH__SPOTIFY__CALLBACKPATH"]
    ?? builder.Configuration["Auth:Spotify:CallbackPath"]
    ?? "/signin-spotify";

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.Name = ".FinanzasPersonales.Auth";
    options.Cookie.HttpOnly = true;
    options.LoginPath = "/";
    options.LogoutPath = "/api/auth/logout";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
})
.AddOAuth("Twitch", options =>
{
    options.ClientId = twitchClientId;
    options.ClientSecret = twitchClientSecret;
    options.CallbackPath = new PathString(twitchCallbackPath);
    options.AuthorizationEndpoint = "https://id.twitch.tv/oauth2/authorize";
    options.TokenEndpoint = "https://id.twitch.tv/oauth2/token";
    options.UserInformationEndpoint = "https://api.twitch.tv/helix/users";
    options.SaveTokens = true;

    options.Scope.Add("user:read:email");

    options.Events = new OAuthEvents
    {
        OnCreatingTicket = async context =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
            request.Headers.Add("Client-Id", context.Options.ClientId);

            var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
            response.EnsureSuccessStatusCode();

            using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var root = payload.RootElement;
            if (root.TryGetProperty("data", out var data) && data.GetArrayLength() > 0)
            {
                var userElem = data[0];
                var id = userElem.GetProperty("id").GetString();
                var login = userElem.GetProperty("login").GetString();
                var displayName = userElem.GetProperty("display_name").GetString();
                var email = userElem.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null;

                if (!string.IsNullOrEmpty(id))
                {
                    context.Identity?.AddClaim(new Claim(ClaimTypes.NameIdentifier, id));
                }
                if (!string.IsNullOrEmpty(displayName) || !string.IsNullOrEmpty(login))
                {
                    context.Identity?.AddClaim(new Claim(ClaimTypes.Name, displayName ?? login!));
                }
                if (!string.IsNullOrEmpty(email))
                {
                    context.Identity?.AddClaim(new Claim(ClaimTypes.Email, email));
                }
            }
        },
        OnTicketReceived = async context =>
        {
            var userService = context.HttpContext.RequestServices.GetRequiredService<IOAuthUserService>();
            await userService.ProcessOAuthUserAsync(context);
        }
    };
})
.AddOAuth("Spotify", options =>
{
    options.ClientId = spotifyClientId;
    options.ClientSecret = spotifyClientSecret;
    options.CallbackPath = new PathString(spotifyCallbackPath);
    options.AuthorizationEndpoint = "https://accounts.spotify.com/authorize";
    options.TokenEndpoint = "https://accounts.spotify.com/api/token";
    options.UserInformationEndpoint = "https://api.spotify.com/v1/me";
    options.SaveTokens = true;

    options.Scope.Add("user-read-email");
    options.Scope.Add("user-read-private");

    options.Events = new OAuthEvents
    {
        OnCreatingTicket = async context =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

            var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
            response.EnsureSuccessStatusCode();

            using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var root = payload.RootElement;

            var id = root.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
            var displayName = root.TryGetProperty("display_name", out var dnProp) ? dnProp.GetString() : null;
            var email = root.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null;

            if (!string.IsNullOrEmpty(id))
            {
                context.Identity?.AddClaim(new Claim(ClaimTypes.NameIdentifier, id));
            }
            if (!string.IsNullOrEmpty(displayName))
            {
                context.Identity?.AddClaim(new Claim(ClaimTypes.Name, displayName));
            }
            if (!string.IsNullOrEmpty(email))
            {
                context.Identity?.AddClaim(new Claim(ClaimTypes.Email, email));
            }
        },
        OnTicketReceived = async context =>
        {
            var userService = context.HttpContext.RequestServices.GetRequiredService<IOAuthUserService>();
            await userService.ProcessOAuthUserAsync(context);
        }
    };
});

builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddDev3Authentication(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddOptions<VinculosIdentidadDev3Options>()
    .Bind(builder.Configuration.GetSection("Dev3"))
    .Validate(options => options.EsValida(), "Los vínculos de identidad Dev3 deben ser explícitos, válidos y no duplicados.")
    .ValidateOnStart();
builder.Services.AddScoped<AccesoDev3Filter>();

builder.Services.AddDbContext<FinanzasDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddHttpClient();

builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<CategoriaService>();
builder.Services.AddScoped<CuentaService>();
builder.Services.AddScoped<MovimientoService>();
builder.Services.AddScoped<DiscordAuthService>();
builder.Services.AddScoped<GoogleAuthService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

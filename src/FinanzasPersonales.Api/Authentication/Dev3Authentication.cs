using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Validators;

namespace FinanzasPersonales.Api.Authentication;

public static class Dev3Authentication
{
    // Una sola cookie local para todos los proveedores que se integren al backend.
    public const string LocalScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    public const string DropboxScheme = "Dev3.Dropbox";
    public const string MicrosoftScheme = "Dev3.Microsoft";
    public const string LoginPath = "/auth/dev3";
    public const string ProviderClaim = "external_provider";

    public static IServiceCollection AddDev3Authentication(
        this IServiceCollection services, IConfiguration configuration)
    {
        var dropbox = configuration.GetSection("Authentication:Dropbox");
        var microsoft = configuration.GetSection("Authentication:Microsoft");
        var dropboxCallback = GetCallback(dropbox, "/auth/dropbox/callback");
        var microsoftCallback = GetCallback(microsoft, "/auth/microsoft/callback");
        if (string.Equals(dropboxCallback, microsoftCallback, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Los callbacks OAuth deben ser distintos.");
        }

        // Los handlers pueden incluir respuestas remotas en sus diagnósticos internos.
        // Se registra solo el resultado sanitizado en RedirectFailure.
        services.AddLogging(logging =>
        {
            logging.AddFilter("Microsoft.AspNetCore.Authentication.OAuth.OAuthHandler", LogLevel.None);
            logging.AddFilter("Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectHandler", LogLevel.None);
        });

        services.AddAntiforgery(options =>
        {
            options.Cookie.Name = "__Host-Finanzas.Antiforgery";
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });

        var authentication = services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = LocalScheme;
            options.DefaultSignInScheme = LocalScheme;
            options.DefaultChallengeScheme = LocalScheme;
        }).AddCookie(LocalScheme, options =>
        {
            options.Cookie.Name = "__Host-Finanzas.Session";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.LoginPath = LoginPath;
            options.AccessDeniedPath = LoginPath;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            options.SlidingExpiration = false;
        });

        // Una instalación sin secretos puede arrancar y mostrar la página de configuración.
        if (HasCredentials(dropbox))
        {
            authentication.AddOAuth(DropboxScheme, options => ConfigureDropbox(options, dropbox, dropboxCallback));
        }

        if (HasCredentials(microsoft))
        {
            authentication.AddOpenIdConnect(MicrosoftScheme,
                options => ConfigureMicrosoft(options, microsoft, microsoftCallback));
        }

        return services;
    }

    private static void ConfigureDropbox(OAuthOptions options, IConfigurationSection config, string callback)
    {
        options.SignInScheme = LocalScheme;
        options.ClientId = config["ClientId"]!;
        options.ClientSecret = config["ClientSecret"]!;
        options.CallbackPath = callback;
        options.AuthorizationEndpoint = "https://www.dropbox.com/oauth2/authorize";
        options.TokenEndpoint = "https://api.dropboxapi.com/oauth2/token";
        options.UserInformationEndpoint = "https://api.dropboxapi.com/2/users/get_current_account";
        options.Scope.Clear();
        options.Scope.Add("account_info.read");
        options.UsePkce = true;
        options.SaveTokens = false;
        options.RemoteAuthenticationTimeout = TimeSpan.FromMinutes(10);
        options.BackchannelTimeout = TimeSpan.FromSeconds(30);
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
        options.CorrelationCookie.HttpOnly = true;
        options.CorrelationCookie.SameSite = SameSiteMode.None;
        options.ClaimActions.MapJsonSubKey(ClaimTypes.Name, "name", "display_name");
        options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
        options.Events = new OAuthEvents
        {
            OnCreatingTicket = async context =>
            {
                // El middleware ya validó state/correlación e intercambió el código.
                if (string.IsNullOrWhiteSpace(context.AccessToken))
                    throw new AuthenticationFailureException("Dropbox no devolvió una identidad válida.");

                using var request = new HttpRequestMessage(HttpMethod.Post, context.Options.UserInformationEndpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                request.Content = new StringContent("null", Encoding.UTF8, "application/json");
                using var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
                if (!response.IsSuccessStatusCode)
                    throw new AuthenticationFailureException("No se pudo consultar la identidad de Dropbox.");

                using var user = await JsonDocument.ParseAsync(
                    await response.Content.ReadAsStreamAsync(context.HttpContext.RequestAborted),
                    cancellationToken: context.HttpContext.RequestAborted);
                if (!user.RootElement.TryGetProperty("account_id", out var accountId) ||
                    accountId.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(accountId.GetString()))
                    throw new AuthenticationFailureException("Dropbox no devolvió una identidad válida.");

                context.RunClaimActions(user.RootElement);
                context.Identity!.AddClaim(new Claim(ClaimTypes.NameIdentifier, "dropbox|" + accountId.GetString()));
                context.Identity.AddClaim(new Claim(ProviderClaim, "Dropbox"));
            },
            OnRemoteFailure = context => RedirectFailure(context, "dropbox"),
            OnAccessDenied = context =>
            {
                context.HandleResponse();
                context.Response.Redirect(LoginPath + "?error=dropbox-cancelled");
                return Task.CompletedTask;
            }
        };
    }

    private static void ConfigureMicrosoft(OpenIdConnectOptions options, IConfigurationSection config, string callback)
    {
        var tenant = config["TenantId"] ?? "common";
        if (tenant is not ("common" or "consumers" or "organizations") && !Guid.TryParse(tenant, out _))
            throw new InvalidOperationException("TenantId debe ser common, consumers, organizations o un GUID de tenant.");

        options.Authority = $"https://login.microsoftonline.com/{tenant}/v2.0";
        options.RequireHttpsMetadata = true;
        options.ClientId = config["ClientId"]!;
        options.ClientSecret = config["ClientSecret"]!;
        options.SignInScheme = LocalScheme;
        options.CallbackPath = callback;
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.ResponseMode = OpenIdConnectResponseMode.FormPost;
        options.UsePkce = true;
        options.SaveTokens = false;
        options.MapInboundClaims = false;
        options.GetClaimsFromUserInfoEndpoint = false;
        // Microsoft no anuncia PAR en todos sus tenants; se usa Authorization Code estándar.
        options.PushedAuthorizationBehavior = PushedAuthorizationBehavior.Disable;
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.RemoteAuthenticationTimeout = TimeSpan.FromMinutes(10);
        options.BackchannelTimeout = TimeSpan.FromSeconds(30);
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
        options.CorrelationCookie.HttpOnly = true;
        options.CorrelationCookie.SameSite = SameSiteMode.None;
        options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
        options.NonceCookie.HttpOnly = true;
        options.NonceCookie.SameSite = SameSiteMode.None;
        options.ProtocolValidator.RequireNonce = true;
        options.TokenValidationParameters.NameClaimType = "name";
        options.TokenValidationParameters.ValidateIssuer = true;
        options.TokenValidationParameters.ValidateAudience = true;
        options.TokenValidationParameters.ValidAudience = options.ClientId;
        options.TokenValidationParameters.ValidateLifetime = true;
        options.TokenValidationParameters.ValidateIssuerSigningKey = true;
        options.TokenValidationParameters.RequireExpirationTime = true;
        options.TokenValidationParameters.RequireSignedTokens = true;
        // Validador oficial para emisores tenant-specific, también con common/consumers.
        options.TokenValidationParameters.IssuerValidator =
            AadIssuerValidator.GetAadIssuerValidator(options.Authority).Validate;
        options.Events = new OpenIdConnectEvents
        {
            OnTokenValidated = context =>
            {
                var subject = context.Principal?.FindFirstValue("sub");
                if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(context.SecurityToken.Issuer))
                {
                    context.Fail("Microsoft no devolvió una identidad válida.");
                    return Task.CompletedTask;
                }

                var identity = (ClaimsIdentity)context.Principal!.Identity!;
                // Se identifica por emisor + subject verificados, nunca por correo.
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, context.SecurityToken.Issuer + "|" + subject));
                identity.AddClaim(new Claim(ProviderClaim, "Microsoft"));
                return Task.CompletedTask;
            },
            OnRemoteFailure = context => RedirectFailure(context, "microsoft"),
            OnAccessDenied = context =>
            {
                context.HandleResponse();
                context.Response.Redirect(LoginPath + "?error=microsoft-cancelled");
                return Task.CompletedTask;
            }
        };
    }

    private static Task RedirectFailure(RemoteFailureContext context, string provider)
    {
        // No devolver ni registrar excepciones, códigos, tokens o textos del proveedor.
        context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("FinanzasPersonales.Authentication.Dev3")
            .LogWarning("No se completó la autenticación externa de {Provider}.", provider);
        context.HandleResponse();
        context.Response.Redirect(LoginPath + "?error=" + provider + "-failed");
        return Task.CompletedTask;
    }

    private static bool HasCredentials(IConfigurationSection section) =>
        !string.IsNullOrWhiteSpace(section["ClientId"]) && !string.IsNullOrWhiteSpace(section["ClientSecret"]);

    private static string GetCallback(IConfigurationSection section, string defaultPath)
    {
        var path = section["CallbackPath"] ?? defaultPath;
        // Callbacks solo bajo /auth/{proveedor}/; no pueden ocupar acciones MVC.
        var prefix = defaultPath[..defaultPath.LastIndexOf('/')];
        if (!Regex.IsMatch(path, "^" + Regex.Escape(prefix) + "/[a-zA-Z0-9_-]+$") ||
            path.Equals(prefix + "/login", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"CallbackPath debe ser una ruta distinta de login bajo {prefix}/.");
        return path;
    }
}

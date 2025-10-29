using System.Net.Security;
using System.Text.Json;
using System.Text.Json.Nodes;

using ApiPortal;

using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.Configure<KeycloakOptions>(builder.Configuration.GetSection("Keycloak"));

var catalogRestAddress = builder.Configuration["services:catalog:rest:0"] ?? "http://catalog:5206";

builder.Services.AddHttpClient("catalog-openapi", client =>
    {
        client.BaseAddress = new Uri(catalogRestAddress);
    })
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        SslOptions = new SslClientAuthenticationOptions
        {
            RemoteCertificateValidationCallback = static (_, _, _, _) => true
        }
    });

builder.Services.AddHttpClient("basket-openapi", client =>
    {
        client.BaseAddress = new Uri("https//basket");
    })
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        SslOptions = new SslClientAuthenticationOptions
        {
            RemoteCertificateValidationCallback = static (_, _, _, _) => true
        }
    });

builder.Services.AddHttpClient("keycloak-token")
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        SslOptions = new SslClientAuthenticationOptions
        {
            RemoteCertificateValidationCallback = static (_, _, _, _) => true
        }
    });

var app = builder.Build();
var logger = app.Logger;

if (app.Environment.IsDevelopment())
{
    app.MapDefaultEndpoints();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/openapi/catalog.json", async (IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("catalog-openapi");
    string? json = null;

    foreach (var path in new[] { "/openapi/v1.json", "/openapi.json", "/swagger/v1/swagger.json" })
    {
        try
        {
            json = await client.GetStringAsync(path);
            json = EnsureBearerAuth(json);
            logger.LogInformation("Loaded Catalog OpenAPI from {Path}", path);
            break;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to load Catalog OpenAPI from {Path}", path);
        }
    }

    return json is null
        ? Results.Problem("Catalog OpenAPI is unavailable. Make sure the service is running and OpenAPI is enabled.",
            statusCode: StatusCodes.Status502BadGateway)
        : Results.Content(json, "application/json");
});

app.MapGet("/openapi/basket.json", async (IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("basket-openapi");
    string? json = null;

    foreach (var path in new[] { "/openapi/v1.json", "/openapi.json", "/swagger/v1/swagger.json" })
    {
        try
        {
            json = await client.GetStringAsync(path);
            json = EnsureBearerAuth(json);
            logger.LogInformation("Loaded Basket OpenAPI from {Path}", path);
            break;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to load Basket OpenAPI from {Path}", path);
        }
    }

    return json is null
        ? Results.Problem("Basket OpenAPI is unavailable. Make sure the service is running and OpenAPI is enabled.",
            statusCode: StatusCodes.Status502BadGateway)
        : Results.Content(json, "application/json");
});

app.MapGet("/keycloak/config", (IOptions<KeycloakOptions> options) =>
    {
        var value = options.Value;
        if (string.IsNullOrWhiteSpace(value.Authority) || string.IsNullOrWhiteSpace(value.Realm))
        {
            return Results.Problem(
                "Keycloak configuration is missing. Check the environment variables injected by AppHost.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        return Results.Ok(new
        {
            value.Authority,
            value.Realm,
            value.ClientId,
            TokenEndpoint = BuildTokenEndpoint(value),
            DefaultUser = new
            {
                value.DefaultUser.Username, HasPassword = !string.IsNullOrWhiteSpace(value.DefaultUser.Password)
            }
        });
    })
    .WithTags("Keycloak");

app.MapPost("/keycloak/token",
        async (KeycloakTokenRequest request, IOptions<KeycloakOptions> options, IHttpClientFactory factory) =>
        {
            var value = options.Value;
            if (string.IsNullOrWhiteSpace(value.Authority) || string.IsNullOrWhiteSpace(value.Realm))
            {
                return Results.Problem(
                    "Keycloak configuration is missing. Check the environment variables injected by AppHost.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }

            var grantType = string.IsNullOrWhiteSpace(request.GrantType) ? "password" : request.GrantType;
            var username = string.IsNullOrWhiteSpace(request.Username) ? value.DefaultUser.Username : request.Username!;
            var password = string.IsNullOrWhiteSpace(request.Password) ? value.DefaultUser.Password : request.Password!;
            var clientId = string.IsNullOrWhiteSpace(request.ClientId) ? value.ClientId : request.ClientId!;
            var clientSecret = string.IsNullOrWhiteSpace(request.ClientSecret)
                ? value.ClientSecret
                : request.ClientSecret;

            if (string.IsNullOrWhiteSpace(clientId))
            {
                return Results.BadRequest(new { error = "client_id is required" });
            }

            if (grantType.Equals("password", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    return Results.BadRequest(new { error = "username or password is missing" });
                }
            }
            else if (grantType.Equals("refresh_token", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(request.RefreshToken))
                {
                    return Results.BadRequest(new { error = "refresh_token is required" });
                }
            }

            var form = new Dictionary<string, string> { ["grant_type"] = grantType, ["client_id"] = clientId };

            if (!string.IsNullOrWhiteSpace(clientSecret))
            {
                form["client_secret"] = clientSecret;
            }

            if (!string.IsNullOrWhiteSpace(request.Scope))
            {
                form["scope"] = request.Scope!;
            }

            if (grantType.Equals("password", StringComparison.OrdinalIgnoreCase))
            {
                form["username"] = username;
                form["password"] = password;
            }
            else if (grantType.Equals("refresh_token", StringComparison.OrdinalIgnoreCase))
            {
                form["refresh_token"] = request.RefreshToken!;
            }

            if (request.Extra is { Count: > 0 })
            {
                foreach (var pair in request.Extra.Where(pair =>
                             !string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value)))
                {
                    form[pair.Key] = pair.Value;
                }
            }

            var tokenEndpoint = BuildTokenEndpoint(value);
            var client = factory.CreateClient("keycloak-token");

            using var content = new FormUrlEncodedContent(form);
            using var response = await client.PostAsync(tokenEndpoint, content);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Keycloak token request failed: {StatusCode} - {Body}", response.StatusCode, body);
                return Results.Json(
                    new { error = "Keycloak token request failed", status = (int)response.StatusCode, response = body },
                    statusCode: StatusCodes.Status502BadGateway);
            }

            return Results.Content(body, "application/json");
        })
    .WithTags("Keycloak");

app.MapGet("/openapi/keycloak.json", (IOptions<KeycloakOptions> options) =>
{
    var json = BuildKeycloakOpenApi(options.Value);
    return Results.Content(json, "application/json");
});

app.MapFallbackToFile("index.html");

app.Run();

static string BuildTokenEndpoint(KeycloakOptions options)
{
    var authority = (options.Authority).TrimEnd('/');
    return $"{authority}/realms/{options.Realm}/protocol/openid-connect/token";
}

static string BuildKeycloakOpenApi(KeycloakOptions options)
{
    var clientId = options.ClientId;
    var username = options.DefaultUser.Username;
    var password = options.DefaultUser.Password;

    var doc = new JsonObject
    {
        ["openapi"] = "3.0.1",
        ["info"] = new JsonObject
        {
            ["title"] = "Keycloak Helper API",
            ["version"] = "1.0.0",
            ["description"] =
                "Helper endpoints for inspecting Keycloak configuration and testing token requests."
        },
        ["servers"] = new JsonArray { new JsonObject { ["url"] = "/" } }
    };

    var paths = new JsonObject();

    paths["/keycloak/config"] = new JsonObject
    {
        ["get"] = new JsonObject
        {
            ["tags"] = new JsonArray("Keycloak"),
            ["summary"] = "Get Keycloak configuration",
            ["responses"] = new JsonObject
            {
                ["200"] = new JsonObject { ["description"] = "Configuration returned" },
                ["500"] = new JsonObject { ["description"] = "Configuration missing" }
            }
        }
    };

    paths["/keycloak/token"] = new JsonObject
    {
        ["post"] = new JsonObject
        {
            ["tags"] = new JsonArray("Keycloak"),
            ["summary"] = "Request Keycloak access token",
            ["requestBody"] = new JsonObject
            {
                ["required"] = true,
                ["content"] = new JsonObject
                {
                    ["application/json"] = new JsonObject
                    {
                        ["example"] = new JsonObject
                        {
                            ["grantType"] = "password",
                            ["clientId"] = clientId,
                            ["username"] = username,
                            ["password"] = password,
                            ["scope"] = "openid profile"
                        }
                    }
                }
            },
            ["responses"] = new JsonObject
            {
                ["200"] = new JsonObject { ["description"] = "Token returned" },
                ["400"] = new JsonObject { ["description"] = "Validation error" },
                ["502"] = new JsonObject { ["description"] = "Keycloak request failed" }
            }
        }
    };

    doc["paths"] = paths;

    doc["components"] = new JsonObject
    {
        ["securitySchemes"] = new JsonObject
        {
            ["none"] = new JsonObject { ["type"] = "http", ["scheme"] = "none" }
        }
    };

    return doc.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
}

static string EnsureBearerAuth(string json)
{
    var node = JsonNode.Parse(json)?.AsObject() ?? new JsonObject();

    var components = node["components"] as JsonObject;
    if (components is null)
    {
        components = new JsonObject();
        node["components"] = components;
    }

    var securitySchemes = components["securitySchemes"] as JsonObject;
    if (securitySchemes is null)
    {
        securitySchemes = new JsonObject();
        components["securitySchemes"] = securitySchemes;
    }

    if (!securitySchemes.ContainsKey("bearerAuth"))
    {
        securitySchemes["bearerAuth"] = new JsonObject
        {
            ["type"] = "http",
            ["scheme"] = "bearer",
            ["bearerFormat"] = "JWT",
            ["description"] = "Paste the Keycloak access token here. Example: Bearer eyJ..."
        };
    }

    var securityArray = node["security"] as JsonArray;
    if (securityArray is null)
    {
        securityArray = new JsonArray();
        node["security"] = securityArray;
    }

    var hasBearerRequirement = securityArray.OfType<JsonObject>()
        .Any(obj => obj.ContainsKey("bearerAuth"));

    if (!hasBearerRequirement)
    {
        securityArray.Add(new JsonObject { ["bearerAuth"] = new JsonArray() });
    }

    return node.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
}

public sealed class KeycloakTokenRequest
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? Scope { get; set; }
    public string GrantType { get; set; } = "password";
    public string? RefreshToken { get; set; }
    public Dictionary<string, string>? Extra { get; set; }
}

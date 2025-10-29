using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.Configure<WebhookOptions>(builder.Configuration.GetSection(WebhookOptions.SectionName));

var app = builder.Build();

app.MapHealthChecks("/health");

app.MapPost("/webhook/keycloak", async (HttpRequest request, IOptions<WebhookOptions> options, ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("WebhookReceiver");

    await using var buffer = new MemoryStream();
    await request.Body.CopyToAsync(buffer);

    var payloadBytes = buffer.ToArray();
    if (payloadBytes.Length == 0)
    {
        return Results.BadRequest(new { error = "Request body must not be empty." });
    }

    var settings = options.Value;
    if (!string.IsNullOrWhiteSpace(settings.Secret))
    {
        var headerName = string.IsNullOrWhiteSpace(settings.SignatureHeader) ? "X-PhaseTwo-Signature" : settings.SignatureHeader;

        if (!request.Headers.TryGetValue(headerName, out var signatureValues))
        {
            logger.LogWarning("Missing signature header {Header}", headerName);
            return Results.Unauthorized();
        }

        var signature = signatureValues.ToString();
        if (!ValidateSignature(signature, payloadBytes, settings.Secret!))
        {
            logger.LogWarning("Rejected webhook due to invalid signature.");
            return Results.Unauthorized();
        }
    }

    using var payload = JsonDocument.Parse(payloadBytes);
    var root = payload.RootElement;
    var eventType = root.TryGetProperty("type", out var typeElement) ? typeElement.GetString() ?? "unknown" : "unknown";
    var userId = root.TryGetProperty("userId", out var userElement) ? userElement.GetString() : null;
    var realm = root.TryGetProperty("realmId", out var realmElement) ? realmElement.GetString() : null;

    logger.LogInformation("Webhook received: type={EventType}, user={UserId}, realm={Realm}", eventType, userId ?? "<unknown>", realm ?? "<unknown>");

    // TODO: Invoke domain logic here to persist or react to the event.
    return Results.Ok(new { status = "received" });
});

app.Run();

static bool ValidateSignature(string? header, byte[] body, string secret)
{
    if (string.IsNullOrWhiteSpace(header))
    {
        return false;
    }

    var trimmed = header.Trim();
    const string Prefix = "sha256=";
    if (trimmed.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
    {
        trimmed = trimmed[Prefix.Length..];
    }

    byte[] headerBytes;
    try
    {
        headerBytes = Convert.FromHexString(trimmed);
    }
    catch (FormatException)
    {
        return false;
    }

    using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
    var computed = hmac.ComputeHash(body);

    return CryptographicOperations.FixedTimeEquals(headerBytes, computed);
}

sealed record WebhookOptions
{
    public const string SectionName = "Webhook";
    public string? Secret { get; init; }
    public string SignatureHeader { get; init; } = "X-PhaseTwo-Signature";
}

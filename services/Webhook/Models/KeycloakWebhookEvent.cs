using System.Text.Json;

namespace Webhook.Models;

public sealed record KeycloakWebhookEvent(
    string EventType,
    string? UserId,
    string? RealmId,
    JsonElement Payload,
    string RawPayload,
    IReadOnlyDictionary<string, string> Headers)
{
    public static KeycloakWebhookEvent From(JsonElement root, string rawPayload, IReadOnlyDictionary<string, string> headers)
    {
        var eventType = root.TryGetProperty("type", out var typeElement)
            ? typeElement.GetString() ?? "unknown"
            : "unknown";

        string? userId = null;
        if (root.TryGetProperty("authDetails", out var authDetails) &&
            authDetails.TryGetProperty("userId", out var authUser))
        {
            userId = authUser.GetString();
        }
        else if (root.TryGetProperty("userId", out var userElement))
        {
            userId = userElement.GetString();
        }

        var realm = root.TryGetProperty("realmId", out var realmElement) ? realmElement.GetString() : null;
        var payloadClone = root.Clone();
        var headersSnapshot = headers is Dictionary<string, string> dictionary
            ? new Dictionary<string, string>(dictionary, StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(headers, StringComparer.OrdinalIgnoreCase);

        return new KeycloakWebhookEvent(eventType, userId, realm, payloadClone, rawPayload, headersSnapshot);
    }
}

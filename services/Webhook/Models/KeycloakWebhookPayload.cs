using System.Text.Json;
using System.Text.Json.Serialization;

namespace Webhook.Models;

public sealed record KeycloakWebhookPayload
{
    [JsonPropertyName("time")] public long? Time { get; init; }

    [JsonPropertyName("realmId")] public string? RealmId { get; init; }

    [JsonPropertyName("uid")] public string? Uid { get; init; }

    [JsonPropertyName("type")] public string? Type { get; init; }

    [JsonPropertyName("authDetails")] public KeycloakWebhookAuthDetails? AuthDetails { get; init; }

    /// <summary>
    /// Keycloak 提供的 details 字段在不同事件类型中差异很大，保留原始 JSON 便于后续扩展。
    /// </summary>
    [JsonPropertyName("details")]
    public JsonElement? Details { get; init; }
}

public sealed record KeycloakWebhookAuthDetails
{
    [JsonPropertyName("realmId")] public string? RealmId { get; init; }

    [JsonPropertyName("clientId")] public string? ClientId { get; init; }

    [JsonPropertyName("userId")] public string? UserId { get; init; }

    [JsonPropertyName("ipAddress")] public string? IpAddress { get; init; }

    [JsonPropertyName("username")] public string? Username { get; init; }

    [JsonPropertyName("sessionId")] public string? SessionId { get; init; }
}

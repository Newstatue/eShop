using System.Text.Json;

namespace ServiceDefaults.Messaging.Events;

public sealed record KeycloakWebhookIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// Keycloak 事件类型，例如 access.LOGIN。
    /// </summary>
    public string EventType { get; init; } = default!;

    /// <summary>
    /// 相关用户标识（如果存在）。
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// 所属 Realm 标识。
    /// </summary>
    public string? RealmId { get; init; }

    /// <summary>
    /// 完整的原始 JSON 载荷。
    /// </summary>
    public string RawPayload { get; init; } = default!;

    /// <summary>
    /// Webhook 请求头快照，便于追踪来源。
    /// </summary>
    public Dictionary<string, string> Headers { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 可选的解析后载荷。使用 JsonElement 保留原始结构，便于不同事件类型自行解析。
    /// </summary>
    public JsonElement? Payload { get; init; }
}
